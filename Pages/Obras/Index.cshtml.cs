using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Obras
{
    public class IndexModel : PageModel
    {
        public List<Obra> Obras { get; set; } = new();
        private List<Cliente> _clientes = new();

        // Filtros de búsqueda (vienen en la URL: ?Buscar=...&Estatus=...)
        [BindProperty(SupportsGet = true)]
        public string? Buscar { get; set; }

        [BindProperty(SupportsGet = true)]
        public EstatusObra? Estatus { get; set; }

        public bool HayFiltro => !string.IsNullOrWhiteSpace(Buscar) || Estatus != null;
        private readonly ObraService _obraService;
        private readonly ClienteService _clienteService;

        public IndexModel(ObraService obraService, ClienteService clienteService)
        {
            _obraService = obraService;
            _clienteService = clienteService;
        }

        public void OnGet()
        {
            Obras = _obraService.Buscar(Buscar, Estatus);
            _clientes = _clienteService.Listar();
        }

        public string NombreCliente(int idCliente)
        {
            return _clientes.FirstOrDefault(c => c.IdCliente == idCliente)?.Nombre ?? "—";
        }

        // Lo gastado lo calcula Negocios con las nóminas autorizadas (RN-12)
        public decimal Gastado(int idObra)
        {
            return _obraService.Gastado(idObra);
        }
    }
}
