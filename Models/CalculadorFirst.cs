using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Calculador del conjunto FIRST para gramáticas libres de contexto
/// Algoritmo visto en clase de teoría de la computación
/// </summary>
public class CalculadorFirst
{
    /// <summary>
    /// Calcula el conjunto FIRST para todos los símbolos de la gramática
    /// </summary>
    public static Dictionary<string, HashSet<string>> CalcularFirst(Gramatica gramatica)
    {
        var firstSets = new Dictionary<string, HashSet<string>>();
        
        // Inicializar conjuntos FIRST vacíos para cada no terminal
        foreach (var produccion in gramatica.Producciones)
        {
            if (!firstSets.ContainsKey(produccion.Izquierda))
            {
                firstSets[produccion.Izquierda] = new HashSet<string>();
            }
        }
        
        // Aplicar el algoritmo iterativamente hasta que no haya cambios
        bool cambios = true;
        while (cambios)
        {
            cambios = false;
            
            foreach (var produccion in gramatica.Producciones)
            {
                foreach (var derecha in produccion.Derechas)
                {
                    var antesCount = firstSets[produccion.Izquierda].Count;
                    CalcularFirstProduccion(produccion.Izquierda, derecha, firstSets, gramatica);
                    
                    if (firstSets[produccion.Izquierda].Count > antesCount)
                    {
                        cambios = true;
                    }
                }
            }
        }
        
        return firstSets;
    }
    
    /// <summary>
    /// Calcula FIRST para una producción específica
    /// </summary>
    private static void CalcularFirstProduccion(
        string noTerminal, 
        string produccion, 
        Dictionary<string, HashSet<string>> firstSets,
        Gramatica gramatica)
    {
        var simbolos = produccion.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (simbolos.Length == 0 || produccion.Trim() == "ε")
        {
            // Si la producción es epsilon
            firstSets[noTerminal].Add("ε");
            return;
        }
        
        // Procesar cada símbolo de la producción
        for (int i = 0; i < simbolos.Length; i++)
        {
            var simbolo = simbolos[i];
            
            if (EsTerminal(simbolo, gramatica))
            {
                // Si es terminal, agregarlo a FIRST y terminar
                firstSets[noTerminal].Add(simbolo);
                break;
            }
            else if (EsNoTerminal(simbolo, firstSets))
            {
                // Si es no terminal, agregar su FIRST (excepto epsilon)
                foreach (var terminal in firstSets[simbolo])
                {
                    if (terminal != "ε")
                    {
                        firstSets[noTerminal].Add(terminal);
                    }
                }
                
                // Si el no terminal no puede derivar epsilon, terminar
                if (!firstSets[simbolo].Contains("ε"))
                {
                    break;
                }
                
                // Si llegamos al último símbolo y todos pueden derivar epsilon
                if (i == simbolos.Length - 1)
                {
                    firstSets[noTerminal].Add("ε");
                }
            }
        }
    }
    
    /// <summary>
    /// Verifica si un símbolo es terminal
    /// Un símbolo es terminal si:
    /// - Es un operador (+, -, *, /, etc.)
    /// - Es NUM o ID
    /// - Es un paréntesis
    /// - Es epsilon
    /// </summary>
    private static bool EsTerminal(string simbolo, Gramatica gramatica)
    {
        // Lista de terminales conocidos
        var terminales = new HashSet<string> 
        { 
            "+", "-", "*", "/", "%",
            "(", ")",
            "NUM", "ID", "ε",
            "int", "double", "float"
        };
        
        // Si está en la lista de terminales
        if (terminales.Contains(simbolo))
            return true;
        
        // Si NO es un no terminal de la gramática, es terminal
        return !gramatica.Producciones.Any(p => p.Izquierda == simbolo);
    }
    
    /// <summary>
    /// Verifica si un símbolo es no terminal
    /// </summary>
    private static bool EsNoTerminal(string simbolo, Dictionary<string, HashSet<string>> firstSets)
    {
        return firstSets.ContainsKey(simbolo);
    }
    
    /// <summary>
    /// Genera un reporte detallado del conjunto FIRST
    /// </summary>
    public static string GenerarReporteFirst(
        Gramatica gramatica,
        Dictionary<string, HashSet<string>> firstSets)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine("CÁLCULO DEL CONJUNTO FIRST");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
        sb.AppendLine();
        
        sb.AppendLine("GRAMÁTICA ANALIZADA:");
        sb.AppendLine("-".PadRight(80, '-'));
        foreach (var prod in gramatica.Producciones)
        {
            sb.AppendLine($"  {prod}");
        }
        sb.AppendLine();
        
        sb.AppendLine("CONJUNTO FIRST CALCULADO:");
        sb.AppendLine("-".PadRight(80, '-'));
        
        // Ordenar no terminales alfabéticamente
        var noTerminalesOrdenados = firstSets.Keys.OrderBy(k => k).ToList();
        
        foreach (var noTerminal in noTerminalesOrdenados)
        {
            var first = firstSets[noTerminal];
            var firstOrdenado = first.OrderBy(t => t == "ε" ? "~" : t); // epsilon al final
            
            sb.AppendLine($"  FIRST({noTerminal}) = {{ {string.Join(", ", firstOrdenado)} }}");
        }
        sb.AppendLine();
        
        sb.AppendLine("ANÁLISIS DETALLADO:");
        sb.AppendLine("-".PadRight(80, '-'));
        
        foreach (var noTerminal in noTerminalesOrdenados)
        {
            sb.AppendLine($"  {noTerminal}:");
            
            var produccion = gramatica.Producciones.FirstOrDefault(p => p.Izquierda == noTerminal);
            if (produccion != null)
            {
                foreach (var derecha in produccion.Derechas)
                {
                    var firstProd = CalcularFirstParaProduccionEspecifica(derecha, firstSets, gramatica);
                    sb.AppendLine($"    {noTerminal} → {derecha}");
                    sb.AppendLine($"      FIRST = {{ {string.Join(", ", firstProd.OrderBy(t => t))} }}");
                }
            }
            sb.AppendLine();
        }
        
        sb.AppendLine("ALGORITMO APLICADO:");
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine("  1. Si X → aα, entonces añadir 'a' a FIRST(X)");
        sb.AppendLine("  2. Si X → ε, entonces añadir 'ε' a FIRST(X)");
        sb.AppendLine("  3. Si X → Y₁Y₂...Yₖ:");
        sb.AppendLine("     - Añadir FIRST(Y₁) - {ε} a FIRST(X)");
        sb.AppendLine("     - Si ε ∈ FIRST(Y₁), añadir FIRST(Y₂) - {ε}");
        sb.AppendLine("     - Continuar mientras Yᵢ pueda derivar ε");
        sb.AppendLine("     - Si todos pueden derivar ε, añadir ε a FIRST(X)");
        sb.AppendLine();
        
        sb.AppendLine("ESTADÍSTICAS:");
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine($"  No terminales: {firstSets.Count}");
        sb.AppendLine($"  Total símbolos en FIRST: {firstSets.Values.Sum(s => s.Count)}");
        
        var conEpsilon = firstSets.Count(kvp => kvp.Value.Contains("ε"));
        sb.AppendLine($"  No terminales que derivan ε: {conEpsilon}");
        sb.AppendLine();
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Calcula FIRST para una producción específica (usado en el reporte)
    /// </summary>
    private static HashSet<string> CalcularFirstParaProduccionEspecifica(
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
            else if (EsNoTerminal(simbolo, firstSets))
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
}
