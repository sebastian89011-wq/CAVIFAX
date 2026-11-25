using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Cuenta
{
    public class CambiarContrasenaModel : PageModel
    {
        [BindProperty, Required(ErrorMessage = "Escribe tu contraseña actual."), DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string Actual { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Escribe la contraseña nueva."), DataType(DataType.Password)]
        [Display(Name = "Contraseña nueva")]
        public string Nueva { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Confirma la contraseña nueva."), DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña nueva")]
        public string Confirmacion { get; set; } = string.Empty;

        private readonly UsuarioService _usuarioService;

        public CambiarContrasenaModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // El id sale de la cookie, no del formulario: nadie puede cambiar la contraseña de otro
                _usuarioService.CambiarContrasena(User.IdUsuario() ?? 0, Actual, Nueva, Confirmacion);
                TempData["Mensaje"] = "Contraseña actualizada.";
                return RedirectToPage("/Index");
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
