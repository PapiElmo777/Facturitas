using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;

namespace blazor.Components.Data
{
    public class ServicioFacturas
    {
        private string connectionString;
        public ServicioFacturas()
        {
            connectionString = "Data Source=mibase.db";
        }

        public List<Factura> GetFacturas()
        {
            var facturas = new List<Factura>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var cmdFacturas = connection.CreateCommand();
                cmdFacturas.CommandText = "SELECT Id, Fecha, NombreCliente, Total FROM Facturas";

                using (var readerFacturas = cmdFacturas.ExecuteReader())
                {
                    while (readerFacturas.Read())
                    {
                        facturas.Add(new Factura
                        {
                            Id = readerFacturas.GetInt32(0),
                            Fecha = readerFacturas.GetString(1),
                            NombreCliente = readerFacturas.GetString(2)
                        });
                    }
                }
                foreach (var f in facturas)
                {
                    var cmdArticulos = connection.CreateCommand();
                    cmdArticulos.CommandText = "SELECT Id, Descripcion, Cantidad, PrecioUnitario FROM Articulos WHERE FacturaId = @FacturaId";
                    cmdArticulos.Parameters.AddWithValue("@FacturaId", f.Id);

                    using (var readerArticulos = cmdArticulos.ExecuteReader())
                    {
                        while (readerArticulos.Read())
                        {
                            f.Items.Add(new Articulo
                            {
                                Id = readerArticulos.GetInt32(0),
                                Descripcion = readerArticulos.GetString(1),
                                Cantidad = readerArticulos.GetInt32(2),
                                PrecioUnitario = readerArticulos.GetDecimal(3),
                                FacturaId = f.Id
                            });
                        }
                    }
                }
            }
            return facturas;
        }
        public void AddFactura(Factura factura)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    long facturaId = 0;
                    
                    var cmdFactura = connection.CreateCommand();
                    cmdFactura.Transaction = transaction;
                    cmdFactura.CommandText = "INSERT INTO Facturas (Fecha, NombreCliente, Total) VALUES (@Fecha, @NombreCliente, @Total); SELECT last_insert_rowid();";
                    cmdFactura.Parameters.AddWithValue("@Fecha", factura.Fecha);
                    cmdFactura.Parameters.AddWithValue("@NombreCliente", factura.NombreCliente);
                    cmdFactura.Parameters.AddWithValue("@Total", factura.Total); 
                    facturaId = (long)cmdFactura.ExecuteScalar();

                    if (facturaId == 0) {
                        transaction.Rollback();
                        return; 
                    }

                    foreach (var item in factura.Items)
                    {
                        var cmdArticulo = connection.CreateCommand();
                        cmdArticulo.Transaction = transaction;
                        cmdArticulo.CommandText = "INSERT INTO Articulos (FacturaId, Descripcion, Cantidad, PrecioUnitario) VALUES (@FacturaId, @Descripcion, @Cantidad, @PrecioUnitario)";
                        cmdArticulo.Parameters.AddWithValue("@FacturaId", facturaId);
                        cmdArticulo.Parameters.AddWithValue("@Descripcion", item.Descripcion);
                        cmdArticulo.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                        cmdArticulo.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
                        cmdArticulo.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }

    }
    
    
    }