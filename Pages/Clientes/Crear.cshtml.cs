using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
namespace Nominas.Web.Pages.Clientes
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Cliente Cliente { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Aquí el backend guardará el cliente en la base de datos
            return RedirectToPage("Index");
        }
    }
}

