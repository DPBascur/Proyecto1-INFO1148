using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Clase para eliminar recursión por la izquierda de una gramática
/// Implementa el algoritmo visto en clase de teoría de la computación
/// </summary>
public class EliminadorRecursion
{
    /// <summary>
    /// Elimina la recursión por la izquierda (directa e indirecta) de una gramática
    /// </summary>
    public static Gramatica EliminarRecursionIzquierda(Gramatica gramaticaOriginal)
    {
        // Crear una nueva gramática transformada
        var gramaticaTransformada = new Gramatica(gramaticaOriginal.SimboloInicial);
        
        // Copiar las producciones originales para trabajar con ellas
        var producciones = CopiarProducciones(gramaticaOriginal.Producciones);
        
        // Paso 1: Eliminar recursión indirecta
        EliminarRecursionIndirecta(producciones);
        
        // Paso 2: Eliminar recursión directa
        foreach (var produccion in producciones.ToList())
        {
            var produccionesTransformadas = EliminarRecursionDirecta(produccion);
            
            // Agregar las producciones transformadas a la gramática
            foreach (var prod in produccionesTransformadas)
            {
                gramaticaTransformada.Producciones.Add(prod);
            }
        }
        
        return gramaticaTransformada;
    }
    
    /// <summary>
    /// Elimina la recursión directa de una producción
    /// Algoritmo: A → Aα₁ | Aα₂ | ... | Aαₘ | β₁ | β₂ | ... | βₙ
    /// Transforma a: A → β₁A' | β₂A' | ... | βₙA'
    ///               A' → α₁A' | α₂A' | ... | αₘA' | ε
    /// </summary>
    private static List<Produccion> EliminarRecursionDirecta(Produccion produccion)
    {
        var resultado = new List<Produccion>();
        var simbolo = produccion.Izquierda;
        
        // Separar producciones recursivas y no recursivas
        var produccionesRecursivas = new List<string>();
        var produccionesNoRecursivas = new List<string>();
        
        foreach (var derecha in produccion.Derechas)
        {
            if (EsRecursivaIzquierda(simbolo, derecha))
            {
                produccionesRecursivas.Add(derecha);
            }
            else
            {
                produccionesNoRecursivas.Add(derecha);
            }
        }
        
        // Si no hay recursión directa, retornar la producción original
        if (produccionesRecursivas.Count == 0)
        {
            resultado.Add(produccion);
            return resultado;
        }
        
        // Crear nuevo símbolo no terminal A'
        var nuevoSimbolo = simbolo + "'";
        
        // Crear producción A → β₁A' | β₂A' | ... | βₙA'
        var produccionOriginal = new Produccion(simbolo);
        
        if (produccionesNoRecursivas.Count == 0)
        {
            // Si no hay producciones no recursivas, agregar A → A'
            produccionOriginal.Derechas.Add(nuevoSimbolo);
        }
        else
        {
            foreach (var beta in produccionesNoRecursivas)
            {
                // Si beta es vacío o solo espacios, solo agregar A'
                if (string.IsNullOrWhiteSpace(beta) || beta == "ε")
                {
                    produccionOriginal.Derechas.Add(nuevoSimbolo);
                }
                else
                {
                    produccionOriginal.Derechas.Add($"{beta} {nuevoSimbolo}");
                }
            }
        }
        
        // Crear producción A' → α₁A' | α₂A' | ... | αₘA' | ε
        var produccionNueva = new Produccion(nuevoSimbolo);
        
        foreach (var alfa in produccionesRecursivas)
        {
            // Remover el símbolo recursivo del inicio
            var sinRecursion = RemoverSimboloInicial(simbolo, alfa);
            
            if (string.IsNullOrWhiteSpace(sinRecursion))
            {
                produccionNueva.Derechas.Add(nuevoSimbolo);
            }
            else
            {
                produccionNueva.Derechas.Add($"{sinRecursion} {nuevoSimbolo}");
            }
        }
        
        // Agregar producción epsilon
        produccionNueva.Derechas.Add("ε");
        
        resultado.Add(produccionOriginal);
        resultado.Add(produccionNueva);
        
        return resultado;
    }
    
