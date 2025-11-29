using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Generador de tabla sintáctica LL(1)
/// Usa gramática transformada, conjuntos FIRST y FOLLOW
/// </summary>
public class GeneradorTablaLL1
{
    /// <summary>
    /// Entrada de la tabla LL(1): (NoTerminal, Terminal) -> Producción
    /// </summary>
    public class EntradaTabla
    {
        public string NoTerminal { get; set; }
        public string Terminal { get; set; }
        public string Produccion { get; set; }
        
        public EntradaTabla(string noTerminal, string terminal, string produccion)
        {
            NoTerminal = noTerminal;
            Terminal = terminal;
            Produccion = produccion;
        }
    }
    
    /// <summary>
    /// Tabla sintáctica LL(1)
    /// </summary>
    public class TablaLL1
    {
        public Dictionary<(string, string), string> Tabla { get; set; }
        public List<string> NoTerminales { get; set; }
        public List<string> Terminales { get; set; }
        
        public TablaLL1()
        {
            Tabla = new Dictionary<(string, string), string>();
            NoTerminales = new List<string>();
            Terminales = new List<string>();
        }
    }
    
    /// <summary>
    /// Genera la tabla sintáctica LL(1)
    /// </summary>
    public static TablaLL1 GenerarTabla(
        Gramatica gramatica,
        Dictionary<string, HashSet<string>> firstSets,
        Dictionary<string, HashSet<string>> followSets)
    {
        var tabla = new TablaLL1();
        
        // Obtener todos los no terminales
        tabla.NoTerminales = gramatica.Producciones
            .Select(p => p.Izquierda)
            .Distinct()
            .OrderBy(nt => nt)
            .ToList();
        
        // Obtener todos los terminales del FIRST y FOLLOW
        var terminalesSet = new HashSet<string>();
        foreach (var first in firstSets.Values)
        {
            foreach (var t in first)
            {
                if (t != "ε")
                    terminalesSet.Add(t);
            }
        }
        foreach (var follow in followSets.Values)
        {
            foreach (var t in follow)
            {
                terminalesSet.Add(t);
            }
        }
        
        tabla.Terminales = terminalesSet.OrderBy(t => t).ToList();
        
        // Construir la tabla
        foreach (var produccion in gramatica.Producciones)
        {
            var A = produccion.Izquierda;
            
            foreach (var alfa in produccion.Derechas)
            {
                // Calcular FIRST(α)
                var firstAlfa = CalcularFirstProduccion(alfa, firstSets, gramatica);
                
                // Para cada terminal en FIRST(α), agregar A → α en M[A, a]
                foreach (var a in firstAlfa)
                {
                    if (a != "ε")
                    {
                        var key = (A, a);
                        if (!tabla.Tabla.ContainsKey(key))
                        {
                            tabla.Tabla[key] = $"{A} → {alfa}";
                        }
                        else
                        {
                            // Conflicto detectado
                            tabla.Tabla[key] += $" | {alfa}";
                        }
                    }
                }
                
                // Si ε está en FIRST(α), para cada terminal en FOLLOW(A), agregar A → α en M[A, b]
                if (firstAlfa.Contains("ε") && followSets.ContainsKey(A))
                {
                    foreach (var b in followSets[A])
                    {
                        var key = (A, b);
                        if (!tabla.Tabla.ContainsKey(key))
                        {
                            tabla.Tabla[key] = $"{A} → {alfa}";
                        }
                        else
                        {
                            // Conflicto detectado
                            tabla.Tabla[key] += $" | {alfa}";
                        }
                    }
                }
            }
        }
        
        return tabla;
    }
    
    /// <summary>
    /// Calcula FIRST para una producción específica
    /// </summary>
    private static HashSet<string> CalcularFirstProduccion(
        string produccion,
        Dictionary<string, HashSet<string>> firstSets,
        Gramatica gramatica)
    {
        var resultado = new HashSet<string>();
        var simbolos = produccion.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (simbolos.Length == 0 || produccion.Trim() == "ε")
        {
            resultado.Add("ε");
            return resultado;
        }
        
        for (int i = 0; i < simbolos.Length; i++)
        {
            var simbolo = simbolos[i];
            
            if (EsTerminal(simbolo, gramatica))
            {
                resultado.Add(simbolo);
                break;
            }
            else if (firstSets.ContainsKey(simbolo))
            {
                foreach (var terminal in firstSets[simbolo])
                {
                    if (terminal != "ε")
                    {
                        resultado.Add(terminal);
                    }
                }
                
                if (!firstSets[simbolo].Contains("ε"))
                {
                    break;
                }
                
                if (i == simbolos.Length - 1)
                {
                    resultado.Add("ε");
                }
            }
        }
        
        return resultado;
    }
    
    private static bool EsTerminal(string simbolo, Gramatica gramatica)
    {
        var terminales = new HashSet<string> 
        { 
            "+", "-", "*", "/", "%",
            "(", ")",
            "NUM", "ID", "ε", "$",
            "int", "double", "float"
        };
        
        return terminales.Contains(simbolo) || 
               !gramatica.Producciones.Any(p => p.Izquierda == simbolo);
    }
    
