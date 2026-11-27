using Nominas.Entidades;

namespace Nominas.Web.Pages;

// Datos temporales mientras el backend conecta la base de datos. Se borrará después.
public static class DatosPrueba
{
    public static List<Puesto> Puestos = new()
    {
        new Puesto { IdPuesto = 1, Nombre = "Ingeniero" },
        new Puesto { IdPuesto = 2, Nombre = "Albañil" },
        new Puesto { IdPuesto = 3, Nombre = "Ayudante general" },
        new Puesto { IdPuesto = 4, Nombre = "Supervisor general" }
    };

    public static List<Empleado> Empleados = new()
    {
        new Empleado { IdEmpleado = 1, IdPuesto = 2, Nombre = "Pedro", ApellidoPaterno = "Ramírez", ApellidoMaterno = "Soto",
                       Telefono = "6861234567", Domicilio = "Calle Durango 120, Mexicali",
                       Curp = "RASP900101HBCMTD05", Nss = "12345678901", ClaveIne = "RMSTPD90010102H100",
                       DocumentosCompletos = true, Especialidad = Especialidad.Soldador,
                       TipoContratacion = TipoContratacion.Permanente, Estatus = EstatusEmpleado.Activo,
                       FechaIngreso = new DateOnly(2021, 3, 15), SueldoDiario = 450m, PagoHoraExtra = 70m },
        new Empleado { IdEmpleado = 2, IdPuesto = 3, Nombre = "Luis", ApellidoPaterno = "Gómez",
                       Telefono = "6869876543", Domicilio = "Av. Colón 45, Mexicali",
                       Curp = "GOLL950505HBCMSS02", Nss = "98765432109", ClaveIne = "GMLSLS95050502H200",
                       DocumentosCompletos = false,
                       TipoContratacion = TipoContratacion.Subcontratado, Estatus = EstatusEmpleado.BajaTemporal,
                       FechaIngreso = new DateOnly(2024, 8, 1), SueldoDiario = 320m, PagoHoraExtra = 50m },
        new Empleado { IdEmpleado = 3, IdPuesto = 2, Nombre = "Mario", ApellidoPaterno = "Ochoa", ApellidoMaterno = "Ruiz",
                       Telefono = "6865551122", Domicilio = "Calle Reforma 800, Mexicali",
                       Curp = "OORM880720HBCCZR09", Nss = "55566677788", ClaveIne = "OCRZMR88072002H300",
                       DocumentosCompletos = true, Especialidad = Especialidad.Electricista,
                       TipoContratacion = TipoContratacion.Permanente, Estatus = EstatusEmpleado.Activo,
                       FechaIngreso = new DateOnly(2019, 1, 10), SueldoDiario = 480m, PagoHoraExtra = 75m },
        new Empleado { IdEmpleado = 4, IdPuesto = 1, Nombre = "Roberto", ApellidoPaterno = "Castañeda",
               Telefono = "6861112233", Domicilio = "Blvd. Lázaro Cárdenas 300, Mexicali",
               Curp = "CARR800312HBCSBB01", Nss = "11122233344", ClaveIne = "CSRBRB80031202H400",
               DocumentosCompletos = true, TipoContratacion = TipoContratacion.Permanente,
               Estatus = EstatusEmpleado.Activo, FechaIngreso = new DateOnly(2015, 6, 1),
               SueldoDiario = 1200m, PagoHoraExtra = 150m },
        new Empleado { IdEmpleado = 5, IdPuesto = 4, Nombre = "Javier", ApellidoPaterno = "Morales", ApellidoMaterno = "Lee",
               Telefono = "6864445566", Domicilio = "Calle Tuxtla 77, Mexicali",
               Curp = "MOLJ850909HBCRXV03", Nss = "44455566677", ClaveIne = "MRLEJV85090902H500",
               DocumentosCompletos = true, TipoContratacion = TipoContratacion.Permanente,
               Estatus = EstatusEmpleado.Activo, FechaIngreso = new DateOnly(2017, 2, 20),
               SueldoDiario = 800m, PagoHoraExtra = 110m }
    };

    public static List<Cliente> Clientes = new()
{
    new Cliente { IdCliente = 1, Nombre = "Cliente1", Domicilio = "Lomas de Valle, Duraznos 555" },
    new Cliente { IdCliente = 2, Nombre = "Cliente2", Domicilio = "Lomas Turbas, Alisos 67" },
    new Cliente { IdCliente = 3, Nombre = "Cliente3", Domicilio = "Vampiros y Rituales" }
};

