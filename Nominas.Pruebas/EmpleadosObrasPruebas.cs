using Nominas.Entidades;

namespace Nominas.Pruebas;

/// <summary>Empleados (RN-01, RN-02, RN-03) y obras (RN-04, RN-05, RN-12).</summary>
public class EmpleadosObrasPruebas : BaseDatosPrueba
{
    private Empleado EmpleadoValido(int idPuesto, string curp = "abcd800101hbcxyz01", string nss = "12345678901")
    {
        return new Empleado
        {
            IdPuesto = idPuesto,
            Nombre = "Mario",
            ApellidoPaterno = "Ochoa",
            Telefono = "6469876543",
            Domicilio = "Calle 5",
            Curp = curp,
            Nss = nss,
            ClaveIne = "ochmar80010102h100",
            TipoContratacion = TipoContratacion.Permanente,
            FechaIngreso = new DateOnly(2025, 3, 1),
            SueldoDiario = 480,
            PagoHoraExtra = 80,
            Estatus = EstatusEmpleado.BajaDefinitiva   // el servicio debe ignorarlo y dejarlo activo
        };
    }

    // ---------------------------------------------------------------- Empleados

    [Fact]
    public void Empleado_Crear_QuedaActivoYConCurpEnMayusculas()
    {
        var puesto = NuevoPuesto();
        var empleado = EmpleadoValido(puesto.IdPuesto);
        Empleados.Crear(empleado);

        Assert.Equal(EstatusEmpleado.Activo, empleado.Estatus);
        Assert.Equal("ABCD800101HBCXYZ01", empleado.Curp);
    }

    [Fact]
    public void Empleado_TelefonoDebeTener10Digitos()
    {
        var e = EmpleadoValido(NuevoPuesto().IdPuesto);
        e.Telefono = "646-123";
        DebeFallar(() => Empleados.Crear(e), "teléfono");
    }

    [Fact]
    public void Empleado_CurpDe18Caracteres()
    {
        var e = EmpleadoValido(NuevoPuesto().IdPuesto, curp: "CORTA");
        DebeFallar(() => Empleados.Crear(e), "CURP");
    }

    [Fact]
    public void Empleado_CurpRepetida_RN01()
    {
        int idPuesto = NuevoPuesto().IdPuesto;
        Empleados.Crear(EmpleadoValido(idPuesto));
        DebeFallar(() => Empleados.Crear(EmpleadoValido(idPuesto, nss: "99999999999")), "CURP");
    }

    [Fact]
    public void Empleado_NssRepetido_RN01()
    {
        int idPuesto = NuevoPuesto().IdPuesto;
        Empleados.Crear(EmpleadoValido(idPuesto));
        DebeFallar(() => Empleados.Crear(EmpleadoValido(idPuesto, curp: "ZZZZ800101HBCXYZ99")), "NSS");
    }

    [Fact]
    public void Empleado_SueldoMayorACero_RN02()
    {
        var e = EmpleadoValido(NuevoPuesto().IdPuesto);
        e.SueldoDiario = 0;
        DebeFallar(() => Empleados.Crear(e), "sueldo");
    }

    [Fact]
    public void Empleado_PuestoInexistente()
    {
        DebeFallar(() => Empleados.Crear(EmpleadoValido(9999)), "puesto");
    }

    [Fact]
    public void Empleado_Buscar_PorVariasPalabrasSinImportarMayusculas()
    {
        NuevoEmpleado("Juan", "Perez");
        NuevoEmpleado("Luis", "Hernandez");

        var encontrados = Empleados.Buscar("JUAN perez", null);

        Assert.Single(encontrados);
        Assert.Equal("Juan", encontrados[0].Nombre);
    }

    [Fact]
    public void Empleado_Buscar_PorEstatus()
    {
        NuevoEmpleado("Activo", "Uno");
        NuevoEmpleado("Baja", "Dos", estatus: EstatusEmpleado.BajaTemporal);

        var bajas = Empleados.Buscar(null, EstatusEmpleado.BajaTemporal);

        Assert.Single(bajas);
        Assert.Equal("Baja", bajas[0].Nombre);
    }

    [Fact]
    public void Empleado_Buscar_PorcentajeSeBuscaComoTexto()
    {
        NuevoEmpleado();
        Assert.Empty(Empleados.Buscar("%", null));
    }

