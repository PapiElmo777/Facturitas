using blazor.Components;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
//---------Base de datos-------
string ruta = "mibase.db";

using var conexion = new SqliteConnection($"DataSource={ruta}");
conexion.Open();
var comando = conexion.CreateCommand();
comando.CommandText = @"
CREATE TABLE IF NOT EXISTS Facturas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Fecha TEXT NOT NULL,
    NombreCliente TEXT NOT NULL,
    Total REAL NOT NULL 
);
";
comando.ExecuteNonQuery();
comando.CommandText = @"
CREATE TABLE IF NOT EXISTS Articulos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FacturaId INTEGER NOT NULL,
    Descripcion TEXT NOT NULL,
    Cantidad INTEGER NOT NULL,
    PrecioUnitario REAL NOT NULL,
    FOREIGN KEY (FacturaId) REFERENCES Facturas(Id) ON DELETE CASCADE
);
";
comando.ExecuteNonQuery();
//-----------------------------
app.Run();
