namespace ProyectoFinal.Models;

/// <summary>
/// Representa un token identificado en el análisis léxico
/// </summary>
public class Token
{
    /// <summary>
    /// Tipo de token (NUM, ID, MAS, MENOS, MULT, DIV, MOD, PAR_IZQ, PAR_DER, etc.)
    /// </summary>
    public TipoToken Tipo { get; set; }
    
    /// <summary>
    /// Valor literal del token
    /// </summary>
    public string Valor { get; set; }
    
    /// <summary>
    /// Línea donde se encontró el token
    /// </summary>
    public int Linea { get; set; }
    
    /// <summary>
    /// Columna donde se encontró el token
    /// </summary>
    public int Columna { get; set; }

    public Token(TipoToken tipo, string valor, int linea, int columna)
    {
        Tipo = tipo;
        Valor = valor;
        Linea = linea;
        Columna = columna;
    }

    public override string ToString()
    {
        return $"<{Tipo}, '{Valor}', L:{Linea}, C:{Columna}>";
    }
}

/// <summary>
/// Tipos de tokens soportados
/// </summary>
public enum TipoToken
{
    // Literales
    NUM,        // Números (enteros y decimales)
    ID,         // Identificadores (variables)
    
    // Operadores aritméticos
    MAS,        // +
    MENOS,      // -
    MULT,       // *
    DIV,        // /
    MOD,        // %
    
    // Paréntesis
    PAR_IZQ,    // (
    PAR_DER,    // )
    
    // Operador de asignación
    ASIGNAR,    // =
    
    // Punto y coma
    PUNTO_COMA, // ;
    
    // Palabras clave de Java
    INT,        // int
    DOUBLE,     // double
    FLOAT,      // float
    
    // Otros
    ESPACIO,    // Espacios, tabs
    NUEVA_LINEA,// Salto de línea
    COMENTARIO, // Comentarios
    FIN,        // Fin de archivo
    DESCONOCIDO // Token no reconocido
}
