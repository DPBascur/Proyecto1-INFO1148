using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProyectoFinal.Models;

/// <summary>
/// Resultado completo de generaciÃ³n de casos de prueba.
/// Incluye todos los casos, mÃ©tricas y metadata del proceso.
/// </summary>
public class ResultadoGeneracion
{
    [JsonPropertyName("gramatica")]
    public InfoGramatica Gramatica { get; set; } = new();

    [JsonPropertyName("casos")]
    public List<CasoPrueba> Casos { get; set; } = new();

    [JsonPropertyName("metricas")]
    public MetricasGeneracion Metricas { get; set; } = new();

    [JsonPropertyName("metadata")]
    public MetadataGeneracion Metadata { get; set; } = new();
}

/// <summary>
/// InformaciÃ³n de la gramÃ¡tica utilizada.
/// </summary>
public class InfoGramatica
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("simbolo_inicial")]
    public string SimboloInicial { get; set; } = string.Empty;

    [JsonPropertyName("variables")]
    public List<string> Variables { get; set; } = new();

    [JsonPropertyName("terminales")]
    public List<string> Terminales { get; set; } = new();

    [JsonPropertyName("producciones")]
    public List<string> Producciones { get; set; } = new();
}

/// <summary>
/// MÃ©tricas de la generaciÃ³n completa.
/// </summary>
public class MetricasGeneracion
{
    [JsonPropertyName("total_casos")]
    public int TotalCasos { get; set; }

    [JsonPropertyName("casos_validos")]
    public int CasosValidos { get; set; }

    [JsonPropertyName("casos_invalidos")]
    public int CasosInvalidos { get; set; }

    [JsonPropertyName("casos_extremos")]
    public int CasosExtremos { get; set; }

    [JsonPropertyName("distribucion_porcentual")]
    public Dictionary<string, double> DistribucionPorcentual { get; set; } = new();

    [JsonPropertyName("longitud_promedio")]
    public double LongitudPromedio { get; set; }

    [JsonPropertyName("profundidad_maxima")]
    public int ProfundidadMaxima { get; set; }

    [JsonPropertyName("operadores_por_tipo")]
    public Dictionary<string, int> OperadoresPorTipo { get; set; } = new();

    [JsonPropertyName("tiempo_ejecucion_ms")]
    public long TiempoEjecucionMs { get; set; }
}

/// <summary>
/// Metadata del proceso de generaciÃ³n.
/// </summary>
public class MetadataGeneracion
{
    [JsonPropertyName("fecha_generacion")]
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;

    [JsonPropertyName("version_aplicacion")]
    public string VersionAplicacion { get; set; } = "1.0.0";

    [JsonPropertyName("configuracion")]
    public Dictionary<string, object> Configuracion { get; set; } = new();
}

/// <summary>
/// Exportador de casos de prueba a formato JSON.
/// 
/// Genera archivos JSON estructurados con todos los casos generados,
/// su clasificaciÃ³n, mÃ©tricas y metadata completa para anÃ¡lisis posterior.
/// </summary>
public class ExportadorJSON
{
    private readonly JsonSerializerOptions _opciones;

