using Microsoft.EntityFrameworkCore;
using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class AsistenciaService
{
    private const decimal MaxHorasExtra = 12;
    private static readonly string[] NombresDias = { "domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado" };

    private readonly NominasDbContext _db;

    public AsistenciaService(NominasDbContext db)
    {

        _db = db;

    }

    /// <summary>Asistencias capturadas entre dos fechas (inclusive).</summary>
    public List<Asistencia> ListarPorPeriodo(DateOnly desde, DateOnly hasta)
    {

        return _db.Asistencias.Where(a => a.Fecha >= desde && a.Fecha <= hasta).ToList();

    }

    /// <summary>
    /// Guarda la cuadrícula de una semana. Primero valida todas las celdas y, si alguna
    /// tiene error, no guarda nada y lanza una sola ValidacionException con todos los mensajes.
    /// </summary>
    public void GuardarSemana(List<Asistencia> registros)
    {
        var errores = new List<string>();
        var empleados = _db.Empleados.ToDictionary(e => e.IdEmpleado);
        var obras = _db.Obras.ToDictionary(o => o.IdObra);

        foreach (var registro in registros)
        {
            // RN-07: una asistencia por empleado y día. Si ya existe se actualiza.
            var existente = _db.Asistencias
                .FirstOrDefault(a => a.IdEmpleado == registro.IdEmpleado && a.Fecha == registro.Fecha);

            // Celda vacía (falta) que nunca se capturó: no hace falta guardar nada.
            if (existente == null && registro.IdObra == null && registro.HorasExtra == 0)
            {
                continue;
            }

            // Celda que no cambió: no se vuelve a validar ni a guardar.
            if (existente != null && existente.IdObra == registro.IdObra && existente.HorasExtra == registro.HorasExtra)
            {
                continue;
            }

            string celda = DescribirCelda(registro, empleados);
            var erroresCelda = ValidarCelda(registro, empleados, obras);
            if (erroresCelda.Count > 0)
            {
                errores.AddRange(erroresCelda.Select(e => $"{celda}: {e}"));
                continue;
            }

            if (existente == null)
            {
                existente = new Asistencia { IdEmpleado = registro.IdEmpleado, Fecha = registro.Fecha };
                _db.Asistencias.Add(existente);
            }

            // IdObra es NULL si y solo si no asistió
            existente.IdObra = registro.IdObra;
            existente.Asistio = registro.IdObra != null;
            existente.HorasExtra = registro.HorasExtra;
        }

        if (errores.Count > 0)
        {
            // Un mensaje por renglón; la página los muestra por separado
            throw new ValidacionException(string.Join("\n", errores));
        }

        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            // Última línea de defensa: si un trigger o una restricción de MySQL rechaza el cambio,
            // se muestra su mensaje en la página en lugar de un error 500.
            throw new ValidacionException("La base de datos rechazó las asistencias: "
                                          + (ex.InnerException?.Message ?? ex.Message));
        }
    }

    private List<string> ValidarCelda(Asistencia registro, Dictionary<int, Empleado> empleados, Dictionary<int, Obra> obras)
    {
        var errores = new List<string>();

        // No se modifican asistencias de una semana cuya nómina ya se autorizó
        bool nominaCerrada = _db.Nominas.Any(n => n.FechaInicio <= registro.Fecha && n.FechaFin >= registro.Fecha
                                                   && n.Estatus != EstatusNomina.Borrador);
        if (nominaCerrada)
        {
            errores.Add("la nómina de esa semana ya está autorizada y no se puede modificar.");
            return errores;
        }

        // RN-06: el empleado debe existir y estar activo
        if (!empleados.TryGetValue(registro.IdEmpleado, out var empleado))
        {
            errores.Add("el empleado no existe.");
            return errores;
        }
        if (empleado.Estatus != EstatusEmpleado.Activo)
        {
            errores.Add("el empleado no está activo.");
        }

        // RN-08: horas extra ≥ 0 y solo si asistió
        if (registro.HorasExtra < 0)
        {
            errores.Add("las horas extra no pueden ser negativas.");
        }
        else if (registro.HorasExtra > MaxHorasExtra)
        {
            errores.Add($"las horas extra no pueden ser más de {MaxHorasExtra}.");
        }

        if (registro.IdObra == null)
        {
            if (registro.HorasExtra > 0)
            {
                errores.Add("no puede tener horas extra si faltó.");
            }
            return errores;
        }

        // RN-06: la obra debe existir, estar activa y la fecha debe estar dentro de su periodo
        if (!obras.TryGetValue(registro.IdObra.Value, out var obra))
        {
            errores.Add("la obra no existe.");
            return errores;
        }
        if (obra.Estatus != EstatusObra.Activa)
        {
            errores.Add($"la obra {obra.Nombre} no está activa.");
        }
        if (registro.Fecha < obra.FechaInicio)
        {
            errores.Add($"la obra {obra.Nombre} empieza el {obra.FechaInicio:dd/MM/yyyy}.");
        }
        // Misma regla que el trigger trg_validar_asistencia_before_insert: la fecha no puede pasar
        // de la fecha de fin programada. Si la obra se alargó, primero se edita esa fecha en Obras.
        if (registro.Fecha > obra.FechaFinProgramada)
        {
            errores.Add($"la obra {obra.Nombre} tiene programado terminar el {obra.FechaFinProgramada:dd/MM/yyyy}; "
                        + "si se alargó, actualiza su fecha de fin programada en Obras.");
        }
        else if (obra.FechaFinReal != null && registro.Fecha > obra.FechaFinReal)
        {
            errores.Add($"la obra {obra.Nombre} terminó el {obra.FechaFinReal:dd/MM/yyyy}.");
        }

        return errores;
    }

    private static string DescribirCelda(Asistencia registro, Dictionary<int, Empleado> empleados)
    {
        string nombre = empleados.TryGetValue(registro.IdEmpleado, out var e)
            ? EmpleadoService.NombreCompleto(e)
            : $"Empleado {registro.IdEmpleado}";
        return $"{nombre}, {NombresDias[(int)registro.Fecha.DayOfWeek]} {registro.Fecha:dd/MM}";
    }
}
