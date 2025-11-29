namespace ProyectoFinal.Models;

/// <summary>
/// Representa un símbolo terminal en una Gramática Libre de Contexto.
/// Los terminales son símbolos constantes que forman parte del alfabeto Σ
/// y no pueden ser expandidos mediante reglas de producción.
/// </summary>
public class Terminal : Symbol
{
    /// <summary>
    /// Constructor para crear un símbolo terminal.
    /// </summary>
    /// <param name="valor">El valor del terminal (ej: '+', '-', 'num', etc.).</param>
    public Terminal(string valor) : base(valor)
    {
    }

    /// <summary>
    /// Indica que este símbolo es terminal.
    /// </summary>
    public override bool EsTerminal => true;

    public override string ToString() => $"'{Valor}'";
}
