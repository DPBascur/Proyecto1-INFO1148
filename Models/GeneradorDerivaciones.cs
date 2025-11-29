using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Representa un paso en el proceso de derivación.
/// Contiene la forma sentencial actual y la producción aplicada.
/// </summary>
public class PasoDerivacion
{
    /// <summary>
    /// La forma sentencial (cadena de símbolos) en este paso.
    /// </summary>
    public List<Symbol> FormaSentencial { get; set; }

    /// <summary>
    /// La producción aplicada para llegar a esta forma sentencial.
    /// Null para el paso inicial.
    /// </summary>
    public Production? ProduccionAplicada { get; set; }

    /// <summary>
    /// Descripción textual del paso.
    /// </summary>
    public string Descripcion { get; set; }

    public PasoDerivacion(List<Symbol> formaSentencial, Production? produccionAplicada, string descripcion)
    {
        FormaSentencial = formaSentencial;
        ProduccionAplicada = produccionAplicada;
        Descripcion = descripcion;
    }

    public override string ToString()
    {
        var forma = FormaSentencial.Count == 0 ? "ε" : string.Join(" ", FormaSentencial.Select(s => s.Valor));
        return ProduccionAplicada != null 
            ? $"{forma} (aplicando: {ProduccionAplicada})"
            : forma;
    }
}

/// <summary>
/// Generador de derivaciones para Gramáticas Libres de Contexto.
/// 
/// Implementa la lógica fundamental de derivación (expansión de no terminales)
/// que es el mecanismo mediante el cual una GLC genera palabras del lenguaje L(G).
/// 
/// Características:
/// - Derivación por la izquierda (leftmost derivation)
/// - Selección aleatoria de producciones para generar variedad
/// - Control de profundidad para evitar derivaciones infinitas
/// - Registro del historial de derivación paso a paso
/// </summary>
public class GeneradorDerivaciones
{
    private readonly ContextFreeGrammar _gramatica;
    private readonly Random _random;
    private readonly int _profundidadMaxima;

    /// <summary>
    /// Historial completo de la derivación (pasos intermedios).
    /// </summary>
    public List<PasoDerivacion> HistorialDerivacion { get; private set; }

    /// <summary>
    /// Constructor del generador de derivaciones.
    /// </summary>
    /// <param name="gramatica">La gramática a utilizar para las derivaciones.</param>
    /// <param name="profundidadMaxima">Profundidad máxima de derivación (por defecto 50).</param>
    /// <param name="semilla">Semilla para el generador aleatorio (opcional, para reproducibilidad).</param>
    public GeneradorDerivaciones(ContextFreeGrammar gramatica, int profundidadMaxima = 50, int? semilla = null)
    {
        _gramatica = gramatica ?? throw new ArgumentNullException(nameof(gramatica));
        _profundidadMaxima = profundidadMaxima;
        _random = semilla.HasValue ? new Random(semilla.Value) : new Random();
        HistorialDerivacion = new List<PasoDerivacion>();
    }

    /// <summary>
    /// Genera una cadena válida del lenguaje L(G) mediante derivación.
    /// Utiliza derivación por la izquierda (leftmost derivation).
    /// </summary>
    /// <returns>La cadena terminal generada.</returns>
    /// <exception cref="InvalidOperationException">Si no se puede completar la derivación.</exception>
    public string GenerarCadena()
    {
        HistorialDerivacion.Clear();

        // Iniciar con el símbolo inicial
        var formaSentencial = new List<Symbol> { _gramatica.SimboloInicial };
        
        HistorialDerivacion.Add(new PasoDerivacion(
            new List<Symbol>(formaSentencial),
            null,
            "Forma sentencial inicial"
        ));

        int pasos = 0;

        // Derivar hasta obtener solo terminales
        while (ContieneNoTerminales(formaSentencial))
        {
            if (pasos >= _profundidadMaxima)
            {
                throw new InvalidOperationException(
                    $"Se alcanzó la profundidad máxima ({_profundidadMaxima}) sin completar la derivación.");
            }

            // Aplicar un paso de derivación por la izquierda
            formaSentencial = DerivacionPorLaIzquierda(formaSentencial, pasos);
            pasos++;
        }

        // Construir la cadena final (solo terminales)
        return ConstruirCadenaTerminal(formaSentencial);
    }

    /// <summary>
    /// Aplica un paso de derivación por la izquierda.
    /// Expande el primer no terminal encontrado de izquierda a derecha.
    /// </summary>
    /// <param name="formaSentencial">La forma sentencial actual.</param>
    /// <param name="pasos">Número de pasos ya realizados.</param>
    /// <returns>Nueva forma sentencial después de la expansión.</returns>
    private List<Symbol> DerivacionPorLaIzquierda(List<Symbol> formaSentencial, int pasos)
    {
        var nuevaForma = new List<Symbol>();
        bool expandido = false;

        for (int i = 0; i < formaSentencial.Count; i++)
        {
            var simbolo = formaSentencial[i];

            // Expandir el primer no terminal encontrado
            if (!expandido && simbolo is NonTerminal noTerminal)
            {
                int pasosRestantes = _profundidadMaxima - pasos;
                var produccion = SeleccionarProduccionAleatoria(noTerminal, pasosRestantes);
                
                // Agregar los símbolos del lado derecho de la producción
                nuevaForma.AddRange(produccion.LadoDerecho);

                // Registrar el paso
                var formaSiguiente = new List<Symbol>(nuevaForma);
                formaSiguiente.AddRange(formaSentencial.Skip(i + 1));
                
                HistorialDerivacion.Add(new PasoDerivacion(
                    new List<Symbol>(formaSiguiente),
                    produccion,
                    $"Expandir {noTerminal.Valor}"
                ));

                expandido = true;
            }
            else
            {
                // Copiar el símbolo sin cambios
                nuevaForma.Add(simbolo);
            }
        }

        return nuevaForma;
    }

