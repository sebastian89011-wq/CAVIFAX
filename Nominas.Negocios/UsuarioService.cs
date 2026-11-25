using System.Security.Cryptography;
using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

/// <summary>
/// Inicio de sesión del responsable (RN-15). Las contraseñas nunca se guardan tal cual:
/// se guarda un hash PBKDF2 con sal, con el formato
///     PBKDF2-SHA256$iteraciones$sal(Base64)$hash(Base64)
/// </summary>
public class UsuarioService
{
    private const string Algoritmo = "PBKDF2-SHA256";
    private const int Iteraciones = 100_000;   // más iteraciones = más lento adivinar por fuerza bruta
    private const int BytesSal = 16;
    private const int BytesHash = 32;
    public const int LongitudMinima = 8;

    private readonly NominasDbContext _db;

    public UsuarioService(NominasDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Devuelve el usuario si el nombre y la contraseña son correctos y la cuenta está activa;
    /// si no, devuelve null. No dice cuál de los dos falló, para no dar pistas a un atacante.
    /// </summary>
    public Usuario? ValidarCredenciales(string nombreUsuario, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(contrasena))
        {
            return null;
        }

        var usuario = _db.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario.Trim());
        if (usuario == null || !usuario.Activo)
        {
            return null;
        }

        return VerificarContrasena(contrasena, usuario.ContrasenaHash) ? usuario : null;
    }

    public Usuario? ObtenerPorId(int id)
    {
        return _db.Usuarios.Find(id);
    }

    /// <summary>Nombre corto del empleado dueño de la cuenta (nombre y apellido paterno), para la barra.</summary>
    public string NombreDelEmpleado(Usuario usuario)
    {
        var empleado = _db.Empleados.Find(usuario.IdEmpleado);
        return empleado == null ? usuario.NombreUsuario : $"{empleado.Nombre} {empleado.ApellidoPaterno}";
    }

    public void CambiarContrasena(int idUsuario, string actual, string nueva, string confirmacion)
    {
        var usuario = _db.Usuarios.Find(idUsuario);
        if (usuario == null || !usuario.Activo)
            throw new ValidacionException("La cuenta no existe o está desactivada.");

        if (!VerificarContrasena(actual ?? "", usuario.ContrasenaHash))
            throw new ValidacionException("La contraseña actual no es correcta.");

        if (string.IsNullOrEmpty(nueva) || nueva.Length < LongitudMinima)
            throw new ValidacionException($"La contraseña nueva debe tener al menos {LongitudMinima} caracteres.");

        if (nueva != confirmacion)
            throw new ValidacionException("La confirmación no coincide con la contraseña nueva.");

        if (nueva == actual)
            throw new ValidacionException("La contraseña nueva debe ser distinta de la actual.");

        usuario.ContrasenaHash = HashContrasena(nueva);
        _db.SaveChanges();
    }

    // ------------------------------------------------------------------ Hash de contraseñas

    /// <summary>
    /// Genera el hash de una contraseña. Cada llamada usa una sal aleatoria distinta,
    /// así que la misma contraseña produce hashes diferentes.
    /// </summary>
    public static string HashContrasena(string contrasena)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(BytesSal);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);
        return $"{Algoritmo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>Recalcula el hash con la misma sal e iteraciones y lo compara con el guardado.</summary>
    public static bool VerificarContrasena(string contrasena, string hashGuardado)
    {
        var partes = (hashGuardado ?? "").Split('$');
        if (partes.Length != 4 || partes[0] != Algoritmo || !int.TryParse(partes[1], out int iteraciones))
        {
            return false;   // formato desconocido (por ejemplo, el hash de ejemplo del script original)
        }

        try
        {
            byte[] sal = Convert.FromBase64String(partes[2]);
            byte[] esperado = Convert.FromBase64String(partes[3]);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

            // Comparación en tiempo constante: no revela cuántos bytes coincidieron
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
