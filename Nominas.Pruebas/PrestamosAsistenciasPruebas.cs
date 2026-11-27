using Nominas.Entidades;

namespace Nominas.Pruebas;

/// <summary>Préstamos (RN-14, RN-16) y asistencias (RN-06, RN-07, RN-08).</summary>
public class PrestamosAsistenciasPruebas : BaseDatosPrueba
{
    // ---------------------------------------------------------------- Préstamos

    private static Prestamo PrestamoNuevo(int idEmpleado, decimal monto = 3000, decimal abono = 500)
    {
        return new Prestamo
        {
            IdEmpleado = idEmpleado,
            Tipo = TipoPrestamo.Prestamo,
            Monto = monto,
            AbonoSemanal = abono,
            Fecha = new DateOnly(2026, 11, 20)
        };
    }

    [Fact]
    public void Prestamo_Crear_SaldoIgualAlMontoYActivo()
    {
        var prestamo = PrestamoNuevo(NuevoEmpleado().IdEmpleado);
        Prestamos.Crear(prestamo);

        Assert.Equal(3000m, prestamo.SaldoPendiente);
        Assert.Equal(EstatusPrestamo.Activo, prestamo.Estatus);
    }

    [Fact]
    public void Prestamo_SoloUnoActivoPorEmpleado_RN14()
    {
        var empleado = NuevoEmpleado();
        Prestamos.Crear(PrestamoNuevo(empleado.IdEmpleado));
        DebeFallar(() => Prestamos.Crear(PrestamoNuevo(empleado.IdEmpleado)), "RN-14");
    }

    [Fact]
    public void Prestamo_AbonoNoMayorAlMonto()
    {
        DebeFallar(() => Prestamos.Crear(PrestamoNuevo(NuevoEmpleado().IdEmpleado, monto: 500, abono: 800)), "abono");
    }

    [Fact]
    public void Anticipo_ElAbonoEsTodoElMonto()
    {
        var anticipo = PrestamoNuevo(NuevoEmpleado().IdEmpleado, monto: 1200, abono: 300);
        anticipo.Tipo = TipoPrestamo.Anticipo;

        Prestamos.Crear(anticipo);

        Assert.Equal(1200m, anticipo.AbonoSemanal);
        Assert.Equal(1200m, anticipo.SaldoPendiente);
    }

    [Fact]
    public void Anticipo_SinAbonoCapturado_SeGuardaConElMonto()
    {
        // El formulario no envía el abono de un anticipo
        var anticipo = PrestamoNuevo(NuevoEmpleado().IdEmpleado, monto: 800, abono: 0);
        anticipo.Tipo = TipoPrestamo.Anticipo;

        Prestamos.Crear(anticipo);

        Assert.Equal(800m, anticipo.AbonoSemanal);
    }

    [Fact]
    public void Anticipo_HastaUnaSemanaDeSueldo_SePermite()
    {
        // Sueldo de 500 diarios: una semana (lunes a sábado) son 3,000
        var anticipo = PrestamoNuevo(NuevoEmpleado(sueldo: 500).IdEmpleado, monto: 3000, abono: 0);
        anticipo.Tipo = TipoPrestamo.Anticipo;

        Prestamos.Crear(anticipo);

        Assert.Equal(EstatusPrestamo.Activo, anticipo.Estatus);
    }

    [Fact]
    public void Anticipo_MayorAUnaSemanaDeSueldo_Falla()
    {
        var anticipo = PrestamoNuevo(NuevoEmpleado(sueldo: 500).IdEmpleado, monto: 3000.01m, abono: 0);
        anticipo.Tipo = TipoPrestamo.Anticipo;

        DebeFallar(() => Prestamos.Crear(anticipo), "semana de sueldo");
    }

    [Fact]
    public void Prestamo_NoTieneLimiteDeUnaSemana()
    {
        // El límite es solo para anticipos: un préstamo se paga en varias semanas
        var prestamo = PrestamoNuevo(NuevoEmpleado(sueldo: 500).IdEmpleado, monto: 10000, abono: 500);

        Prestamos.Crear(prestamo);

        Assert.Equal(500m, prestamo.AbonoSemanal);
    }

    [Fact]
    public void Prestamo_SoloAEmpleadosActivos()
    {
        var baja = NuevoEmpleado(estatus: EstatusEmpleado.BajaTemporal);
        DebeFallar(() => Prestamos.Crear(PrestamoNuevo(baja.IdEmpleado)), "activos");
    }

    [Fact]
    public void Prestamo_EmpleadosDisponibles_ExcluyeAQuienYaDebe()
    {
        var conPrestamo = NuevoEmpleado("Con", "Prestamo");
        var libre = NuevoEmpleado("Libre", "Sindeuda");
        NuevoPrestamo(conPrestamo.IdEmpleado, 1000, 250);

        var disponibles = Prestamos.EmpleadosDisponibles();

        Assert.Contains(disponibles, e => e.IdEmpleado == libre.IdEmpleado);
        Assert.DoesNotContain(disponibles, e => e.IdEmpleado == conPrestamo.IdEmpleado);
    }

    [Fact]
    public void Prestamo_SeCancelaSiNoTieneAbonos_RN16()
    {
        var prestamo = NuevoPrestamo(NuevoEmpleado().IdEmpleado, 1000, 250);
        Prestamos.Cancelar(prestamo.IdPrestamo);
        Assert.Null(Prestamos.ObtenerPorId(prestamo.IdPrestamo));
    }

