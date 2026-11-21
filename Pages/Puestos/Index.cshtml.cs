using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Puestos
{
    public class IndexModel : PageModel
    {
        public List<Puesto> Puestos { get; set; } = new();

        public void OnGet()
        {
            Puestos = new List<Puesto>
            {
                new Puesto { IdPuesto = 1, Nombre = "Ingeniero", Descripcion = "Responsable de obra" },
                new Puesto { IdPuesto = 2, Nombre = "Albañil", Descripcion = "Construcción general" },
                new Puesto { IdPuesto = 3, Nombre = "Ayudante general" }
            };
        }
    }
}
