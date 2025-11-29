using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Calculador del conjunto FOLLOW para gramáticas libres de contexto
/// Necesario para completar la tabla LL(1)
/// </summary>
public class CalculadorFollow
{
    /// <summary>
    /// Calcula el conjunto FOLLOW para todos los no terminales de la gramática
    /// </summary>
    public static Dictionary<string, HashSet<string>> CalcularFollow(
        Gramatica gramatica,
        Dictionary<string, HashSet<string>> firstSets)
    {
        var followSets = new Dictionary<string, HashSet<string>>();
        
        // Inicializar conjuntos FOLLOW vacíos para cada no terminal
        foreach (var produccion in gramatica.Producciones)
        {
            if (!followSets.ContainsKey(produccion.Izquierda))
            {
                followSets[produccion.Izquierda] = new HashSet<string>();
            }
        }
        
        // Regla 1: Agregar $ al FOLLOW del símbolo inicial
        if (!string.IsNullOrEmpty(gramatica.SimboloInicial))
        {
            followSets[gramatica.SimboloInicial].Add("$");
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
                    var simbolos = derecha.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    for (int i = 0; i < simbolos.Length; i++)
                    {
                        var simbolo = simbolos[i];
                        
                        // Solo procesamos no terminales
                        if (!EsNoTerminal(simbolo, followSets))
                            continue;
                        
                        var antesCount = followSets[simbolo].Count;
                        
                        // Si hay símbolos después de este
                        if (i < simbolos.Length - 1)
                        {
                            // Regla 2: Si A → αBβ, agregar FIRST(β) - {ε} a FOLLOW(B)
                            var beta = string.Join(" ", simbolos.Skip(i + 1));
                            var firstBeta = CalcularFirstCadena(beta, firstSets, gramatica);
                            
                            foreach (var terminal in firstBeta)
                            {
                                if (terminal != "ε")
                                {
                                    followSets[simbolo].Add(terminal);
                                }
                            }
                            
                            // Si β puede derivar ε, agregar FOLLOW(A) a FOLLOW(B)
                            if (firstBeta.Contains("ε"))
                            {
                                foreach (var terminal in followSets[produccion.Izquierda])
                                {
                                    followSets[simbolo].Add(terminal);
                                }
                            }
                        }
                        else
                        {
                            // Regla 3: Si A → αB, agregar FOLLOW(A) a FOLLOW(B)
                            foreach (var terminal in followSets[produccion.Izquierda])
                            {
                                followSets[simbolo].Add(terminal);
                            }
                        }
                        
                        if (followSets[simbolo].Count > antesCount)
                        {
                            cambios = true;
                        }
                    }
                }
            }
        }
        
        return followSets;
    }
    
    /// <summary>
    /// Calcula FIRST para una cadena de símbolos
    /// </summary>
    private static HashSet<string> CalcularFirstCadena(
        string cadena,
        Dictionary<string, HashSet<string>> firstSets,
        Gramatica gramatica)
    {
        var resultado = new HashSet<string>();
        var simbolos = cadena.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (simbolos.Length == 0)
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
    
    private static bool EsNoTerminal(string simbolo, Dictionary<string, HashSet<string>> followSets)
    {
        return followSets.ContainsKey(simbolo);
    }
}
