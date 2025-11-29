using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Reporte de mÃ©tricas de generaciÃ³n de casos de prueba.
/// </summary>
public class ReporteMetricas
{
    public int TotalCasos { get; set; }
    public int CasosValidos { get; set; }
    public int CasosInvalidos { get; set; }
    public int CasosExtremos { get; set; }
    
    public double PorcentajeValidos { get; set; }
    public double PorcentajeInvalidos { get; set; }
    public double PorcentajeExtremos { get; set; }
    
    public double LongitudPromedio { get; set; }
    public int LongitudMinima { get; set; }
    public int LongitudMaxima { get; set; }
    
    public int ProfundidadMaxima { get; set; }
    public double ProfundidadPromedio { get; set; }
    
    public Dictionary<string, int> OperadoresPorTipo { get; set; } = new();
    public int TotalOperadores { get; set; }
    
    public Dictionary<TipoMutacion, int> MutacionesPorTipo { get; set; } = new();
    public Dictionary<TipoCasoExtremo, int> ExtremosPorTipo { get; set; } = new();
    
    public long TiempoEjecucionMs { get; set; }
    public double TiempoPorCasoMs { get; set; }
}

/// <summary>
/// Generador de mÃ©tricas y estadÃ­sticas para casos de prueba.
/// 
/// Calcula mÃ©tricas detalladas del proceso de generaciÃ³n:
/// - DistribuciÃ³n por categorÃ­a
/// - EstadÃ­sticas de longitud y complejidad
/// - AnÃ¡lisis de operadores y profundidad
/// - Tiempos de ejecuciÃ³n
/// - DistribuciÃ³n de tipos de mutaciÃ³n y casos extremos
/// </summary>
public class GeneradorMetricas
{
    private Stopwatch _cronometro;

    public GeneradorMetricas()
    {
        _cronometro = new Stopwatch();
    }

    /// <summary>
    /// Inicia el temporizador de mÃ©tricas.
    /// </summary>
    public void IniciarMedicion()
    {
        _cronometro.Restart();
    }

    /// <summary>
    /// Detiene el temporizador.
    /// </summary>
    public void DetenerMedicion()
    {
        _cronometro.Stop();
    }

    /// <summary>
    /// Obtiene el tiempo transcurrido en milisegundos.
    /// </summary>
    public long ObtenerTiempoMs()
    {
        return _cronometro.ElapsedMilliseconds;
    }

    /// <summary>
    /// Genera un reporte completo de mÃ©tricas.
    /// </summary>
    /// <param name="casos">Lista de casos clasificados.</param>
    /// <returns>Reporte de mÃ©tricas.</returns>
    public ReporteMetricas GenerarReporte(List<CasoPrueba> casos)
    {
        if (casos.Count == 0)
        {
            return new ReporteMetricas();
        }

        var reporte = new ReporteMetricas();
        var tiempoMs = ObtenerTiempoMs();

        // Conteo por categorÃ­a
        reporte.TotalCasos = casos.Count;
        reporte.CasosValidos = casos.Count(c => c.Categoria == CategoriaCaso.Valido);
        reporte.CasosInvalidos = casos.Count(c => c.Categoria == CategoriaCaso.Invalido);
        reporte.CasosExtremos = casos.Count(c => c.Categoria == CategoriaCaso.Extremo);

        // Porcentajes
        reporte.PorcentajeValidos = Math.Round(reporte.CasosValidos * 100.0 / reporte.TotalCasos, 2);
        reporte.PorcentajeInvalidos = Math.Round(reporte.CasosInvalidos * 100.0 / reporte.TotalCasos, 2);
        reporte.PorcentajeExtremos = Math.Round(reporte.CasosExtremos * 100.0 / reporte.TotalCasos, 2);

        // EstadÃ­sticas de longitud
        var longitudes = casos
            .Where(c => c.Metadata.ContainsKey("num_tokens"))
            .Select(c => Convert.ToInt32(c.Metadata["num_tokens"]))
            .ToList();

        if (longitudes.Count > 0)
        {
            reporte.LongitudPromedio = Math.Round(longitudes.Average(), 2);
            reporte.LongitudMinima = longitudes.Min();
            reporte.LongitudMaxima = longitudes.Max();
        }

        // EstadÃ­sticas de profundidad
        var profundidades = casos
            .Where(c => c.Metadata.ContainsKey("profundidad"))
            .Select(c => Convert.ToInt32(c.Metadata["profundidad"]))
            .ToList();

        if (profundidades.Count > 0)
        {
            reporte.ProfundidadMaxima = profundidades.Max();
            reporte.ProfundidadPromedio = Math.Round(profundidades.Average(), 2);
        }

        // Operadores por tipo
        reporte.OperadoresPorTipo = ContarOperadores(casos);
        reporte.TotalOperadores = reporte.OperadoresPorTipo.Values.Sum();

        // DistribuciÃ³n de mutaciones
        reporte.MutacionesPorTipo = ContarMutaciones(casos);

        // DistribuciÃ³n de casos extremos
        reporte.ExtremosPorTipo = ContarCasosExtremos(casos);

        // Tiempos
        reporte.TiempoEjecucionMs = tiempoMs;
        reporte.TiempoPorCasoMs = reporte.TotalCasos > 0 
            ? Math.Round((double)tiempoMs / reporte.TotalCasos, 2) 
            : 0;

        return reporte;
    }

