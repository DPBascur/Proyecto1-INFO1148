using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoFinal.Models;

/// <summary>
/// CategorÃ­a de un caso de prueba.
/// </summary>
public enum CategoriaCaso
{
    Valido,
    Invalido,
    Extremo
}

/// <summary>
/// Representa un caso de prueba clasificado con toda su metadata.
/// </summary>
public class CasoPrueba
{
    public string Id { get; set; }
    public string Cadena { get; set; }
    public CategoriaCaso Categoria { get; set; }
    public string Descripcion { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime FechaGeneracion { get; set; }

    public CasoPrueba()
    {
        Id = Guid.NewGuid().ToString();
        Cadena = string.Empty;
        Categoria = CategoriaCaso.Valido;
        Descripcion = string.Empty;
        Metadata = new Dictionary<string, object>();
        FechaGeneracion = DateTime.Now;
    }

    public CasoPrueba(string cadena, CategoriaCaso categoria, string descripcion) : this()
    {
        Cadena = cadena;
        Categoria = categoria;
        Descripcion = descripcion;
    }

    /// <summary>
    /// Agrega informaciÃ³n de metadata.
    /// </summary>
    public void AgregarMetadata(string clave, object valor)
    {
        Metadata[clave] = valor;
    }
}

/// <summary>
/// Clasificador automÃ¡tico de casos de prueba.
/// 
/// Toma casos generados de diferentes fuentes (derivaciÃ³n, mutaciÃ³n, extremos)
/// y los clasifica con metadata completa para anÃ¡lisis posterior.
/// </summary>
public class Clasificador
{
    private int _contadorCasos = 0;

    /// <summary>
    /// Clasifica un caso vÃ¡lido generado por derivaciÃ³n.
    /// </summary>
    public CasoPrueba ClasificarCasoValido(string cadena, GeneradorDerivaciones generador)
    {
        _contadorCasos++;
        
        var caso = new CasoPrueba(cadena, CategoriaCaso.Valido, "Caso vÃ¡lido generado por derivaciÃ³n");
        caso.Id = $"VALID_{_contadorCasos:D4}";
        
        // Metadata especÃ­fica de derivaciÃ³n
        caso.AgregarMetadata("profundidad", generador.HistorialDerivacion.Count);
        caso.AgregarMetadata("pasos_derivacion", generador.HistorialDerivacion.Count);
        caso.AgregarMetadata("metodo_generacion", "derivacion_leftmost");
        
        // MÃ©tricas de la cadena
        AgregarMetricasCadena(caso);
        
        return caso;
    }

    /// <summary>
    /// Clasifica un caso invÃ¡lido generado por mutaciÃ³n.
    /// </summary>
    public CasoPrueba ClasificarCasoInvalido(CasoInvalido casoInvalido)
    {
        _contadorCasos++;
        
        var caso = new CasoPrueba(
            casoInvalido.Cadena, 
            CategoriaCaso.Invalido, 
            casoInvalido.Descripcion
        );
        caso.Id = $"INVALID_{_contadorCasos:D4}";
        
        // Metadata especÃ­fica de mutaciÃ³n
        caso.AgregarMetadata("tipo_mutacion", casoInvalido.TipoMutacion.ToString());
        caso.AgregarMetadata("cadena_original", casoInvalido.CadenaOriginal);
        caso.AgregarMetadata("metodo_generacion", "mutacion");
        
        // MÃ©tricas de la cadena
        AgregarMetricasCadena(caso);
        
        return caso;
    }

