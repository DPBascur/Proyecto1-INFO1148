using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Tipos de mutaciÃ³n aplicables a cadenas vÃ¡lidas para generar casos invÃ¡lidos.
/// </summary>
public enum TipoMutacion
{
    ParentesisDesbalanceados,
    OperadorDuplicado,
    OperadorAlInicio,
    OperadorAlFinal,
    ParentesisVacio,
    OperadorFaltante,
    IdentificadorFaltante,
    CaracterInvalido,
    EspaciosEnMedio
}

/// <summary>
/// Representa un caso de prueba invÃ¡lido generado por mutaciÃ³n.
/// </summary>
public class CasoInvalido
{
    public string Cadena { get; set; }
    public TipoMutacion TipoMutacion { get; set; }
    public string Descripcion { get; set; }
    public string CadenaOriginal { get; set; }

    public CasoInvalido(string cadena, TipoMutacion tipo, string descripcion, string original)
    {
        Cadena = cadena;
        TipoMutacion = tipo;
        Descripcion = descripcion;
        CadenaOriginal = original;
    }
}

/// <summary>
/// Generador de casos invÃ¡lidos mediante mutaciÃ³n sintÃ¡ctica.
/// 
/// Aplica transformaciones controladas a cadenas vÃ¡lidas para introducir
/// errores sintÃ¡cticos especÃ­ficos, Ãºtiles para probar validadores y parsers.
/// </summary>
public class Mutador
{
    private readonly Random _random;

    public Mutador(int? semilla = null)
    {
        _random = semilla.HasValue ? new Random(semilla.Value) : new Random();
    }

    /// <summary>
    /// Genera mÃºltiples casos invÃ¡lidos a partir de una cadena vÃ¡lida.
    /// </summary>
    /// <param name="cadenaValida">Cadena vÃ¡lida de entrada.</param>
    /// <param name="cantidad">NÃºmero de mutaciones a generar.</param>
    /// <returns>Lista de casos invÃ¡lidos.</returns>
    public List<CasoInvalido> GenerarCasosInvalidos(string cadenaValida, int cantidad)
    {
        var casosInvalidos = new List<CasoInvalido>();
        var tiposMutacion = Enum.GetValues(typeof(TipoMutacion)).Cast<TipoMutacion>().ToList();

        for (int i = 0; i < cantidad; i++)
        {
            // Seleccionar tipo de mutaciÃ³n aleatoriamente
            var tipo = tiposMutacion[_random.Next(tiposMutacion.Count)];
            var casoInvalido = AplicarMutacion(cadenaValida, tipo);
            
            if (casoInvalido != null)
            {
                casosInvalidos.Add(casoInvalido);
            }
        }

        return casosInvalidos;
    }

    /// <summary>
    /// Aplica una mutaciÃ³n especÃ­fica a una cadena vÃ¡lida.
    /// </summary>
    public CasoInvalido? AplicarMutacion(string cadenaValida, TipoMutacion tipo)
    {
        var tokens = cadenaValida.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (tokens.Length == 0)
        {
            return null;
        }

        string cadenaMutada;
        string descripcion;

        switch (tipo)
        {
            case TipoMutacion.ParentesisDesbalanceados:
                cadenaMutada = MutarParentesisDesbalanceados(tokens);
                descripcion = "ParÃ©ntesis no balanceados";
                break;

            case TipoMutacion.OperadorDuplicado:
                cadenaMutada = MutarOperadorDuplicado(tokens);
                descripcion = "Operador duplicado";
                break;

            case TipoMutacion.OperadorAlInicio:
                cadenaMutada = MutarOperadorAlInicio(tokens);
                descripcion = "Operador al inicio de la expresiÃ³n";
                break;

            case TipoMutacion.OperadorAlFinal:
                cadenaMutada = MutarOperadorAlFinal(tokens);
                descripcion = "Operador al final de la expresiÃ³n";
                break;

            case TipoMutacion.ParentesisVacio:
                cadenaMutada = MutarParentesisVacio(tokens);
                descripcion = "ParÃ©ntesis vacÃ­o";
                break;

            case TipoMutacion.OperadorFaltante:
                cadenaMutada = MutarOperadorFaltante(tokens);
                descripcion = "Operador faltante entre operandos";
                break;

            case TipoMutacion.IdentificadorFaltante:
                cadenaMutada = MutarIdentificadorFaltante(tokens);
                descripcion = "Identificador faltante";
                break;

            case TipoMutacion.CaracterInvalido:
                cadenaMutada = MutarCaracterInvalido(tokens);
                descripcion = "CarÃ¡cter invÃ¡lido insertado";
                break;

            case TipoMutacion.EspaciosEnMedio:
                cadenaMutada = MutarEspaciosEnMedio(tokens);
                descripcion = "Espacios en medio de token";
                break;

            default:
                return null;
        }

        return new CasoInvalido(cadenaMutada, tipo, descripcion, cadenaValida);
    }

