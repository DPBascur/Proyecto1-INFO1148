using System;
using System.IO;
using System.Threading.Tasks;

namespace ProyectoFinal.Models;

/// <summary>
/// Exportador para gramáticas transformadas sin recursión por la izquierda
/// </summary>
public static class ExportadorRecursion
{
    /// <summary>
    /// Exporta la gramática transformada y el reporte de eliminación
    /// </summary>
    public static async Task<string> ExportarGramaticaTransformadaAsync(
        Gramatica gramaticaOriginal, 
        Gramatica gramaticaTransformada)
    {
        var rutaGramaticas = AdministradorRutas.ObtenerRutaGramaticas();
        var nombreArchivo = AdministradorRutas.GenerarNombreArchivo("grammar_sin_recursion");
        var rutaArchivo = Path.Combine(rutaGramaticas, nombreArchivo);
        
        // Generar reporte completo
        var reporte = EliminadorRecursion.GenerarReporteEliminacion(gramaticaOriginal, gramaticaTransformada);
        
        await File.WriteAllTextAsync(rutaArchivo, reporte);
        return rutaArchivo;
    }
    
    /// <summary>
    /// Exporta solo la gramática transformada en formato simple
    /// </summary>
    public static async Task<string> ExportarSoloGramaticaAsync(Gramatica gramaticaTransformada)
    {
        var rutaGramaticas = AdministradorRutas.ObtenerRutaGramaticas();
        var nombreArchivo = AdministradorRutas.GenerarNombreArchivo("grammar_transformada");
        var rutaArchivo = Path.Combine(rutaGramaticas, nombreArchivo);
        
        await File.WriteAllTextAsync(rutaArchivo, gramaticaTransformada.ATexto());
        return rutaArchivo;
    }
}
