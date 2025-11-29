using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProyectoFinal.Models;

/// <summary>
/// Analizador léxico que extrae tokens de código fuente Java
/// </summary>
public class AnalizadorLexico
{
    private readonly string _codigoFuente;
    private int _posicion;
    private int _linea;
    private int _columna;

    public AnalizadorLexico(string codigoFuente)
    {
        _codigoFuente = codigoFuente ?? string.Empty;
        _posicion = 0;
        _linea = 1;
        _columna = 1;
    }

    /// <summary>
    /// Analiza el código fuente y devuelve una lista de tokens
    /// </summary>
    public List<Token> Tokenizar()
    {
        var tokens = new List<Token>();
        
        while (_posicion < _codigoFuente.Length)
        {
            var token = ObtenerSiguienteToken();
            
            // Solo agregar tokens relevantes (ignorar espacios, comentarios)
            if (token.Tipo != TipoToken.ESPACIO && 
                token.Tipo != TipoToken.NUEVA_LINEA && 
                token.Tipo != TipoToken.COMENTARIO)
            {
                tokens.Add(token);
            }
        }
        
        // Agregar token FIN
        tokens.Add(new Token(TipoToken.FIN, "", _linea, _columna));
        
        return tokens;
    }

    /// <summary>
    /// Obtiene el siguiente token del código fuente
    /// </summary>
    private Token ObtenerSiguienteToken()
    {
        if (_posicion >= _codigoFuente.Length)
        {
            return new Token(TipoToken.FIN, "", _linea, _columna);
        }

        char caracterActual = _codigoFuente[_posicion];
        int lineaToken = _linea;
        int columnaToken = _columna;

        // Ignorar espacios en blanco
        if (char.IsWhiteSpace(caracterActual))
        {
            return ConsumirEspacios();
        }

        // Comentarios de una línea
        if (caracterActual == '/' && Mirar() == '/')
        {
            return ConsumirComentarioLinea();
        }

        // Comentarios de múltiples líneas
        if (caracterActual == '/' && Mirar() == '*')
        {
            return ConsumirComentarioBloque();
        }

        // Números
        if (char.IsDigit(caracterActual))
        {
            return ConsumirNumero();
        }

        // Identificadores y palabras clave
        if (char.IsLetter(caracterActual) || caracterActual == '_')
        {
            return ConsumirIdentificadorOPalabraClave();
        }

        // Operadores y símbolos
        switch (caracterActual)
        {
            case '+':
                Avanzar();
                return new Token(TipoToken.MAS, "+", lineaToken, columnaToken);
            
            case '-':
                Avanzar();
                return new Token(TipoToken.MENOS, "-", lineaToken, columnaToken);
            
            case '*':
                Avanzar();
                return new Token(TipoToken.MULT, "*", lineaToken, columnaToken);
            
            case '/':
                Avanzar();
                return new Token(TipoToken.DIV, "/", lineaToken, columnaToken);
            
            case '%':
                Avanzar();
                return new Token(TipoToken.MOD, "%", lineaToken, columnaToken);
            
            case '(':
                Avanzar();
                return new Token(TipoToken.PAR_IZQ, "(", lineaToken, columnaToken);
            
            case ')':
                Avanzar();
                return new Token(TipoToken.PAR_DER, ")", lineaToken, columnaToken);
            
            case '=':
                Avanzar();
                return new Token(TipoToken.ASIGNAR, "=", lineaToken, columnaToken);
            
            case ';':
                Avanzar();
                return new Token(TipoToken.PUNTO_COMA, ";", lineaToken, columnaToken);
            
            default:
                Avanzar();
                return new Token(TipoToken.DESCONOCIDO, caracterActual.ToString(), lineaToken, columnaToken);
        }
    }

    /// <summary>
    /// Consume espacios en blanco
    /// </summary>
    private Token ConsumirEspacios()
    {
        var sb = new StringBuilder();
        int lineaInicio = _linea;
        int columnaInicio = _columna;
        
        while (_posicion < _codigoFuente.Length && char.IsWhiteSpace(_codigoFuente[_posicion]))
        {
            char ch = _codigoFuente[_posicion];
            sb.Append(ch);
            
            if (ch == '\n')
            {
                _linea++;
                _columna = 1;
                _posicion++;
                return new Token(TipoToken.NUEVA_LINEA, "\n", lineaInicio, columnaInicio);
            }
            else
            {
                Avanzar();
            }
        }
        
        return new Token(TipoToken.ESPACIO, sb.ToString(), lineaInicio, columnaInicio);
    }

