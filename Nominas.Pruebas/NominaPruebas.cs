using Nominas.Entidades;

namespace Nominas.Pruebas;

/// <summary>
/// Ciclo completo de la nómina: borrador (RN-09, RN-10, RN-11), recalcular, eliminar,
/// autorizar (RN-14) y entrega de pagos (RN-13 y pregunta 9).
/// </summary>
public class NominaPruebas : BaseDatosPrueba
{
    // ---------------------------------------------------------------- Borrador

    [Fact]
    public void Borrador_CubreDeLunesASabado_AunqueSePidaConOtroDia()
    {
        var obra = NuevaObra();
        Asistio(NuevoEmpleado().IdEmpleado, obra.IdObra, Lunes, 1);

        int id = Nominas.GenerarBorrador(Lunes.AddDays(2));   // miércoles
        var nomina = Nominas.ObtenerPorId(id)!;

        Assert.Equal(Lunes, nomina.FechaInicio);
        Assert.Equal(Sabado, nomina.FechaFin);
        Assert.Equal(EstatusNomina.Borrador, nomina.Estatus);
    }

    [Fact]
    public void Borrador_CalculaCostoBrutoYPagoNeto_RN10()
    {
        var obra = NuevaObra();
        var juan = NuevoEmpleado(sueldo: 500, horaExtra: 85);
        NuevoPrestamo(juan.IdEmpleado, 3000, 500, saldo: 2500);
        Asistio(juan.IdEmpleado, obra.IdObra, Lunes, 5, horasExtraPrimerDia: 2);

        int id = Nominas.GenerarBorrador(Lunes);

        var detalle = Db.DetallesNomina.Single(d => d.IdNomina == id);
        Assert.Equal(5, detalle.DiasTrabajados);
        Assert.Equal(2670m, detalle.CostoBruto);

        var pago = PagoDe(id, juan.IdEmpleado);
        Assert.Equal(2670m, pago.TotalBruto);
        Assert.Equal(500m, pago.TotalDescuentos);
        Assert.Equal(2170m, pago.TotalNeto);
        Assert.Equal(EstatusPago.Pendiente, pago.Estatus);
    }

    [Fact]
    public void Borrador_UnDetallePorObra_YUnPagoPorEmpleado()
    {
        var obraA = NuevaObra("Obra A");
        var obraB = NuevaObra("Obra B");
        var empleado = NuevoEmpleado(sueldo: 400);
        Asistio(empleado.IdEmpleado, obraA.IdObra, Lunes, 2);
        Asistio(empleado.IdEmpleado, obraB.IdObra, Lunes.AddDays(2), 3);

        int id = Nominas.GenerarBorrador(Lunes);

        Assert.Equal(2, Db.DetallesNomina.Count(d => d.IdNomina == id && d.IdEmpleado == empleado.IdEmpleado));
        Assert.Equal(2000m, PagoDe(id, empleado.IdEmpleado).TotalBruto);
    }

    [Fact]
    public void Borrador_AbonoDiferido_SiElBrutoNoAlcanza()
    {
        var obra = NuevaObra();
        var luis = NuevoEmpleado(sueldo: 350);
        NuevoPrestamo(luis.IdEmpleado, 800, 400);
        Asistio(luis.IdEmpleado, obra.IdObra, Lunes, 1);   // bruto 350 < abono 400

        int id = Nominas.GenerarBorrador(Lunes);

        var pago = PagoDe(id, luis.IdEmpleado);
        Assert.Equal(0m, pago.TotalDescuentos);
        Assert.Equal(350m, pago.TotalNeto);
    }

    [Fact]
    public void Borrador_UnaNominaPorSemana_RN09()
    {
        var obra = NuevaObra();
        Asistio(NuevoEmpleado().IdEmpleado, obra.IdObra, Lunes, 1);

        int primero = Nominas.GenerarBorrador(Lunes);
        int segundo = Nominas.GenerarBorrador(Sabado);

        Assert.Equal(primero, segundo);
        Assert.Single(Nominas.Listar());
    }

    [Fact]
    public void Borrador_SinAsistencias_NoCreaNada()
    {
        DebeFallar(() => Nominas.GenerarBorrador(Lunes), "No hay asistencias");
        Recargar();
        Assert.Empty(Nominas.Listar());
    }

