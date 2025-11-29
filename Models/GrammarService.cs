using System;

namespace ProyectoFinal.Models;

public static class ServicioGramatica
{
    public static Gramatica ObtenerGramaticaAritmeticaInicial()
    {
        // Gramática para expresiones aritméticas (E, T, F)
        // E -> E + T | E - T | T
        // T -> T * F | T / F | F
        // F -> ( E ) | NUM | ID
        var g = new Gramatica("E");

        g.Agregar("E", "E + T", "E - T", "T");
        g.Agregar("T", "T * F", "T / F", "F");
        g.Agregar("F", "( E )", "NUM", "ID");

        return g;
    }
}
