using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Empleados
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Empleado Empleado { get; set; } = new();

        public List<SelectListItem> Puestos { get; set; } = new();

        public void OnGet()
        {
            CargarPuestos();
            Empleado.FechaIngreso = DateOnly.FromDateTime(DateTime.Today);
        }

        public IActionResult OnPost()
        {
            // Aquí el backend guardará el empleado (con estatus Activo) en la base de datos
            return RedirectToPage("Index");
        }

        private void CargarPuestos()
        {
            // Datos de prueba: el backend los cambiará por la consulta de puestos
            Puestos = new List<SelectListItem>
    {
        new SelectListItem("Ingeniero", "1"),
        new SelectListItem("Albañil", "2"),
        new SelectListItem("Ayudante general", "3")
    };
        }
    }
}