    public static List<Obra> Obras = new()
{
    new Obra { IdObra = 1, IdCliente = 1, IdResponsable = 4, IdSupervisor = 5,
               Nombre = "Casa habitación Lomas", Ubicacion = "Fracc. Lomas de Valle, Mexicali",
               FechaInicio = new DateOnly(2026, 9, 1), FechaFinProgramada = new DateOnly(2026, 12, 15),
               PresupuestoManoObra = 250000m, LimiteAlertaSaldo = 30000m,
               BonoTerminacionPorTrabajador = 500m, Estatus = EstatusObra.Activa },
    new Obra { IdObra = 2, IdCliente = 2, IdResponsable = 4, IdSupervisor = 5,
               Nombre = "Bodega industrial Pacífico", Ubicacion = "Parque Industrial, Mexicali",
               FechaInicio = new DateOnly(2026, 7, 15), FechaFinProgramada = new DateOnly(2026, 11, 30),
               PresupuestoManoObra = 220000m, LimiteAlertaSaldo = 25000m,
               BonoTerminacionPorTrabajador = 400m, Estatus = EstatusObra.Activa },
    new Obra { IdObra = 3, IdCliente = 3, IdResponsable = 4, IdSupervisor = 5,
               Nombre = "Remodelación de local", Ubicacion = "Centro, Mexicali",
               FechaInicio = new DateOnly(2026, 10, 1), FechaFinProgramada = new DateOnly(2027, 1, 31),
               PresupuestoManoObra = 80000m, LimiteAlertaSaldo = 10000m,
               BonoTerminacionPorTrabajador = 300m, Estatus = EstatusObra.Suspendida }
};

    // Lo gastado en mano de obra por obra. El backend lo calculará sumando las nóminas autorizadas (RN-12).
    public static Dictionary<int, decimal> GastadoPorObra = new()
{
    { 1, 85000m },
    { 2, 200000m },
    { 3, 20000m }
};

    // ---------- Asistencias de la semana del 16 al 21 de noviembre de 2026 ----------
    // IdObra = null significa falta (RN-08: sin horas extra).
    public static List<Asistencia> Asistencias = new()
    {
        // Pedro: obra 1 de lunes a jueves, falta el viernes
        new Asistencia { IdAsistencia = 1,  IdEmpleado = 1, IdObra = 1,    Fecha = new DateOnly(2026, 11, 16), Asistio = true, HorasExtra = 1 },
        new Asistencia { IdAsistencia = 2,  IdEmpleado = 1, IdObra = 1,    Fecha = new DateOnly(2026, 11, 17), Asistio = true },
        new Asistencia { IdAsistencia = 3,  IdEmpleado = 1, IdObra = 1,    Fecha = new DateOnly(2026, 11, 18), Asistio = true },
        new Asistencia { IdAsistencia = 4,  IdEmpleado = 1, IdObra = 1,    Fecha = new DateOnly(2026, 11, 19), Asistio = true },
        new Asistencia { IdAsistencia = 5,  IdEmpleado = 1, IdObra = null, Fecha = new DateOnly(2026, 11, 20), Asistio = false },
        // Mario: obra 2 de lunes a viernes
        new Asistencia { IdAsistencia = 6,  IdEmpleado = 3, IdObra = 2, Fecha = new DateOnly(2026, 11, 16), Asistio = true },
        new Asistencia { IdAsistencia = 7,  IdEmpleado = 3, IdObra = 2, Fecha = new DateOnly(2026, 11, 17), Asistio = true, HorasExtra = 2 },
        new Asistencia { IdAsistencia = 8,  IdEmpleado = 3, IdObra = 2, Fecha = new DateOnly(2026, 11, 18), Asistio = true },
        new Asistencia { IdAsistencia = 9,  IdEmpleado = 3, IdObra = 2, Fecha = new DateOnly(2026, 11, 19), Asistio = true },
        new Asistencia { IdAsistencia = 10, IdEmpleado = 3, IdObra = 2, Fecha = new DateOnly(2026, 11, 20), Asistio = true },
        // Roberto (ingeniero): obra 1 de lunes a miércoles y obra 2 jueves y viernes
        new Asistencia { IdAsistencia = 11, IdEmpleado = 4, IdObra = 1, Fecha = new DateOnly(2026, 11, 16), Asistio = true },
        new Asistencia { IdAsistencia = 12, IdEmpleado = 4, IdObra = 1, Fecha = new DateOnly(2026, 11, 17), Asistio = true },
        new Asistencia { IdAsistencia = 13, IdEmpleado = 4, IdObra = 1, Fecha = new DateOnly(2026, 11, 18), Asistio = true },
        new Asistencia { IdAsistencia = 14, IdEmpleado = 4, IdObra = 2, Fecha = new DateOnly(2026, 11, 19), Asistio = true },
        new Asistencia { IdAsistencia = 15, IdEmpleado = 4, IdObra = 2, Fecha = new DateOnly(2026, 11, 20), Asistio = true },
        // Javier (supervisor): obra 1 de lunes a viernes
        new Asistencia { IdAsistencia = 16, IdEmpleado = 5, IdObra = 1, Fecha = new DateOnly(2026, 11, 16), Asistio = true },
        new Asistencia { IdAsistencia = 17, IdEmpleado = 5, IdObra = 1, Fecha = new DateOnly(2026, 11, 17), Asistio = true },
        new Asistencia { IdAsistencia = 18, IdEmpleado = 5, IdObra = 1, Fecha = new DateOnly(2026, 11, 18), Asistio = true },
        new Asistencia { IdAsistencia = 19, IdEmpleado = 5, IdObra = 1, Fecha = new DateOnly(2026, 11, 19), Asistio = true },
        new Asistencia { IdAsistencia = 20, IdEmpleado = 5, IdObra = 1, Fecha = new DateOnly(2026, 11, 20), Asistio = true }
    };

