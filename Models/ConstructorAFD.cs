using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

/// <summary>
/// Constructor de Autómata Finito Determinista (AFD) a partir de una gramática LL(1)
/// Utiliza la tabla sintáctica LL(1) para generar el AFD
/// </summary>
public class ConstructorAFD
{
    /// <summary>
    /// Representa un estado del AFD
    /// </summary>
    public class EstadoAFD
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool EsEstadoFinal { get; set; }
        public bool EsEstadoInicial { get; set; }
        public Dictionary<string, int> Transiciones { get; set; } = new();

        public EstadoAFD(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }
    }

    /// <summary>
    /// Representa un AFD completo
    /// </summary>
    public class AFD
    {
        public List<EstadoAFD> Estados { get; set; } = new();
        public HashSet<string> Alfabeto { get; set; } = new();
        public int EstadoInicial { get; set; }
        public HashSet<int> EstadosFinales { get; set; } = new();
        public Dictionary<(int, string), int> MatrizTransicion { get; set; } = new();
        
        // Información adicional
        public Gramatica GramaticaOriginal { get; set; }
        public GeneradorTablaLL1.TablaLL1 TablaLL1 { get; set; }

        public AFD(Gramatica gramatica, GeneradorTablaLL1.TablaLL1 tabla)
        {
            GramaticaOriginal = gramatica;
            TablaLL1 = tabla;
        }
    }

    /// <summary>
    /// Construye un AFD a partir de una gramática LL(1) y su tabla sintáctica
    /// </summary>
    public static AFD ConstruirAFD(
        Gramatica gramatica,
        GeneradorTablaLL1.TablaLL1 tablaLL1,
        Dictionary<string, HashSet<string>> firstSets,
        Dictionary<string, HashSet<string>> followSets)
    {
        var afd = new AFD(gramatica, tablaLL1);
        
        // Construir alfabeto (terminales de la gramática)
        afd.Alfabeto = new HashSet<string>(tablaLL1.Terminales);
        
        // Crear estados basados en no terminales y producciones
        int estadoId = 0;
        var estadosPorNoTerminal = new Dictionary<string, EstadoAFD>();
        
        // Estado inicial (basado en el símbolo inicial de la gramática)
        var estadoInicial = new EstadoAFD(estadoId++, $"q0_{gramatica.SimboloInicial}");
        estadoInicial.EsEstadoInicial = true;
        estadosPorNoTerminal[gramatica.SimboloInicial] = estadoInicial;
        afd.Estados.Add(estadoInicial);
        afd.EstadoInicial = estadoInicial.Id;
        
        // Crear estados para cada no terminal
        foreach (var noTerminal in tablaLL1.NoTerminales)
        {
            if (noTerminal == gramatica.SimboloInicial) continue;
            
            var estado = new EstadoAFD(estadoId++, $"q{estadoId - 1}_{noTerminal}");
            estadosPorNoTerminal[noTerminal] = estado;
            afd.Estados.Add(estado);
        }
        
        // Estado de aceptación
        var estadoFinal = new EstadoAFD(estadoId++, "qF_Aceptacion");
        estadoFinal.EsEstadoFinal = true;
        afd.Estados.Add(estadoFinal);
        afd.EstadosFinales.Add(estadoFinal.Id);
        
        // Construir transiciones basadas en la tabla LL(1)
        foreach (var entrada in tablaLL1.Tabla)
        {
            var (noTerminal, terminal) = entrada.Key;
            var produccion = entrada.Value;
            
            if (!estadosPorNoTerminal.ContainsKey(noTerminal))
                continue;
            
            var estadoOrigen = estadosPorNoTerminal[noTerminal];
            
            // Determinar estado destino basado en la producción
            var primeraProduccion = produccion.Split('|')[0].Trim();
            var ladoDerecho = primeraProduccion.Split("→")[1].Trim();
            var simbolos = ladoDerecho.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            int estadoDestino;
            
            // Si la producción es epsilon, ir al estado final
            if (ladoDerecho == "ε")
            {
                estadoDestino = estadoFinal.Id;
            }
            // Si el primer símbolo es un no terminal, ir a su estado
            else if (simbolos.Length > 0 && estadosPorNoTerminal.ContainsKey(simbolos[0]))
            {
                estadoDestino = estadosPorNoTerminal[simbolos[0]].Id;
            }
            // Si es un terminal, permanecer en el estado actual o avanzar
            else
            {
                // Para simplificar, crear transición al mismo estado o siguiente
                estadoDestino = estadoOrigen.Id;
            }
            
            // Agregar transición
            if (!estadoOrigen.Transiciones.ContainsKey(terminal))
            {
                estadoOrigen.Transiciones[terminal] = estadoDestino;
                afd.MatrizTransicion[(estadoOrigen.Id, terminal)] = estadoDestino;
            }
        }
        
        // Agregar transiciones al estado final desde estados que puedan aceptar
        foreach (var noTerminal in tablaLL1.NoTerminales)
        {
            if (!followSets.ContainsKey(noTerminal)) continue;
            
            var estado = estadosPorNoTerminal[noTerminal];
            
            // Si el FOLLOW contiene $, puede ir al estado final
            if (followSets[noTerminal].Contains("$"))
            {
                estado.Transiciones["$"] = estadoFinal.Id;
                afd.MatrizTransicion[(estado.Id, "$")] = estadoFinal.Id;
            }
        }
        
        return afd;
    }

    /// <summary>
    /// Valida una cadena usando el AFD construido
    /// </summary>
    public static (bool valido, List<(int estadoOrigen, string simbolo, int estadoDestino)> traza) 
        ValidarCadena(AFD afd, List<Token> tokens)
    {
        var traza = new List<(int estadoOrigen, string simbolo, int estadoDestino)>();
        int estadoActual = afd.EstadoInicial;
        
        foreach (var token in tokens)
        {
            // Convertir tipo de token a símbolo del alfabeto
            var simbolo = ConvertirTokenASimbolo(token);
            
            // Buscar transición
            if (afd.MatrizTransicion.ContainsKey((estadoActual, simbolo)))
            {
                int estadoDestino = afd.MatrizTransicion[(estadoActual, simbolo)];
                traza.Add((estadoActual, simbolo, estadoDestino));
                estadoActual = estadoDestino;
            }
            else
            {
                // No hay transición válida, cadena rechazada
                return (false, traza);
            }
        }
        
        // Verificar si terminamos en un estado final
        bool esValido = afd.EstadosFinales.Contains(estadoActual);
        
        return (esValido, traza);
    }

    /// <summary>
    /// Convierte un token a su símbolo correspondiente en el alfabeto del AFD
    /// </summary>
    private static string ConvertirTokenASimbolo(Token token)
    {
        return token.Tipo switch
        {
            TipoToken.NUM => "NUM",
            TipoToken.ID => "ID",
            TipoToken.MAS => "+",
            TipoToken.MENOS => "-",
            TipoToken.MULT => "*",
            TipoToken.DIV => "/",
            TipoToken.MOD => "%",
            TipoToken.PAR_IZQ => "(",
            TipoToken.PAR_DER => ")",
            TipoToken.FIN => "$",
            _ => token.Valor
        };
    }

    /// <summary>
    /// Genera un reporte detallado del AFD
    /// </summary>
    public static string GenerarReporteAFD(AFD afd)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=".PadRight(100, '='));
        sb.AppendLine("AUTÓMATA FINITO DETERMINISTA (AFD)");
        sb.AppendLine("=".PadRight(100, '='));
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
        sb.AppendLine();
        
        sb.AppendLine("DEFINICIÓN FORMAL:");
        sb.AppendLine("-".PadRight(100, '-'));
        sb.AppendLine($"  AFD = (Q, Σ, δ, q₀, F)");
        sb.AppendLine();
        
        sb.AppendLine("COMPONENTES:");
        sb.AppendLine("-".PadRight(100, '-'));
        sb.AppendLine($"  Q (Estados): {{{string.Join(", ", afd.Estados.Select(e => e.Nombre))}}}");
        sb.AppendLine($"  |Q| = {afd.Estados.Count}");
        sb.AppendLine();
        sb.AppendLine($"  Σ (Alfabeto): {{{string.Join(", ", afd.Alfabeto.OrderBy(s => s))}}}");
        sb.AppendLine($"  |Σ| = {afd.Alfabeto.Count}");
        sb.AppendLine();
        
        var estadoInicialNombre = afd.Estados.FirstOrDefault(e => e.Id == afd.EstadoInicial)?.Nombre ?? "desconocido";
        sb.AppendLine($"  q₀ (Estado inicial): {estadoInicialNombre}");
        sb.AppendLine();
        
        var estadosFinalesNombres = afd.Estados.Where(e => afd.EstadosFinales.Contains(e.Id)).Select(e => e.Nombre);
        sb.AppendLine($"  F (Estados finales): {{{string.Join(", ", estadosFinalesNombres)}}}");
        sb.AppendLine();
        
        sb.AppendLine("ESTADOS:");
        sb.AppendLine("-".PadRight(100, '-'));
        foreach (var estado in afd.Estados.OrderBy(e => e.Id))
        {
            var marcas = new List<string>();
            if (estado.EsEstadoInicial) marcas.Add("INICIAL");
            if (estado.EsEstadoFinal) marcas.Add("FINAL");
            
            var marcasStr = marcas.Count > 0 ? $" [{string.Join(", ", marcas)}]" : "";
            sb.AppendLine($"  {estado.Id}: {estado.Nombre}{marcasStr}");
        }
        sb.AppendLine();
        
        sb.AppendLine("FUNCIÓN DE TRANSICIÓN δ:");
        sb.AppendLine("-".PadRight(100, '-'));
        sb.AppendLine($"  {"Estado",-25} {"Símbolo",-15} {"→",-5} {"Estado Destino",-25}");
        sb.AppendLine("  " + "-".PadRight(95, '-'));
        
        foreach (var transicion in afd.MatrizTransicion.OrderBy(t => t.Key.Item1).ThenBy(t => t.Key.Item2))
        {
            var (estadoId, simbolo) = transicion.Key;
            var estadoDestino = transicion.Value;
            
            var estadoNombre = afd.Estados.FirstOrDefault(e => e.Id == estadoId)?.Nombre ?? $"q{estadoId}";
            var destinoNombre = afd.Estados.FirstOrDefault(e => e.Id == estadoDestino)?.Nombre ?? $"q{estadoDestino}";
            
            sb.AppendLine($"  δ({estadoNombre,-20}, {simbolo,-10}) = {destinoNombre}");
        }
        sb.AppendLine();
        
        sb.AppendLine("MATRIZ DE TRANSICIÓN:");
        sb.AppendLine("-".PadRight(100, '-'));
        
        // Encabezado
        sb.Append("  δ".PadRight(25));
        foreach (var simbolo in afd.Alfabeto.OrderBy(s => s))
        {
            sb.Append($"| {simbolo,-10} ");
        }
        sb.AppendLine("|");
        sb.AppendLine("  " + "-".PadRight(95, '-'));
        
        // Filas
        foreach (var estado in afd.Estados.OrderBy(e => e.Id))
        {
            var marcador = estado.EsEstadoInicial ? "→" : (estado.EsEstadoFinal ? "*" : " ");
            sb.Append($"  {marcador} {estado.Nombre,-22}");
            
            foreach (var simbolo in afd.Alfabeto.OrderBy(s => s))
            {
                if (afd.MatrizTransicion.ContainsKey((estado.Id, simbolo)))
                {
                    var destino = afd.MatrizTransicion[(estado.Id, simbolo)];
                    var destinoNombre = afd.Estados.FirstOrDefault(e => e.Id == destino)?.Nombre ?? $"q{destino}";
                    sb.Append($"| {destinoNombre,-10} ");
                }
                else
                {
                    sb.Append($"| {"-",-10} ");
                }
            }
            sb.AppendLine("|");
        }
        sb.AppendLine("  " + "-".PadRight(95, '-'));
        sb.AppendLine("  Leyenda: → = Estado inicial, * = Estado final");
        sb.AppendLine();
        
        sb.AppendLine("GRAMÁTICA ASOCIADA:");
        sb.AppendLine("-".PadRight(100, '-'));
        foreach (var prod in afd.GramaticaOriginal.Producciones)
        {
            sb.AppendLine($"  {prod}");
        }
        sb.AppendLine();
        
        sb.AppendLine("ESTADÍSTICAS:");
        sb.AppendLine("-".PadRight(100, '-'));
        sb.AppendLine($"  Total de estados: {afd.Estados.Count}");
        sb.AppendLine($"  Total de transiciones: {afd.MatrizTransicion.Count}");
        sb.AppendLine($"  Símbolos del alfabeto: {afd.Alfabeto.Count}");
        sb.AppendLine($"  Estados finales: {afd.EstadosFinales.Count}");
        
        // Calcular densidad de transiciones
        int transicionesPosibles = afd.Estados.Count * afd.Alfabeto.Count;
        double densidad = transicionesPosibles > 0 
            ? (double)afd.MatrizTransicion.Count / transicionesPosibles * 100 
            : 0;
        sb.AppendLine($"  Densidad de transiciones: {densidad:F2}% ({afd.MatrizTransicion.Count}/{transicionesPosibles})");
        sb.AppendLine();
        
        sb.AppendLine("PROPIEDADES DEL AFD:");
        sb.AppendLine("-".PadRight(100, '-'));
        sb.AppendLine($"  ✓ Determinista: Sí (cada estado tiene máximo una transición por símbolo)");
        sb.AppendLine($"  ✓ Completo: {(densidad == 100 ? "Sí" : "No")} ({densidad:F0}% de transiciones definidas)");
        sb.AppendLine($"  ✓ Estados accesibles desde q₀: {ContarEstadosAccesibles(afd)}");
        sb.AppendLine();
        
        return sb.ToString();
    }

    /// <summary>
    /// Cuenta cuántos estados son accesibles desde el estado inicial
    /// </summary>
    private static int ContarEstadosAccesibles(AFD afd)
    {
        var visitados = new HashSet<int>();
        var cola = new Queue<int>();
        
        cola.Enqueue(afd.EstadoInicial);
        visitados.Add(afd.EstadoInicial);
        
        while (cola.Count > 0)
        {
            int estadoActual = cola.Dequeue();
            
            foreach (var transicion in afd.MatrizTransicion.Where(t => t.Key.Item1 == estadoActual))
            {
                int estadoDestino = transicion.Value;
                if (!visitados.Contains(estadoDestino))
                {
                    visitados.Add(estadoDestino);
                    cola.Enqueue(estadoDestino);
                }
            }
        }
        
        return visitados.Count;
    }

    /// <summary>
    /// Genera representación en formato DOT (Graphviz) para visualización
    /// </summary>
    public static string GenerarDOT(AFD afd)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("digraph AFD {");
        sb.AppendLine("  rankdir=LR;");
        sb.AppendLine("  node [shape=circle];");
        sb.AppendLine();
        
        // Nodo invisible para flecha inicial
        sb.AppendLine("  _start [style=invis];");
        
        // Estados finales con doble círculo
        foreach (var estadoFinal in afd.EstadosFinales)
        {
            var nombreEstado = afd.Estados.FirstOrDefault(e => e.Id == estadoFinal)?.Nombre ?? $"q{estadoFinal}";
            sb.AppendLine($"  \"{nombreEstado}\" [shape=doublecircle];");
        }
        sb.AppendLine();
        
        // Flecha al estado inicial
        var nombreInicial = afd.Estados.FirstOrDefault(e => e.Id == afd.EstadoInicial)?.Nombre ?? "q0";
        sb.AppendLine($"  _start -> \"{nombreInicial}\";");
        sb.AppendLine();
        
        // Transiciones
        var transicionesPorPar = new Dictionary<(string, string), List<string>>();
        
        foreach (var transicion in afd.MatrizTransicion.OrderBy(t => t.Key.Item1).ThenBy(t => t.Key.Item2))
        {
            var (estadoId, simbolo) = transicion.Key;
            var estadoDestino = transicion.Value;
            
            var origen = afd.Estados.FirstOrDefault(e => e.Id == estadoId)?.Nombre ?? $"q{estadoId}";
            var destino = afd.Estados.FirstOrDefault(e => e.Id == estadoDestino)?.Nombre ?? $"q{estadoDestino}";
            
            var par = (origen, destino);
            if (!transicionesPorPar.ContainsKey(par))
            {
                transicionesPorPar[par] = new List<string>();
            }
            transicionesPorPar[par].Add(simbolo);
        }
        
        foreach (var kvp in transicionesPorPar)
        {
            var (origen, destino) = kvp.Key;
            var simbolos = string.Join(", ", kvp.Value);
            sb.AppendLine($"  \"{origen}\" -> \"{destino}\" [label=\"{simbolos}\"];");
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }
}
