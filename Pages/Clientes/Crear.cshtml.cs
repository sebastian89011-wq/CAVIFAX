using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Clientes
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Cliente Cliente { get; set; } = new();
        private readonly ClienteService _clienteService;

        public CrearModel(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            try
            {
                _clienteService.Crear(Cliente);
                return RedirectToPage("Index");
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
