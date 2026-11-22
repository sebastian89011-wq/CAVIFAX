using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Prestamos
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Prestamo Prestamo { get; set; } = new();

        public List<SelectListItem> Empleados { get; set; } = new();

        public void OnGet()
        {
            CargarEmpleados();
            Prestamo.Fecha = DateOnly.FromDateTime(DateTime.Today);
        }

        public IActionResult OnPost()
        {
            // Aquí el backend guardará el préstamo con SaldoPendiente = Monto y Estatus = Activo
            return RedirectToPage("Index");
        }

        private void CargarEmpleados()
        {
            // RN-14: un empleado solo puede tener un préstamo o anticipo activo a la vez
            var conPrestamoActivo = DatosPrueba.Prestamos
                .Where(p => p.Estatus == EstatusPrestamo.Activo)
                .Select(p => p.IdEmpleado)
                .ToList();

            Empleados = DatosPrueba.Empleados
                .Where(e => e.Estatus == EstatusEmpleado.Activo && !conPrestamoActivo.Contains(e.IdEmpleado))
                .Select(e => new SelectListItem(e.Nombre + " " + e.ApellidoPaterno, e.IdEmpleado.ToString()))
                .ToList();
        }
    }
}
