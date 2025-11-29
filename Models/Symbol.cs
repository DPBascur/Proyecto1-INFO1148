using System;

namespace ProyectoFinal.Models;

/// <summary>
/// Clase base abstracta que representa un símbolo en una Gramática Libre de Contexto.
/// Un símbolo puede ser terminal (constante) o no terminal (variable).
/// </summary>
public abstract class Symbol
{
    /// <summary>
    /// Nombre o valor del símbolo.
    /// </summary>
    public string Valor { get; set; }

    /// <summary>
    /// Constructor protegido para inicializar el símbolo con su valor.
    /// </summary>
    /// <param name="valor">El valor del símbolo.</param>
    protected Symbol(string valor)
    {
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
    }

    /// <summary>
    /// Indica si este símbolo es terminal.
    /// </summary>
    public abstract bool EsTerminal { get; }

    /// <summary>
    /// Indica si este símbolo es no terminal.
    /// </summary>
    public bool EsNoTerminal => !EsTerminal;

    public override string ToString() => Valor;

    public override bool Equals(object? obj)
    {
        if (obj is Symbol other)
        {
            return Valor == other.Valor && EsTerminal == other.EsTerminal;
        }
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Valor, EsTerminal);
}