    /// <summary>
    /// Genera un reporte detallado de la tabla LL(1)
    /// </summary>
    public static string GenerarReporte(
        TablaLL1 tabla,
        Gramatica gramatica,
        Dictionary<string, HashSet<string>> firstSets,
        Dictionary<string, HashSet<string>> followSets)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=".PadRight(120, '='));
        sb.AppendLine("TABLA SINTÁCTICA LL(1)");
        sb.AppendLine("=".PadRight(120, '='));
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
        sb.AppendLine();
        
        sb.AppendLine("GRAMÁTICA UTILIZADA:");
        sb.AppendLine("-".PadRight(120, '-'));
        foreach (var prod in gramatica.Producciones)
        {
            sb.AppendLine($"  {prod}");
        }
        sb.AppendLine();
        
        sb.AppendLine("CONJUNTOS FIRST:");
        sb.AppendLine("-".PadRight(120, '-'));
        foreach (var nt in tabla.NoTerminales)
        {
            if (firstSets.ContainsKey(nt))
            {
                sb.AppendLine($"  FIRST({nt}) = {{ {string.Join(", ", firstSets[nt].OrderBy(t => t))} }}");
            }
        }
        sb.AppendLine();
        
        sb.AppendLine("CONJUNTOS FOLLOW:");
        sb.AppendLine("-".PadRight(120, '-'));
        foreach (var nt in tabla.NoTerminales)
        {
            if (followSets.ContainsKey(nt))
            {
                sb.AppendLine($"  FOLLOW({nt}) = {{ {string.Join(", ", followSets[nt].OrderBy(t => t))} }}");
            }
        }
        sb.AppendLine();
        
        sb.AppendLine("TABLA LL(1):");
        sb.AppendLine("-".PadRight(120, '-'));
        
        // Encabezado
        sb.Append("M".PadRight(8));
        foreach (var terminal in tabla.Terminales)
        {
            sb.Append("| " + terminal.PadRight(20));
        }
        sb.AppendLine("|");
        sb.AppendLine("-".PadRight(120, '-'));
        
        // Filas
        foreach (var noTerminal in tabla.NoTerminales)
        {
            sb.Append(noTerminal.PadRight(8));
            
            foreach (var terminal in tabla.Terminales)
            {
                var key = (noTerminal, terminal);
                if (tabla.Tabla.ContainsKey(key))
                {
                    var produccion = tabla.Tabla[key];
                    sb.Append("| " + produccion.PadRight(20));
                }
                else
                {
                    sb.Append("| " + "".PadRight(20));
                }
            }
            sb.AppendLine("|");
        }
        sb.AppendLine("-".PadRight(120, '-'));
        sb.AppendLine();
        
        // Detectar conflictos
        var conflictos = tabla.Tabla.Where(kvp => kvp.Value.Contains(" | ")).ToList();
        if (conflictos.Any())
        {
            sb.AppendLine("⚠ CONFLICTOS DETECTADOS:");
            sb.AppendLine("-".PadRight(120, '-'));
            foreach (var conflicto in conflictos)
            {
                sb.AppendLine($"  M[{conflicto.Key.Item1}, {conflicto.Key.Item2}] = {conflicto.Value}");
            }
            sb.AppendLine();
            sb.AppendLine("  La gramática NO es LL(1) debido a conflictos.");
        }
        else
        {
            sb.AppendLine("✓ GRAMÁTICA LL(1) VÁLIDA");
            sb.AppendLine("-".PadRight(120, '-'));
            sb.AppendLine("  No se detectaron conflictos en la tabla.");
            sb.AppendLine("  La gramática es LL(1) y puede ser usada para análisis sintáctico descendente.");
        }
        sb.AppendLine();
        
        sb.AppendLine("ESTADÍSTICAS:");
        sb.AppendLine("-".PadRight(120, '-'));
        sb.AppendLine($"  No terminales: {tabla.NoTerminales.Count}");
        sb.AppendLine($"  Terminales: {tabla.Terminales.Count}");
        sb.AppendLine($"  Entradas en tabla: {tabla.Tabla.Count}");
        sb.AppendLine($"  Entradas vacías: {(tabla.NoTerminales.Count * tabla.Terminales.Count) - tabla.Tabla.Count}");
        sb.AppendLine();
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Genera la tabla en formato CSV
    /// </summary>
    public static string GenerarCSV(TablaLL1 tabla)
    {
        var sb = new StringBuilder();
        
        // Encabezado
        sb.Append("No Terminal");
        foreach (var terminal in tabla.Terminales)
        {
            sb.Append($",{terminal}");
        }
        sb.AppendLine();
        
        // Filas
        foreach (var noTerminal in tabla.NoTerminales)
        {
            sb.Append(noTerminal);
            
            foreach (var terminal in tabla.Terminales)
            {
                var key = (noTerminal, terminal);
                if (tabla.Tabla.ContainsKey(key))
                {
                    var produccion = tabla.Tabla[key].Replace(",", ";");
                    sb.Append($",\"{produccion}\"");
                }
                else
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
}