    [Fact]
    public void Borrador_BonoPorTerminarATiempo_RN11()
    {
        // La obra terminó el miércoles de la semana, antes de su fecha programada
        var obra = NuevaObra(bono: 800, estatus: EstatusObra.Terminada,
            finProgramada: Sabado, finReal: Lunes.AddDays(2));
        var trabajoEstaSemana = NuevoEmpleado("Juan", "Perez", sueldo: 500);
        var trabajoAntes = NuevoEmpleado("Miguel", "Torres", sueldo: 520);
        Asistio(trabajoEstaSemana.IdEmpleado, obra.IdObra, Lunes, 2);
        Asistio(trabajoAntes.IdEmpleado, obra.IdObra, new DateOnly(2026, 11, 2), 1);  // otra semana

        int id = Nominas.GenerarBorrador(Lunes);

        var conDias = Db.DetallesNomina.Single(d => d.IdNomina == id && d.IdEmpleado == trabajoEstaSemana.IdEmpleado);
        Assert.Equal(800m, conDias.Bono);
        Assert.Equal(1800m, conDias.CostoBruto);   // 2 × 500 + 800

        var soloBono = Db.DetallesNomina.Single(d => d.IdNomina == id && d.IdEmpleado == trabajoAntes.IdEmpleado);
        Assert.Equal(0, soloBono.DiasTrabajados);
        Assert.Equal(800m, soloBono.CostoBruto);
    }

    [Fact]
    public void Borrador_SinBono_SiLaObraTerminoTarde_RN11()
    {
        var obra = NuevaObra(bono: 800, estatus: EstatusObra.Terminada,
            finProgramada: Lunes, finReal: Lunes.AddDays(2));
        var empleado = NuevoEmpleado();
        Asistio(empleado.IdEmpleado, obra.IdObra, Lunes, 1);

        int id = Nominas.GenerarBorrador(Lunes);

        Assert.Equal(0m, Db.DetallesNomina.Single(d => d.IdNomina == id).Bono);
    }

    // ---------------------------------------------------------------- Recalcular y eliminar

    [Fact]
    public void Recalcular_TomaLasAsistenciasNuevas()
    {
        var obra = NuevaObra();
        var pedro = NuevoEmpleado(sueldo: 360, horaExtra: 60);
        Asistio(pedro.IdEmpleado, obra.IdObra, Lunes, 2);
        int id = Nominas.GenerarBorrador(Lunes);

        Asistio(pedro.IdEmpleado, obra.IdObra, Sabado, 1, horasExtraPrimerDia: 1);
        Nominas.Recalcular(id);
        Recargar();

        Assert.Equal(1140m, PagoDe(id, pedro.IdEmpleado).TotalBruto);   // 3 × 360 + 1 × 60
        Assert.Single(Db.PagosEmpleado.Where(p => p.IdNomina == id));
    }

    [Fact]
    public void Recalcular_NoSePermiteEnNominaAutorizada()
    {
        var obra = NuevaObra();
        Asistio(NuevoEmpleado().IdEmpleado, obra.IdObra, Lunes, 1);
        int id = Nominas.GenerarBorrador(Lunes);
        Nominas.Autorizar(id, 1, 500);

        DebeFallar(() => Nominas.Recalcular(id), "borrador");
    }

    [Fact]
    public void EliminarBorrador_BorraNominaDetallesYPagos()
    {
        var obra = NuevaObra();
        Asistio(NuevoEmpleado().IdEmpleado, obra.IdObra, Lunes, 1);
        int id = Nominas.GenerarBorrador(Lunes);

        Nominas.EliminarBorrador(id);
        Recargar();

        Assert.Null(Nominas.ObtenerPorId(id));
        Assert.Empty(Db.DetallesNomina.Where(d => d.IdNomina == id));
        Assert.Empty(Db.PagosEmpleado.Where(p => p.IdNomina == id));
    }

    [Fact]
    public void EliminarBorrador_NoSePermiteEnNominaAutorizada_RN16()
    {
        var obra = NuevaObra();
        Asistio(NuevoEmpleado().IdEmpleado, obra.IdObra, Lunes, 1);
        int id = Nominas.GenerarBorrador(Lunes);
        Nominas.Autorizar(id, 1, 500);

        DebeFallar(() => Nominas.EliminarBorrador(id), "borrador");
    }

    // ---------------------------------------------------------------- Autorizar