    /// <summary>
    /// Elimina la recursión indirecta
    /// Algoritmo: Ordenar no terminales y sustituir producciones
    /// </summary>
    private static void EliminarRecursionIndirecta(List<Produccion> producciones)
    {
        var noTerminales = producciones.Select(p => p.Izquierda).Distinct().ToList();
        
        // Para cada par de no terminales (i, j) donde i < j
        for (int i = 0; i < noTerminales.Count; i++)
        {
            for (int j = 0; j < i; j++)
            {
                var Ai = noTerminales[i];
                var Aj = noTerminales[j];
                
                // Encontrar producción Ai
                var produccionAi = producciones.FirstOrDefault(p => p.Izquierda == Ai);
                if (produccionAi == null) continue;
                
                // Encontrar producción Aj
                var produccionAj = producciones.FirstOrDefault(p => p.Izquierda == Aj);
                if (produccionAj == null) continue;
                
                // Sustituir Ai → Aj γ por Ai → δ₁ γ | δ₂ γ | ... donde Aj → δ₁ | δ₂ | ...
                var nuevasDerechas = new List<string>();
                var derechasModificadas = false;
                
                foreach (var derecha in produccionAi.Derechas.ToList())
                {
                    if (ComenzaCon(derecha, Aj))
                    {
                        derechasModificadas = true;
                        var gamma = RemoverSimboloInicial(Aj, derecha);
                        
                        // Sustituir con todas las producciones de Aj
                        foreach (var delta in produccionAj.Derechas)
                        {
                            if (string.IsNullOrWhiteSpace(gamma))
                            {
                                nuevasDerechas.Add(delta);
                            }
                            else
                            {
                                nuevasDerechas.Add($"{delta} {gamma}");
                            }
                        }
                    }
                    else
                    {
                        nuevasDerechas.Add(derecha);
                    }
                }
                
                if (derechasModificadas)
                {
                    produccionAi.Derechas.Clear();
                    produccionAi.Derechas.AddRange(nuevasDerechas);
                }
            }
        }
    }
    
    /// <summary>
    /// Verifica si una producción es recursiva por la izquierda
    /// </summary>
    private static bool EsRecursivaIzquierda(string simbolo, string produccion)
    {
        if (string.IsNullOrWhiteSpace(produccion))
            return false;
            
        // La producción es recursiva si comienza con el mismo símbolo
        return ComenzaCon(produccion, simbolo);
    }
    
    /// <summary>
    /// Verifica si una producción comienza con un símbolo específico
    /// </summary>
    private static bool ComenzaCon(string produccion, string simbolo)
    {
        var tokens = produccion.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length > 0 && tokens[0] == simbolo;
    }
    
    /// <summary>
    /// Remueve el símbolo inicial de una producción
    /// </summary>
    private static string RemoverSimboloInicial(string simbolo, string produccion)
    {
        var tokens = produccion.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (tokens.Length > 0 && tokens[0] == simbolo)
        {
            // Remover el primer token y retornar el resto
            var resto = string.Join(" ", tokens.Skip(1));
            return resto;
        }
        
        return produccion;
    }
    
    /// <summary>
    /// Copia profunda de las producciones
    /// </summary>
    private static List<Produccion> CopiarProducciones(List<Produccion> originales)
    {
        var copias = new List<Produccion>();
        
        foreach (var original in originales)
        {
            var copia = new Produccion(original.Izquierda);
            copia.Derechas.AddRange(original.Derechas);
            copias.Add(copia);
        }
        
        return copias;
    }
    
    /// <summary>
    /// Genera un reporte detallado del proceso de eliminación
    /// </summary>
    public static string GenerarReporteEliminacion(Gramatica original, Gramatica transformada)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine("ELIMINACIÓN DE RECURSIÓN POR LA IZQUIERDA");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine();
        
        sb.AppendLine("GRAMÁTICA ORIGINAL:");
        sb.AppendLine("-".PadRight(80, '-'));
        foreach (var prod in original.Producciones)
        {
            sb.AppendLine($"  {prod}");
        }
        sb.AppendLine();
        
        sb.AppendLine("GRAMÁTICA TRANSFORMADA (SIN RECURSIÓN IZQUIERDA):");
        sb.AppendLine("-".PadRight(80, '-'));
        foreach (var prod in transformada.Producciones)
        {
            sb.AppendLine($"  {prod}");
        }
        sb.AppendLine();
        
        sb.AppendLine("ANÁLISIS:");
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine($"  Producciones originales: {original.Producciones.Count}");
        sb.AppendLine($"  Producciones transformadas: {transformada.Producciones.Count}");
        sb.AppendLine($"  Símbolos nuevos agregados: {transformada.Producciones.Count - original.Producciones.Count}");
        sb.AppendLine();
        
        // Detectar qué producciones tenían recursión
        sb.AppendLine("PRODUCCIONES CON RECURSIÓN DETECTADA:");
        sb.AppendLine("-".PadRight(80, '-'));
        
        bool hayRecursion = false;
        foreach (var prod in original.Producciones)
        {
            foreach (var derecha in prod.Derechas)
            {
                if (EsRecursivaIzquierda(prod.Izquierda, derecha))
                {
                    hayRecursion = true;
                    sb.AppendLine($"  {prod.Izquierda} → {derecha} (Recursión directa)");
                }
            }
        }
        
        if (!hayRecursion)
        {
            sb.AppendLine("  No se detectó recursión por la izquierda");
        }
        
        sb.AppendLine();
        sb.AppendLine("ALGORITMO APLICADO:");
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine("  1. Detección de recursión indirecta");
        sb.AppendLine("  2. Eliminación de recursión directa usando:");
        sb.AppendLine("     A → Aα | β  ⟹  A → βA' ; A' → αA' | ε");
        sb.AppendLine();
        
        return sb.ToString();
    }
}
