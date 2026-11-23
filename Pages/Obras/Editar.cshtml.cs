using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Obras
{
    public class EditarModel : PageModel
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

        public EditarModel(ObraService obraService, ClienteService clienteService,
                           EmpleadoService empleadoService, PuestoService puestoService)
        {
            _obraService = obraService;
            _clienteService = clienteService;
            _empleadoService = empleadoService;
            _puestoService = puestoService;
        }

        public IActionResult OnGet(int id)
        {
            var encontrada = _obraService.ObtenerPorId(id);
            if (encontrada == null)
            {
                return NotFound();
            }

            Obra = encontrada;
            CargarListas();
            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                _obraService.Editar(Obra);
                TempData["Mensaje"] = "Obra actualizada.";
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

            // Empleados activos, más el responsable y el supervisor actuales aunque ya no estén activos
            var puestos = _puestoService.Listar();
            var candidatos = _empleadoService.ListarActivos();
            foreach (int idActual in new[] { Obra.IdResponsable, Obra.IdSupervisor })
            {
                if (!candidatos.Any(e => e.IdEmpleado == idActual))
                {
                    var actual = _empleadoService.ObtenerPorId(idActual);
                    if (actual != null) candidatos.Add(actual);
                }
            }

            var empleados = candidatos
                .Select(e => new SelectListItem(
                    EmpleadoService.NombreCompleto(e) + " (" + (puestos.FirstOrDefault(p => p.IdPuesto == e.IdPuesto)?.Nombre ?? "—") + ")",
                    e.IdEmpleado.ToString()))
                .ToList();

            Responsables = empleados;
            Supervisores = empleados;
        }
    }
}