    [Fact]
    public void Autorizar_CambiaEstatusYGuardaEfectivoYUsuario()
    {
        var obra = NuevaObra();
        Asistio(NuevoEmpleado().IdEmpleado, obra.IdObra, Lunes, 1);
        int id = Nominas.GenerarBorrador(Lunes);

        Nominas.Autorizar(id, 7, 4000);
        Recargar();

        var nomina = Nominas.ObtenerPorId(id)!;
        Assert.Equal(EstatusNomina.Autorizada, nomina.Estatus);
        Assert.Equal(4000m, nomina.EfectivoRecibido);
        Assert.Equal(7, nomina.IdUsuarioAutoriza);
        Assert.NotNull(nomina.FechaAutorizacion);
    }

    [Fact]
    public void Autorizar_CreaAbonoYBajaElSaldo_RN14()
    {
        var obra = NuevaObra();
        var juan = NuevoEmpleado(sueldo: 500);
        var prestamo = NuevoPrestamo(juan.IdEmpleado, 3000, 500, saldo: 2500);
        Asistio(juan.IdEmpleado, obra.IdObra, Lunes, 5);
        int id = Nominas.GenerarBorrador(Lunes);

        Nominas.Autorizar(id, 1, 2000);
        Recargar();

        var abono = Db.AbonosPrestamo.Single(a => a.IdNomina == id);
        Assert.Equal(prestamo.IdPrestamo, abono.IdPrestamo);
        Assert.Equal(500m, abono.MontoAbonado);

        var actualizado = Prestamos.ObtenerPorId(prestamo.IdPrestamo)!;
        Assert.Equal(2000m, actualizado.SaldoPendiente);
        Assert.Equal(EstatusPrestamo.Activo, actualizado.Estatus);
    }

    [Fact]
    public void Autorizar_LiquidaElPrestamoQueLlegaACero()
    {
        var obra = NuevaObra();
        var luis = NuevoEmpleado(sueldo: 350);
        var prestamo = NuevoPrestamo(luis.IdEmpleado, 800, 400, saldo: 400);
        Asistio(luis.IdEmpleado, obra.IdObra, Lunes, 3);   // bruto 1,050
        int id = Nominas.GenerarBorrador(Lunes);

        Nominas.Autorizar(id, 1, 1000);
        Recargar();

        var liquidado = Prestamos.ObtenerPorId(prestamo.IdPrestamo)!;
        Assert.Equal(0m, liquidado.SaldoPendiente);
        Assert.Equal(EstatusPrestamo.Liquidado, liquidado.Estatus);
    }

    [Fact]
    public void Autorizar_DosVeces_Falla()
    {
        var obra = NuevaObra();
        Asistio(NuevoEmpleado().IdEmpleado, obra.IdObra, Lunes, 1);
        int id = Nominas.GenerarBorrador(Lunes);
        Nominas.Autorizar(id, 1, 500);

        DebeFallar(() => Nominas.Autorizar(id, 1, 500), "borrador");
    }

    [Fact]
    public void Autorizar_NominaInexistente_Falla()
    {
        DebeFallar(() => Nominas.Autorizar(999, 1, 0), "no existe");
    }

    [Fact]
    public void Autorizar_DescuentoSinPrestamoActivo_Falla()
    {
        var obra = NuevaObra();
        var juan = NuevoEmpleado(sueldo: 500);
        var prestamo = NuevoPrestamo(juan.IdEmpleado, 1000, 500);
        Asistio(juan.IdEmpleado, obra.IdObra, Lunes, 5);
        int id = Nominas.GenerarBorrador(Lunes);
        Prestamos.Cancelar(prestamo.IdPrestamo);   // el borrador ya calculó el descuento

        DebeFallar(() => Nominas.Autorizar(id, 1, 2000), "préstamo activo");
    }

    // ---------------------------------------------------------------- Entrega de pagos

    /// <summary>Nómina autorizada con dos empleados; devuelve (idNomina, empleado1, empleado2).</summary>
    private (int id, Empleado uno, Empleado dos) NominaAutorizadaConDosPagos()
    {
        var obra = NuevaObra();
        var uno = NuevoEmpleado("Juan", "Perez", sueldo: 500);
        var dos = NuevoEmpleado("Pedro", "Sanchez", sueldo: 360);
        Asistio(uno.IdEmpleado, obra.IdObra, Lunes, 2);
        Asistio(dos.IdEmpleado, obra.IdObra, Lunes, 2);
        int id = Nominas.GenerarBorrador(Lunes);
        Nominas.Autorizar(id, 1, 1720);
        return (id, uno, dos);
    }

