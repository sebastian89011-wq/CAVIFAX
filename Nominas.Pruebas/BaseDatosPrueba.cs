using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nominas.Datos;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Pruebas;

/// <summary>
/// El mismo NominasDbContext de la aplicación, pero sobre SQLite en memoria.
/// SQLite no sabe sumar ni comparar decimal, así que en las pruebas los decimal se guardan como double.
/// Todo lo demás (snake_case, enums como texto, DateOnly, índices únicos) es el modelo real.
/// </summary>
public class ContextoPrueba : NominasDbContext
{
    public ContextoPrueba(DbContextOptions<NominasDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var aDouble = new ValueConverter<decimal, double>(v => (double)v, v => (decimal)v);
        foreach (var propiedad in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal)))
        {
            propiedad.SetValueConverter(aDouble);
        }
    }
}

/// <summary>
/// Cada prueba crea su propia base vacía en memoria: las pruebas no se afectan entre sí
/// y nunca tocan la base real de MySQL.
/// </summary>
public abstract class BaseDatosPrueba : IDisposable
{
    // Semana de prueba: lunes 23 a sábado 28 de noviembre de 2026
    protected static readonly DateOnly Lunes = new(2026, 11, 23);
    protected static readonly DateOnly Sabado = new(2026, 11, 28);

    private readonly SqliteConnection _conexion;
    protected readonly ContextoPrueba Db;

    protected readonly PuestoService Puestos;
    protected readonly ClienteService Clientes;
    protected readonly EmpleadoService Empleados;
    protected readonly ObraService Obras;
    protected readonly PrestamoService Prestamos;
    protected readonly AsistenciaService Asistencias;
    protected readonly NominaService Nominas;
    protected readonly UsuarioService Usuarios;

    private int _consecutivo;

    protected BaseDatosPrueba()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<NominasDbContext>().UseSqlite(_conexion).Options;
        Db = new ContextoPrueba(opciones);
        Db.Database.EnsureCreated();

        Puestos = new PuestoService(Db);
        Clientes = new ClienteService(Db);
        Empleados = new EmpleadoService(Db);
        Obras = new ObraService(Db);
        Prestamos = new PrestamoService(Db);
        Asistencias = new AsistenciaService(Db);
        Nominas = new NominaService(Db);
        Usuarios = new UsuarioService(Db);
    }

    public void Dispose()
    {
        Db.Dispose();
        _conexion.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Olvida lo que EF tiene en memoria para leer de nuevo desde la base.</summary>
    protected void Recargar() => Db.ChangeTracker.Clear();

    // ------------------------------------------------------------------ Datos de prueba
    // Se insertan directo en la base (sin pasar por los servicios) para preparar cada caso.

    protected Puesto NuevoPuesto(string nombre = "Albañil")
    {
        var puesto = new Puesto { Nombre = nombre + " " + (++_consecutivo), Descripcion = "Prueba" };
        Db.Puestos.Add(puesto);
        Db.SaveChanges();
        return puesto;
    }

    protected Empleado NuevoEmpleado(string nombre = "Juan", string apellido = "Perez",
        decimal sueldo = 500, decimal horaExtra = 85, EstatusEmpleado estatus = EstatusEmpleado.Activo)
    {
        int n = ++_consecutivo;
        var empleado = new Empleado
        {
            IdPuesto = NuevoPuesto().IdPuesto,
            Nombre = nombre,
            ApellidoPaterno = apellido,
            ApellidoMaterno = "Lopez",
            Telefono = "6461234567",
            Domicilio = "Calle 1, Ensenada",
            Curp = $"CURP{n:D14}",
            Nss = $"{n:D11}",
            ClaveIne = $"INE{n:D15}",
            DocumentosCompletos = true,
            TipoContratacion = TipoContratacion.Permanente,
            FechaIngreso = new DateOnly(2025, 1, 1),
            SueldoDiario = sueldo,
            PagoHoraExtra = horaExtra,
            Estatus = estatus
        };
        Db.Empleados.Add(empleado);
        Db.SaveChanges();
        return empleado;
    }

    protected Cliente NuevoCliente(string nombre = "Inmobiliaria Costa Azul")
    {
        var cliente = new Cliente { Nombre = nombre, Domicilio = "Av. Reforma 100" };
        Db.Clientes.Add(cliente);
        Db.SaveChanges();
        return cliente;
    }

    protected Obra NuevaObra(string nombre = "Residencial Punta Banda", decimal presupuesto = 120000,
        decimal limiteAlerta = 20000, decimal bono = 1000, EstatusObra estatus = EstatusObra.Activa,
        DateOnly? inicio = null, DateOnly? finProgramada = null, DateOnly? finReal = null, int? idCliente = null)
    {
        var responsable = NuevoEmpleado("Roberto", "Mendoza");
        var obra = new Obra
        {
            IdCliente = idCliente ?? NuevoCliente().IdCliente,
            IdResponsable = responsable.IdEmpleado,
            IdSupervisor = responsable.IdEmpleado,
            Nombre = nombre,
            Ubicacion = "Ensenada",
            FechaInicio = inicio ?? new DateOnly(2026, 10, 1),
            FechaFinProgramada = finProgramada ?? new DateOnly(2026, 12, 31),
            FechaFinReal = finReal,
            PresupuestoManoObra = presupuesto,
            LimiteAlertaSaldo = limiteAlerta,
            BonoTerminacionPorTrabajador = bono,
            Estatus = estatus
        };
        Db.Obras.Add(obra);
        Db.SaveChanges();
        return obra;
    }

    protected Prestamo NuevoPrestamo(int idEmpleado, decimal monto, decimal abono, decimal? saldo = null)
    {
        var prestamo = new Prestamo
        {
            IdEmpleado = idEmpleado,
            Tipo = TipoPrestamo.Prestamo,
            Monto = monto,
            Fecha = new DateOnly(2026, 11, 1),
            AbonoSemanal = abono,
            SaldoPendiente = saldo ?? monto,
            Estatus = EstatusPrestamo.Activo
        };
        Db.Prestamos.Add(prestamo);
        Db.SaveChanges();
        return prestamo;
    }

    /// <summary>Registra asistencias directo en la base (un día por fecha).</summary>
    protected void Asistio(int idEmpleado, int idObra, DateOnly desde, int dias, decimal horasExtraPrimerDia = 0)
    {
        for (int i = 0; i < dias; i++)
        {
            Db.Asistencias.Add(new Asistencia
            {
                IdEmpleado = idEmpleado,
                IdObra = idObra,
                Fecha = desde.AddDays(i),
                Asistio = true,
                HorasExtra = i == 0 ? horasExtraPrimerDia : 0
            });
        }
        Db.SaveChanges();
    }

    /// <summary>Celda de la cuadrícula de asistencias, como la envía la página.</summary>
    protected static Asistencia Celda(int idEmpleado, DateOnly fecha, int? idObra, decimal horasExtra = 0)
    {
        return new Asistencia { IdEmpleado = idEmpleado, Fecha = fecha, IdObra = idObra, HorasExtra = horasExtra };
    }

    protected PagoEmpleado PagoDe(int idNomina, int idEmpleado)
    {
        return Db.PagosEmpleado.Single(p => p.IdNomina == idNomina && p.IdEmpleado == idEmpleado);
    }

    /// <summary>Comprueba que la acción lance ValidacionException con un mensaje que contenga el texto.</summary>
    protected static void DebeFallar(Action accion, string textoEsperado)
    {
        var ex = Assert.Throws<ValidacionException>(accion);
        Assert.Contains(textoEsperado, ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
