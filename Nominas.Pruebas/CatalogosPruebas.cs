using Nominas.Entidades;

namespace Nominas.Pruebas;

/// <summary>Puestos y clientes: validaciones y eliminación protegida (RN-16).</summary>
public class CatalogosPruebas : BaseDatosPrueba
{
    [Fact]
    public void Puesto_Crear_Valido()
    {
        Puestos.Crear(new Puesto { Nombre = "Soldador", Descripcion = "Estructuras metálicas" });
        Assert.Contains(Puestos.Listar(), p => p.Nombre == "Soldador");
    }

    [Fact]
    public void Puesto_NombreObligatorio()
    {
        DebeFallar(() => Puestos.Crear(new Puesto { Nombre = "  " }), "obligatorio");
    }

    [Fact]
    public void Puesto_NombreMasDe40Caracteres()
    {
        DebeFallar(() => Puestos.Crear(new Puesto { Nombre = new string('a', 41) }), "40");
    }

    [Fact]
    public void Puesto_NombreRepetido()
    {
        Puestos.Crear(new Puesto { Nombre = "Electricista" });
        DebeFallar(() => Puestos.Crear(new Puesto { Nombre = "Electricista" }), "Ya existe");
    }

    [Fact]
    public void Puesto_Editar_ConservaSuPropioNombre()
    {
        var puesto = new Puesto { Nombre = "Plomero" };
        Puestos.Crear(puesto);
        Recargar();

        Puestos.Editar(new Puesto { IdPuesto = puesto.IdPuesto, Nombre = "Plomero", Descripcion = "Nueva" });
        Recargar();
        Assert.Equal("Nueva", Puestos.ObtenerPorId(puesto.IdPuesto)!.Descripcion);
    }

    [Fact]
    public void Puesto_NoSeEliminaSiTieneEmpleados()
    {
        var empleado = NuevoEmpleado();
        DebeFallar(() => Puestos.Eliminar(empleado.IdPuesto), "empleados");
    }

    [Fact]
    public void Puesto_SeEliminaSiNadieLoUsa()
    {
        var puesto = NuevoPuesto("Pintor");
        Puestos.Eliminar(puesto.IdPuesto);
        Assert.Null(Puestos.ObtenerPorId(puesto.IdPuesto));
    }

    [Fact]
    public void Cliente_DomicilioObligatorio()
    {
        DebeFallar(() => Clientes.Crear(new Cliente { Nombre = "Constructora", Domicilio = "" }), "domicilio");
    }

    [Fact]
    public void Cliente_NoSeEliminaSiTieneObras()
    {
        var obra = NuevaObra();
        DebeFallar(() => Clientes.Eliminar(obra.IdCliente), "obras");
    }

    [Fact]
    public void Cliente_SeEliminaSiNoTieneObras()
    {
        var cliente = NuevoCliente("Cliente sin obras");
        Clientes.Eliminar(cliente.IdCliente);
        Assert.Null(Clientes.ObtenerPorId(cliente.IdCliente));
    }
}