    /// <summary>
    /// Cuenta operadores por tipo.
    /// </summary>
    private Dictionary<string, int> ContarOperadores(List<CasoPrueba> casos)
    {
        var conteo = new Dictionary<string, int>
        {
            { "+", 0 },
            { "*", 0 },
            { "-", 0 },
            { "/", 0 }
        };

        foreach (var caso in casos)
        {
            if (caso.Metadata.ContainsKey("operadores"))
            {
                var ops = caso.Metadata["operadores"];
                if (ops is Dictionary<string, int> dict)
                {
                    foreach (var kvp in dict)
                    {
                        if (conteo.ContainsKey(kvp.Key))
                        {
                            conteo[kvp.Key] += kvp.Value;
                        }
                    }
                }
            }
        }

        return conteo;
    }

    /// <summary>
    /// Cuenta mutaciones por tipo.
    /// </summary>
    private Dictionary<TipoMutacion, int> ContarMutaciones(List<CasoPrueba> casos)
    {
        var conteo = new Dictionary<TipoMutacion, int>();

        foreach (var tipo in Enum.GetValues(typeof(TipoMutacion)).Cast<TipoMutacion>())
        {
            conteo[tipo] = 0;
        }

        foreach (var caso in casos.Where(c => c.Categoria == CategoriaCaso.Invalido))
        {
            if (caso.Metadata.ContainsKey("tipo_mutacion"))
            {
                var tipoStr = caso.Metadata["tipo_mutacion"].ToString();
                if (Enum.TryParse<TipoMutacion>(tipoStr, out var tipo))
                {
                    conteo[tipo]++;
                }
            }
        }

        return conteo;
    }

    /// <summary>
    /// Cuenta casos extremos por tipo.
    /// </summary>
    private Dictionary<TipoCasoExtremo, int> ContarCasosExtremos(List<CasoPrueba> casos)
    {
        var conteo = new Dictionary<TipoCasoExtremo, int>();

        foreach (var tipo in Enum.GetValues(typeof(TipoCasoExtremo)).Cast<TipoCasoExtremo>())
        {
            conteo[tipo] = 0;
        }

        foreach (var caso in casos.Where(c => c.Categoria == CategoriaCaso.Extremo))
        {
            if (caso.Metadata.ContainsKey("tipo_extremo"))
            {
                var tipoStr = caso.Metadata["tipo_extremo"].ToString();
                if (Enum.TryParse<TipoCasoExtremo>(tipoStr, out var tipo))
                {
                    conteo[tipo]++;
                }
            }
        }

        return conteo;
    }

