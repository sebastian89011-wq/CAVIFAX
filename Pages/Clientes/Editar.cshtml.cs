using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Clientes
{
    public class EditarModel : PageModel
    {
        [BindProperty]
        public Cliente Cliente { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            // Datos de prueba: el backend los cambiará por la búsqueda en la base de datos
            var clientes = new List<Cliente>
        {
            new Cliente { IdCliente = 1, Nombre = "Constructora del Valle", Domicilio = "Lomas de Valle, Duraznos 555" },
            new Cliente { IdCliente = 2, Nombre = "Inmobiliaria Pacífico", Domicilio = "Lomas Turbas, Alisos 67" },
            new Cliente { IdCliente = 3, Nombre = "Cliente3", Domicilio = "Vampiros y Rituales" }
        };

            var encontrado = clientes.FirstOrDefault(c => c.IdCliente == id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Cliente = encontrado;
            return Page();
        }

        public IActionResult OnPost()
        {
            // Aquí el backend actualizará el cliente en la base de datos
            return RedirectToPage("Index");
        }
    }
}