    [Fact]
    public void Entregar_MarcaFechaYSemanaDeEntrega_RN13()
    {
        var (id, uno, _) = NominaAutorizadaConDosPagos();
        int idPago = PagoDe(id, uno.IdEmpleado).IdPagoEmpleado;

        Nominas.MarcarPagoEntregado(idPago, id);
        Recargar();

        var pago = PagoDe(id, uno.IdEmpleado);
        Assert.Equal(EstatusPago.Entregado, pago.Estatus);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), pago.FechaPago);
        Assert.Equal(id, pago.IdNominaPago);
        Assert.Equal(EstatusNomina.Autorizada, Nominas.ObtenerPorId(id)!.Estatus);   // todavía falta uno
    }

    [Fact]
    public void Entregar_ElUltimoPago_PasaLaNominaAPagada()
    {
        var (id, uno, dos) = NominaAutorizadaConDosPagos();
        Nominas.MarcarPagoEntregado(PagoDe(id, uno.IdEmpleado).IdPagoEmpleado, id);
        Nominas.MarcarPagoEntregado(PagoDe(id, dos.IdEmpleado).IdPagoEmpleado, id);
        Recargar();

        Assert.Equal(EstatusNomina.Pagada, Nominas.ObtenerPorId(id)!.Estatus);
    }

    [Fact]
    public void Entregar_DosVeces_Falla()
    {
        var (id, uno, _) = NominaAutorizadaConDosPagos();
        int idPago = PagoDe(id, uno.IdEmpleado).IdPagoEmpleado;
        Nominas.MarcarPagoEntregado(idPago, id);

        DebeFallar(() => Nominas.MarcarPagoEntregado(idPago, id), "ya fue entregado");
    }

    [Fact]
    public void Entregar_PagoDeUnBorrador_Falla()
    {
        var obra = NuevaObra();
        var empleado = NuevoEmpleado();
        Asistio(empleado.IdEmpleado, obra.IdObra, Lunes, 1);
        int id = Nominas.GenerarBorrador(Lunes);

        DebeFallar(() => Nominas.MarcarPagoEntregado(PagoDe(id, empleado.IdEmpleado).IdPagoEmpleado, id), "sin autorizar");
    }

    [Fact]
    public void Entregar_PagoInexistente_Falla()
    {
        DebeFallar(() => Nominas.MarcarPagoEntregado(999, 1), "no existe");
    }

    [Fact]
    public void Entregar_EnUnaNominaInexistente_Falla()
    {
        var (id, uno, _) = NominaAutorizadaConDosPagos();
        DebeFallar(() => Nominas.MarcarPagoEntregado(PagoDe(id, uno.IdEmpleado).IdPagoEmpleado, 999), "no existe");
    }

    [Fact]
    public void PagoPendiente_ApareceEnLaSemanaSiguiente_RN13()
    {
        var (id, uno, dos) = NominaAutorizadaConDosPagos();
        Nominas.MarcarPagoEntregado(PagoDe(id, uno.IdEmpleado).IdPagoEmpleado, id);

        // Semana siguiente
        var obra = NuevaObra("Otra obra");
        Asistio(uno.IdEmpleado, obra.IdObra, Lunes.AddDays(7), 1);
        int siguiente = Nominas.GenerarBorrador(Lunes.AddDays(7));

        var pendientes = Nominas.PagosPendientesAnteriores(siguiente);
        Assert.Single(pendientes);
        Assert.Equal(dos.IdEmpleado, pendientes[0].IdEmpleado);

        // Efectivo sugerido = neto de esta semana + lo pendiente
        decimal netoSemana = Nominas.TotalNeto(siguiente);
        Assert.Equal(netoSemana + 720m, Nominas.EfectivoSugerido(siguiente));

        // Se entrega en la semana siguiente y la primera nómina queda pagada
        Nominas.MarcarPagoEntregado(pendientes[0].IdPagoEmpleado, siguiente);
        Recargar();
        Assert.Equal(siguiente, PagoDe(id, dos.IdEmpleado).IdNominaPago);
        Assert.Equal(EstatusNomina.Pagada, Nominas.ObtenerPorId(id)!.Estatus);
    }

    [Fact]
    public void PagosPorEntregar_NoIncluyeLosDelBorrador()
    {
        var obra = NuevaObra();
        Asistio(NuevoEmpleado().IdEmpleado, obra.IdObra, Lunes, 1);
        int id = Nominas.GenerarBorrador(Lunes);
        Assert.Empty(Nominas.PagosPorEntregar());

        Nominas.Autorizar(id, 1, 500);
        Assert.Single(Nominas.PagosPorEntregar());
    }
}
