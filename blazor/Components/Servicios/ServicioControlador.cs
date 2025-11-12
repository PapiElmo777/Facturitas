using blazor.Components.Data;
using System.Collections.Generic;

namespace blazor.Components.Servicios
{
    public class ServicioControlador
    {
        private readonly ServicioFacturas _servicioFacturas;
        public List<Factura> facturas { get; private set; }
        public ServicioControlador(ServicioFacturas servicioFacturas)
        {
            _servicioFacturas = servicioFacturas;
            CargarFacturas();
        }
        public void CargarFacturas()
        {
            facturas = _servicioFacturas.GetFacturas();
        }
        public void AgregarFactura(Factura factura)
        {
            if (factura.Items == null || factura.Items.Count == 0)
            {
                Console.WriteLine("La factura debe tener al menos un artículo.");
                return;
            }

            _servicioFacturas.AddFactura(factura);
            CargarFacturas();
        }
        public void ActualizarFactura(Factura factura)
        {
            if (factura.Items == null || factura.Items.Count == 0)
            {
                Console.WriteLine("La factura debe tener al menos un artículo.");
                return;
            }
            _servicioFacturas.UpdateFactura(factura);
            CargarFacturas();
        }                
        public void EliminarFactura(int id)
        {
            _servicioFacturas.DeleteFactura(id);
            CargarFacturas();
        }
    }
    
}
