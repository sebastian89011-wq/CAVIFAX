using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Clientes
{
    public class IndexModel : PageModel
    {
        public List<Cliente> Clientes { get; set; } = new();
        private readonly ClienteService _clienteService;

        public IndexModel(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public void OnGet()
        {
            Clientes = _clienteService.Listar();
        }
    }
}