    /// <summary>
    /// Expande un no terminal específico usando una de sus producciones.
    /// Este es el método fundamental de derivación.
    /// </summary>
    /// <param name="noTerminal">El no terminal a expandir.</param>
    /// <returns>Lista de símbolos resultantes de la expansión.</returns>
    public List<Symbol> ExpandirNoTerminal(NonTerminal noTerminal)
    {
        var produccion = SeleccionarProduccionAleatoria(noTerminal, int.MaxValue);
        return new List<Symbol>(produccion.LadoDerecho);
    }

    /// <summary>
    /// Selecciona aleatoriamente una producción válida para un no terminal dado.
    /// Usa una estrategia inteligente: cuando está cerca del límite de profundidad,
    /// prefiere producciones que no tienen recursión izquierda.
    /// </summary>
    /// <param name="noTerminal">El no terminal para el cual seleccionar una producción.</param>
    /// <param name="pasosRestantes">Pasos restantes antes de alcanzar la profundidad máxima.</param>
    /// <returns>La producción seleccionada.</returns>
    /// <exception cref="InvalidOperationException">Si no hay producciones para el no terminal.</exception>
    private Production SeleccionarProduccionAleatoria(NonTerminal noTerminal, int pasosRestantes = int.MaxValue)
    {
        var producciones = _gramatica.ObtenerProduccionesPara(noTerminal);

        if (producciones.Count == 0)
        {
            throw new InvalidOperationException(
                $"No hay producciones definidas para el no terminal '{noTerminal.Valor}'");
        }

        // Si quedan pocos pasos, preferir producciones sin recursión izquierda
        if (pasosRestantes < _profundidadMaxima / 3)
        {
            // Filtrar producciones que NO tienen recursión izquierda
            var produccionesNoRecursivas = producciones.Where(p =>
                p.LadoDerecho.Count == 0 || 
                !(p.LadoDerecho[0] is NonTerminal nt && nt.Equals(noTerminal))
            ).ToList();

            if (produccionesNoRecursivas.Count > 0)
            {
                producciones = produccionesNoRecursivas;
            }
        }

        // Seleccionar aleatoriamente una producción
        var indice = _random.Next(producciones.Count);
        return producciones[indice];
    }

    /// <summary>
    /// Selecciona una producción específica por índice (para derivaciones controladas).
    /// </summary>
    /// <param name="noTerminal">El no terminal.</param>
    /// <param name="indice">Índice de la producción a seleccionar.</param>
    /// <returns>La producción seleccionada.</returns>
    public Production SeleccionarProduccion(NonTerminal noTerminal, int indice)
    {
        var producciones = _gramatica.ObtenerProduccionesPara(noTerminal);

        if (indice < 0 || indice >= producciones.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(indice),
                $"Índice {indice} fuera de rango. Producciones disponibles: {producciones.Count}");
        }

        return producciones[indice];
    }

    /// <summary>
    /// Verifica si una forma sentencial contiene al menos un no terminal.
    /// </summary>
    private bool ContieneNoTerminales(List<Symbol> formaSentencial)
    {
        return formaSentencial.Any(s => s is NonTerminal);
    }

    /// <summary>
    /// Construye una cadena terminal a partir de una lista de símbolos terminales.
    /// </summary>
    private string ConstruirCadenaTerminal(List<Symbol> simbolos)
    {
        if (simbolos.Count == 0)
        {
            return "ε"; // Cadena vacía
        }

        var cadena = new StringBuilder();
        foreach (var simbolo in simbolos)
        {
            if (simbolo is Terminal terminal)
            {
                cadena.Append(terminal.Valor);
                cadena.Append(" ");
            }
        }

        return cadena.ToString().Trim();
    }

    /// <summary>
    /// Obtiene la representación textual del historial de derivación.
    /// </summary>
    public string ObtenerHistorialTexto()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Historial de Derivación:");
        sb.AppendLine(new string('-', 50));

        for (int i = 0; i < HistorialDerivacion.Count; i++)
        {
            var paso = HistorialDerivacion[i];
            sb.AppendLine($"Paso {i}: {paso}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Genera múltiples cadenas válidas del lenguaje.
    /// </summary>
    /// <param name="cantidad">Número de cadenas a generar.</param>
    /// <returns>Lista de cadenas generadas.</returns>
    public List<string> GenerarMultiplesCadenas(int cantidad)
    {
        var cadenas = new List<string>();

        for (int i = 0; i < cantidad; i++)
        {
            try
            {
                var cadena = GenerarCadena();
                cadenas.Add(cadena);
            }
            catch (InvalidOperationException)
            {
                // Ignorar cadenas que no se pudieron generar
                continue;
            }
        }

        return cadenas;
    }
}
