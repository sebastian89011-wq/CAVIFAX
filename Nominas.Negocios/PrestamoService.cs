using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class PrestamoService
{
    private readonly NominasDbContext _db;

    public PrestamoService(NominasDbContext db)
    {
        _db = db;
    }

    // Primero los activos y después los más recientes
    public List<Prestamo> Listar()
    {
        return _db.Prestamos.ToList()
            .OrderBy(p => p.Estatus)
            .ThenByDescending(p => p.Fecha)
            .ToList();
    }

    public Prestamo? ObtenerPorId(int id)
    {
        return _db.Prestamos.Find(id);
    }

    public List<AbonoPrestamo> ListarAbonos(int idPrestamo)
    {
        return _db.AbonosPrestamo
            .Where(a => a.IdPrestamo == idPrestamo)
            .OrderBy(a => a.Fecha)
            .ToList();
    }

    // Nóminas en las que se descontaron abonos, para mostrar la semana de cada una
    public List<Nomina> NominasDeAbonos(List<AbonoPrestamo> abonos)
    {
        var ids = abonos.Select(a => a.IdNomina).Distinct().ToList();
        return _db.Nominas.Where(n => ids.Contains(n.IdNomina)).ToList();
    }

    // RN-14: empleados activos que no tienen otro préstamo o anticipo activo
    public List<Empleado> EmpleadosDisponibles()
    {
        var conPrestamoActivo = _db.Prestamos
            .Where(p => p.Estatus == EstatusPrestamo.Activo)
            .Select(p => p.IdEmpleado)
            .ToList();

        return _db.Empleados
            .Where(e => e.Estatus == EstatusEmpleado.Activo && !conPrestamoActivo.Contains(e.IdEmpleado))
            .OrderBy(e => e.Nombre)
            .ThenBy(e => e.ApellidoPaterno)
            .ToList();
    }

    public void Crear(Prestamo prestamo)
    {
        // Un préstamo nuevo debe todo su monto y está activo
        prestamo.SaldoPendiente = prestamo.Monto;
        prestamo.Estatus = EstatusPrestamo.Activo;

        this.ValidarDatos(prestamo);
        _db.Prestamos.Add(prestamo);
        _db.SaveChanges();
    }

    private void ValidarDatos(Prestamo prestamo)
    {
        var empleado = _db.Empleados.Find(prestamo.IdEmpleado);
        if (empleado == null)
            throw new ValidacionException("Selecciona un empleado válido.");
        if (empleado.Estatus != EstatusEmpleado.Activo)
            throw new ValidacionException("Solo se puede prestar a empleados activos.");

        if (_db.Prestamos.Any(p => p.IdEmpleado == prestamo.IdEmpleado && p.Estatus == EstatusPrestamo.Activo))
            throw new ValidacionException("El empleado ya tiene un préstamo o anticipo activo (RN-14).");

        if (prestamo.Monto <= 0)
            throw new ValidacionException("El monto debe ser mayor a 0.");
        if (prestamo.AbonoSemanal <= 0)
            throw new ValidacionException("El abono semanal debe ser mayor a 0.");
        if (prestamo.AbonoSemanal > prestamo.Monto)
            throw new ValidacionException("El abono semanal no puede ser mayor al monto.");
    }
}
