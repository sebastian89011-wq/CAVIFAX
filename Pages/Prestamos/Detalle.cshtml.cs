using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Prestamos
{
    public class DetalleModel : PageModel
    {
        public Prestamo Prestamo { get; set; } = new();
        public List<AbonoPrestamo> Abonos { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            var encontrado = DatosPrueba.Prestamos.FirstOrDefault(p => p.IdPrestamo == id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Prestamo = encontrado;
            Abonos = DatosPrueba.AbonosPrestamo
                .Where(a => a.IdPrestamo == id)
                .OrderBy(a => a.Fecha)
                .ToList();
            return Page();
        }

        public string SemanaDeNomina(int idNomina)
        {
            var nomina = DatosPrueba.Nominas.FirstOrDefault(n => n.IdNomina == idNomina);
            return nomina == null ? "—" : DatosPrueba.Semana(nomina);
        }
    }
}