    /// <summary>
    /// Consume un comentario de una línea
    /// </summary>
    private Token ConsumirComentarioLinea()
    {
        var sb = new StringBuilder();
        int lineaInicio = _linea;
        int columnaInicio = _columna;
        
        while (_posicion < _codigoFuente.Length && _codigoFuente[_posicion] != '\n')
        {
            sb.Append(_codigoFuente[_posicion]);
            Avanzar();
        }
        
        return new Token(TipoToken.COMENTARIO, sb.ToString(), lineaInicio, columnaInicio);
    }

    /// <summary>
    /// Consume un comentario de múltiples líneas
    /// </summary>
    private Token ConsumirComentarioBloque()
    {
        var sb = new StringBuilder();
        int lineaInicio = _linea;
        int columnaInicio = _columna;
        
        // Consumir /*
        sb.Append(_codigoFuente[_posicion]);
        Avanzar();
        sb.Append(_codigoFuente[_posicion]);
        Avanzar();
        
        while (_posicion < _codigoFuente.Length - 1)
        {
            if (_codigoFuente[_posicion] == '*' && _codigoFuente[_posicion + 1] == '/')
            {
                sb.Append(_codigoFuente[_posicion]);
                Avanzar();
                sb.Append(_codigoFuente[_posicion]);
                Avanzar();
                break;
            }
            
            if (_codigoFuente[_posicion] == '\n')
            {
                _linea++;
                _columna = 0;
            }
            
            sb.Append(_codigoFuente[_posicion]);
            Avanzar();
        }
        
        return new Token(TipoToken.COMENTARIO, sb.ToString(), lineaInicio, columnaInicio);
    }

    /// <summary>
    /// Consume un número (entero o decimal)
    /// </summary>
    private Token ConsumirNumero()
    {
        var sb = new StringBuilder();
        int lineaInicio = _linea;
        int columnaInicio = _columna;
        
        while (_posicion < _codigoFuente.Length && 
               (char.IsDigit(_codigoFuente[_posicion]) || _codigoFuente[_posicion] == '.'))
        {
            sb.Append(_codigoFuente[_posicion]);
            Avanzar();
        }
        
        return new Token(TipoToken.NUM, sb.ToString(), lineaInicio, columnaInicio);
    }

    /// <summary>
    /// Consume un identificador o palabra clave
    /// </summary>
    private Token ConsumirIdentificadorOPalabraClave()
    {
        var sb = new StringBuilder();
        int lineaInicio = _linea;
        int columnaInicio = _columna;
        
        while (_posicion < _codigoFuente.Length && 
               (char.IsLetterOrDigit(_codigoFuente[_posicion]) || _codigoFuente[_posicion] == '_'))
        {
            sb.Append(_codigoFuente[_posicion]);
            Avanzar();
        }
        
        string valor = sb.ToString();
        TipoToken tipo = ObtenerTipoPalabraClave(valor);
        
        return new Token(tipo, valor, lineaInicio, columnaInicio);
    }

    /// <summary>
    /// Determina si una palabra es una palabra clave de Java
    /// </summary>
    private TipoToken ObtenerTipoPalabraClave(string palabra)
    {
        return palabra switch
        {
            "int" => TipoToken.INT,
            "double" => TipoToken.DOUBLE,
            "float" => TipoToken.FLOAT,
            _ => TipoToken.ID
        };
    }

    /// <summary>
    /// Avanza al siguiente carácter
    /// </summary>
    private void Avanzar()
    {
        if (_posicion < _codigoFuente.Length)
        {
            _posicion++;
            _columna++;
        }
    }

    /// <summary>
    /// Mira el siguiente carácter sin consumirlo
    /// </summary>
    private char Mirar(int desplazamiento = 1)
    {
        int pos = _posicion + desplazamiento;
        if (pos < _codigoFuente.Length)
        {
            return _codigoFuente[pos];
        }
        return '\0';
    }
}