    // ---------------------------------------------------------------- Obras

    private Obra ObraValida(int idCliente, int idResponsable)
    {
        return new Obra
        {
            IdCliente = idCliente,
            IdResponsable = idResponsable,
            IdSupervisor = idResponsable,
            Nombre = "Bodega Pacífico",
            Ubicacion = "El Sauzal",
            FechaInicio = new DateOnly(2026, 11, 1),
            FechaFinProgramada = new DateOnly(2027, 2, 1),
            PresupuestoManoObra = 250000,
            LimiteAlertaSaldo = 40000,
            BonoTerminacionPorTrabajador = 1500,
            Estatus = EstatusObra.Terminada   // el servicio debe ignorarlo y dejarla activa
        };
    }

    [Fact]
    public void Obra_Crear_QuedaActivaYSinFechaReal()
    {
        var obra = ObraValida(NuevoCliente().IdCliente, NuevoEmpleado().IdEmpleado);
        obra.FechaFinReal = new DateOnly(2026, 12, 1);
        Obras.Crear(obra);

        Assert.Equal(EstatusObra.Activa, obra.Estatus);
        Assert.Null(obra.FechaFinReal);
    }

    [Fact]
    public void Obra_FinProgramadaAntesDelInicio_RN05()
    {
        var obra = ObraValida(NuevoCliente().IdCliente, NuevoEmpleado().IdEmpleado);
        obra.FechaFinProgramada = new DateOnly(2026, 10, 1);
        DebeFallar(() => Obras.Crear(obra), "fin programada");
    }

    [Fact]
    public void Obra_PresupuestoMayorACero_RN05()
    {
        var obra = ObraValida(NuevoCliente().IdCliente, NuevoEmpleado().IdEmpleado);
        obra.PresupuestoManoObra = 0;
        DebeFallar(() => Obras.Crear(obra), "presupuesto");
    }

    [Fact]
    public void Obra_LimiteDeAlertaNoMayorAlPresupuesto()
    {
        var obra = ObraValida(NuevoCliente().IdCliente, NuevoEmpleado().IdEmpleado);
        obra.LimiteAlertaSaldo = 300000;
        DebeFallar(() => Obras.Crear(obra), "límite");
    }

    [Fact]
    public void Obra_ResponsableDebeEstarActivo_RN04()
    {
        var inactivo = NuevoEmpleado(estatus: EstatusEmpleado.BajaDefinitiva);
        DebeFallar(() => Obras.Crear(ObraValida(NuevoCliente().IdCliente, inactivo.IdEmpleado)), "activo");
    }

    [Fact]
    public void Obra_TerminadaNecesitaFechaReal()
    {
        var obra = NuevaObra();
        Recargar();
        obra.Estatus = EstatusObra.Terminada;
        obra.FechaFinReal = null;
        DebeFallar(() => Obras.Editar(obra), "fecha de fin real");
    }

    [Fact]
    public void Obra_Buscar_PorNombreDelCliente()
    {
        NuevaObra("Casa Lomas", idCliente: NuevoCliente("Desarrollos del Norte").IdCliente);
        NuevaObra("Nave Sauzal", idCliente: NuevoCliente("Inmobiliaria Pacífico").IdCliente);

        var encontradas = Obras.Buscar("norte", null);

        Assert.Single(encontradas);
        Assert.Equal("Casa Lomas", encontradas[0].Nombre);
    }

    [Fact]
    public void Obra_Gastado_SoloCuentaNominasAutorizadas_RN12()
    {
        var obra = NuevaObra(presupuesto: 10000, limiteAlerta: 8000);
        var empleado = NuevoEmpleado(sueldo: 500);
        Asistio(empleado.IdEmpleado, obra.IdObra, Lunes, 5);

        int idNomina = Nominas.GenerarBorrador(Lunes);
        Assert.Equal(0m, Obras.Gastado(obra.IdObra));          // el borrador no cuenta

        Nominas.Autorizar(idNomina, 1, 2500);
        Assert.Equal(2500m, Obras.Gastado(obra.IdObra));

        var resumen = Obras.ObtenerResumen(obra);
        Assert.Equal(7500m, resumen.Saldo);
        Assert.Equal(25, resumen.Porcentaje);
        Assert.True(resumen.Alerta);                           // 7,500 ≤ 8,000
    }
}
