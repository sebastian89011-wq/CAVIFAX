namespace Nominas.Entidades;

/// <summary>Cliente para el que se realizan obras.</summary>
public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Domicilio { get; set; } = string.Empty;
}
