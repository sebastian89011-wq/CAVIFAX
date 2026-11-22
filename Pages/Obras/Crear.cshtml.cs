using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Obras
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Obra Obra { get; set; } = new();

        public List<SelectListItem> Clientes { get; set; } = new();
        public List<SelectListItem> Responsables { get; set; } = new();
        public List<SelectListItem> Supervisores { get; set; } = new();

        public void OnGet()
        {
            CargarListas();
            Obra.FechaInicio = DateOnly.FromDateTime(DateTime.Today);
            Obra.FechaFinProgramada = Obra.FechaInicio.AddMonths(3);
        }

        public IActionResult OnPost()
        {
            // Aquí el backend validará (RN-05) y guardará la obra con estatus Activa
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
