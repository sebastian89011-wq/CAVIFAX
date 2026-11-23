using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Prestamos
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Prestamo Prestamo { get; set; } = new();

        public List<SelectListItem> Empleados { get; set; } = new();
        private readonly PrestamoService _prestamoService;

        public CrearModel(PrestamoService prestamoService)
        {
            _prestamoService = prestamoService;
        }

        public void OnGet()
        {
            CargarEmpleados();
            Prestamo.Fecha = DateOnly.FromDateTime(DateTime.Today);
        }

        public IActionResult OnPost()
        {
            try
            {
                _prestamoService.Crear(Prestamo);
                return RedirectToPage("Index");
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                CargarEmpleados();
                return Page();
            }
        }

        private void CargarEmpleados()
        {
            // RN-14: solo empleados activos sin otro préstamo o anticipo activo
            Empleados = _prestamoService.EmpleadosDisponibles()
                .Select(e => new SelectListItem(EmpleadoService.NombreCompleto(e), e.IdEmpleado.ToString()))
                .ToList();
        }
    }
}
