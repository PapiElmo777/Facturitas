using System.ComponentModel.DataAnnotations;

namespace blazor.Components.Data
{
    public class Factura
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;//cambiar datetime y cambiar en html
        public string NombreCliente { get; set; } = string.Empty;
        public List<Articulo> Items { get; set; } = new List<Articulo>();
        public decimal Total => Items.Sum(item => item.TotalLinea);
    }
}