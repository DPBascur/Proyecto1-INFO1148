using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoFinal.Models;

/// <summary>
/// Tipo de caso extremo generado.
/// </summary>
public enum TipoCasoExtremo
{
    ProfundidadMaxima,      // DerivaciÃ³n muy profunda
    ProfundidadMinima,      // DerivaciÃ³n mÃ­nima (caso base)
    ComplejidadMaxima,      // MÃ¡ximo nÃºmero de operadores
    ExpresionLarga,         // Muchos terminales
    ExpresionCorta,         // Pocos terminales
    AnidamientoMaximo       // Muchos niveles de parÃ©ntesis
}

/// <summary>
/// Representa un caso de prueba extremo.
/// </summary>
public class CasoExtremo
{
    public string Cadena { get; set; }
    public TipoCasoExtremo Tipo { get; set; }
    public string Descripcion { get; set; }
    public int Profundidad { get; set; }
    public int NumeroOperadores { get; set; }
    public int NumeroTerminales { get; set; }
    public int NivelAnidamiento { get; set; }

    public CasoExtremo(string cadena, TipoCasoExtremo tipo, string descripcion)
    {
        Cadena = cadena;
        Tipo = tipo;
        Descripcion = descripcion;
        Profundidad = 0;
        NumeroOperadores = 0;
        NumeroTerminales = 0;
        NivelAnidamiento = 0;
    }
}

/// <summary>
/// Generador de casos extremos (edge cases) para pruebas exhaustivas.
/// 
/// Crea casos lÃ­mite que prueban los extremos del comportamiento del sistema:
/// - Casos mÃ­nimos (mÃ¡s simples posibles)
/// - Casos mÃ¡ximos (mÃ¡s complejos/largos permitidos)
/// - Casos con caracterÃ­sticas especÃ­ficas al lÃ­mite
/// </summary>
public class GeneradorCasosExtremos
{
    private readonly ContextFreeGrammar _gramatica;
    private readonly Random _random;

    public GeneradorCasosExtremos(ContextFreeGrammar gramatica, int? semilla = null)
    {
        _gramatica = gramatica ?? throw new ArgumentNullException(nameof(gramatica));
        _random = semilla.HasValue ? new Random(semilla.Value) : new Random();
    }

    /// <summary>
    /// Genera mÃºltiples casos extremos de diferentes tipos.
    /// </summary>
    public List<CasoExtremo> GenerarCasosExtremos(int cantidadPorTipo = 2)
    {
        var casos = new List<CasoExtremo>();
        var tipos = Enum.GetValues(typeof(TipoCasoExtremo)).Cast<TipoCasoExtremo>();

        foreach (var tipo in tipos)
        {
            for (int i = 0; i < cantidadPorTipo; i++)
            {
                try
                {
                    var caso = GenerarCasoExtremo(tipo);
                    if (caso != null)
                    {
                        casos.Add(caso);
                    }
                }
                catch
                {
                    // Ignorar errores en casos extremos especÃ­ficos
                    continue;
                }
            }
        }

        return casos;
    }

    /// <summary>
    /// Genera un caso extremo especÃ­fico segÃºn el tipo.
    /// </summary>
    public CasoExtremo? GenerarCasoExtremo(TipoCasoExtremo tipo)
    {
        switch (tipo)
        {
            case TipoCasoExtremo.ProfundidadMaxima:
                return GenerarProfundidadMaxima();

            case TipoCasoExtremo.ProfundidadMinima:
                return GenerarProfundidadMinima();

            case TipoCasoExtremo.ComplejidadMaxima:
                return GenerarComplejidadMaxima();

            case TipoCasoExtremo.ExpresionLarga:
                return GenerarExpresionLarga();

            case TipoCasoExtremo.ExpresionCorta:
                return GenerarExpresionCorta();

            case TipoCasoExtremo.AnidamientoMaximo:
                return GenerarAnidamientoMaximo();

            default:
                return null;
        }
    }

