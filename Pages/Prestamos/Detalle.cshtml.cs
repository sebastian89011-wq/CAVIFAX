using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Prestamos
{
    public class DetalleModel : PageModel
    {
        public Prestamo Prestamo { get; set; } = new();
        public List<AbonoPrestamo> Abonos { get; set; } = new();
        public string NombreEmpleado { get; set; } = string.Empty;
        private List<Nomina> _nominas = new();
        private readonly PrestamoService _prestamoService;
        private readonly EmpleadoService _empleadoService;

        public DetalleModel(PrestamoService prestamoService, EmpleadoService empleadoService)
        {
            _prestamoService = prestamoService;
            _empleadoService = empleadoService;
        }

        public IActionResult OnGet(int id)
        {
            var encontrado = _prestamoService.ObtenerPorId(id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Prestamo = encontrado;
            Abonos = _prestamoService.ListarAbonos(id);
            _nominas = _prestamoService.NominasDeAbonos(Abonos);

            var empleado = _empleadoService.ObtenerPorId(Prestamo.IdEmpleado);
            NombreEmpleado = empleado == null ? "—" : EmpleadoService.NombreCompleto(empleado);
            return Page();
        }

        public string SemanaDeNomina(int idNomina)
        {
            var n = _nominas.FirstOrDefault(x => x.IdNomina == idNomina);
            return n == null ? "—" : $"{n.FechaInicio:dd/MM} al {n.FechaFin:dd/MM/yyyy}";
        }
    }
}
