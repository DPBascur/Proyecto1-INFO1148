using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Generador de reportes automÃ¡ticos con mÃ©tricas formales.
/// 
/// Calcula mÃ©tricas cuantitativas del proceso de generaciÃ³n:
/// - Cantidad total de casos
/// - DistribuciÃ³n porcentual por categorÃ­a
/// - Longitud promedio de expresiones
/// - Profundidad mÃ¡xima del Ã¡rbol sintÃ¡ctico
/// 
/// Estas mÃ©tricas estÃ¡n directamente relacionadas con el Ã¡rbol de derivaciÃ³n
/// y anÃ¡lisis sintÃ¡ctico de las gramÃ¡ticas libres de contexto.
/// </summary>
public class ReportGenerator
{
    private readonly GeneradorMetricas _generadorMetricas;
    private readonly ExportadorJSON _exportador;

    public ReportGenerator()
    {
        _generadorMetricas = new GeneradorMetricas();
        _exportador = new ExportadorJSON();
    }

    /// <summary>
    /// Genera un reporte completo con todas las mÃ©tricas requeridas.
    /// </summary>
    /// <param name="casos">Lista de casos de prueba generados.</param>
    /// <returns>Reporte de mÃ©tricas.</returns>
    public ReporteMetricas GenerarReporte(List<CasoPrueba> casos)
    {
        return _generadorMetricas.GenerarReporte(casos);
    }

    /// <summary>
    /// Calcula la cantidad total de casos generados.
    /// </summary>
    public int CalcularCantidadTotal(List<CasoPrueba> casos)
    {
        return casos.Count;
    }

    /// <summary>
    /// Calcula la distribuciÃ³n porcentual por categorÃ­a.
    /// </summary>
    /// <returns>Diccionario con porcentajes por categorÃ­a.</returns>
    public Dictionary<string, double> CalcularDistribucionPorcentual(List<CasoPrueba> casos)
    {
        if (casos.Count == 0)
        {
            return new Dictionary<string, double>
            {
                { "valid", 0.0 },
                { "invalid", 0.0 },
                { "extreme", 0.0 }
            };
        }

        var total = casos.Count;
        var validos = casos.Count(c => c.Categoria == CategoriaCaso.Valido);
        var invalidos = casos.Count(c => c.Categoria == CategoriaCaso.Invalido);
        var extremos = casos.Count(c => c.Categoria == CategoriaCaso.Extremo);

        return new Dictionary<string, double>
        {
            { "valid", Math.Round(validos * 100.0 / total, 2) },
            { "invalid", Math.Round(invalidos * 100.0 / total, 2) },
            { "extreme", Math.Round(extremos * 100.0 / total, 2) }
        };
    }

