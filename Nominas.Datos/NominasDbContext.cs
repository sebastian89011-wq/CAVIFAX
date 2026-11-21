using Microsoft.EntityFrameworkCore;
using Nominas.Entidades;

namespace Nominas.Datos;

public class NominasDbContext : DbContext
{
    public NominasDbContext(DbContextOptions<NominasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Puesto> Puestos => Set<Puesto>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Prestamo> Prestamos { get { return Set<Prestamo>(); } }
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Obra> Obras => Set<Obra>();
    public DbSet<Nomina> Nominas => Set<Nomina>();
    public DbSet<PagoEmpleado> PagosEmpleado => Set<PagoEmpleado>();
    public DbSet<AbonoPrestamo> AbonosPrestamo => Set<AbonoPrestamo>();
    public DbSet<DetalleNomina> DetallesNomina => Set<DetalleNomina>();

}