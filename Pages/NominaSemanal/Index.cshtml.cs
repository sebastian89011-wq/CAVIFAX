using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.NominaSemanal
{
    public class IndexModel : PageModel
    {
        public List<Nomina> Nominas { get; set; } = new();

        // Cualquier día de la semana que se quiere generar
        [BindProperty]
        public DateOnly SemanaAGenerar { get; set; }

        public void OnGet()
        {
            // Datos de prueba, de la semana más reciente a la más antigua
            Nominas = DatosPrueba.Nominas.OrderByDescending(n => n.FechaInicio).ToList();
            SemanaAGenerar = DateOnly.FromDateTime(DateTime.Today);
        }

        public IActionResult OnPost()
        {
            // Aquí el backend generará el borrador a partir de las asistencias de esa semana.
            // Si ya existe una nómina para esa semana, devuelve la existente (RN-09: una por semana).
            int idNomina = 3; // dato de prueba: el borrador actual
            return RedirectToPage("Detalle", new { id = idNomina });
        }

        public decimal TotalNeto(int idNomina)
        {
            return DatosPrueba.PagosEmpleado.Where(p => p.IdNomina == idNomina).Sum(p => p.TotalNeto);
        }

        public int PagosPendientes(int idNomina)
        {
            return DatosPrueba.PagosEmpleado.Count(p => p.IdNomina == idNomina && p.Estatus == EstatusPago.Pendiente);
        }
    }
}
