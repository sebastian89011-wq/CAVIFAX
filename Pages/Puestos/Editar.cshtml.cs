using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Puestos
{
    public class EditarModel : PageModel
    {
        [BindProperty]
        public Puesto Puesto { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            // Datos de prueba: el backend los cambiará por la búsqueda en la base de datos
            var puestos = new List<Puesto>
        {
            new Puesto { IdPuesto = 1, Nombre = "Ingeniero", Descripcion = "Responsable de obra" },
            new Puesto { IdPuesto = 2, Nombre = "Albañil", Descripcion = "Construcción general" },
            new Puesto { IdPuesto = 3, Nombre = "Ayudante general" }
        };

            var encontrado = puestos.FirstOrDefault(p => p.IdPuesto == id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Puesto = encontrado;
            return Page();
        }

        public IActionResult OnPost()
        {
            // Aquí el backend actualizará el puesto en la base de datos
            return RedirectToPage("Index");
        }
    }

}
