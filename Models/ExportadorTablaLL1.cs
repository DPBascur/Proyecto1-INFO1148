using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProyectoFinal.Models;

/// <summary>
/// Exportador para tabla sintáctica LL(1)
/// </summary>
public static class ExportadorTablaLL1
{
    /// <summary>
    /// Exporta la tabla LL(1) con reporte completo en formato TXT
    /// </summary>
    public static async Task<string> ExportarTablaCompletaAsync(
        GeneradorTablaLL1.TablaLL1 tabla,
        Gramatica gramatica,
        Dictionary<string, HashSet<string>> firstSets,
        Dictionary<string, HashSet<string>> followSets)
    {
        var rutaTablas = AdministradorRutas.ObtenerRutaTablas();
        var nombreArchivo = AdministradorRutas.GenerarNombreArchivo("tabla_ll1");
        var rutaArchivo = Path.Combine(rutaTablas, nombreArchivo);
        
        // Generar reporte completo
        var reporte = GeneradorTablaLL1.GenerarReporte(tabla, gramatica, firstSets, followSets);
        
        await File.WriteAllTextAsync(rutaArchivo, reporte);
        return rutaArchivo;
    }
    
    /// <summary>
    /// Exporta la tabla LL(1) en formato CSV
    /// </summary>
    public static async Task<string> ExportarTablaCSVAsync(GeneradorTablaLL1.TablaLL1 tabla)
    {
        var rutaTablas = AdministradorRutas.ObtenerRutaTablas();
        var nombreArchivo = AdministradorRutas.GenerarNombreArchivo("tabla_ll1", "csv");
        var rutaArchivo = Path.Combine(rutaTablas, nombreArchivo);
        
        // Generar CSV
        var csv = GeneradorTablaLL1.GenerarCSV(tabla);
        
        await File.WriteAllTextAsync(rutaArchivo, csv);
        return rutaArchivo;
    }
    
    /// <summary>
    /// Exporta ambos formatos: TXT completo y CSV
    /// </summary>
    public static async Task<(string txt, string csv)> ExportarTablaAmbasFormatosAsync(
        GeneradorTablaLL1.TablaLL1 tabla,
        Gramatica gramatica,
        Dictionary<string, HashSet<string>> firstSets,
        Dictionary<string, HashSet<string>> followSets)
    {
        var txtPath = await ExportarTablaCompletaAsync(tabla, gramatica, firstSets, followSets);
        var csvPath = await ExportarTablaCSVAsync(tabla);
        
        return (txtPath, csvPath);
    }
}
