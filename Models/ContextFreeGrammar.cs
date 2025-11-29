using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoFinal.Models;

/// <summary>
/// Representa una Gramática Libre de Contexto (GLC) o Context-Free Grammar (CFG).
/// Formalmente, una GLC se define como una 4-tupla G = (V, Σ, R, S) donde:
/// - V: Conjunto de símbolos no terminales (variables)
/// - Σ: Conjunto de símbolos terminales (alfabeto)
/// - R: Conjunto de reglas de producción
/// - S: Símbolo inicial (start symbol)
/// </summary>
public class ContextFreeGrammar
{
    /// <summary>
    /// V: Conjunto de símbolos no terminales (variables).
    /// </summary>
    public HashSet<NonTerminal> Variables { get; set; }

    /// <summary>
    /// Σ (Sigma): Conjunto de símbolos terminales (alfabeto).
    /// </summary>
    public HashSet<Terminal> Terminales { get; set; }

    /// <summary>
    /// R: Conjunto de reglas de producción.
    /// </summary>
    public List<Production> Producciones { get; set; }

    /// <summary>
    /// S: Símbolo inicial (start symbol).
    /// Debe ser un no terminal perteneciente al conjunto V.
    /// </summary>
    public NonTerminal SimboloInicial { get; set; }

    /// <summary>
    /// Constructor para crear una Gramática Libre de Contexto.
    /// </summary>
    /// <param name="variables">Conjunto de no terminales (V).</param>
    /// <param name="terminales">Conjunto de terminales (Σ).</param>
    /// <param name="producciones">Conjunto de producciones (R).</param>
    /// <param name="simboloInicial">Símbolo inicial (S).</param>
    public ContextFreeGrammar(
        HashSet<NonTerminal> variables,
        HashSet<Terminal> terminales,
        List<Production> producciones,
        NonTerminal simboloInicial)
    {
        Variables = variables ?? throw new ArgumentNullException(nameof(variables));
        Terminales = terminales ?? throw new ArgumentNullException(nameof(terminales));
        Producciones = producciones ?? throw new ArgumentNullException(nameof(producciones));
        SimboloInicial = simboloInicial ?? throw new ArgumentNullException(nameof(simboloInicial));

        ValidarGramatica();
    }

    /// <summary>
    /// Constructor vacío para inicialización progresiva.
    /// </summary>
    public ContextFreeGrammar()
    {
        Variables = new HashSet<NonTerminal>();
        Terminales = new HashSet<Terminal>();
        Producciones = new List<Production>();
        SimboloInicial = null!;
    }

    /// <summary>
    /// Valida que la gramática sea consistente:
    /// - El símbolo inicial debe pertenecer a V
    /// - Todas las producciones deben tener lados izquierdos en V
    /// - Los símbolos del lado derecho deben estar en V ∪ Σ
    /// </summary>
    private void ValidarGramatica()
    {
        if (SimboloInicial != null && !Variables.Contains(SimboloInicial))
        {
            throw new InvalidOperationException(
                $"El símbolo inicial '{SimboloInicial}' no pertenece al conjunto de variables V.");
        }

        foreach (var produccion in Producciones)
        {
            if (!Variables.Contains(produccion.LadoIzquierdo))
            {
                throw new InvalidOperationException(
                    $"La producción '{produccion}' tiene un lado izquierdo que no está en V.");
            }

            foreach (var simbolo in produccion.LadoDerecho)
            {
                if (simbolo is NonTerminal nt && !Variables.Contains(nt))
                {
                    throw new InvalidOperationException(
                        $"El no terminal '{simbolo}' en la producción '{produccion}' no está en V.");
                }
                else if (simbolo is Terminal t && !Terminales.Contains(t))
                {
                    throw new InvalidOperationException(
                        $"El terminal '{simbolo}' en la producción '{produccion}' no está en Σ.");
                }
            }
        }
    }

    /// <summary>
    /// Obtiene todas las producciones para un no terminal específico.
    /// </summary>
    /// <param name="noTerminal">El no terminal a buscar.</param>
    /// <returns>Lista de producciones donde el lado izquierdo es el no terminal dado.</returns>
    public List<Production> ObtenerProduccionesPara(NonTerminal noTerminal)
    {
        return Producciones.Where(p => p.LadoIzquierdo.Equals(noTerminal)).ToList();
    }

    /// <summary>
    /// Agrega una nueva producción a la gramática.
    /// </summary>
    public void AgregarProduccion(Production produccion)
    {
        if (!Producciones.Contains(produccion))
        {
            Producciones.Add(produccion);
        }
    }

    /// <summary>
    /// Representación en string de la gramática.
    /// </summary>
    public override string ToString()
    {
        var resultado = "Gramática Libre de Contexto:\n";
        resultado += $"V = {{ {string.Join(", ", Variables.Select(v => v.Valor))} }}\n";
        resultado += $"Σ = {{ {string.Join(", ", Terminales.Select(t => t.Valor))} }}\n";
        resultado += $"S = {SimboloInicial?.Valor ?? "null"}\n";
        resultado += "R = {\n";
        foreach (var produccion in Producciones)
        {
            resultado += $"  {produccion}\n";
        }
        resultado += "}";
        return resultado;
    }
}