    // ---------- Nóminas semanales (lunes a domingo) ----------
    public static List<Nomina> Nominas = new()
    {
        new Nomina { IdNomina = 1, FechaInicio = new DateOnly(2026, 11, 2), FechaFin = new DateOnly(2026, 11, 8),
                     Estatus = EstatusNomina.Pagada, IdUsuarioAutoriza = 1,
                     FechaAutorizacion = new DateTime(2026, 11, 8, 17, 30, 0), EfectivoRecibido = 17080m },
        new Nomina { IdNomina = 2, FechaInicio = new DateOnly(2026, 11, 9), FechaFin = new DateOnly(2026, 11, 15),
                     Estatus = EstatusNomina.Autorizada, IdUsuarioAutoriza = 1,
                     FechaAutorizacion = new DateTime(2026, 11, 15, 18, 0, 0), EfectivoRecibido = 17445m },
        new Nomina { IdNomina = 3, FechaInicio = new DateOnly(2026, 11, 16), FechaFin = new DateOnly(2026, 11, 22),
                     Estatus = EstatusNomina.Borrador }
    };

    // Costo BRUTO por empleado y obra (RN-10): días × sueldo + horas extra × tarifa + bono.
    public static List<DetalleNomina> DetallesNomina = new()
    {
        // Nómina 1
        new DetalleNomina { IdDetalle = 1,  IdNomina = 1, IdEmpleado = 1, IdObra = 1, DiasTrabajados = 6, SueldoDiarioAplicado = 450m,  PagoHoraExtraAplicado = 70m,  CostoBruto = 2700m },
        new DetalleNomina { IdDetalle = 2,  IdNomina = 1, IdEmpleado = 3, IdObra = 2, DiasTrabajados = 6, SueldoDiarioAplicado = 480m,  PagoHoraExtraAplicado = 75m,  CostoBruto = 2880m },
        new DetalleNomina { IdDetalle = 3,  IdNomina = 1, IdEmpleado = 4, IdObra = 1, DiasTrabajados = 6, SueldoDiarioAplicado = 1200m, PagoHoraExtraAplicado = 150m, CostoBruto = 7200m },
        new DetalleNomina { IdDetalle = 4,  IdNomina = 1, IdEmpleado = 5, IdObra = 1, DiasTrabajados = 6, SueldoDiarioAplicado = 800m,  PagoHoraExtraAplicado = 110m, CostoBruto = 4800m },
        // Nómina 2 (Pedro y Roberto trabajaron en dos obras)
        new DetalleNomina { IdDetalle = 5,  IdNomina = 2, IdEmpleado = 1, IdObra = 1, DiasTrabajados = 4, HorasExtra = 2, SueldoDiarioAplicado = 450m, PagoHoraExtraAplicado = 70m, CostoBruto = 1940m },
        new DetalleNomina { IdDetalle = 6,  IdNomina = 2, IdEmpleado = 1, IdObra = 2, DiasTrabajados = 2, SueldoDiarioAplicado = 450m,  PagoHoraExtraAplicado = 70m,  CostoBruto = 900m },
        new DetalleNomina { IdDetalle = 7,  IdNomina = 2, IdEmpleado = 3, IdObra = 2, DiasTrabajados = 6, HorasExtra = 3, SueldoDiarioAplicado = 480m, PagoHoraExtraAplicado = 75m, CostoBruto = 3105m },
        new DetalleNomina { IdDetalle = 8,  IdNomina = 2, IdEmpleado = 4, IdObra = 1, DiasTrabajados = 3, SueldoDiarioAplicado = 1200m, PagoHoraExtraAplicado = 150m, CostoBruto = 3600m },
        new DetalleNomina { IdDetalle = 9,  IdNomina = 2, IdEmpleado = 4, IdObra = 2, DiasTrabajados = 3, SueldoDiarioAplicado = 1200m, PagoHoraExtraAplicado = 150m, CostoBruto = 3600m },
        new DetalleNomina { IdDetalle = 10, IdNomina = 2, IdEmpleado = 5, IdObra = 1, DiasTrabajados = 6, SueldoDiarioAplicado = 800m,  PagoHoraExtraAplicado = 110m, CostoBruto = 4800m },
        // Nómina 3 (borrador, sale de las asistencias de arriba)
        new DetalleNomina { IdDetalle = 11, IdNomina = 3, IdEmpleado = 1, IdObra = 1, DiasTrabajados = 4, HorasExtra = 1, SueldoDiarioAplicado = 450m, PagoHoraExtraAplicado = 70m, CostoBruto = 1870m },
        new DetalleNomina { IdDetalle = 12, IdNomina = 3, IdEmpleado = 3, IdObra = 2, DiasTrabajados = 5, HorasExtra = 2, SueldoDiarioAplicado = 480m, PagoHoraExtraAplicado = 75m, CostoBruto = 2550m },
        new DetalleNomina { IdDetalle = 13, IdNomina = 3, IdEmpleado = 4, IdObra = 1, DiasTrabajados = 3, SueldoDiarioAplicado = 1200m, PagoHoraExtraAplicado = 150m, CostoBruto = 3600m },
        new DetalleNomina { IdDetalle = 14, IdNomina = 3, IdEmpleado = 4, IdObra = 2, DiasTrabajados = 2, SueldoDiarioAplicado = 1200m, PagoHoraExtraAplicado = 150m, CostoBruto = 2400m },
        new DetalleNomina { IdDetalle = 15, IdNomina = 3, IdEmpleado = 5, IdObra = 1, DiasTrabajados = 5, SueldoDiarioAplicado = 800m,  PagoHoraExtraAplicado = 110m, CostoBruto = 4000m }
    };

