using Nominas.Entidades;

namespace Nominas.Negocios;

/// <summary>Lo que costó una obra en una nómina: una fila de la tabla de costos por semana.</summary>
public class CostoSemanaObra
{
    public Nomina Nomina { get; set; } = new();
    public int Trabajadores { get; set; }
    /// <summary>Días-hombre: suma de los días trabajados de todos los trabajadores.</summary>
    public int Dias { get; set; }
    public decimal Total { get; set; }
}
