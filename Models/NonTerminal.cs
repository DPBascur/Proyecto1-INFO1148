namespace ProyectoFinal.Models;

/// <summary>
/// Representa un símbolo no terminal (variable) en una Gramática Libre de Contexto.
/// Los no terminales forman parte del conjunto V y pueden ser expandidos
/// mediante reglas de producción.
/// </summary>
public class NonTerminal : Symbol
{
    /// <summary>
    /// Constructor para crear un símbolo no terminal.
    /// </summary>
    /// <param name="valor">El nombre de la variable (ej: 'E', 'T', 'F', etc.).</param>
    public NonTerminal(string valor) : base(valor)
    {
    }

    /// <summary>
    /// Indica que este símbolo NO es terminal.
    /// </summary>
    public override bool EsTerminal => false;

    public override string ToString() => $"<{Valor}>";
}
