using Nominas.Negocios;

namespace Nominas.Pruebas;

/// <summary>RN-10 (costo bruto) y RN-14 (abono del préstamo, diferido si no alcanza).</summary>
public class CalculadoraNominaPruebas
{
    [Fact]
    public void CostoBruto_DiasMasHorasExtra()
    {
        // 5 días × $500 + 2 h × $85
        Assert.Equal(2670m, CalculadoraNomina.CalcularCostoBruto(5, 2, 500, 85, 0));
    }

    [Fact]
    public void CostoBruto_IncluyeBono()
    {
        Assert.Equal(3500m, CalculadoraNomina.CalcularCostoBruto(5, 0, 500, 85, 1000));
    }

    [Fact]
    public void CostoBruto_SoloBono_CuandoNoTrabajoEsaSemana()
    {
        Assert.Equal(800m, CalculadoraNomina.CalcularCostoBruto(0, 0, 500, 85, 800));
    }

    [Fact]
    public void Abono_Normal_CobraElAbonoSemanal()
    {
        Assert.Equal(500m, CalculadoraNomina.CalcularAbono(2670, 500, 2500));
    }

    [Fact]
    public void Abono_UltimoAbono_NoSuperaElSaldo()
    {
        Assert.Equal(300m, CalculadoraNomina.CalcularAbono(2670, 500, 300));
    }

    [Fact]
    public void Abono_SeDifiere_SiElBrutoNoAlcanza()
    {
        Assert.Equal(0m, CalculadoraNomina.CalcularAbono(350, 400, 800));
    }

    [Fact]
    public void Abono_SeDifiere_SiElNetoQuedariaEnCero()
    {
        Assert.Equal(0m, CalculadoraNomina.CalcularAbono(400, 400, 800));
    }
}