    // Pago NETO por empleado y nómina: suma de brutos − abono del préstamo.
    public static List<PagoEmpleado> PagosEmpleado = new()
    {
        // Nómina 1 (todo entregado)
        new PagoEmpleado { IdPagoEmpleado = 1, IdNomina = 1, IdEmpleado = 1, TotalBruto = 2700m, TotalDescuentos = 200m, TotalNeto = 2500m, Estatus = EstatusPago.Entregado, FechaPago = new DateOnly(2026, 11, 8), IdNominaPago = 1 },
        new PagoEmpleado { IdPagoEmpleado = 2, IdNomina = 1, IdEmpleado = 3, TotalBruto = 2880m, TotalDescuentos = 300m, TotalNeto = 2580m, Estatus = EstatusPago.Entregado, FechaPago = new DateOnly(2026, 11, 8), IdNominaPago = 1 },
        new PagoEmpleado { IdPagoEmpleado = 3, IdNomina = 1, IdEmpleado = 4, TotalBruto = 7200m, TotalNeto = 7200m, Estatus = EstatusPago.Entregado, FechaPago = new DateOnly(2026, 11, 8), IdNominaPago = 1 },
        new PagoEmpleado { IdPagoEmpleado = 4, IdNomina = 1, IdEmpleado = 5, TotalBruto = 4800m, TotalNeto = 4800m, Estatus = EstatusPago.Entregado, FechaPago = new DateOnly(2026, 11, 8), IdNominaPago = 1 },
        // Nómina 2 (a Mario no se le entregó: queda pendiente, RN-13)
        new PagoEmpleado { IdPagoEmpleado = 5, IdNomina = 2, IdEmpleado = 1, TotalBruto = 2840m, TotalDescuentos = 200m, TotalNeto = 2640m, Estatus = EstatusPago.Entregado, FechaPago = new DateOnly(2026, 11, 15), IdNominaPago = 2 },
        new PagoEmpleado { IdPagoEmpleado = 6, IdNomina = 2, IdEmpleado = 3, TotalBruto = 3105m, TotalDescuentos = 300m, TotalNeto = 2805m, Estatus = EstatusPago.Pendiente },
        new PagoEmpleado { IdPagoEmpleado = 7, IdNomina = 2, IdEmpleado = 4, TotalBruto = 7200m, TotalNeto = 7200m, Estatus = EstatusPago.Entregado, FechaPago = new DateOnly(2026, 11, 15), IdNominaPago = 2 },
        new PagoEmpleado { IdPagoEmpleado = 8, IdNomina = 2, IdEmpleado = 5, TotalBruto = 4800m, TotalNeto = 4800m, Estatus = EstatusPago.Entregado, FechaPago = new DateOnly(2026, 11, 15), IdNominaPago = 2 },
        // Nómina 3 (borrador: vista previa)
        new PagoEmpleado { IdPagoEmpleado = 9,  IdNomina = 3, IdEmpleado = 1, TotalBruto = 1870m, TotalDescuentos = 200m, TotalNeto = 1670m, Estatus = EstatusPago.Pendiente },
        new PagoEmpleado { IdPagoEmpleado = 10, IdNomina = 3, IdEmpleado = 3, TotalBruto = 2550m, TotalNeto = 2550m, Estatus = EstatusPago.Pendiente },
        new PagoEmpleado { IdPagoEmpleado = 11, IdNomina = 3, IdEmpleado = 4, TotalBruto = 6000m, TotalNeto = 6000m, Estatus = EstatusPago.Pendiente },
        new PagoEmpleado { IdPagoEmpleado = 12, IdNomina = 3, IdEmpleado = 5, TotalBruto = 4000m, TotalNeto = 4000m, Estatus = EstatusPago.Pendiente }
    };