    /// <summary>
    /// Elimina un parÃ©ntesis de apertura o cierre para desbalancear.
    /// </summary>
    private string MutarParentesisDesbalanceados(string[] tokens)
    {
        var lista = tokens.ToList();
        var parentesis = lista.Where(t => t == "(" || t == ")").ToList();

        if (parentesis.Count > 0)
        {
            // Eliminar un parÃ©ntesis aleatorio
            var indice = lista.IndexOf(parentesis[_random.Next(parentesis.Count)]);
            lista.RemoveAt(indice);
        }
        else
        {
            // Si no hay parÃ©ntesis, agregar uno desbalanceado
            lista.Insert(_random.Next(lista.Count + 1), _random.Next(2) == 0 ? "(" : ")");
        }

        return string.Join(" ", lista);
    }

    /// <summary>
    /// Duplica un operador.
    /// </summary>
    private string MutarOperadorDuplicado(string[] tokens)
    {
        var lista = tokens.ToList();
        var operadores = new[] { "+", "*", "-", "/" };
        var indicesOperadores = lista.Select((t, i) => new { t, i })
                                     .Where(x => operadores.Contains(x.t))
                                     .ToList();

        if (indicesOperadores.Count > 0)
        {
            var seleccionado = indicesOperadores[_random.Next(indicesOperadores.Count)];
            lista.Insert(seleccionado.i + 1, seleccionado.t);
        }
        else
        {
            // Si no hay operadores, insertar uno duplicado
            var pos = _random.Next(lista.Count);
            lista.Insert(pos, "+");
            lista.Insert(pos, "+");
        }

        return string.Join(" ", lista);
    }

    /// <summary>
    /// Coloca un operador al inicio de la expresiÃ³n.
    /// </summary>
    private string MutarOperadorAlInicio(string[] tokens)
    {
        var lista = tokens.ToList();
        var operadores = new[] { "+", "*", "-", "/" };
        lista.Insert(0, operadores[_random.Next(operadores.Length)]);
        return string.Join(" ", lista);
    }

    /// <summary>
    /// Coloca un operador al final de la expresiÃ³n.
    /// </summary>
    private string MutarOperadorAlFinal(string[] tokens)
    {
        var lista = tokens.ToList();
        var operadores = new[] { "+", "*", "-", "/" };
        lista.Add(operadores[_random.Next(operadores.Length)]);
        return string.Join(" ", lista);
    }

    /// <summary>
    /// Crea parÃ©ntesis vacÃ­os.
    /// </summary>
    private string MutarParentesisVacio(string[] tokens)
    {
        var lista = tokens.ToList();
        var pos = _random.Next(lista.Count + 1);
        lista.Insert(pos, "(");
        lista.Insert(pos + 1, ")");
        return string.Join(" ", lista);
    }

    /// <summary>
    /// Elimina un operador para crear operandos consecutivos.
    /// </summary>
    private string MutarOperadorFaltante(string[] tokens)
    {
        var lista = tokens.ToList();
        var operadores = new[] { "+", "*", "-", "/" };
        var indicesOperadores = lista.Select((t, i) => new { t, i })
                                     .Where(x => operadores.Contains(x.t))
                                     .Select(x => x.i)
                                     .ToList();

        if (indicesOperadores.Count > 0)
        {
            lista.RemoveAt(indicesOperadores[_random.Next(indicesOperadores.Count)]);
        }

        return string.Join(" ", lista);
    }

    /// <summary>
    /// Elimina un identificador.
    /// </summary>
    private string MutarIdentificadorFaltante(string[] tokens)
    {
        var lista = tokens.ToList();
        var indicesIds = lista.Select((t, i) => new { t, i })
                              .Where(x => x.t == "id" || x.t.StartsWith("id"))
                              .Select(x => x.i)
                              .ToList();

        if (indicesIds.Count > 0)
        {
            lista.RemoveAt(indicesIds[_random.Next(indicesIds.Count)]);
        }

        return string.Join(" ", lista);
    }

    /// <summary>
    /// Inserta un carÃ¡cter invÃ¡lido.
    /// </summary>
    private string MutarCaracterInvalido(string[] tokens)
    {
        var lista = tokens.ToList();
        var caracteresInvalidos = new[] { "@", "#", "$", "%", "&", "!", "?" };
        var pos = _random.Next(lista.Count + 1);
        lista.Insert(pos, caracteresInvalidos[_random.Next(caracteresInvalidos.Length)]);
        return string.Join(" ", lista);
    }

    /// <summary>
    /// Divide un token con espacios en medio.
    /// </summary>
    private string MutarEspaciosEnMedio(string[] tokens)
    {
        if (tokens.Length == 0) return string.Empty;
        
        var tokenSeleccionado = tokens[_random.Next(tokens.Length)];
        if (tokenSeleccionado.Length > 1)
        {
            var pos = _random.Next(1, tokenSeleccionado.Length);
            var tokenMutado = tokenSeleccionado.Insert(pos, " ");
            return string.Join(" ", tokens).Replace(tokenSeleccionado, tokenMutado);
        }

        return string.Join(" ", tokens);
    }
}

