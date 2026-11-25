using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Nominas.Datos;
using Nominas.Negocios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NominasDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("Nominas")!));

builder.Services.AddScoped<PuestoService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<EmpleadoService>();
builder.Services.AddScoped<ObraService>();
builder.Services.AddScoped<PrestamoService>();
builder.Services.AddScoped<AsistenciaService>();
builder.Services.AddScoped<NominaService>();
builder.Services.AddScoped<UsuarioService>();

// Inicio de sesión con cookie (RN-15): al entrar, el servidor manda una cookie cifrada
// con los datos del usuario; el navegador la regresa en cada petición.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";          // a dónde mandar a quien no ha entrado
        options.LogoutPath = "/Cuenta/Salir";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;              // se renueva mientras se use
        options.Cookie.Name = "CAVIFAX.Sesion";
        options.Cookie.HttpOnly = true;                // JavaScript no puede leerla
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Todas las páginas piden sesión, menos el login y la página de error.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Cuenta/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Primero se identifica al usuario (lee la cookie) y después se revisa si puede entrar
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
