using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Obras
{
    public class IndexModel : PageModel
    {
        public List<Obra> Obras { get; set; } = new();

        public void OnGet()
        {
            Obras = DatosPrueba.Obras;
        }

        public string NombreCliente(int idCliente)
        {
            return DatosPrueba.Clientes.FirstOrDefault(c => c.IdCliente == idCliente)?.Nombre ?? "—";
        }

        public decimal Gastado(int idObra)
        {
            return DatosPrueba.GastadoPorObra.GetValueOrDefault(idObra);
        }
    }
}