    public ExportadorJSON()
    {
        _opciones = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    /// <summary>
    /// Exporta casos de prueba a un archivo JSON.
    /// </summary>
    /// <param name="casos">Lista de casos clasificados.</param>
    /// <param name="gramatica">GramÃ¡tica utilizada.</param>
    /// <param name="rutaArchivo">Ruta del archivo de salida.</param>
    /// <param name="configuracion">ConfiguraciÃ³n utilizada.</param>
    /// <param name="tiempoEjecucionMs">Tiempo de ejecuciÃ³n en milisegundos.</param>
    public void ExportarACaso(
        List<CasoPrueba> casos,
        ContextFreeGrammar gramatica,
        string rutaArchivo,
        Dictionary<string, object>? configuracion = null,
        long tiempoEjecucionMs = 0)
    {
        var resultado = ConstruirResultado(casos, gramatica, configuracion, tiempoEjecucionMs);
        var json = JsonSerializer.Serialize(resultado, _opciones);
        
        // Asegurar que el directorio existe
        var directorio = Path.GetDirectoryName(rutaArchivo);
        if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        File.WriteAllText(rutaArchivo, json, Encoding.UTF8);
    }

    /// <summary>
    /// Exporta casos de prueba a un string JSON.
    /// </summary>
    public string ExportarAString(
        List<CasoPrueba> casos,
        ContextFreeGrammar gramatica,
        Dictionary<string, object>? configuracion = null,
        long tiempoEjecucionMs = 0)
    {
        var resultado = ConstruirResultado(casos, gramatica, configuracion, tiempoEjecucionMs);
        return JsonSerializer.Serialize(resultado, _opciones);
    }

    /// <summary>
    /// Construye el objeto de resultado completo.
    /// </summary>
    private ResultadoGeneracion ConstruirResultado(
        List<CasoPrueba> casos,
        ContextFreeGrammar gramatica,
        Dictionary<string, object>? configuracion,
        long tiempoEjecucionMs)
    {
        var resultado = new ResultadoGeneracion
        {
            Gramatica = ConstruirInfoGramatica(gramatica),
            Casos = casos,
            Metricas = CalcularMetricas(casos, tiempoEjecucionMs),
            Metadata = new MetadataGeneracion
            {
                FechaGeneracion = DateTime.Now,
                VersionAplicacion = "1.0.0",
                Configuracion = configuracion ?? new Dictionary<string, object>()
            }
        };

        return resultado;
    }

    /// <summary>
    /// Construye informaciÃ³n de la gramÃ¡tica.
    /// </summary>
    private InfoGramatica ConstruirInfoGramatica(ContextFreeGrammar gramatica)
    {
        return new InfoGramatica
        {
            Nombre = "GramÃ¡tica de Expresiones AritmÃ©ticas",
            SimboloInicial = gramatica.SimboloInicial?.Valor ?? "S",
            Variables = gramatica.Variables.Select(v => v.Valor).ToList(),
            Terminales = gramatica.Terminales.Select(t => t.Valor).ToList(),
            Producciones = gramatica.Producciones.Select(p => p.ToString()).ToList()
        };
    }

    /// <summary>
    /// Calcula mÃ©tricas completas de los casos generados.
    /// </summary>
    private MetricasGeneracion CalcularMetricas(List<CasoPrueba> casos, long tiempoEjecucionMs)
    {
        var total = casos.Count;
        var validos = casos.Count(c => c.Categoria == CategoriaCaso.Valido);
        var invalidos = casos.Count(c => c.Categoria == CategoriaCaso.Invalido);
        var extremos = casos.Count(c => c.Categoria == CategoriaCaso.Extremo);

        var metricas = new MetricasGeneracion
        {
            TotalCasos = total,
            CasosValidos = validos,
            CasosInvalidos = invalidos,
            CasosExtremos = extremos,
            TiempoEjecucionMs = tiempoEjecucionMs
        };

        // DistribuciÃ³n porcentual
        if (total > 0)
        {
            metricas.DistribucionPorcentual = new Dictionary<string, double>
            {
                { "validos", Math.Round(validos * 100.0 / total, 2) },
                { "invalidos", Math.Round(invalidos * 100.0 / total, 2) },
                { "extremos", Math.Round(extremos * 100.0 / total, 2) }
            };
        }

        // Longitud promedio
        if (casos.Count > 0)
        {
            metricas.LongitudPromedio = Math.Round(
                casos.Average(c => c.Metadata.ContainsKey("num_tokens") 
                    ? Convert.ToInt32(c.Metadata["num_tokens"]) 
                    : 0),
                2
            );
        }

        // Profundidad mÃ¡xima
        metricas.ProfundidadMaxima = casos
            .Where(c => c.Metadata.ContainsKey("profundidad"))
            .Select(c => Convert.ToInt32(c.Metadata["profundidad"]))
            .DefaultIfEmpty(0)
            .Max();

        // Operadores por tipo
        var operadoresPorTipo = new Dictionary<string, int>
        {
            { "+", 0 },
            { "*", 0 },
            { "-", 0 },
            { "/", 0 }
        };

        foreach (var caso in casos)
        {
            if (caso.Metadata.ContainsKey("operadores") && 
                caso.Metadata["operadores"] is JsonElement element)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (operadoresPorTipo.ContainsKey(prop.Name))
                    {
                        operadoresPorTipo[prop.Name] += prop.Value.GetInt32();
                    }
                }
            }
            else if (caso.Metadata.ContainsKey("operadores") &&
                     caso.Metadata["operadores"] is Dictionary<string, int> dict)
            {
                foreach (var kvp in dict)
                {
                    if (operadoresPorTipo.ContainsKey(kvp.Key))
                    {
                        operadoresPorTipo[kvp.Key] += kvp.Value;
                    }
                }
            }
        }

        metricas.OperadoresPorTipo = operadoresPorTipo;

        return metricas;
    }

    /// <summary>
    /// Genera un nombre de archivo automÃ¡tico con timestamp.
    /// </summary>
    public static string GenerarNombreArchivo(string directorio = ".")
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var nombreArchivo = $"casos_prueba_{timestamp}.json";
        return Path.Combine(directorio, nombreArchivo);
    }
}

