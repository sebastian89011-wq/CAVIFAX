using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;
using Nominas.Negocios;

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

        private readonly AsistenciaService _asistenciaService;
        private readonly EmpleadoService _empleadoService;
        private readonly ObraService _obraService;

        public IndexModel(AsistenciaService asistenciaService, EmpleadoService empleadoService, ObraService obraService)
        {
            _asistenciaService = asistenciaService;
            _empleadoService = empleadoService;
            _obraService = obraService;
        }

        public void OnGet()
        {
            var capturadas = CargarSemana();
            Registros = ArmarCuadricula(capturadas);
        }

        public IActionResult OnPost()
        {
            // Si una celda trae algo que no es número, el model binding ya lo marcó como error
            var errores = new List<string>();
            if (!ModelState.IsValid)
            {
                errores.Add("Las horas extra deben ser números.");
            }
            else
            {
                try
                {
                    _asistenciaService.GuardarSemana(Registros);
                    return RedirectToPage(new { Semana = Semana.ToString("yyyy-MM-dd") });
                }
                catch (ValidacionException ex)
                {
                    errores.AddRange(ex.Message.Split('\n'));
                }
            }

            // Se vuelve a mostrar lo que el usuario capturó (no lo guardado), acomodado por empleado y día.
            // Se limpia ModelState para que cada celda tome su valor de Registros y no del índice enviado.
            var capturadas = CargarSemana();
            var enviados = Registros;
            ModelState.Clear();
            foreach (var error in errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            Registros = ArmarCuadricula(enviados.Concat(capturadas).ToList());
            return Page();
        }

        // Una celda por empleado activo y día, en el orden de la tabla. Si la fuente trae
        // un registro para esa celda se usa el primero que aparezca; si no, se crea vacía.
        private List<Asistencia> ArmarCuadricula(List<Asistencia> fuente)
        {
            var cuadricula = new List<Asistencia>();
            foreach (var empleado in EmpleadosActivos)
            {
                foreach (var dia in Dias)
                {
                    var existente = fuente
                        .FirstOrDefault(a => a.IdEmpleado == empleado.IdEmpleado && a.Fecha == dia);

                    cuadricula.Add(existente ?? new Asistencia { IdEmpleado = empleado.IdEmpleado, Fecha = dia });
                }
            }
            return cuadricula;
        }

        private List<Asistencia> CargarSemana()
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

            // Solo empleados activos registran asistencia (RN-06)
            EmpleadosActivos = _empleadoService.ListarActivos();

            var capturadas = _asistenciaService.ListarPorPeriodo(Lunes, Dias[^1]);

            // Obras activas, más las que ya aparecen en la semana aunque ya no estén activas,
            // para que la celda muestre la obra guardada y no "Falta"
            var idsCapturadas = capturadas.Where(a => a.IdObra != null).Select(a => a.IdObra!.Value).ToHashSet();
            ObrasActivas = _obraService.Listar()
                .Where(o => o.Estatus == EstatusObra.Activa || idsCapturadas.Contains(o.IdObra))
                .Select(o => new SelectListItem(o.Nombre, o.IdObra.ToString()))
                .ToList();

            return capturadas;
        }
    }
}
