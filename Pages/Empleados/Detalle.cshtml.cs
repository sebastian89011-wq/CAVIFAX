using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Empleados
{
    public class DetalleModel : PageModel
    {
        public Empleado Empleado { get; set; } = new();
        public string NombrePuesto { get; set; } = string.Empty;

        public IActionResult OnGet(int id)
        {
            var encontrado = DatosPrueba.Empleados.FirstOrDefault(e => e.IdEmpleado == id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Empleado = encontrado;
            NombrePuesto = DatosPrueba.Puestos.FirstOrDefault(p => p.IdPuesto == Empleado.IdPuesto)?.Nombre ?? "—";
            return Page();
        }
    }
}
