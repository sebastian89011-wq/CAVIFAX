using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Obras
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Obra Obra { get; set; } = new();

        public List<SelectListItem> Clientes { get; set; } = new();
        public List<SelectListItem> Responsables { get; set; } = new();
        public List<SelectListItem> Supervisores { get; set; } = new();
        private readonly ObraService _obraService;
        private readonly ClienteService _clienteService;
        private readonly EmpleadoService _empleadoService;
        private readonly PuestoService _puestoService;

        public CrearModel(ObraService obraService, ClienteService clienteService,
                          EmpleadoService empleadoService, PuestoService puestoService)
        {
            _obraService = obraService;
            _clienteService = clienteService;
            _empleadoService = empleadoService;
            _puestoService = puestoService;
        }

        public void OnGet()
        {
            CargarListas();
            Obra.FechaInicio = DateOnly.FromDateTime(DateTime.Today);
            Obra.FechaFinProgramada = Obra.FechaInicio.AddMonths(3);
        }

        public IActionResult OnPost()
        {
            try
            {
                _obraService.Crear(Obra);
                TempData["Mensaje"] = "Obra registrada.";
                return RedirectToPage("Index");
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                CargarListas();
                return Page();
            }
        }

        private void CargarListas()
        {
            Clientes = _clienteService.Listar()
                .Select(c => new SelectListItem(c.Nombre, c.IdCliente.ToString()))
                .ToList();

            // Responsable y supervisor: cualquier empleado activo; se muestra su puesto para elegir mejor
            var puestos = _puestoService.Listar();
            var empleados = _empleadoService.ListarActivos()
                .Select(e => new SelectListItem(
                    EmpleadoService.NombreCompleto(e) + " (" + (puestos.FirstOrDefault(p => p.IdPuesto == e.IdPuesto)?.Nombre ?? "—") + ")",
                    e.IdEmpleado.ToString()))
                .ToList();

            Responsables = empleados;
            Supervisores = empleados;
        }
    }
}
