using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class NominaService
{
    private readonly NominasDbContext _db;

    public NominaService(NominasDbContext db)
    {
        _db = db;
    }

    // ---------------------------------------------------------------- Consultas

    /// <summary>Todas las nóminas, de la más reciente a la más antigua.</summary>
    public List<Nomina> Listar()
    {
        return _db.Nominas.OrderByDescending(n => n.FechaInicio).ToList();
    }

    public Nomina? ObtenerPorId(int id)
    {
        return _db.Nominas.Find(id);
    }

    /// <summary>Texto de la semana, por ejemplo "16/11 al 21/11/2026".</summary>
    public static string Semana(Nomina n)
    {
        return $"{n.FechaInicio:dd/MM} al {n.FechaFin:dd/MM/yyyy}";
    }

    /// <summary>Σ TotalNeto de los pagos de una nómina.</summary>
    public decimal TotalNeto(int idNomina)
    {
        return _db.PagosEmpleado.Where(p => p.IdNomina == idNomina).Sum(p => p.TotalNeto);
    }

    /// <summary>Cuántos pagos de una nómina siguen sin entregarse.</summary>
    public int ContarPagosPendientes(int idNomina)
    {
        return _db.PagosEmpleado.Count(p => p.IdNomina == idNomina && p.Estatus == EstatusPago.Pendiente);
    }

    /// <summary>
    /// RN-13: pagos pendientes de nóminas anteriores a esta que ya no están en borrador.
    /// </summary>
    public List<PagoEmpleado> PagosPendientesAnteriores(int idNomina)
    {
        var nomina = _db.Nominas.Find(idNomina);
        if (nomina == null)
        {
            return new List<PagoEmpleado>();
        }

        var idsAnteriores = _db.Nominas
            .Where(n => n.FechaInicio < nomina.FechaInicio && n.Estatus != EstatusNomina.Borrador)
            .Select(n => n.IdNomina)
            .ToList();

        return _db.PagosEmpleado
            .Where(p => idsAnteriores.Contains(p.IdNomina) && p.Estatus == EstatusPago.Pendiente)
            .ToList();
    }

    /// <summary>Efectivo sugerido = neto de esta nómina + pendientes de semanas anteriores.</summary>
    public decimal EfectivoSugerido(int idNomina)
    {
        return TotalNeto(idNomina) + PagosPendientesAnteriores(idNomina).Sum(p => p.TotalNeto);
    }

    /// <summary>Todo lo que muestra la página Detalle: nómina, detalles, pagos, pendientes, nombres y totales.</summary>
    public NominaDetalle? ObtenerDetalle(int idNomina)
    {
        var nomina = _db.Nominas.Find(idNomina);
        if (nomina == null)
        {
            return null;
        }

        var resultado = new NominaDetalle
        {
            Nomina = nomina,
            Detalles = _db.DetallesNomina.Where(d => d.IdNomina == idNomina)
                .OrderBy(d => d.IdObra).ThenBy(d => d.IdEmpleado).ToList(),
            Pagos = _db.PagosEmpleado.Where(p => p.IdNomina == idNomina)
                .OrderBy(p => p.IdEmpleado).ToList(),
            PendientesAnteriores = PagosPendientesAnteriores(idNomina)
        };

        resultado.NombresEmpleado = _db.Empleados.ToList()
            .ToDictionary(e => e.IdEmpleado, e => EmpleadoService.NombreCompleto(e));
        resultado.NombresObra = _db.Obras.ToDictionary(o => o.IdObra, o => o.Nombre);

        var idsSemanas = resultado.PendientesAnteriores.Select(p => p.IdNomina).Distinct().ToList();
        resultado.Semanas = _db.Nominas.Where(n => idsSemanas.Contains(n.IdNomina)).ToList()
            .ToDictionary(n => n.IdNomina, n => Semana(n));

        return resultado;
    }

    /// <summary>
    /// Temporal mientras no hay inicio de sesión: el primer usuario activo, o null si no hay.
    /// </summary>
    public int? PrimerUsuarioActivo()
    {
        return _db.Usuarios.Where(u => u.Activo).OrderBy(u => u.IdUsuario)
            .Select(u => (int?)u.IdUsuario).FirstOrDefault();
    }

    // ---------------------------------------------------------------- Borrador

    /// <summary>
    /// Crea el borrador de la semana (lunes a sábado) que contiene la fecha dada.
    /// RN-09: si ya hay una nómina para esa semana, devuelve la existente.
    /// </summary>
    public int GenerarBorrador(DateOnly fechaCualquieraDeLaSemana)
    {
        // En .NET el domingo es 0; se retrocede hasta el lunes
        int diasDesdeLunes = ((int)fechaCualquieraDeLaSemana.DayOfWeek + 6) % 7;
        DateOnly lunes = fechaCualquieraDeLaSemana.AddDays(-diasDesdeLunes);
        DateOnly sabado = lunes.AddDays(5);

        var existente = _db.Nominas.FirstOrDefault(n => n.FechaInicio == lunes);
        if (existente != null)
        {
            return existente.IdNomina;
        }

        // Las entidades no tienen propiedades de navegación, así que el IdNomina
        // solo se conoce después de guardar la nómina. Por eso hay dos SaveChanges,
        // dentro de una transacción: si algo falla, no queda una nómina a medias.
        using var transaccion = _db.Database.BeginTransaction();

        var nomina = new Nomina
        {
            FechaInicio = lunes,
            FechaFin = sabado,
            Estatus = EstatusNomina.Borrador,
            IdUsuarioAutoriza = null,
            FechaAutorizacion = null,
            EfectivoRecibido = 0
        };
        _db.Nominas.Add(nomina);
        _db.SaveChanges();

        int detallesCreados = CalcularDetallesYPagos(nomina);
        if (detallesCreados == 0)
        {
            transaccion.Rollback();
            throw new ValidacionException(
                $"No hay asistencias capturadas en la semana del {Semana(nomina)}. Captúralas primero en Asistencias.");
        }

        _db.SaveChanges();
        transaccion.Commit();
        return nomina.IdNomina;
    }

    /// <summary>Vuelve a calcular un borrador con las asistencias y tarifas actuales.</summary>
    public void Recalcular(int idNomina)
    {
        var nomina = _db.Nominas.Find(idNomina);
        if (nomina == null)
        {
            throw new ValidacionException("La nómina no existe.");
        }
        if (nomina.Estatus != EstatusNomina.Borrador)
        {
            throw new ValidacionException("Solo se puede recalcular una nómina en borrador.");
        }

        _db.DetallesNomina.RemoveRange(_db.DetallesNomina.Where(d => d.IdNomina == idNomina));
        _db.PagosEmpleado.RemoveRange(_db.PagosEmpleado.Where(p => p.IdNomina == idNomina));

        CalcularDetallesYPagos(nomina);

        // Borrar y volver a crear va en un solo SaveChanges (EF lo hace en una transacción)
        _db.SaveChanges();
    }

    /// <summary>
    /// Borra un borrador con sus detalles y pagos. Las nóminas autorizadas o pagadas no se borran (RN-16).
    /// </summary>
    public void EliminarBorrador(int idNomina)
    {
        var nomina = _db.Nominas.Find(idNomina);
        if (nomina == null)
            throw new ValidacionException("La nómina no existe.");

        if (nomina.Estatus != EstatusNomina.Borrador)
            throw new ValidacionException("No se puede eliminar: solo se eliminan nóminas en borrador.");

        // Un pago de otra semana pudo haberse entregado en esta nómina (IdNominaPago)
        if (_db.PagosEmpleado.Any(p => p.IdNominaPago == idNomina && p.IdNomina != idNomina))
            throw new ValidacionException("No se puede eliminar: en esta semana se entregaron pagos de semanas anteriores.");

        // EF no conoce la relación (no hay propiedades de navegación), así que no sabe
        // que debe borrar primero los hijos. Se guardan en dos pasos, dentro de una transacción.
        using var transaccion = _db.Database.BeginTransaction();

        _db.DetallesNomina.RemoveRange(_db.DetallesNomina.Where(d => d.IdNomina == idNomina));
        _db.PagosEmpleado.RemoveRange(_db.PagosEmpleado.Where(p => p.IdNomina == idNomina));
        _db.SaveChanges();

        _db.Nominas.Remove(nomina);
        _db.SaveChanges();
        transaccion.Commit();
    }

    /// <summary>
    /// El cálculo compartido por GenerarBorrador y Recalcular. Agrega al contexto
    /// (sin guardar) los DetalleNomina y PagoEmpleado de la nómina. Devuelve cuántos detalles creó.
    /// </summary>
    private int CalcularDetallesYPagos(Nomina nomina)
    {
        var empleados = _db.Empleados.ToDictionary(e => e.IdEmpleado);

        // 1. Días y horas extra por empleado y obra (solo días en que asistió)
        var detalles = _db.Asistencias
            .Where(a => a.Fecha >= nomina.FechaInicio && a.Fecha <= nomina.FechaFin
                        && a.Asistio && a.IdObra != null)
            .ToList()
            .GroupBy(a => new { a.IdEmpleado, IdObra = a.IdObra!.Value })
            .Select(g => new DetalleNomina
            {
                IdNomina = nomina.IdNomina,
                IdEmpleado = g.Key.IdEmpleado,
                IdObra = g.Key.IdObra,
                DiasTrabajados = (byte)g.Count(),
                HorasExtra = g.Sum(a => a.HorasExtra),
                // Tarifas actuales del empleado; quedan fijas en el detalle
                SueldoDiarioAplicado = empleados[g.Key.IdEmpleado].SueldoDiario,
                PagoHoraExtraAplicado = empleados[g.Key.IdEmpleado].PagoHoraExtra,
                Bono = 0
            })
            .ToList();

        // 2. Bono de terminación (RN-11)
        AplicarBonos(nomina, detalles, empleados);

        // 3. Costo bruto de cada detalle (RN-10)
        foreach (var d in detalles)
        {
            d.CostoBruto = CalculadoraNomina.CalcularCostoBruto(
                d.DiasTrabajados, d.HorasExtra, d.SueldoDiarioAplicado, d.PagoHoraExtraAplicado, d.Bono);
        }
        _db.DetallesNomina.AddRange(detalles);

        // 4. Un pago por empleado: bruto − abono del préstamo activo = neto
        foreach (var grupo in detalles.GroupBy(d => d.IdEmpleado))
        {
            decimal totalBruto = grupo.Sum(d => d.CostoBruto);

            var prestamo = _db.Prestamos
                .FirstOrDefault(p => p.IdEmpleado == grupo.Key && p.Estatus == EstatusPrestamo.Activo);

            decimal descuento = prestamo == null
                ? 0
                : CalculadoraNomina.CalcularAbono(totalBruto, prestamo.AbonoSemanal, prestamo.SaldoPendiente);

            _db.PagosEmpleado.Add(new PagoEmpleado
            {
                IdNomina = nomina.IdNomina,
                IdEmpleado = grupo.Key,
                TotalBruto = totalBruto,
                TotalDescuentos = descuento,
                TotalNeto = totalBruto - descuento,
                Estatus = EstatusPago.Pendiente,
                FechaPago = null,
                IdNominaPago = null
            });
        }

        // El préstamo NO se toca aquí: los abonos se crean al autorizar
        return detalles.Count;
    }

    /// <summary>
    /// Si una obra terminó a tiempo (FechaFinReal ≤ FechaFinProgramada) durante esta semana,
    /// cada empleado que haya asistido alguna vez a esa obra recibe el bono una sola vez.
    /// </summary>
    private void AplicarBonos(Nomina nomina, List<DetalleNomina> detalles, Dictionary<int, Empleado> empleados)
    {
        var obrasConBono = _db.Obras
            .Where(o => o.Estatus == EstatusObra.Terminada
                        && o.FechaFinReal != null
                        && o.FechaFinReal <= o.FechaFinProgramada
                        && o.FechaFinReal >= nomina.FechaInicio
                        && o.FechaFinReal <= nomina.FechaFin
                        && o.BonoTerminacionPorTrabajador > 0)
            .ToList();

        foreach (var obra in obrasConBono)
        {
            // Quienes trabajaron en la obra, en cualquier fecha
            var idsEmpleados = _db.Asistencias
                .Where(a => a.IdObra == obra.IdObra && a.Asistio)
                .Select(a => a.IdEmpleado)
                .Distinct()
                .ToList();

            foreach (int idEmpleado in idsEmpleados)
            {
                // Una sola vez por empleado y obra: se ignora la nómina actual (en Recalcular
                // sus detalles viejos todavía no se borran de la BD)
                bool yaLoRecibio = _db.DetallesNomina.Any(d => d.IdEmpleado == idEmpleado && d.IdObra == obra.IdObra
                                                               && d.Bono > 0 && d.IdNomina != nomina.IdNomina);
                if (yaLoRecibio)
                {
                    continue;
                }

                var detalle = detalles.FirstOrDefault(d => d.IdEmpleado == idEmpleado && d.IdObra == obra.IdObra);
                if (detalle == null)
                {
                    // No trabajó esta semana en la obra: detalle con 0 días y solo el bono
                    var empleado = empleados[idEmpleado];
                    detalle = new DetalleNomina
                    {
                        IdNomina = nomina.IdNomina,
                        IdEmpleado = idEmpleado,
                        IdObra = obra.IdObra,
                        DiasTrabajados = 0,
                        HorasExtra = 0,
                        SueldoDiarioAplicado = empleado.SueldoDiario,
                        PagoHoraExtraAplicado = empleado.PagoHoraExtra
                    };
                    detalles.Add(detalle);
                }

                detalle.Bono = obra.BonoTerminacionPorTrabajador;
            }
        }
    }



    public void Autorizar(int idNomina, int idUsuario, decimal efectivoRecibido)
    {


        var nomina = _db.Nominas.Find(idNomina);
       if (nomina == null)
        {
            throw new ValidacionException("La nomina no existe");
        }

        if (nomina.Estatus != EstatusNomina.Borrador)
            throw new ValidacionException("Solo se puede autorizar una nómina en borrador.");

        nomina.Estatus = EstatusNomina.Autorizada;
        nomina.FechaAutorizacion = DateTime.Now;
        nomina.EfectivoRecibido = efectivoRecibido;
        nomina.IdUsuarioAutoriza = idUsuario;
         
        var pagos = _db.PagosEmpleado.Where(p => p.IdNomina == idNomina).ToList();

        foreach (var pago in pagos)
        {
            if (pago.TotalDescuentos <= 0)
                continue;

            var prestamo = _db.Prestamos.FirstOrDefault(p => p.IdEmpleado == pago.IdEmpleado && p.Estatus == EstatusPrestamo.Activo);

            if (prestamo == null)
            {
                throw new ValidacionException("El empleado tiene un descuento pero no tiene un préstamo activo.");
            }
            _db.AbonosPrestamo.Add(new AbonoPrestamo
            {
                IdPrestamo = prestamo.IdPrestamo,
                IdNomina = pago.IdNomina,
                MontoAbonado = pago.TotalDescuentos,
                Fecha = DateOnly.FromDateTime(DateTime.Today)
            });

            prestamo.SaldoPendiente -= pago.TotalDescuentos;

            if (prestamo.SaldoPendiente <= 0)
            {
                prestamo.Estatus = EstatusPrestamo.Liquidado;
            }

        }


        

        


        _db.SaveChanges();
    }


    /// <summary>
    /// Entrega un pago pendiente en la semana de la nómina indicada (RN-13).
    /// Si era el último pendiente de su nómina, esa nómina pasa a Pagada (pregunta 9).
    /// </summary>
    public void MarcarPagoEntregado(int idPagoEmpleado, int idNominaEntrega)
    {
        var pago = _db.PagosEmpleado.Find(idPagoEmpleado);
        if (pago == null)
        {
            throw new ValidacionException("El pago no existe.");
        }

        if (pago.Estatus == EstatusPago.Entregado)
        {
            throw new ValidacionException("Este pago ya fue entregado.");
        }

        var nominaDelPago = _db.Nominas.Find(pago.IdNomina);
        if (nominaDelPago == null || nominaDelPago.Estatus == EstatusNomina.Borrador)
        {
            throw new ValidacionException("No se puede entregar un pago de una nómina sin autorizar.");
        }

        var nominaEntrega = _db.Nominas.Find(idNominaEntrega);
        if (nominaEntrega == null)
        {
            throw new ValidacionException("La nómina de entrega no existe.");
        }

        pago.Estatus = EstatusPago.Entregado;
        pago.FechaPago = DateOnly.FromDateTime(DateTime.Today);
        pago.IdNominaPago = idNominaEntrega;

        // La consulta va a la BD, que todavía no ve el cambio de arriba: se excluye este pago
        bool quedanPendientes = _db.PagosEmpleado.Any(p => p.IdNomina == pago.IdNomina
                                                            && p.IdPagoEmpleado != pago.IdPagoEmpleado
                                                            && p.Estatus == EstatusPago.Pendiente);
        if (!quedanPendientes)
        {
            nominaDelPago.Estatus = EstatusNomina.Pagada;
        }

        _db.SaveChanges();
    }
}
