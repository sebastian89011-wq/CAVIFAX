using Microsoft.EntityFrameworkCore;
using Nominas.Datos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NominasDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("Nominas")!));

// Add services to the container.
builder.Services.AddRazorPages();

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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// TEMPORAL: borrar después de la prueba
app.MapGet("/prueba-bd", (NominasDbContext db) => db.Puestos.ToList());

app.Run();
