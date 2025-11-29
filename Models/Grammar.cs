using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProyectoFinal.Models;

public class Produccion
{
    public string Izquierda { get; set; }
    public List<string> Derechas { get; set; } = new();

    public Produccion(string izquierda)
    {
        Izquierda = izquierda;
    }

    public override string ToString()
    {
        return $"{Izquierda} -> {string.Join(" | ", Derechas)}";
    }
}

public class Gramatica
{
    public string SimboloInicial { get; set; }
    public List<Produccion> Producciones { get; set; } = new();

    public Gramatica(string simboloInicial)
    {
        SimboloInicial = simboloInicial;
    }

    public void Agregar(string izquierda, params string[] derechas)
    {
        var p = new Produccion(izquierda);
        p.Derechas.AddRange(derechas);
        Producciones.Add(p);
    }

    public string ATexto()
    {
        var sb = new StringBuilder();
        sb.AppendLine("GRAMÁTICA INICIAL - G(L)");
        sb.AppendLine();
        foreach (var p in Producciones)
        {
            sb.AppendLine(p.ToString());
        }
        return sb.ToString();
    }
}
