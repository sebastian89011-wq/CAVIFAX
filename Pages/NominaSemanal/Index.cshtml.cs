using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.NominaSemanal
{
    public class IndexModel : PageModel
    {
        public List<Nomina> Nominas { get; set; } = new();

        // Cualquier día de la semana que se quiere generar
        [BindProperty]
        public DateOnly SemanaAGenerar { get; set; }

        private readonly NominaService _nominaService;

        public IndexModel(NominaService nominaService)
        {
            _nominaService = nominaService;
        }

        public void OnGet()
        {
            Nominas = _nominaService.Listar();
            SemanaAGenerar = DateOnly.FromDateTime(DateTime.Today);
        }

        public IActionResult OnPost()
        {
            try
            {
                // Si ya existe una nómina para esa semana, devuelve la existente (RN-09)
                int idNomina = _nominaService.GenerarBorrador(SemanaAGenerar);
                return RedirectToPage("Detalle", new { id = idNomina });
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Nominas = _nominaService.Listar();
                return Page();
            }
        }

        public decimal TotalNeto(int idNomina)
        {
            return _nominaService.TotalNeto(idNomina);
        }

        public int PagosPendientes(int idNomina)
        {
            return _nominaService.ContarPagosPendientes(idNomina);
        }

        public string Semana(Nomina n)
        {
            return NominaService.Semana(n);
        }
    }
}
