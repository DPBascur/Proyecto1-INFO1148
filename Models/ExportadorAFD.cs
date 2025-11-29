using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoFinal.Models;

/// <summary>
/// Exportador para Autómatas Finitos Deterministas
/// </summary>
public static class ExportadorAFD
{
    /// <summary>
    /// Exporta el AFD con reporte completo en formato TXT
    /// </summary>
    public static async Task<string> ExportarAFDCompletoAsync(
        ConstructorAFD.AFD afd,
        List<(string expresion, bool resultado, List<(int, string, int)> traza)>? validaciones = null)
    {
        var rutaAFD = AdministradorRutas.ObtenerRutaAFD();
        var nombreArchivo = AdministradorRutas.GenerarNombreArchivo("afd_completo");
        var rutaArchivo = Path.Combine(rutaAFD, nombreArchivo);
        
        // Generar reporte completo
        var reporte = ConstructorAFD.GenerarReporteAFD(afd);
        
        // Agregar validaciones si existen
        if (validaciones != null && validaciones.Count > 0)
        {
            reporte += GenerarReporteValidaciones(validaciones, afd);
        }
        
        await File.WriteAllTextAsync(rutaArchivo, reporte);
        return rutaArchivo;
    }
    
    /// <summary>
    /// Exporta la matriz de transición en formato CSV
    /// </summary>
    public static async Task<string> ExportarMatrizCSVAsync(ConstructorAFD.AFD afd)
    {
        var rutaAFD = AdministradorRutas.ObtenerRutaAFD();
        var nombreArchivo = AdministradorRutas.GenerarNombreArchivo("afd_matriz", "csv");
        var rutaArchivo = Path.Combine(rutaAFD, nombreArchivo);
        
        var csv = GenerarMatrizCSV(afd);
        
        await File.WriteAllTextAsync(rutaArchivo, csv);
        return rutaArchivo;
    }
    
    /// <summary>
    /// Exporta el AFD en formato DOT (Graphviz) para visualización gráfica
    /// </summary>
    public static async Task<string> ExportarDOTAsync(ConstructorAFD.AFD afd)
    {
        var rutaAFD = AdministradorRutas.ObtenerRutaAFD();
        var nombreArchivo = AdministradorRutas.GenerarNombreArchivo("afd_grafo", "dot");
        var rutaArchivo = Path.Combine(rutaAFD, nombreArchivo);
        
        var dot = ConstructorAFD.GenerarDOT(afd);
        
        await File.WriteAllTextAsync(rutaArchivo, dot);
        return rutaArchivo;
    }
    
    /// <summary>
    /// Exporta todos los formatos: TXT completo, CSV y DOT
    /// </summary>
    public static async Task<(string txt, string csv, string dot)> ExportarAFDTodosFormatosAsync(
        ConstructorAFD.AFD afd,
        List<(string expresion, bool resultado, List<(int, string, int)> traza)>? validaciones = null)
    {
        var txtPath = await ExportarAFDCompletoAsync(afd, validaciones);
        var csvPath = await ExportarMatrizCSVAsync(afd);
        var dotPath = await ExportarDOTAsync(afd);
        
        return (txtPath, csvPath, dotPath);
    }
    
    /// <summary>
    /// Genera la matriz de transición en formato CSV
    /// </summary>
    private static string GenerarMatrizCSV(ConstructorAFD.AFD afd)
    {
        var sb = new System.Text.StringBuilder();
        
        // Encabezado
        sb.Append("Estado,Tipo");
        foreach (var simbolo in afd.Alfabeto.OrderBy(s => s))
        {
            sb.Append($",{simbolo}");
        }
        sb.AppendLine();
        
        // Filas
        foreach (var estado in afd.Estados.OrderBy(e => e.Id))
        {
            var tipo = estado.EsEstadoInicial ? "Inicial" : (estado.EsEstadoFinal ? "Final" : "Normal");
            sb.Append($"{estado.Nombre},{tipo}");
            
            foreach (var simbolo in afd.Alfabeto.OrderBy(s => s))
            {
                if (afd.MatrizTransicion.ContainsKey((estado.Id, simbolo)))
                {
                    var destino = afd.MatrizTransicion[(estado.Id, simbolo)];
                    var destinoNombre = afd.Estados.FirstOrDefault(e => e.Id == destino)?.Nombre ?? $"q{destino}";
                    sb.Append($",{destinoNombre}");
                }
                else
                {
                    sb.Append($",-");
                }
            }
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Genera reporte de validaciones de expresiones
    /// </summary>
    private static string GenerarReporteValidaciones(
        List<(string expresion, bool resultado, List<(int, string, int)> traza)> validaciones,
        ConstructorAFD.AFD afd)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine();
        sb.AppendLine("VALIDACIÓN DE EXPRESIONES:");
        sb.AppendLine("=".PadRight(100, '='));
        sb.AppendLine();
        
        for (int i = 0; i < validaciones.Count; i++)
        {
            var (expresion, resultado, traza) = validaciones[i];
            
            sb.AppendLine($"Expresión {i + 1}: {expresion}");
            sb.AppendLine($"Resultado: {(resultado ? "✓ ACEPTADA" : "✗ RECHAZADA")}");
            sb.AppendLine();
            
            if (traza.Count > 0)
            {
                sb.AppendLine("Traza de ejecución:");
                sb.AppendLine($"  {"Estado Origen",-25} {"Símbolo",-15} {"→",-5} {"Estado Destino",-25}");
                sb.AppendLine("  " + "-".PadRight(75, '-'));
                
                foreach (var (origen, simbolo, destino) in traza)
                {
                    var origenNombre = afd.Estados.FirstOrDefault(e => e.Id == origen)?.Nombre ?? $"q{origen}";
                    var destinoNombre = afd.Estados.FirstOrDefault(e => e.Id == destino)?.Nombre ?? $"q{destino}";
                    sb.AppendLine($"  {origenNombre,-25} {simbolo,-15} → {destinoNombre}");
                }
                
                sb.AppendLine();
            }
            
            sb.AppendLine("-".PadRight(100, '-'));
            sb.AppendLine();
        }
        
        // Estadísticas de validaciones
        int aceptadas = validaciones.Count(v => v.resultado);
        int rechazadas = validaciones.Count - aceptadas;
        double porcentajeAceptadas = validaciones.Count > 0 ? (double)aceptadas / validaciones.Count * 100 : 0;
        
        sb.AppendLine("ESTADÍSTICAS DE VALIDACIÓN:");
        sb.AppendLine("-".PadRight(100, '-'));
        sb.AppendLine($"  Total de expresiones validadas: {validaciones.Count}");
        sb.AppendLine($"  Expresiones aceptadas: {aceptadas} ({porcentajeAceptadas:F1}%)");
        sb.AppendLine($"  Expresiones rechazadas: {rechazadas} ({100 - porcentajeAceptadas:F1}%)");
        sb.AppendLine();
        
        return sb.ToString();
    }
}
