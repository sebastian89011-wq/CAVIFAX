using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Puestos
{
    public class EditarModel : PageModel

    {
        [BindProperty]
        public Puesto Puesto { get; set; } = new();
        private readonly PuestoService _puestoService;

        public EditarModel(PuestoService puestoService) {

            _puestoService = puestoService;
        
        }

        public IActionResult OnGet(int id)
        {
            // Datos de prueba: el backend los cambiará por la búsqueda en la base de datos
           

            var encontrado = _puestoService.ObtenerPorId(id);
            if (encontrado == null)
            {
                return NotFound();
            }

            Puesto = encontrado;
            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                _puestoService.Editar(Puesto);
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
