using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Prestamos
{
    public class IndexModel : PageModel
    {
        public List<Prestamo> Prestamos { get; set; } = new();

        public void OnGet()
        {
            // Datos de prueba. Primero los activos, luego los más recientes
            Prestamos = DatosPrueba.Prestamos
                .OrderBy(p => p.Estatus)
                .ThenByDescending(p => p.Fecha)
                .ToList();
        }
    }
}
