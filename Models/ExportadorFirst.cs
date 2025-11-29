using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProyectoFinal.Models;

/// <summary>
/// Exportador para conjuntos FIRST calculados
/// </summary>
public static class ExportadorFirst
{
    /// <summary>
    /// Exporta el conjunto FIRST con reporte detallado
    /// </summary>
    public static async Task<string> ExportarConjuntoFirstAsync(
        Gramatica gramatica,
        Dictionary<string, HashSet<string>> firstSets)
    {
        var rutaFirst = AdministradorRutas.ObtenerRutaFirst();
        var nombreArchivo = AdministradorRutas.GenerarNombreArchivo("first_set");
        var rutaArchivo = Path.Combine(rutaFirst, nombreArchivo);
        
        // Generar reporte completo
        var reporte = CalculadorFirst.GenerarReporteFirst(gramatica, firstSets);
        
        await File.WriteAllTextAsync(rutaArchivo, reporte);
        return rutaArchivo;
    }
}
