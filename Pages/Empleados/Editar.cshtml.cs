using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Empleados
{
    public class EditarModel : PageModel
    {
        [BindProperty]
        public Empleado Empleado { get; set; } = new();

        public List<SelectListItem> Puestos { get; set; } = new();
        private readonly EmpleadoService _empleadoService;
        private readonly PuestoService _puestoService;

        public EditarModel(EmpleadoService empleadoService, PuestoService puestoService)
        {
            _empleadoService = empleadoService;
            _puestoService = puestoService;
        }

        public IActionResult OnGet(int id)
        {
            var encontrado = _empleadoService.ObtenerPorId(id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Empleado = encontrado;
            CargarPuestos();
            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                _empleadoService.Editar(Empleado);
                TempData["Mensaje"] = "Empleado actualizado.";
                return RedirectToPage("Index");
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                CargarPuestos();
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
