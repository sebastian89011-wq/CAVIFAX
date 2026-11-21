using Microsoft.EntityFrameworkCore;
using Nominas.Entidades;
using System.Text;

namespace Nominas.Datos;

public class NominasDbContext : DbContext
{
    public NominasDbContext(DbContextOptions<NominasDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Puesto>().HasKey(p => p.IdPuesto);
        modelBuilder.Entity<Empleado>().HasKey(e => e.IdEmpleado);
        modelBuilder.Entity<Cliente>().HasKey(c => c.IdCliente);
        modelBuilder.Entity<Obra>().HasKey(o => o.IdObra);
        modelBuilder.Entity<Asistencia>().HasKey(a => a.IdAsistencia);
        modelBuilder.Entity<Prestamo>().HasKey(p => p.IdPrestamo);
        modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
        modelBuilder.Entity<Nomina>().HasKey(n => n.IdNomina);
        modelBuilder.Entity<DetalleNomina>().HasKey(d => d.IdDetalle);
        modelBuilder.Entity<AbonoPrestamo>().HasKey(a => a.IdAbono);
        modelBuilder.Entity<PagoEmpleado>().HasKey(p => p.IdPagoEmpleado);

        modelBuilder.Entity<Empleado>().Property(e => e.Estatus).HasConversion(v => ASnakeCase(v.ToString()),
             v => Enum.Parse<EstatusEmpleado>(v.Replace("_", ""), true));

        modelBuilder.Entity<Empleado>().Property(e => e.TipoContratacion).HasConversion
            (v => ASnakeCase(v.ToString()), v => Enum.Parse<TipoContratacion>(v.Replace("_", ""), true));

        modelBuilder.Entity<Obra>().Property(o => o.Estatus).HasConversion(
            v => ASnakeCase(v.ToString()), v => Enum.Parse<EstatusObra>(v.Replace("_", ""), true));

        modelBuilder.Entity<Nomina>().Property(n => n.Estatus).HasConversion(
            v => ASnakeCase(v.ToString()), v => Enum.Parse<EstatusNomina>(v.Replace("_", ""), true));

        modelBuilder.Entity<PagoEmpleado>().Property(p => p.Estatus).HasConversion(
            v => ASnakeCase(v.ToString()), v => Enum.Parse<EstatusPago>(v.Replace("_", ""), true));

        modelBuilder.Entity<Prestamo>().Property(p => p.Tipo).HasConversion(
            v => ASnakeCase(v.ToString()), v => Enum.Parse<TipoPrestamo>(v.Replace("_", ""), true));

        modelBuilder.Entity<Prestamo>().Property(p => p.Estatus).HasConversion(
            v => ASnakeCase(v.ToString()), v => Enum.Parse<EstatusPrestamo>(v.Replace("_", ""), true));

        ///no repetidos

        modelBuilder.Entity<Empleado>().HasIndex(e => e.Curp).IsUnique();
        modelBuilder.Entity<Asistencia>().HasIndex(a => new { a.IdEmpleado, a.Fecha }).IsUnique();
        modelBuilder.Entity<Puesto>().HasIndex(e => e.Nombre).IsUnique();
        modelBuilder.Entity<DetalleNomina>().HasIndex(e => new { e.IdNomina, e.IdEmpleado, e.IdObra }).IsUnique();
        modelBuilder.Entity<Empleado>().HasIndex(e => e.Nss).IsUnique();
        modelBuilder.Entity<Usuario>().HasIndex(u => u.IdEmpleado).IsUnique();
        modelBuilder.Entity<Usuario>().HasIndex(u => u.NombreUsuario).IsUnique();
        modelBuilder.Entity<Nomina>().HasIndex(n => n.FechaInicio).IsUnique();
        modelBuilder.Entity<PagoEmpleado>().HasIndex(p => new { p.IdNomina, p.IdEmpleado }).IsUnique();
        modelBuilder.Entity<AbonoPrestamo>().HasIndex(a => new { a.IdPrestamo, a.IdNomina }).IsUnique();

        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            entidad.SetTableName(ASnakeCase(entidad.ClrType.Name));

            foreach (var propiedad in entidad.GetProperties())
            {
                propiedad.SetColumnName(ASnakeCase(propiedad.Name));
            }
        }
    }

    private static string ASnakeCase(string nombre)
    {
        StringBuilder sb = new StringBuilder();


        for (int i = 0; i < nombre.Length; i++)
        {

            char c = nombre[i];

            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    sb.Append('_');
                }

                sb.Append(char.ToLower(c));

            }
            else
            {
                sb.Append(c);
            }
        }


        return sb.ToString();



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