using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Clientes
{
    public class IndexModel : PageModel
    {
        public List<Cliente> Clientes { get; set; } = new();
        public void OnGet()
        {
            Clientes = new List<Cliente>
            {
                new Cliente { IdCliente = 1, Nombre = "Constructora del Valle", Domicilio = "Lomas de Valle, Duraznos 555" },
                new Cliente { IdCliente = 2, Nombre = "Inmobiliaria Pacífico", Domicilio = "Lomas Turbas, Alisos 67" },
                new Cliente { IdCliente = 3, Nombre = "Cliente3", Domicilio = "Vampiros y Rituales" }
            };
        }

    }
}