    /// <summary>
    /// Genera un caso con la mÃ¡xima profundidad de derivaciÃ³n posible.
    /// Usa recursividad al mÃ¡ximo antes de terminar.
    /// </summary>
    private CasoExtremo GenerarProfundidadMaxima()
    {
        var generador = new GeneradorDerivaciones(_gramatica, profundidadMaxima: 100);
        
        // Intentar mÃºltiples veces para obtener una derivaciÃ³n profunda
        string? mejorCadena = null;
        int mejorProfundidad = 0;

        for (int intento = 0; intento < 10; intento++)
        {
            try
            {
                var cadena = generador.GenerarCadena();
                var profundidad = generador.HistorialDerivacion.Count;

                if (profundidad > mejorProfundidad)
                {
                    mejorProfundidad = profundidad;
                    mejorCadena = cadena;
                }
            }
            catch
            {
                // Intentar de nuevo
                continue;
            }
        }

        if (mejorCadena == null)
        {
            throw new InvalidOperationException("No se pudo generar caso de profundidad mÃ¡xima");
        }

        var caso = new CasoExtremo(
            mejorCadena,
            TipoCasoExtremo.ProfundidadMaxima,
            $"DerivaciÃ³n con {mejorProfundidad} pasos"
        );
        caso.Profundidad = mejorProfundidad;
        CalcularMetricas(caso);

        return caso;
    }

    /// <summary>
    /// Genera el caso mÃ¡s simple posible (derivaciÃ³n directa a terminal).
    /// </summary>
    private CasoExtremo GenerarProfundidadMinima()
    {
        // Buscar el camino mÃ¡s corto desde el sÃ­mbolo inicial a solo terminales
        var generador = new GeneradorDerivacionDeterminista(_gramatica);
        var cadena = generador.GenerarCasoMinimo();

        var caso = new CasoExtremo(
            cadena,
            TipoCasoExtremo.ProfundidadMinima,
            "Caso mÃ­nimo (derivaciÃ³n mÃ¡s corta)"
        );
        caso.Profundidad = ContarPasos(cadena);
        CalcularMetricas(caso);

        return caso;
    }

    /// <summary>
    /// Genera un caso con el mÃ¡ximo nÃºmero de operadores.
    /// </summary>
    private CasoExtremo GenerarComplejidadMaxima()
    {
        // Generar mÃºltiples casos y seleccionar el que tiene mÃ¡s operadores
        string? mejorCadena = null;
        int maxOperadores = 0;

        var generador = new GeneradorDerivaciones(_gramatica, profundidadMaxima: 80);

        for (int i = 0; i < 20; i++)
        {
            try
            {
                var cadena = generador.GenerarCadena();
                var numOperadores = ContarOperadores(cadena);

                if (numOperadores > maxOperadores)
                {
                    maxOperadores = numOperadores;
                    mejorCadena = cadena;
                }
            }
            catch
            {
                continue;
            }
        }

        if (mejorCadena == null)
        {
            mejorCadena = "id + id * id + id * id";
            maxOperadores = 4;
        }

        var caso = new CasoExtremo(
            mejorCadena,
            TipoCasoExtremo.ComplejidadMaxima,
            $"ExpresiÃ³n con {maxOperadores} operadores"
        );
        caso.NumeroOperadores = maxOperadores;
        CalcularMetricas(caso);

        return caso;
    }

    /// <summary>
    /// Genera una expresiÃ³n muy larga.
    /// </summary>
    private CasoExtremo GenerarExpresionLarga()
    {
        var generador = new GeneradorDerivaciones(_gramatica, profundidadMaxima: 100);
        string? mejorCadena = null;
        int maxTerminales = 0;

        for (int i = 0; i < 15; i++)
        {
            try
            {
                var cadena = generador.GenerarCadena();
                var numTerminales = ContarTerminales(cadena);

                if (numTerminales > maxTerminales)
                {
                    maxTerminales = numTerminales;
                    mejorCadena = cadena;
                }
            }
            catch
            {
                continue;
            }
        }

        if (mejorCadena == null)
        {
            throw new InvalidOperationException("No se pudo generar expresiÃ³n larga");
        }

        var caso = new CasoExtremo(
            mejorCadena,
            TipoCasoExtremo.ExpresionLarga,
            $"ExpresiÃ³n larga con {maxTerminales} sÃ­mbolos terminales"
        );
        caso.NumeroTerminales = maxTerminales;
        CalcularMetricas(caso);

        return caso;
    }

