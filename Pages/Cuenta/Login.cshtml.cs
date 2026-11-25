using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Cuenta
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Escribe tu usuario.")]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Escribe tu contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Mantener la sesión iniciada")]
        public bool Recordarme { get; set; }

        // Página a la que se quería entrar antes de que pidiera el login
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        private readonly UsuarioService _usuarioService;

        public LoginModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public IActionResult OnGet()
        {
            // Si ya hay sesión, no tiene caso mostrar el login
            if (User.Identity?.IsAuthenticated == true)
            {
                return Redirect(DestinoSeguro());
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var usuario = _usuarioService.ValidarCredenciales(NombreUsuario, Contrasena);
            if (usuario == null)
            {
                // Mismo mensaje si falla el usuario o la contraseña: no se dan pistas
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                Contrasena = string.Empty;
                return Page();
            }

            var principal = Sesion.CrearPrincipal(usuario, _usuarioService.NombreDelEmpleado(usuario),
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = Recordarme });

            return Redirect(DestinoSeguro());
        }

        // Solo se regresa a direcciones de este mismo sitio. Si no, alguien podría mandar
        // un enlace "/Cuenta/Login?ReturnUrl=https://sitio-falso.com" (redirección abierta).
        private string DestinoSeguro()
        {
            return !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/";
        }
    }
}