    /// <summary>
    /// Calcula la longitud promedio de las expresiones generadas.
    /// La longitud se mide en nÃºmero de tokens (sÃ­mbolos).
    /// </summary>
    public double CalcularLongitudPromedio(List<CasoPrueba> casos)
    {
        if (casos.Count == 0)
        {
            return 0.0;
        }

        var longitudes = casos
            .Where(c => c.Metadata.ContainsKey("num_tokens"))
            .Select(c => Convert.ToInt32(c.Metadata["num_tokens"]))
            .ToList();

        if (longitudes.Count == 0)
        {
            // Calcular desde las cadenas si no hay metadata
            longitudes = casos
                .Select(c => c.Cadena.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                .ToList();
        }

        return Math.Round(longitudes.Average(), 2);
    }

    /// <summary>
    /// Calcula la profundidad mÃ¡xima del Ã¡rbol sintÃ¡ctico.
    /// 
    /// La profundidad del Ã¡rbol de derivaciÃ³n es una mÃ©trica formal que
    /// cuantifica la complejidad estructural de las sentencias generadas.
    /// EstÃ¡ directamente relacionada con el nÃºmero de pasos en la derivaciÃ³n.
    /// </summary>
    public int CalcularProfundidadMaxima(List<CasoPrueba> casos)
    {
        return casos
            .Where(c => c.Metadata.ContainsKey("profundidad"))
            .Select(c => Convert.ToInt32(c.Metadata["profundidad"]))
            .DefaultIfEmpty(0)
            .Max();
    }

    /// <summary>
    /// Exporta los casos de prueba a formato JSON.
    /// 
    /// Cada caso incluye:
    /// - Cadena generada
    /// - CategorÃ­a (valid, invalid, extreme)
    /// - Metadata completa
    /// </summary>
    /// <param name="testCases">Casos de prueba a exportar.</param>
    /// <param name="metrics">MÃ©tricas calculadas.</param>
    /// <param name="gramatica">GramÃ¡tica utilizada.</param>
    /// <param name="rutaArchivo">Ruta del archivo JSON de salida.</param>
    public void ExportToJson(
        List<CasoPrueba> testCases,
        ReporteMetricas metrics,
        ContextFreeGrammar gramatica,
        string rutaArchivo)
    {
        var configuracion = new Dictionary<string, object>
        {
            { "metricas_reporte", ConvertirMetricasADiccionario(metrics) }
        };

        _exportador.ExportarACaso(
            testCases,
            gramatica,
            rutaArchivo,
            configuracion,
            metrics.TiempoEjecucionMs
        );
    }

    /// <summary>
    /// Exporta los casos de prueba con etiquetas de categorÃ­a.
    /// </summary>
    /// <param name="testCases">Casos de prueba.</param>
    /// <param name="gramatica">GramÃ¡tica utilizada.</param>
    /// <param name="rutaArchivo">Ruta de salida.</param>
    public void ExportToJson(
        List<CasoPrueba> testCases,
        ContextFreeGrammar gramatica,
        string rutaArchivo)
    {
        var metrics = GenerarReporte(testCases);
        ExportToJson(testCases, metrics, gramatica, rutaArchivo);
    }

    /// <summary>
    /// Genera un reporte en formato texto legible.
    /// </summary>
    public string GenerarReporteTexto(List<CasoPrueba> casos)
    {
        var reporte = GenerarReporte(casos);
        return _generadorMetricas.GenerarReporteTexto(reporte);
    }

    /// <summary>
    /// Genera un reporte resumido con las mÃ©tricas principales.
    /// </summary>
    public string GenerarResumen(List<CasoPrueba> casos)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
        sb.AppendLine("       RESUMEN DE MÃ‰TRICAS - CASOS DE PRUEBA       ");
        sb.AppendLine("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
        sb.AppendLine();

        // Cantidad total
        var total = CalcularCantidadTotal(casos);
        sb.AppendLine($"ðŸ“Š Cantidad Total de Casos: {total}");
        sb.AppendLine();

        // DistribuciÃ³n porcentual
        var distribucion = CalcularDistribucionPorcentual(casos);
        sb.AppendLine("ðŸ“ˆ DistribuciÃ³n Porcentual:");
        sb.AppendLine($"   â€¢ VÃ¡lidos:   {distribucion["valid"],6:F2}%");
        sb.AppendLine($"   â€¢ InvÃ¡lidos: {distribucion["invalid"],6:F2}%");
        sb.AppendLine($"   â€¢ Extremos:  {distribucion["extreme"],6:F2}%");
        sb.AppendLine();

        // Longitud promedio
        var longitudPromedio = CalcularLongitudPromedio(casos);
        sb.AppendLine($"ðŸ“ Longitud Promedio: {longitudPromedio:F2} tokens");
        sb.AppendLine();

        // Profundidad mÃ¡xima
        var profundidadMaxima = CalcularProfundidadMaxima(casos);
        sb.AppendLine($"ðŸŒ³ Profundidad MÃ¡xima del Ãrbol SintÃ¡ctico: {profundidadMaxima} pasos");
        sb.AppendLine();

        sb.AppendLine("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

        return sb.ToString();
    }

    /// <summary>
    /// Convierte el reporte de mÃ©tricas a diccionario para exportaciÃ³n.
    /// </summary>
    private Dictionary<string, object> ConvertirMetricasADiccionario(ReporteMetricas metrics)
    {
        return new Dictionary<string, object>
        {
            { "total_casos", metrics.TotalCasos },
            { "casos_validos", metrics.CasosValidos },
            { "casos_invalidos", metrics.CasosInvalidos },
            { "casos_extremos", metrics.CasosExtremos },
            { "porcentaje_validos", metrics.PorcentajeValidos },
            { "porcentaje_invalidos", metrics.PorcentajeInvalidos },
            { "porcentaje_extremos", metrics.PorcentajeExtremos },
            { "longitud_promedio", metrics.LongitudPromedio },
            { "longitud_minima", metrics.LongitudMinima },
            { "longitud_maxima", metrics.LongitudMaxima },
            { "profundidad_maxima", metrics.ProfundidadMaxima },
            { "profundidad_promedio", metrics.ProfundidadPromedio },
            { "total_operadores", metrics.TotalOperadores },
            { "tiempo_ejecucion_ms", metrics.TiempoEjecucionMs },
            { "tiempo_por_caso_ms", metrics.TiempoPorCasoMs }
        };
    }

    /// <summary>
    /// Valida que los casos tengan las etiquetas correctas de categorÃ­a.
    /// </summary>
    public bool ValidarEtiquetas(List<CasoPrueba> casos)
    {
        return casos.All(c => 
            c.Categoria == CategoriaCaso.Valido ||
            c.Categoria == CategoriaCaso.Invalido ||
            c.Categoria == CategoriaCaso.Extremo
        );
    }

    /// <summary>
    /// Obtiene estadÃ­sticas detalladas por categorÃ­a.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> ObtenerEstadisticasPorCategoria(List<CasoPrueba> casos)
    {
        var estadisticas = new Dictionary<string, Dictionary<string, object>>();

        foreach (var categoria in new[] { CategoriaCaso.Valido, CategoriaCaso.Invalido, CategoriaCaso.Extremo })
        {
            var casosCat = casos.Where(c => c.Categoria == categoria).ToList();
            var nombreCat = categoria.ToString().ToLower();

            if (casosCat.Count > 0)
            {
                estadisticas[nombreCat] = new Dictionary<string, object>
                {
                    { "cantidad", casosCat.Count },
                    { "longitud_promedio", CalcularLongitudPromedio(casosCat) },
                    { "profundidad_maxima", CalcularProfundidadMaxima(casosCat) }
                };
            }
        }

        return estadisticas;
    }
}

