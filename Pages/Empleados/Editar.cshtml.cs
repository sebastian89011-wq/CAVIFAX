using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Empleados
{
    public class EditarModel : PageModel
    {
        [BindProperty]
        public Empleado Empleado { get; set; } = new();

        public List<SelectListItem> Puestos { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            var encontrado = DatosPrueba.Empleados.FirstOrDefault(e => e.IdEmpleado == id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Empleado = encontrado;
            Puestos = DatosPrueba.Puestos
                .Select(p => new SelectListItem(p.Nombre, p.IdPuesto.ToString()))
                .ToList();
            return Page();
        }

        public IActionResult OnPost()
        {
            // Aquí el backend actualizará el empleado en la base de datos
            return RedirectToPage("Index");
        }
    }
}
