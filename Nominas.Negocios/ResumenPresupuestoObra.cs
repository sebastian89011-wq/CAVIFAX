namespace Nominas.Negocios;

/// <summary>Estado del presupuesto de mano de obra de una obra (RN-12).</summary>
public class ResumenPresupuestoObra
{
    public decimal Presupuesto { get; set; }
    /// <summary>Suma del costo bruto de las nóminas autorizadas o pagadas.</summary>
    public decimal Gastado { get; set; }
    public decimal Saldo { get; set; }
    /// <summary>Porcentaje del presupuesto ya gastado (entero, para la barra de avance).</summary>
    public int Porcentaje { get; set; }
    /// <summary>true si el saldo es menor o igual al límite de alerta de la obra.</summary>
    public bool Alerta { get; set; }
}
