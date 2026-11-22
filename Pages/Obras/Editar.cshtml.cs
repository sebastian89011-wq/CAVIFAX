using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Obras
{
    public class EditarModel : PageModel
    {
        [BindProperty]
        public Obra Obra { get; set; } = new();

        public List<SelectListItem> Clientes { get; set; } = new();
        public List<SelectListItem> Responsables { get; set; } = new();
        public List<SelectListItem> Supervisores { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            var encontrada = DatosPrueba.Obras.FirstOrDefault(o => o.IdObra == id);
            if (encontrada == null)
            {
                return NotFound();
            }

            Obra = encontrada;
            CargarListas();
            return Page();
        }

        public IActionResult OnPost()
        {
            // Aquí el backend actualizará la obra en la base de datos
            return RedirectToPage("Index");
        }

        private void CargarListas()
        {
            // Datos de prueba. Puesto 1 = Ingeniero, Puesto 4 = Supervisor general
            Clientes = DatosPrueba.Clientes
                .Select(c => new SelectListItem(c.Nombre, c.IdCliente.ToString()))
                .ToList();

            Responsables = DatosPrueba.Empleados
                .Where(e => e.IdPuesto == 1 && e.Estatus == EstatusEmpleado.Activo)
                .Select(e => new SelectListItem(e.Nombre + " " + e.ApellidoPaterno, e.IdEmpleado.ToString()))
                .ToList();

            Supervisores = DatosPrueba.Empleados
                .Where(e => e.IdPuesto == 4 && e.Estatus == EstatusEmpleado.Activo)
                .Select(e => new SelectListItem(e.Nombre + " " + e.ApellidoPaterno, e.IdEmpleado.ToString()))
                .ToList();
        }
    }
}
