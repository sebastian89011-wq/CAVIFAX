using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Prestamos
{
    public class IndexModel : PageModel
    {
        public List<Prestamo> Prestamos { get; set; } = new();
        private List<Empleado> _empleados = new();
        private readonly PrestamoService _prestamoService;
        private readonly EmpleadoService _empleadoService;

        public IndexModel(PrestamoService prestamoService, EmpleadoService empleadoService)
        {
            _prestamoService = prestamoService;
            _empleadoService = empleadoService;
        }

        public void OnGet()
        {
            Prestamos = _prestamoService.Listar();
            _empleados = _empleadoService.Listar();
        }

        public string NombreEmpleado(int idEmpleado)
        {
            var e = _empleados.FirstOrDefault(x => x.IdEmpleado == idEmpleado);
            return e == null ? "—" : EmpleadoService.NombreCompleto(e);
        }
    }
}
