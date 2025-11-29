using System;
using System.Collections.Generic;

namespace ProyectoFinal.Models;

/// <summary>
/// Clase que define y construye la Gramática Libre de Contexto para expresiones aritméticas básicas.
/// 
/// La gramática implementa precedencia de operadores de forma no ambigua:
/// - Multiplicación (*) tiene mayor precedencia que suma (+)
/// - Los paréntesis permiten agrupar subexpresiones
/// - Asociatividad a izquierda para ambos operadores
/// 
/// Gramática:
///   E → E + T | T
///   T → T * F | F
///   F → ( E ) | id
/// 
/// Donde:
///   E = Expresión (nivel más bajo de precedencia - suma)
///   T = Término (nivel medio de precedencia - multiplicación)
///   F = Factor (nivel más alto - valores básicos y paréntesis)
/// </summary>
public class GramaticaExpresionesAritmeticas
{
    /// <summary>
    /// La gramática completa para expresiones aritméticas.
    /// </summary>
    public ContextFreeGrammar Gramatica { get; private set; } = null!;

    // Símbolos No Terminales (Variables)
    public NonTerminal Expresion { get; private set; } = null!;
    public NonTerminal Termino { get; private set; } = null!;
    public NonTerminal Factor { get; private set; } = null!;

    // Símbolos Terminales
    public Terminal Suma { get; private set; } = null!;
    public Terminal Multiplicacion { get; private set; } = null!;
    public Terminal ParentesisAbierto { get; private set; } = null!;
    public Terminal ParentesisCerrado { get; private set; } = null!;
    public Terminal Identificador { get; private set; } = null!;

    /// <summary>
    /// Constructor que inicializa y construye la gramática completa.
    /// </summary>
    public GramaticaExpresionesAritmeticas()
    {
        InicializarSimbolos();
        ConstruirGramatica();
    }

    /// <summary>
    /// Inicializa todos los símbolos terminales y no terminales.
    /// </summary>
    private void InicializarSimbolos()
    {
        // Símbolos No Terminales (Variables - V)
        Expresion = new NonTerminal("E");
        Termino = new NonTerminal("T");
        Factor = new NonTerminal("F");

        // Símbolos Terminales (Alfabeto - Σ)
        Suma = new Terminal("+");
        Multiplicacion = new Terminal("*");
        ParentesisAbierto = new Terminal("(");
        ParentesisCerrado = new Terminal(")");
        Identificador = new Terminal("id");
    }

    /// <summary>
    /// Construye la gramática completa con sus producciones.
    /// </summary>
    private void ConstruirGramatica()
    {
        // Conjunto de Variables (V)
        var variables = new HashSet<NonTerminal>
        {
            Expresion,
            Termino,
            Factor
        };

        // Conjunto de Terminales (Σ)
        var terminales = new HashSet<Terminal>
        {
            Suma,
            Multiplicacion,
            ParentesisAbierto,
            ParentesisCerrado,
            Identificador
        };

        // Conjunto de Producciones (R)
        var producciones = new List<Production>
        {
            // E → E + T
            new Production(Expresion, Expresion, Suma, Termino),
            
            // E → T
            new Production(Expresion, Termino),
            
            // T → T * F
            new Production(Termino, Termino, Multiplicacion, Factor),
            
            // T → F
            new Production(Termino, Factor),
            
            // F → ( E )
            new Production(Factor, ParentesisAbierto, Expresion, ParentesisCerrado),
            
            // F → id
            new Production(Factor, Identificador)
        };

        // Símbolo Inicial (S)
        var simboloInicial = Expresion;

        // Crear la gramática G = (V, Σ, R, S)
        Gramatica = new ContextFreeGrammar(
            variables,
            terminales,
            producciones,
            simboloInicial
        );
    }

    /// <summary>
    /// Obtiene una representación en string de la gramática.
    /// </summary>
    public override string ToString()
    {
        return Gramatica.ToString();
    }

    /// <summary>
    /// Verifica si un símbolo es terminal en esta gramática.
    /// </summary>
    public bool EsTerminal(Symbol simbolo)
    {
        if (simbolo is Terminal terminal)
            return Gramatica.Terminales.Contains(terminal);
        return false;
    }

    /// <summary>
    /// Verifica si un símbolo es no terminal en esta gramática.
    /// </summary>
    public bool EsNoTerminal(Symbol simbolo)
    {
        if (simbolo is NonTerminal noTerminal)
            return Gramatica.Variables.Contains(noTerminal);
        return false;
    }

    /// <summary>
    /// Obtiene todas las producciones para un no terminal dado.
    /// </summary>
    public List<Production> ObtenerProducciones(NonTerminal noTerminal)
    {
        return Gramatica.ObtenerProduccionesPara(noTerminal);
    }
}
