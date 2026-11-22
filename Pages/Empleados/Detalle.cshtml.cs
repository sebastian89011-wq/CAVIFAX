using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Empleados
{
    public class DetalleModel : PageModel
    {
        public Empleado Empleado { get; set; } = new();
        public string NombrePuesto { get; set; } = string.Empty;
        private readonly EmpleadoService _empleadoService;
        private readonly PuestoService _puestoService;

        public DetalleModel(EmpleadoService empleadoService, PuestoService puestoService)
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
            NombrePuesto = _puestoService.ObtenerPorId(Empleado.IdPuesto)?.Nombre ?? "—";
            return Page();
        }
    }
}
