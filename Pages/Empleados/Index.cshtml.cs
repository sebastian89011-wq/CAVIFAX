using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Empleados
{
    public class IndexModel : PageModel
    {
        public List<Empleado> Empleados { get; set; } = new();
        public List<Puesto> Puestos { get; set; } = new();

        public void OnGet()
        {
            Puestos = DatosPrueba.Puestos;
            Empleados = DatosPrueba.Empleados;
        }

        public string NombrePuesto(int idPuesto)
        {
            var puesto = Puestos.FirstOrDefault(p => p.IdPuesto == idPuesto);
            return puesto?.Nombre ?? "—";
        }
    }
}
