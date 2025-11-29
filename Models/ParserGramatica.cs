using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Parser para cargar y analizar archivos de texto (.txt) que contienen
/// definiciones de Gramáticas Libres de Contexto.
/// 
/// Formato esperado del archivo:
/// - Una producción por línea en formato: A -> B C D
/// - El símbolo '->' separa el lado izquierdo del derecho
/// - Los símbolos se separan por espacios
/// - La primera producción define el símbolo inicial
/// - Convención: No terminales en mayúsculas (E, T, F)
/// - Convención: Terminales en minúsculas o símbolos especiales (+, *, id)
/// - Producción épsilon: A -> ε o A -> epsilon
/// - Líneas vacías y comentarios (iniciados con #) son ignorados
/// 
/// Ejemplo:
/// # Gramática de expresiones aritméticas
/// E -> E + T
/// E -> T
/// T -> T * F
/// T -> F
/// F -> ( E )
/// F -> id
/// </summary>
public class ParserGramatica
{
    private readonly HashSet<NonTerminal> _variables;
    private readonly HashSet<Terminal> _terminales;
    private readonly List<Production> _producciones;
    private readonly Dictionary<string, NonTerminal> _noTerminalesCache;
    private readonly Dictionary<string, Terminal> _terminalesCache;
    private NonTerminal? _simboloInicial;

    /// <summary>
    /// Constructor del parser.
    /// </summary>
    public ParserGramatica()
    {
        _variables = new HashSet<NonTerminal>();
        _terminales = new HashSet<Terminal>();
        _producciones = new List<Production>();
        _noTerminalesCache = new Dictionary<string, NonTerminal>();
        _terminalesCache = new Dictionary<string, Terminal>();
        _simboloInicial = null;
    }

    /// <summary>
    /// Carga y parsea una gramática desde un archivo de texto.
    /// </summary>
    /// <param name="rutaArchivo">Ruta al archivo .txt con la definición de la gramática.</param>
    /// <returns>La gramática libre de contexto construida.</returns>
    /// <exception cref="FileNotFoundException">Si el archivo no existe.</exception>
    /// <exception cref="FormatException">Si el formato del archivo es inválido.</exception>
    public ContextFreeGrammar CargarDesdeArchivo(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException($"No se encontró el archivo: {rutaArchivo}");
        }

        // Limpiar estado previo
        LimpiarEstado();

        // Leer todas las líneas del archivo
        var lineas = File.ReadAllLines(rutaArchivo, Encoding.UTF8);
        int numeroLinea = 0;