    // ---------- Préstamos y anticipos ----------
    public static List<Prestamo> Prestamos = new()
    {
        new Prestamo { IdPrestamo = 1, IdEmpleado = 1, Tipo = TipoPrestamo.Prestamo, Monto = 2000m, Fecha = new DateOnly(2026, 10, 30),
                       AbonoSemanal = 200m, SaldoPendiente = 1600m, Estatus = EstatusPrestamo.Activo },
        new Prestamo { IdPrestamo = 2, IdEmpleado = 3, Tipo = TipoPrestamo.Anticipo, Monto = 600m, Fecha = new DateOnly(2026, 10, 30),
                       AbonoSemanal = 600m, SaldoPendiente = 0m, Estatus = EstatusPrestamo.Liquidado }
    };

    public static List<AbonoPrestamo> AbonosPrestamo = new()
    {
        new AbonoPrestamo { IdAbono = 1, IdPrestamo = 1, IdNomina = 1, MontoAbonado = 200m, Fecha = new DateOnly(2026, 11, 8) },
        new AbonoPrestamo { IdAbono = 2, IdPrestamo = 1, IdNomina = 2, MontoAbonado = 200m, Fecha = new DateOnly(2026, 11, 15) },
        new AbonoPrestamo { IdAbono = 3, IdPrestamo = 2, IdNomina = 1, MontoAbonado = 600m, Fecha = new DateOnly(2026, 11, 8) }
    };

    // ---------- Ayudas para mostrar nombres en lugar de ids ----------
    public static string NombreEmpleado(int idEmpleado)
    {
        var e = Empleados.FirstOrDefault(x => x.IdEmpleado == idEmpleado);
        return e == null ? "—" : $"{e.Nombre} {e.ApellidoPaterno} {e.ApellidoMaterno}".Trim();
    }

    public static string NombreObra(int? idObra)
    {
        return Obras.FirstOrDefault(o => o.IdObra == idObra)?.Nombre ?? "—";
    }

    public static string NombreCliente(int idCliente)
    {
        return Clientes.FirstOrDefault(c => c.IdCliente == idCliente)?.Nombre ?? "—";
    }

    public static string Semana(Nomina n)
    {
        return $"{n.FechaInicio:dd/MM} al {n.FechaFin:dd/MM/yyyy}";
    }
}