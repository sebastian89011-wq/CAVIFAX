using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Pruebas;

/// <summary>Inicio de sesión (RN-15): hash de contraseñas, credenciales y cambio de contraseña.</summary>
public class UsuariosPruebas : BaseDatosPrueba
{
    private const string Contrasena = "Prueba-Segura-2026";

    private Usuario NuevoUsuario(string nombre = "rmendoza", bool activo = true)
    {
        var usuario = new Usuario
        {
            IdEmpleado = NuevoEmpleado("Roberto", "Mendoza").IdEmpleado,
            NombreUsuario = nombre,
            ContrasenaHash = UsuarioService.HashContrasena(Contrasena),
            Activo = activo
        };
        Db.Usuarios.Add(usuario);
        Db.SaveChanges();
        return usuario;
    }

    // ---------------------------------------------------------------- Hash

    [Fact]
    public void Hash_NoGuardaLaContrasenaEnTexto()
    {
        string hash = UsuarioService.HashContrasena(Contrasena);
        Assert.DoesNotContain(Contrasena, hash);
        Assert.StartsWith("PBKDF2-SHA256$", hash);
    }

    [Fact]
    public void Hash_LaMismaContrasenaDaHashesDistintos_PorLaSal()
    {
        Assert.NotEqual(UsuarioService.HashContrasena(Contrasena), UsuarioService.HashContrasena(Contrasena));
    }

    [Fact]
    public void Hash_VerificaLaCorrectaYRechazaOtra()
    {
        string hash = UsuarioService.HashContrasena(Contrasena);
        Assert.True(UsuarioService.VerificarContrasena(Contrasena, hash));
        Assert.False(UsuarioService.VerificarContrasena("prueba-segura-2026", hash));   // distingue mayúsculas
    }

    [Fact]
    public void Hash_FormatoDesconocido_NoEntra()
    {
        // El hash de ejemplo que traía el script original no es de ninguna contraseña
        Assert.False(UsuarioService.VerificarContrasena("lo que sea", "$2a$12$KIX9v8ExampleHashBcrypt"));
        Assert.False(UsuarioService.VerificarContrasena("lo que sea", ""));
    }

    // ---------------------------------------------------------------- Credenciales

    [Fact]
    public void Login_Correcto_DevuelveElUsuario()
    {
        var usuario = NuevoUsuario();
        var resultado = Usuarios.ValidarCredenciales("rmendoza", Contrasena);
        Assert.NotNull(resultado);
        Assert.Equal(usuario.IdUsuario, resultado!.IdUsuario);
    }

    [Fact]
    public void Login_ContrasenaIncorrecta_DevuelveNull()
    {
        NuevoUsuario();
        Assert.Null(Usuarios.ValidarCredenciales("rmendoza", "otra-contrasena"));
    }

    [Fact]
    public void Login_UsuarioInexistente_DevuelveNull()
    {
        NuevoUsuario();
        Assert.Null(Usuarios.ValidarCredenciales("nadie", Contrasena));
    }

    [Fact]
    public void Login_CuentaDesactivada_NoEntra()
    {
        NuevoUsuario(activo: false);
        Assert.Null(Usuarios.ValidarCredenciales("rmendoza", Contrasena));
    }

    [Fact]
    public void Login_CamposVacios_DevuelveNull()
    {
        NuevoUsuario();
        Assert.Null(Usuarios.ValidarCredenciales("", ""));
    }

    [Fact]
    public void NombreDelEmpleado_ParaLaBarra()
    {
        var usuario = NuevoUsuario();
        Assert.Equal("Roberto Mendoza", Usuarios.NombreDelEmpleado(usuario));
    }

    // ---------------------------------------------------------------- Cambiar contraseña

    [Fact]
    public void CambiarContrasena_LaNuevaFuncionaYLaViejaNo()
    {
        var usuario = NuevoUsuario();
        Usuarios.CambiarContrasena(usuario.IdUsuario, Contrasena, "Nueva-Clave-99", "Nueva-Clave-99");

        Assert.NotNull(Usuarios.ValidarCredenciales("rmendoza", "Nueva-Clave-99"));
        Assert.Null(Usuarios.ValidarCredenciales("rmendoza", Contrasena));
    }

    [Fact]
    public void CambiarContrasena_ActualIncorrecta()
    {
        var usuario = NuevoUsuario();
        DebeFallar(() => Usuarios.CambiarContrasena(usuario.IdUsuario, "mal", "Nueva-Clave-99", "Nueva-Clave-99"), "actual");
    }

    [Fact]
    public void CambiarContrasena_MuyCorta()
    {
        var usuario = NuevoUsuario();
        DebeFallar(() => Usuarios.CambiarContrasena(usuario.IdUsuario, Contrasena, "corta", "corta"), "8");
    }

    [Fact]
    public void CambiarContrasena_ConfirmacionDistinta()
    {
        var usuario = NuevoUsuario();
        DebeFallar(() => Usuarios.CambiarContrasena(usuario.IdUsuario, Contrasena, "Nueva-Clave-99", "Nueva-Clave-98"), "confirmación");
    }
}
