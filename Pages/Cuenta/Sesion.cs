using System.Security.Claims;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Cuenta
{
    /// <summary>
    /// Lo que se guarda dentro de la cookie de sesión. Cada dato es un "claim":
    /// un par tipo/valor que el servidor firmó y que el usuario no puede alterar.
    /// </summary>
    public static class Sesion
    {
        public const string ClaimNombreCompleto = "nombre_completo";
        public const string ClaimIdEmpleado = "id_empleado";

        public static ClaimsPrincipal CrearPrincipal(Usuario usuario, string nombreCompleto, string esquema)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new(ClaimNombreCompleto, nombreCompleto),
                new(ClaimIdEmpleado, usuario.IdEmpleado.ToString())
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, esquema));
        }

        /// <summary>Id del usuario que inició sesión, o null si no hay sesión.</summary>
        public static int? IdUsuario(this ClaimsPrincipal user)
        {
            return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;
        }

        public static string NombreCompleto(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimNombreCompleto) ?? user.Identity?.Name ?? "";
        }
    }
}
