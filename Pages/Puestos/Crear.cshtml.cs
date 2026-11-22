using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
namespace Nominas.Web.Pages.Puestos
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Puesto Puesto { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Aquí el backend guardará el puesto en la base de datos
            return RedirectToPage("Index");
        }
    }
}