        foreach (var linea in lineas)
        {
            numeroLinea++;
            var lineaLimpia = LimpiarLinea(linea);

            // Ignorar líneas vacías y comentarios
            if (string.IsNullOrWhiteSpace(lineaLimpia) || lineaLimpia.StartsWith("#"))
            {
                continue;
            }

            try
            {
                ParsearProduccion(lineaLimpia);
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    $"Error en línea {numeroLinea}: {ex.Message}\nLínea: {linea}", ex);
            }
        }

        // Validar que se haya parseado al menos una producción
        if (_producciones.Count == 0)
        {
            throw new FormatException("El archivo no contiene producciones válidas.");
        }

        // Validar que se haya definido un símbolo inicial
        if (_simboloInicial == null)
        {
            throw new FormatException("No se pudo determinar el símbolo inicial.");
        }

        // Construir y retornar la gramática
        return new ContextFreeGrammar(_variables, _terminales, _producciones, _simboloInicial);
    }

    /// <summary>
    /// Parsea una gramática desde un string con múltiples líneas.
    /// </summary>
    /// <param name="contenido">El contenido de texto con las producciones.</param>
    /// <returns>La gramática libre de contexto construida.</returns>
    public ContextFreeGrammar ParsearDesdeTexto(string contenido)
    {
        // Crear archivo temporal
        var archivoTemporal = Path.GetTempFileName();
        try
        {
            File.WriteAllText(archivoTemporal, contenido, Encoding.UTF8);
            return CargarDesdeArchivo(archivoTemporal);
        }
        finally
        {
            // Limpiar archivo temporal
            if (File.Exists(archivoTemporal))
            {
                File.Delete(archivoTemporal);
            }
        }
    }

    /// <summary>
    /// Limpia una línea removiendo comentarios y espacios innecesarios.
    /// </summary>
    private string LimpiarLinea(string linea)
    {
        // Remover comentarios inline
        var indiceComentario = linea.IndexOf('#');
        if (indiceComentario >= 0)
        {
            linea = linea.Substring(0, indiceComentario);
        }

        return linea.Trim();
    }

    /// <summary>
    /// Parsea una línea que representa una producción.
    /// </summary>
    /// <param name="linea">Línea en formato: A -> B C D</param>
    private void ParsearProduccion(string linea)
    {
        // Buscar el separador '->'
        var partesProduccion = linea.Split(new[] { "->" }, StringSplitOptions.None);

        if (partesProduccion.Length != 2)
        {
            throw new FormatException(
                "Formato de producción inválido. Se esperaba 'A -> B C D'");
        }

        // Parsear lado izquierdo (debe ser un solo no terminal)
        var ladoIzquierdoStr = partesProduccion[0].Trim();
        if (string.IsNullOrWhiteSpace(ladoIzquierdoStr))
        {
            throw new FormatException("El lado izquierdo de la producción está vacío.");
        }

        var ladoIzquierdo = ObtenerOCrearNoTerminal(ladoIzquierdoStr);

        // Si es la primera producción, definir el símbolo inicial
        if (_simboloInicial == null)
        {
            _simboloInicial = ladoIzquierdo;
        }

        // Parsear lado derecho (secuencia de símbolos)
        var ladoDerechoStr = partesProduccion[1].Trim();
        var ladoDerecho = ParsearLadoDerecho(ladoDerechoStr);

        // Crear la producción
        var produccion = new Production(ladoIzquierdo, ladoDerecho);
        _producciones.Add(produccion);
    }

    /// <summary>
    /// Parsea el lado derecho de una producción.
    /// </summary>
    /// <param name="ladoDerechoStr">String con los símbolos del lado derecho.</param>
    /// <returns>Lista de símbolos parseados.</returns>
    private List<Symbol> ParsearLadoDerecho(string ladoDerechoStr)
    {
        var simbolos = new List<Symbol>();

        // Producción épsilon
        if (string.IsNullOrWhiteSpace(ladoDerechoStr) ||
            ladoDerechoStr == "ε" ||
            ladoDerechoStr.ToLower() == "epsilon" ||
            ladoDerechoStr.ToLower() == "lambda")
        {
            return simbolos; // Lista vacía representa épsilon
        }

        // Dividir por espacios
        var tokens = ladoDerechoStr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            var simbolo = ClasificarYCrearSimbolo(token);
            simbolos.Add(simbolo);
        }

        return simbolos;
    }

    /// <summary>
    /// Clasifica un token como terminal o no terminal según convenciones.
    /// 
    /// Convención:
    /// - No terminales: Una sola letra mayúscula (E, T, F) o palabra en PascalCase
    /// - Terminales: Todo lo demás (operadores, identificadores, palabras en minúsculas)
    /// </summary>
    private Symbol ClasificarYCrearSimbolo(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new FormatException("Token vacío encontrado.");
        }

        // Criterios para no terminales:
        // 1. Una sola letra mayúscula (E, T, F, etc.)
        // 2. Palabra que comienza con mayúscula y contiene solo letras (Expresion, Termino)
        if (EsNoTerminal(token))
        {
            return ObtenerOCrearNoTerminal(token);
        }
        else
        {
            return ObtenerOCrearTerminal(token);
        }
    }

    /// <summary>
    /// Determina si un token representa un no terminal.
    /// </summary>
    private bool EsNoTerminal(string token)
    {
        // Una sola letra mayúscula
        if (token.Length == 1 && char.IsUpper(token[0]))
        {
            return true;
        }

        // Palabra en PascalCase (empieza con mayúscula, solo letras)
        if (char.IsUpper(token[0]) && token.All(char.IsLetter) && token.Length > 1)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Obtiene o crea un no terminal desde el cache.
    /// </summary>
    private NonTerminal ObtenerOCrearNoTerminal(string valor)
    {
        if (!_noTerminalesCache.TryGetValue(valor, out var noTerminal))
        {
            noTerminal = new NonTerminal(valor);
            _noTerminalesCache[valor] = noTerminal;
            _variables.Add(noTerminal);
        }
        return noTerminal;
    }

    /// <summary>
    /// Obtiene o crea un terminal desde el cache.
    /// </summary>
    private Terminal ObtenerOCrearTerminal(string valor)
    {
        if (!_terminalesCache.TryGetValue(valor, out var terminal))
        {
            terminal = new Terminal(valor);
            _terminalesCache[valor] = terminal;
            _terminales.Add(terminal);
        }
        return terminal;
    }

    /// <summary>
    /// Limpia el estado interno del parser.
    /// </summary>
    private void LimpiarEstado()
    {
        _variables.Clear();
        _terminales.Clear();
        _producciones.Clear();
        _noTerminalesCache.Clear();
        _terminalesCache.Clear();
        _simboloInicial = null;
    }
}
