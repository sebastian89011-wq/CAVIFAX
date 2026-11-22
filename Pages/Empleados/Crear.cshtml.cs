using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Empleados
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Empleado Empleado { get; set; } = new();

        public List<SelectListItem> Puestos { get; set; } = new();
        private readonly EmpleadoService _empleadoService;
        private readonly PuestoService _puestoService;

        public CrearModel(EmpleadoService empleadoService, PuestoService puestoService)
        {
            _empleadoService = empleadoService;
            _puestoService = puestoService;
        }

        public void OnGet()
        {
            CargarPuestos();
            Empleado.FechaIngreso = DateOnly.FromDateTime(DateTime.Today);
        }

        public IActionResult OnPost()
        {
            try
            {
                _empleadoService.Crear(Empleado);
                return RedirectToPage("Index");
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                CargarPuestos(); // la lista no viaja en el formulario: hay que volver a llenarla
                return Page();
            }
        }

        private void CargarPuestos()
        {
            Puestos = _puestoService.Listar()
                .Select(p => new SelectListItem(p.Nombre, p.IdPuesto.ToString()))
                .ToList();
        }
    }
}