    [Fact]
    public void Prestamo_NoSeCancelaSiYaTieneAbonos_RN16()
    {
        var prestamo = NuevoPrestamo(NuevoEmpleado().IdEmpleado, 1000, 250);
        Db.AbonosPrestamo.Add(new AbonoPrestamo { IdPrestamo = prestamo.IdPrestamo, IdNomina = 1, MontoAbonado = 250, Fecha = Lunes });
        Db.SaveChanges();

        DebeFallar(() => Prestamos.Cancelar(prestamo.IdPrestamo), "abonos");
    }

    // ---------------------------------------------------------------- Asistencias

    [Fact]
    public void Asistencia_Guardar_MarcaQueAsistio()
    {
        var obra = NuevaObra();
        var empleado = NuevoEmpleado();

        Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, obra.IdObra, 2) });

        var guardada = Db.Asistencias.Single(a => a.IdEmpleado == empleado.IdEmpleado);
        Assert.True(guardada.Asistio);
        Assert.Equal(2m, guardada.HorasExtra);
    }

    [Fact]
    public void Asistencia_FaltaNuncaCapturada_NoSeGuarda()
    {
        var empleado = NuevoEmpleado();
        Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, null) });
        Assert.Empty(Db.Asistencias.Where(a => a.IdEmpleado == empleado.IdEmpleado));
    }

    [Fact]
    public void Asistencia_UnaPorEmpleadoYDia_SeActualiza_RN07()
    {
        var obra = NuevaObra();
        var empleado = NuevoEmpleado();

        Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, obra.IdObra) });
        Recargar();
        Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, null) });
        Recargar();

        var registros = Db.Asistencias.Where(a => a.IdEmpleado == empleado.IdEmpleado).ToList();
        Assert.Single(registros);
        Assert.False(registros[0].Asistio);
        Assert.Null(registros[0].IdObra);
    }

    [Fact]
    public void Asistencia_HorasExtraEnFalta_RN08()
    {
        var empleado = NuevoEmpleado();
        DebeFallar(() => Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, null, 2) }),
            "faltó");
    }

    [Fact]
    public void Asistencia_HorasExtraNegativas_RN08()
    {
        var obra = NuevaObra();
        var empleado = NuevoEmpleado();
        DebeFallar(() => Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, obra.IdObra, -1) }),
            "negativas");
    }

    [Fact]
    public void Asistencia_MasDe12HorasExtra()
    {
        var obra = NuevaObra();
        var empleado = NuevoEmpleado();
        DebeFallar(() => Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, obra.IdObra, 13) }),
            "12");
    }

    [Fact]
    public void Asistencia_EmpleadoInactivo_RN06()
    {
        var obra = NuevaObra();
        var baja = NuevoEmpleado(estatus: EstatusEmpleado.BajaTemporal);
        DebeFallar(() => Asistencias.GuardarSemana(new List<Asistencia> { Celda(baja.IdEmpleado, Lunes, obra.IdObra) }),
            "no está activo");
    }

    [Fact]
    public void Asistencia_ObraNoActiva_RN06()
    {
        var obra = NuevaObra(estatus: EstatusObra.Suspendida);
        var empleado = NuevoEmpleado();
        DebeFallar(() => Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, obra.IdObra) }),
            "no está activa");
    }

    [Fact]
    public void Asistencia_AntesDelInicioDeLaObra_RN06()
    {
        var obra = NuevaObra(inicio: new DateOnly(2026, 12, 1));
        var empleado = NuevoEmpleado();
        DebeFallar(() => Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes, obra.IdObra) }),
            "empieza");
    }

    [Fact]
    public void Asistencia_DespuesDeLaFechaProgramada_IgualQueElTrigger_RN06()
    {
        // La obra debía terminar el lunes; el martes ya no se puede registrar
        var obra = NuevaObra(finProgramada: Lunes);
        var empleado = NuevoEmpleado();
        DebeFallar(() => Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes.AddDays(1), obra.IdObra) }),
            "fecha de fin programada");
    }

    [Fact]
    public void Asistencia_SiUnaCeldaFalla_NoSeGuardaNinguna()
    {
        var obra = NuevaObra();
        var empleado = NuevoEmpleado();
        var celdas = new List<Asistencia>
        {
            Celda(empleado.IdEmpleado, Lunes, obra.IdObra),               // válida
            Celda(empleado.IdEmpleado, Lunes.AddDays(1), null, 3)         // inválida
        };

        Assert.Throws<Nominas.Negocios.ValidacionException>(() => Asistencias.GuardarSemana(celdas));
        Recargar();
        Assert.Empty(Db.Asistencias.Where(a => a.IdEmpleado == empleado.IdEmpleado));
    }

    [Fact]
    public void Asistencia_NoSeModificaSiLaNominaYaEstaAutorizada()
    {
        var obra = NuevaObra();
        var empleado = NuevoEmpleado();
        Asistio(empleado.IdEmpleado, obra.IdObra, Lunes, 2);
        int idNomina = Nominas.GenerarBorrador(Lunes);
        Nominas.Autorizar(idNomina, 1, 1000);

        DebeFallar(() => Asistencias.GuardarSemana(new List<Asistencia> { Celda(empleado.IdEmpleado, Lunes.AddDays(2), obra.IdObra) }),
            "autorizada");
    }
}
