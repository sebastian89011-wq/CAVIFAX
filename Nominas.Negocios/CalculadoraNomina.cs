namespace Nominas.Negocios;


public static class CalculadoraNomina
{
    public static decimal CalcularCostoBruto(int dias, decimal horasExtra,
        decimal sueldoDiario, decimal pagoHoraExtra, decimal bono)
    {
        return (dias * sueldoDiario) + (horasExtra * pagoHoraExtra) + bono;
    }

    public static decimal CalcularAbono(decimal totalBruto, decimal abonoSemanal, decimal saldoPendiente)
    {

        decimal cobrar = Math.Min(saldoPendiente, abonoSemanal);

        if (totalBruto <= cobrar)
        {
            return 0;
        }
        
           
         return cobrar;

        


    }

}