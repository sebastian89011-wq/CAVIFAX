using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Puestos
{
    public class CrearModel : PageModel
    {
        [BindProperty]
        public Puesto Puesto { get; set; } = new();
        private readonly PuestoService _puestoService;

        public CrearModel(PuestoService puestoService)
        {
            this._puestoService = puestoService;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            try
            {
                _puestoService.Crear(Puesto);

                TempData["Mensaje"] = "Puesto registrado.";
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
