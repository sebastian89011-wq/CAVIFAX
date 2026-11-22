using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class EmpleadoService
{
    private readonly NominasDbContext _db;

    public EmpleadoService(NominasDbContext db)
    {
        _db = db;
    }

    public List<Empleado> Listar()
    {
        return _db.Empleados.OrderBy(e => e.IdEmpleado).ToList();
    }

    // Solo los activos pueden ser responsables de obra, registrar asistencia o recibir préstamos
    public List<Empleado> ListarActivos()
    {
        return _db.Empleados
            .Where(e => e.Estatus == EstatusEmpleado.Activo)
            .OrderBy(e => e.Nombre)
            .ThenBy(e => e.ApellidoPaterno)
            .ToList();
    }

    public Empleado? ObtenerPorId(int id)
    {
        return _db.Empleados.Find(id);
    }

    public static string NombreCompleto(Empleado e)
    {
        return $"{e.Nombre} {e.ApellidoPaterno} {e.ApellidoMaterno}".Trim();
    }

    public void Crear(Empleado empleado)
    {
        // Todo empleado nuevo entra como activo
        empleado.Estatus = EstatusEmpleado.Activo;

        this.ValidarDatos(empleado);
        _db.Empleados.Add(empleado);
        _db.SaveChanges();
    }

    public void Editar(Empleado empleado)
    {
        var existente = _db.Empleados.Find(empleado.IdEmpleado);
        if (existente == null)
            throw new ValidacionException("El empleado no existe.");

        this.ValidarDatos(empleado);

        existente.IdPuesto = empleado.IdPuesto;
        existente.Nombre = empleado.Nombre;
        existente.ApellidoPaterno = empleado.ApellidoPaterno;
        existente.ApellidoMaterno = empleado.ApellidoMaterno;
        existente.Telefono = empleado.Telefono;
        existente.Domicilio = empleado.Domicilio;
        existente.Curp = empleado.Curp;
        existente.Nss = empleado.Nss;
        existente.ClaveIne = empleado.ClaveIne;
        existente.DocumentosCompletos = empleado.DocumentosCompletos;
        existente.Especialidad = empleado.Especialidad;
        existente.TipoContratacion = empleado.TipoContratacion;
        existente.FechaIngreso = empleado.FechaIngreso;
        existente.SueldoDiario = empleado.SueldoDiario;
        existente.PagoHoraExtra = empleado.PagoHoraExtra;
        existente.Estatus = empleado.Estatus;
        _db.SaveChanges();
    }

    // Reglas tomadas de la tabla empleado (longitudes, CHECK y UNIQUE)
    private void ValidarDatos(Empleado empleado)
    {
        // CURP y clave INE se guardan en mayúsculas
        empleado.Curp = (empleado.Curp ?? string.Empty).Trim().ToUpper();
        empleado.ClaveIne = (empleado.ClaveIne ?? string.Empty).Trim().ToUpper();
        empleado.Nss = (empleado.Nss ?? string.Empty).Trim();
        empleado.Telefono = (empleado.Telefono ?? string.Empty).Trim();

        if (!_db.Puestos.Any(p => p.IdPuesto == empleado.IdPuesto))
            throw new ValidacionException("Selecciona un puesto válido.");

        if (string.IsNullOrWhiteSpace(empleado.Nombre))
            throw new ValidacionException("El nombre es obligatorio.");
        if (empleado.Nombre.Length > 50)
            throw new ValidacionException("El nombre no puede tener más de 50 caracteres.");

        if (string.IsNullOrWhiteSpace(empleado.ApellidoPaterno))
            throw new ValidacionException("El apellido paterno es obligatorio.");
        if (empleado.ApellidoPaterno.Length > 40)
            throw new ValidacionException("El apellido paterno no puede tener más de 40 caracteres.");
        if (empleado.ApellidoMaterno != null && empleado.ApellidoMaterno.Length > 40)
            throw new ValidacionException("El apellido materno no puede tener más de 40 caracteres.");

        if (empleado.Telefono.Length != 10 || !empleado.Telefono.All(char.IsDigit))
            throw new ValidacionException("El teléfono debe tener 10 dígitos.");

        if (string.IsNullOrWhiteSpace(empleado.Domicilio))
            throw new ValidacionException("El domicilio es obligatorio.");
        if (empleado.Domicilio.Length > 200)
            throw new ValidacionException("El domicilio no puede tener más de 200 caracteres.");

        if (empleado.Curp.Length != 18)
            throw new ValidacionException("La CURP debe tener 18 caracteres.");
        if (empleado.Nss.Length != 11 || !empleado.Nss.All(char.IsDigit))
            throw new ValidacionException("El NSS debe tener 11 dígitos.");
        if (empleado.ClaveIne.Length != 18)
            throw new ValidacionException("La clave de elector debe tener 18 caracteres.");

        if (empleado.SueldoDiario <= 0)
            throw new ValidacionException("El sueldo diario debe ser mayor a 0.");
        if (empleado.PagoHoraExtra <= 0)
            throw new ValidacionException("El pago por hora extra debe ser mayor a 0.");

        if (_db.Empleados.Any(e => e.Curp == empleado.Curp && e.IdEmpleado != empleado.IdEmpleado))
            throw new ValidacionException("Ya existe un empleado con esa CURP.");
        if (_db.Empleados.Any(e => e.Nss == empleado.Nss && e.IdEmpleado != empleado.IdEmpleado))
            throw new ValidacionException("Ya existe un empleado con ese NSS.");
    }
}
