namespace blazor.Components.Data
{
    public class Articulo
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; } = 0;
        public decimal TotalLinea => Cantidad * PrecioUnitario;
    }
}