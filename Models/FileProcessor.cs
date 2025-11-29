using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoFinal.Models;

/// <summary>
/// Servicio para procesar archivos fuente Java
/// </summary>
public class ProcesadorArchivos
{
    /// <summary>
    /// Lee un archivo Java y extrae tokens
    /// </summary>
    public static async Task<(List<Token> tokens, string codigoFuente)> ProcesarArchivoJavaAsync(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException($"El archivo no existe: {rutaArchivo}");
        }

        // Leer el contenido del archivo
        string codigoFuente = await File.ReadAllTextAsync(rutaArchivo);

        // Crear el analizador léxico y tokenizar
        var analizador = new AnalizadorLexico(codigoFuente);
        var tokens = analizador.Tokenizar();

        return (tokens, codigoFuente);
    }

    /// <summary>
    /// Guarda los tokens en un archivo de texto
    /// </summary>
    public static async Task GuardarTokensEnArchivoAsync(List<Token> tokens, string rutaSalida)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine("ANÁLISIS LÉXICO - TOKENS RECONOCIDOS");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();
        sb.AppendLine($"Total de tokens: {tokens.Count}");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine($"{"#",-5} {"TIPO",-15} {"VALOR",-20} {"LÍNEA",-8} {"COLUMNA",-8}");
        sb.AppendLine("-".PadRight(80, '-'));

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            sb.AppendLine($"{i + 1,-5} {token.Tipo,-15} {token.Valor,-20} {token.Linea,-8} {token.Columna,-8}");
        }

        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine();

        // Estadísticas
        var estadisticas = ObtenerEstadisticasTokens(tokens);
        sb.AppendLine("ESTADÍSTICAS:");
        sb.AppendLine();
        foreach (var stat in estadisticas.OrderByDescending(x => x.Value))
        {
            sb.AppendLine($"  {stat.Key,-15}: {stat.Value,5}");
        }

        await File.WriteAllTextAsync(rutaSalida, sb.ToString());
    }

    /// <summary>
    /// Calcula estadísticas de los tokens
    /// </summary>
    private static Dictionary<string, int> ObtenerEstadisticasTokens(List<Token> tokens)
    {
        var estadisticas = new Dictionary<string, int>();

        foreach (var token in tokens)
        {
            string clave = token.Tipo.ToString();
            if (estadisticas.ContainsKey(clave))
            {
                estadisticas[clave]++;
            }
            else
            {
                estadisticas[clave] = 1;
            }
        }

        return estadisticas;
    }

    /// <summary>
    /// Filtra solo las expresiones aritméticas del código
    /// </summary>
    public static List<List<Token>> ExtraerExpresionesAritmeticas(List<Token> tokens)
    {
        var expresiones = new List<List<Token>>();
        var expresionActual = new List<Token>();

        foreach (var token in tokens)
        {
            // Si es un token relevante para expresiones aritméticas
            if (EsTokenAritmetico(token))
            {
                expresionActual.Add(token);
            }
            // Si encontramos un punto y coma, termina la expresión
            else if (token.Tipo == TipoToken.PUNTO_COMA && expresionActual.Count > 0)
            {
                expresiones.Add(new List<Token>(expresionActual));
                expresionActual.Clear();
            }
        }

        // Agregar la última expresión si no terminó con ;
        if (expresionActual.Count > 0)
        {
            expresiones.Add(expresionActual);
        }

        return expresiones;
    }

    /// <summary>
    /// Verifica si un token es relevante para expresiones aritméticas
    /// </summary>
    private static bool EsTokenAritmetico(Token token)
    {
        return token.Tipo switch
        {
            TipoToken.NUM => true,
            TipoToken.ID => true,
            TipoToken.MAS => true,
            TipoToken.MENOS => true,
            TipoToken.MULT => true,
            TipoToken.DIV => true,
            TipoToken.MOD => true,
            TipoToken.PAR_IZQ => true,
            TipoToken.PAR_DER => true,
            TipoToken.ASIGNAR => true,
            _ => false
        };
    }

    /// <summary>
    /// Guarda las expresiones aritméticas en un archivo
    /// </summary>
    public static async Task GuardarExpresionesEnArchivoAsync(List<List<Token>> expresiones, string rutaSalida)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine("EXPRESIONES ARITMÉTICAS EXTRAÍDAS");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();
        sb.AppendLine($"Total de expresiones: {expresiones.Count}");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine();

        for (int i = 0; i < expresiones.Count; i++)
        {
            sb.AppendLine($"Expresión {i + 1}:");
            sb.AppendLine($"  Tokens: {string.Join(" ", expresiones[i].Select(t => t.Valor))}");
            sb.AppendLine($"  Tipos:  {string.Join(" ", expresiones[i].Select(t => t.Tipo))}");
            sb.AppendLine();
        }

        await File.WriteAllTextAsync(rutaSalida, sb.ToString());
    }
}
