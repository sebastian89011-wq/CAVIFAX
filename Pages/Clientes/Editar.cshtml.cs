using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Clientes
{
    public class EditarModel : PageModel
    {
        [BindProperty]
        public Cliente Cliente { get; set; } = new();
        private readonly ClienteService _clienteService;

        public EditarModel(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public IActionResult OnGet(int id)
        {
            var encontrado = _clienteService.ObtenerPorId(id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Cliente = encontrado;
            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                _clienteService.Editar(Cliente);
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
