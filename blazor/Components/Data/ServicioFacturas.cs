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
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                try
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Facturas ADD COLUMN Archivada INTEGER DEFAULT 0;";
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                }
            }
        }

        public List<Factura> GetFacturas()
        {
            var facturas = new List<Factura>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var cmdFacturas = connection.CreateCommand();
                cmdFacturas.CommandText = "SELECT Id, Fecha, NombreCliente, Total, Archivada FROM Facturas";

                using (var readerFacturas = cmdFacturas.ExecuteReader())
                {
                    while (readerFacturas.Read())
                    {
                        facturas.Add(new Factura
                        {
                            Id = readerFacturas.GetInt32(0),
                            Fecha = DateTime.Parse(readerFacturas.GetString(1)),
                            NombreCliente = readerFacturas.GetString(2),
                            Archivada = !readerFacturas.IsDBNull(4) && (readerFacturas.GetBoolean(4) || readerFacturas.GetInt32(4) == 1),
                            Items = new List<Articulo>()
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
                    cmdFactura.CommandText = "INSERT INTO Facturas (Fecha, NombreCliente, Total, Archivada) VALUES (@Fecha, @NombreCliente, @Total, @Archivada); SELECT last_insert_rowid();";
                    cmdFactura.Parameters.AddWithValue("@Fecha", factura.Fecha);
                    cmdFactura.Parameters.AddWithValue("@NombreCliente", factura.NombreCliente);
                    cmdFactura.Parameters.AddWithValue("@Total", factura.Total);
                    cmdFactura.Parameters.AddWithValue("@Archivada", factura.Archivada ? 1 : 0);
                    
                    facturaId = (long)cmdFactura.ExecuteScalar();

                    if (facturaId == 0)
                    {
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

        public void DeleteFactura(int id)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Facturas WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateFactura(Factura factura)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var cmdFactura = connection.CreateCommand();
                    cmdFactura.Transaction = transaction;
                    cmdFactura.CommandText = "UPDATE Facturas SET Fecha = @Fecha, NombreCliente = @NombreCliente, Total = @Total, Archivada = @Archivada WHERE Id = @Id";
                    cmdFactura.Parameters.AddWithValue("@Fecha", factura.Fecha);
                    cmdFactura.Parameters.AddWithValue("@NombreCliente", factura.NombreCliente);
                    cmdFactura.Parameters.AddWithValue("@Total", factura.Total);
                    cmdFactura.Parameters.AddWithValue("@Archivada", factura.Archivada ? 1 : 0);
                    cmdFactura.Parameters.AddWithValue("@Id", factura.Id);
                    cmdFactura.ExecuteNonQuery();

                    var cmdBorrarArticulos = connection.CreateCommand();
                    cmdBorrarArticulos.Transaction = transaction;
                    cmdBorrarArticulos.CommandText = "DELETE FROM Articulos WHERE FacturaId = @FacturaId";
                    cmdBorrarArticulos.Parameters.AddWithValue("@FacturaId", factura.Id);
                    cmdBorrarArticulos.ExecuteNonQuery();
                    foreach (var item in factura.Items)
                    {
                        var cmdArticulo = connection.CreateCommand();
                        cmdArticulo.Transaction = transaction;
                        cmdArticulo.CommandText = "INSERT INTO Articulos (FacturaId, Descripcion, Cantidad, PrecioUnitario) VALUES (@FacturaId, @Descripcion, @Cantidad, @PrecioUnitario)";
                        cmdArticulo.Parameters.AddWithValue("@FacturaId", factura.Id);
                        cmdArticulo.Parameters.AddWithValue("@Descripcion", item.Descripcion);
                        cmdArticulo.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                        cmdArticulo.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
                        cmdArticulo.ExecuteNonQuery();
                    }
                    
                    transaction.Commit();
                }
            }
        }

        public void CambiarEstadoArchivo(int id, bool archivada)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "UPDATE Facturas SET Archivada = @Archivada WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Archivada", archivada ? 1 : 0);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}