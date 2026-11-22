using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Asistencias
{
    public class IndexModel : PageModel
    {
        // Cualquier día de la semana que se quiere ver. Llega en la dirección: /Asistencias?Semana=2026-11-16
        [BindProperty(SupportsGet = true)]
        public DateOnly Semana { get; set; }

        // Todas las celdas de la cuadrícula en una sola lista: empleado 1 lunes, empleado 1 martes, ...
        [BindProperty]
        public List<Asistencia> Registros { get; set; } = new();

        public DateOnly Lunes { get; set; }
        public List<DateOnly> Dias { get; set; } = new();
        public List<Empleado> EmpleadosActivos { get; set; } = new();
        public List<SelectListItem> ObrasActivas { get; set; } = new();

        public void OnGet()
        {
            CargarSemana();

            // Una celda por empleado y día. Si ya hay asistencia capturada se usa; si no, se crea vacía.
            Registros = new List<Asistencia>();
            foreach (var empleado in EmpleadosActivos)
            {
                foreach (var dia in Dias)
                {
                    var existente = DatosPrueba.Asistencias
                        .FirstOrDefault(a => a.IdEmpleado == empleado.IdEmpleado && a.Fecha == dia);

                    Registros.Add(existente ?? new Asistencia { IdEmpleado = empleado.IdEmpleado, Fecha = dia });
                }
            }
        }

        public IActionResult OnPost()
        {
            // Aquí el backend guardará los Registros:
            //  - Asistio = (IdObra != null); si no asistió, HorasExtra = 0 (RN-08)
            //  - un registro por empleado y día (RN-07), solo obras activas y dentro de sus fechas (RN-06)
            return RedirectToPage(new { Semana = Semana.ToString("yyyy-MM-dd") });
        }

        private void CargarSemana()
        {
            if (Semana == default)
            {
                Semana = DateOnly.FromDateTime(DateTime.Today);
            }

            // Retrocede hasta el lunes de esa semana (en .NET el domingo es 0)
            int diasDesdeLunes = ((int)Semana.DayOfWeek + 6) % 7;
            Lunes = Semana.AddDays(-diasDesdeLunes);

            // Se trabaja de lunes a sábado
            Dias = Enumerable.Range(0, 6).Select(i => Lunes.AddDays(i)).ToList();

            // Solo empleados activos registran asistencia, y solo en obras activas (RN-06)
            EmpleadosActivos = DatosPrueba.Empleados
                .Where(e => e.Estatus == EstatusEmpleado.Activo)
                .ToList();

            ObrasActivas = DatosPrueba.Obras
                .Where(o => o.Estatus == EstatusObra.Activa)
                .Select(o => new SelectListItem(o.Nombre, o.IdObra.ToString()))
                .ToList();
        }
    }
}
