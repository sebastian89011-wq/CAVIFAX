namespace Nominas.Entidades;

/// <summary>Usuario con acceso al sistema.</summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    /// <summary>FK a Empleado.</summary>
    public int IdEmpleado { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    /// <summary>Hash de la contraseña; nunca la contraseña en texto plano.</summary>
    public string ContrasenaHash { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