    /// <summary>
    /// Genera una expresiÃ³n muy corta.
    /// </summary>
    private CasoExtremo GenerarExpresionCorta()
    {
        var caso = new CasoExtremo(
            "id",
            TipoCasoExtremo.ExpresionCorta,
            "ExpresiÃ³n mÃ¡s corta posible"
        );
        caso.NumeroTerminales = 1;
        CalcularMetricas(caso);

        return caso;
    }

    /// <summary>
    /// Genera un caso con mÃ¡ximo anidamiento de parÃ©ntesis.
    /// </summary>
    private CasoExtremo GenerarAnidamientoMaximo()
    {
        // Construir expresiÃ³n con mÃºltiples niveles de parÃ©ntesis anidados
        var niveles = 5;
        var expresion = "id";

        for (int i = 0; i < niveles; i++)
        {
            expresion = $"( {expresion} )";
            if (i < niveles - 1)
            {
                expresion += " + id";
            }
        }

        var caso = new CasoExtremo(
            expresion,
            TipoCasoExtremo.AnidamientoMaximo,
            $"ExpresiÃ³n con {niveles} niveles de anidamiento"
        );
        caso.NivelAnidamiento = niveles;
        CalcularMetricas(caso);

        return caso;
    }

    /// <summary>
    /// Calcula mÃ©tricas adicionales para un caso extremo.
    /// </summary>
    private void CalcularMetricas(CasoExtremo caso)
    {
        if (caso.NumeroOperadores == 0)
            caso.NumeroOperadores = ContarOperadores(caso.Cadena);
        
        if (caso.NumeroTerminales == 0)
            caso.NumeroTerminales = ContarTerminales(caso.Cadena);
        
        if (caso.NivelAnidamiento == 0)
            caso.NivelAnidamiento = ContarNivelAnidamiento(caso.Cadena);
    }

    private int ContarOperadores(string cadena)
    {
        var operadores = new[] { "+", "*", "-", "/" };
        return cadena.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                     .Count(t => operadores.Contains(t));
    }

    private int ContarTerminales(string cadena)
    {
        return cadena.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private int ContarNivelAnidamiento(string cadena)
    {
        int nivel = 0;
        int maxNivel = 0;

        foreach (var token in cadena.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token == "(")
            {
                nivel++;
                maxNivel = Math.Max(maxNivel, nivel);
            }
            else if (token == ")")
            {
                nivel--;
            }
        }

        return maxNivel;
    }

    private int ContarPasos(string cadena)
    {
        // EstimaciÃ³n bÃ¡sica de pasos basada en la complejidad
        return ContarTerminales(cadena) + ContarOperadores(cadena);
    }
}

/// <summary>
/// Generador determinista para crear el caso mÃ¡s simple posible.
/// </summary>
internal class GeneradorDerivacionDeterminista
{
    private readonly ContextFreeGrammar _gramatica;

    public GeneradorDerivacionDeterminista(ContextFreeGrammar gramatica)
    {
        _gramatica = gramatica;
    }

    public string GenerarCasoMinimo()
    {
        // Buscar el camino mÃ¡s corto: preferir producciones que lleven directamente a terminales
        var forma = new List<Symbol> { _gramatica.SimboloInicial };

        while (ContieneNoTerminales(forma))
        {
            forma = ExpandirPrimerNoTerminalConProduccionMinima(forma);
        }

        return string.Join(" ", forma.Select(s => s.Valor));
    }

    private List<Symbol> ExpandirPrimerNoTerminalConProduccionMinima(List<Symbol> forma)
    {
        var nuevaForma = new List<Symbol>();

        bool expandido = false;
        foreach (var simbolo in forma)
        {
            if (!expandido && simbolo is NonTerminal nt)
            {
                var producciones = _gramatica.ObtenerProduccionesPara(nt);
                
                // Preferir producciones que no tienen no-terminales o tienen menos
                var produccionMinima = producciones
                    .OrderBy(p => p.LadoDerecho.Count(s => s is NonTerminal))
                    .ThenBy(p => p.LadoDerecho.Count)
                    .First();

                nuevaForma.AddRange(produccionMinima.LadoDerecho);
                expandido = true;
            }
            else
            {
                nuevaForma.Add(simbolo);
            }
        }

        return nuevaForma;
    }

    private bool ContieneNoTerminales(List<Symbol> forma)
    {
        return forma.Any(s => s is NonTerminal);
    }
}

