using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nominas.Web.Pages.Cuenta
{
    // Cerrar sesión es solo POST (botón en un formulario con token antifalsificación):
    // así otro sitio no puede sacarte de la sesión con un simple enlace o imagen.
    public class SalirModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Index");

        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Cuenta/Login");
        }
    }
}
