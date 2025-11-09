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
    }
    
}