    /// <summary>
    /// Genera un reporte en formato texto legible.
    /// </summary>
    public string GenerarReporteTexto(ReporteMetricas reporte)
    {
        var sb = new StringBuilder();

        sb.AppendLine("â•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—");
        sb.AppendLine("â•‘          REPORTE DE MÃ‰TRICAS - GENERACIÃ“N DE CASOS           â•‘");
        sb.AppendLine("â•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
        sb.AppendLine();

        // Resumen general
        sb.AppendLine("ðŸ“Š RESUMEN GENERAL");
        sb.AppendLine("â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
        sb.AppendLine($"  Total de casos generados: {reporte.TotalCasos}");
        sb.AppendLine($"  Tiempo de ejecuciÃ³n: {reporte.TiempoEjecucionMs} ms");
        sb.AppendLine($"  Tiempo promedio por caso: {reporte.TiempoPorCasoMs} ms");
        sb.AppendLine();

        // DistribuciÃ³n por categorÃ­a
        sb.AppendLine("ðŸ“ˆ DISTRIBUCIÃ“N POR CATEGORÃA");
        sb.AppendLine("â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
        sb.AppendLine($"  âœ“ VÃ¡lidos:    {reporte.CasosValidos,5} ({reporte.PorcentajeValidos,5:F1}%)");
        sb.AppendLine($"  âœ— InvÃ¡lidos:  {reporte.CasosInvalidos,5} ({reporte.PorcentajeInvalidos,5:F1}%)");
        sb.AppendLine($"  âš¡ Extremos:   {reporte.CasosExtremos,5} ({reporte.PorcentajeExtremos,5:F1}%)");
        sb.AppendLine();

        // EstadÃ­sticas de longitud
        sb.AppendLine("ðŸ“ ESTADÃSTICAS DE LONGITUD (tokens)");
        sb.AppendLine("â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
        sb.AppendLine($"  Promedio: {reporte.LongitudPromedio:F2}");
        sb.AppendLine($"  MÃ­nima:   {reporte.LongitudMinima}");
        sb.AppendLine($"  MÃ¡xima:   {reporte.LongitudMaxima}");
        sb.AppendLine();

        // EstadÃ­sticas de profundidad
        if (reporte.ProfundidadMaxima > 0)
        {
            sb.AppendLine("ðŸŒ³ ESTADÃSTICAS DE PROFUNDIDAD");
            sb.AppendLine("â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
            sb.AppendLine($"  MÃ¡xima:   {reporte.ProfundidadMaxima}");
            sb.AppendLine($"  Promedio: {reporte.ProfundidadPromedio:F2}");
            sb.AppendLine();
        }

        // Operadores generados
        sb.AppendLine("âž• OPERADORES GENERADOS");
        sb.AppendLine("â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
        foreach (var kvp in reporte.OperadoresPorTipo.OrderByDescending(x => x.Value))
        {
            if (kvp.Value > 0)
            {
                var porcentaje = reporte.TotalOperadores > 0 
                    ? kvp.Value * 100.0 / reporte.TotalOperadores 
                    : 0;
                sb.AppendLine($"  {kvp.Key,3} : {kvp.Value,5} ({porcentaje,5:F1}%)");
            }
        }
        sb.AppendLine($"  Total: {reporte.TotalOperadores}");
        sb.AppendLine();

        // Tipos de mutaciÃ³n
        if (reporte.MutacionesPorTipo.Values.Sum() > 0)
        {
            sb.AppendLine("ðŸ”§ TIPOS DE MUTACIÃ“N APLICADOS");
            sb.AppendLine("â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
            foreach (var kvp in reporte.MutacionesPorTipo.Where(x => x.Value > 0).OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"  {kvp.Key,-30} : {kvp.Value,3}");
            }
            sb.AppendLine();
        }

        // Tipos de casos extremos
        if (reporte.ExtremosPorTipo.Values.Sum() > 0)
        {
            sb.AppendLine("âš¡ TIPOS DE CASOS EXTREMOS");
            sb.AppendLine("â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
            foreach (var kvp in reporte.ExtremosPorTipo.Where(x => x.Value > 0).OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"  {kvp.Key,-30} : {kvp.Value,3}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

        return sb.ToString();
    }
}