    /// <summary>
    /// Clasifica un caso extremo.
    /// </summary>
    public CasoPrueba ClasificarCasoExtremo(CasoExtremo casoExtremo)
    {
        _contadorCasos++;
        
        var caso = new CasoPrueba(
            casoExtremo.Cadena, 
            CategoriaCaso.Extremo, 
            casoExtremo.Descripcion
        );
        caso.Id = $"EXTREME_{_contadorCasos:D4}";
        
        // Metadata especÃ­fica de casos extremos
        caso.AgregarMetadata("tipo_extremo", casoExtremo.Tipo.ToString());
        caso.AgregarMetadata("profundidad", casoExtremo.Profundidad);
        caso.AgregarMetadata("num_operadores", casoExtremo.NumeroOperadores);
        caso.AgregarMetadata("num_terminales", casoExtremo.NumeroTerminales);
        caso.AgregarMetadata("nivel_anidamiento", casoExtremo.NivelAnidamiento);
        caso.AgregarMetadata("metodo_generacion", "caso_extremo");
        
        // MÃ©tricas de la cadena
        AgregarMetricasCadena(caso);
        
        return caso;
    }

    /// <summary>
    /// Clasifica mÃºltiples casos vÃ¡lidos.
    /// </summary>
    public List<CasoPrueba> ClasificarCasosValidos(List<string> cadenas, GeneradorDerivaciones generador)
    {
        var casos = new List<CasoPrueba>();
        
        foreach (var cadena in cadenas)
        {
            // Para casos mÃºltiples, crear metadata simplificada
            _contadorCasos++;
            var caso = new CasoPrueba(cadena, CategoriaCaso.Valido, "Caso vÃ¡lido generado por derivaciÃ³n");
            caso.Id = $"VALID_{_contadorCasos:D4}";
            caso.AgregarMetadata("metodo_generacion", "derivacion_leftmost");
            AgregarMetricasCadena(caso);
            casos.Add(caso);
        }
        
        return casos;
    }

    /// <summary>
    /// Agrega mÃ©tricas generales de la cadena.
    /// </summary>
    private void AgregarMetricasCadena(CasoPrueba caso)
    {
        var tokens = caso.Cadena.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        caso.AgregarMetadata("longitud_cadena", caso.Cadena.Length);
        caso.AgregarMetadata("num_tokens", tokens.Length);
        caso.AgregarMetadata("num_parentesis_abiertos", tokens.Count(t => t == "("));
        caso.AgregarMetadata("num_parentesis_cerrados", tokens.Count(t => t == ")"));
        caso.AgregarMetadata("parentesis_balanceados", 
            tokens.Count(t => t == "(") == tokens.Count(t => t == ")"));
        
        // Contar operadores
        var operadores = new[] { "+", "*", "-", "/" };
        var conteoOperadores = new Dictionary<string, int>();
        foreach (var op in operadores)
        {
            conteoOperadores[op] = tokens.Count(t => t == op);
        }
        caso.AgregarMetadata("operadores", conteoOperadores);
        caso.AgregarMetadata("total_operadores", conteoOperadores.Values.Sum());
        
        // Contar identificadores
        caso.AgregarMetadata("num_identificadores", tokens.Count(t => t == "id" || t.StartsWith("id")));
    }

    /// <summary>
    /// Reinicia el contador de casos.
    /// </summary>
    public void ReiniciarContador()
    {
        _contadorCasos = 0;
    }

    /// <summary>
    /// Genera un reporte de clasificaciÃ³n.
    /// </summary>
    public string GenerarReporte(List<CasoPrueba> casos)
    {
        if (casos.Count == 0)
        {
            return "No hay casos para reportar.";
        }

        var validos = casos.Count(c => c.Categoria == CategoriaCaso.Valido);
        var invalidos = casos.Count(c => c.Categoria == CategoriaCaso.Invalido);
        var extremos = casos.Count(c => c.Categoria == CategoriaCaso.Extremo);
        var total = casos.Count;

        var reporte = "â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•\n";
        reporte += "   REPORTE DE CLASIFICACIÃ“N\n";
        reporte += "â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•\n\n";
        reporte += $"Total de casos: {total}\n\n";
        reporte += $"âœ“ VÃ¡lidos:    {validos,4} ({validos * 100.0 / total:F1}%)\n";
        reporte += $"âœ— InvÃ¡lidos:  {invalidos,4} ({invalidos * 100.0 / total:F1}%)\n";
        reporte += $"âš¡ Extremos:   {extremos,4} ({extremos * 100.0 / total:F1}%)\n\n";
        reporte += "â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•\n";

        return reporte;
    }
}

