using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoFinal.Models;

/// <summary>
/// Representa una regla de producción en una Gramática Libre de Contexto.
/// Una producción tiene la forma A → α, donde:
/// - A es un no terminal (lado izquierdo)
/// - α es una secuencia de símbolos terminales y/o no terminales (lado derecho)
/// </summary>
public class Production
{
    /// <summary>
    /// Lado izquierdo de la producción (debe ser un no terminal).
    /// </summary>
    public NonTerminal LadoIzquierdo { get; set; }

    /// <summary>
    /// Lado derecho de la producción (secuencia de símbolos).
    /// </summary>
    public List<Symbol> LadoDerecho { get; set; }

    /// <summary>
    /// Constructor para crear una producción.
    /// </summary>
    /// <param name="ladoIzquierdo">El no terminal del lado izquierdo.</param>
    /// <param name="ladoDerecho">La secuencia de símbolos del lado derecho.</param>
    public Production(NonTerminal ladoIzquierdo, List<Symbol> ladoDerecho)
    {
        LadoIzquierdo = ladoIzquierdo ?? throw new ArgumentNullException(nameof(ladoIzquierdo));
        LadoDerecho = ladoDerecho ?? throw new ArgumentNullException(nameof(ladoDerecho));
    }

    /// <summary>
    /// Constructor alternativo que acepta múltiples símbolos como parámetros variables.
    /// </summary>
    public Production(NonTerminal ladoIzquierdo, params Symbol[] ladoDerecho)
        : this(ladoIzquierdo, ladoDerecho.ToList())
    {
    }

    /// <summary>
    /// Indica si esta es una producción épsilon (λ o ε), es decir, el lado derecho está vacío.
    /// </summary>
    public bool EsProduccionEpsilon => LadoDerecho.Count == 0;

    public override string ToString()
    {
        var derecha = LadoDerecho.Count == 0 ? "ε" : string.Join(" ", LadoDerecho);
        return $"{LadoIzquierdo.Valor} → {derecha}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is Production other)
        {
            return LadoIzquierdo.Equals(other.LadoIzquierdo) && 
                   LadoDerecho.SequenceEqual(other.LadoDerecho);
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(LadoIzquierdo);
        foreach (var simbolo in LadoDerecho)
        {
            hash.Add(simbolo);
        }
        return hash.ToHashCode();
    }
}
