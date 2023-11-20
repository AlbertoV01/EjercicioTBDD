using RDN.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RDN
{
    public class opInventarios
    {

        public String sLastError = String.Empty;
        public String mensaje = String.Empty;
        //public Boolean ObtenerSaldos(String sProductoId, double dCantidad, String sSucursal)
        //{
        //    Boolean ok = true;
        //    SqlConnection conexion = new SqlConnection($"server={Conexion.servidor} ; database=Inventarios ;User ID ={Conexion.usuario}; Password={Conexion.password}");

        //    DataTable dtSaldo = new DataTable();
        //        SqlCommand cmdSaldos = new SqlCommand();
        //        conexion.Open();
        //        cmdSaldos.Connection = conexion;
        //        cmdSaldos.CommandText = $"select Saldo from Saldos where ProductoID ='{sProductoId}' and Sucursal ='{sSucursal}'";
        //        dtSaldo.Load(cmdSaldos.ExecuteReader());
        //        conexion.Close();

        //        if (dtSaldo.Rows.Count==0)
        //        {
        //            ok = true;
        //            return ok;
        //        }
        //        if (Convert.ToDouble(dtSaldo.Rows[0][0].ToString())< dCantidad)
        //        {
        //            ok= false;
        //        }
        //        dtSaldo.Clear();    
        //       return ok;
        //}
        public Boolean Alta(datosInventarios datos)
        {
            Boolean bAllOk = true;
            try
            {
                using (SqlConnection conn = new SqlConnection($"Server={Conexion.servidor}; Database = Inventarios; User Id = {Conexion.usuario}; Password = {Conexion.password}"))
                {
                    conn.Open();
                    SqlTransaction transaccion = conn.BeginTransaction();
                    try
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO Inventarios (Folio, Sucursal, Fecha, Total, TipoMovimiento)" +
                                        $"VALUES ({datos.nFolio}, '{datos.sSucursal}','{datos.dtFecha.ToString("yyyy-MM-dd HH:mm:ss.fff")}', '{datos.dTotal}', '{datos.sTipoMovimiento}')";
                        cmd.Transaction = transaccion;
                        cmd.ExecuteNonQuery();

                        for (int i = 0; i < datos.NumRow(); i++)
                        {
                            String sProductoId = String.Empty;
                            String sDescripcion = String.Empty;
                            Double dPrecio = 0;
                            Double dCantidad = 0;
                            DataTable dataTable= new DataTable();
                           
                            datos.GetRow(i, ref sProductoId, ref dPrecio, ref dCantidad);

                            DataTable dt = new DataTable();
                            SqlCommand cmdExiste = new SqlCommand();
                            cmdExiste.Connection= conn;
                            cmdExiste.Transaction=transaccion;
                            cmdExiste.CommandText = $"select count (*) from Saldos where ProductoID = '{sProductoId}' and Sucursal = '{datos.sSucursal}'";
                            dt.Load(cmdExiste.ExecuteReader());

                            if (Int32.Parse(dt.Rows[0][0].ToString()) <= 0)
                            {
                                SqlCommand cmdSaldo = new SqlCommand();
                                cmdSaldo.Connection = conn;
                                cmdSaldo.Transaction = transaccion;
                                cmdSaldo.CommandText = "INSERT INTO Saldos (ProductoId, Sucursal, Saldo)" +
                                    $"values ('{sProductoId}','{datos.sSucursal}','{dCantidad}')";

                                cmdSaldo.ExecuteNonQuery();
                            }
                            else
                            {
                                if (datos.sTipoMovimiento.ToUpper() == "ENTRADA")
                                {
                                    SqlCommand cmdSaldo = new SqlCommand();
                                    cmdSaldo.Connection = conn;
                                    cmdSaldo.Transaction = transaccion;
                                    cmdSaldo.CommandText = $"update Saldos set Saldo = Saldo + {dCantidad} where ProductoID = '{sProductoId}' and Sucursal = '{datos.sSucursal}'";
                                    cmdSaldo.ExecuteNonQuery();
                                }
                                if (datos.sTipoMovimiento.ToUpper() == "SALIDA")
                                {
                                    DataTable dataTable1 = new DataTable();
                                    SqlCommand cmdSaldos = new SqlCommand();
                                    cmdSaldos.Connection = conn;
                                    cmdSaldos.Transaction = transaccion;
                                    cmdSaldos.CommandText = $"select Saldo from Saldos where ProductoID ='{sProductoId}' and Sucursal ='{datos.sSucursal}'";
                                    dataTable1.Load(cmdSaldos.ExecuteReader());

                                    if (dataTable1.Rows.Count > 0)
                                    {
                                        if (Convert.ToDouble(dataTable1.Rows[0][0].ToString()) < dCantidad)
                                        {
                                            bAllOk = false;
                                            transaccion.Rollback();
                                            return bAllOk;
                                        }
                                        else
                                        {
                                            SqlCommand cmdSaldo = new SqlCommand();
                                            cmdSaldo.Connection = conn;
                                            cmdSaldo.Transaction = transaccion;
                                            cmdSaldo.CommandText = $"update Saldos set Saldo = Saldo - {dCantidad} where ProductoID = '{sProductoId}' and Sucursal = '{datos.sSucursal}'";
                                            cmdSaldo.ExecuteNonQuery();
                                        }
                                    }
                                    else
                                    {
                                        SqlCommand cmdSaldo = new SqlCommand();
                                        cmdSaldo.Connection = conn;
                                        cmdSaldo.Transaction = transaccion;
                                        cmdSaldo.CommandText = $"update Saldos set Saldo = Saldo - {dCantidad} where ProductoID = '{sProductoId}' and Sucursal = '{datos.sSucursal}'";
                                        cmdSaldo.ExecuteNonQuery();
                                    }
                                   
                                }

                            }

               

                            SqlCommand cmdDetalle = new SqlCommand();
                            cmdDetalle.Connection = conn;
                            cmdDetalle.CommandText = "INSERT INTO InventarioDetalle  (Folio, Sucursal, ProductoId, Precio, Cantidad)" +
                                $"VALUES ({datos.nFolio},'{datos.sSucursal}','{sProductoId}','{dPrecio}','{dCantidad}')";

                            cmdDetalle.Transaction = transaccion;
                            cmdDetalle.ExecuteNonQuery();
                            
                            
                        }
                        transaccion.Commit();
                        conn.Close();
                    }
                    catch (Exception error)
                    {
                        bAllOk = false;
                        sLastError = error.Message;
                        
                        transaccion.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                sLastError = ex.Message;
                bAllOk = false;

            }
            return bAllOk;
        }

        //public Boolean ExisteSaldo(String sProductoId, String sSucursal)
        //{
        //    Boolean bExiste = true;
        //    DataTable dt = new DataTable();

        //    using (SqlConnection conn = new SqlConnection("Server=(local); Database=Inventarios; User Id = sa; Password = 12345"))
        //    {
        //        conn.Open();
        //        String sCmd = "select count (*) from Saldos " +
        //            $"where ProductoId ='{sProductoId}' and Sucursal = '{sSucursal}'";
        //        SqlCommand cmd = new SqlCommand(sCmd, conn);
                
        //        dt.Load(cmd.ExecuteReader());
        //        if (Int32.Parse(dt.Rows[0][0].ToString()) <= 0)
        //            bExiste = false;

        //        //if (Int32.Parse(cmd.ExecuteScalar().ToString()) <= 0)
        //        //    bExiste = false;
        //        conn.Close();
        //    }
        //    return bExiste;

        //}

        //public DataTable FilasInventarioDetalle()
        //{
        //    DataTable dt = new DataTable();

        //    using (SqlConnection conn = new SqlConnection($"Server={Conexion.servidor}; Database = Inventarios; User Id = {Conexion.usuario}; Password = {Conexion.password}"))
        //    {
        //        try
        //        {
        //            SqlCommand cmd = new SqlCommand();
        //            conn.Open();
        //            cmd.Connection = conn;
        //            cmd.CommandText = "select Folio, Sucursal, Renglon, ProductoId, Precio, Cantidad from InventarioDetalle";
        //            dt.Load(cmd.ExecuteReader());
        //        }
        //        catch (Exception error)
        //        {
        //            sLastError = error.Message;
        //        }
        //        finally
        //        {
        //            conn.Close();
        //        }
        //    }
        //    return dt;
        //}

        //public string Total()
        //{
        //    String Total = String.Empty;
        //    using (SqlConnection conn = new SqlConnection($"Server={Conexion.servidor}; Database = Inventarios; User Id = {Conexion.usuario}; Password = {Conexion.password}"))
        //    {
        //        try
        //        {
        //            SqlCommand cmd = new SqlCommand();
        //            conn.Open();
        //            cmd.Connection = conn;
        //            cmd.CommandText = "select sum (Precio * Cantidad) from InventarioDetalle";
        //            Total = Convert.ToString(cmd.ExecuteScalar());
        //        }
        //        catch (Exception error)
        //        {
        //            sLastError = error.Message;
        //        }
        //        finally
        //        {
        //            conn.Close();
        //        }
        //    }
        //    return Total;
        //}

        public Boolean Baja(String nFolio, String sSucursal, String sProductoID)
        {

            Boolean bAllOk = true;

            using (SqlConnection conn = new SqlConnection($"Server={Conexion.servidor}; Database = Inventarios; User Id = {Conexion.usuario}; Password = {Conexion.password}"))
            {
                conn.Open();

                SqlTransaction transaccion = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = conn;
                    cmd.Transaction = transaccion;
                    cmd.CommandText = $"delete from InventarioDetalle where Folio={nFolio} and Sucursal = '{sSucursal}'";
                    cmd.ExecuteNonQuery();
                    
                    SqlCommand cmdInvDet = new SqlCommand();
                    cmdInvDet.Connection = conn;
                    cmdInvDet.Transaction = transaccion;
                    cmdInvDet.CommandText = $"delete from Inventarios where Folio = {nFolio} and Sucursal = '{sSucursal}'";
                    cmdInvDet.ExecuteNonQuery();

                    SqlCommand cmdSaldos = new SqlCommand();
                    cmdSaldos.Connection = conn;
                    cmdSaldos.Transaction = transaccion;
                    cmd.CommandText = $"delete from Saldos where  ProductoID='{sProductoID}' and Sucursal = {sSucursal}";
                    cmd.ExecuteNonQuery();


                    transaccion.Commit();

                }
                catch (Exception error)
                {
                    transaccion.Rollback();                    
                    bAllOk = false;
                    sLastError = error.Message;
                   
                }
                return bAllOk;

            }
        }

        public Int32 ExisteFolio(Int32 folio)
        {
            InventariosEntities1 oInvFolio = new InventariosEntities1();
            var consulta = (from range in oInvFolio.Inventarios
                            where range.Folio == folio
                            select new
                            {
                                range.Folio,
                            }).ToList().Count();
            return consulta;
        }

        public DataTable FiltrarPorDescripcion(String sDescripcion)
        {
            DataTable dt = new DataTable();
            InventariosEntities1 oInvEnt = new InventariosEntities1();
            var consulta = (from range in oInvEnt.Productos
                            where range.Descripcion.Contains(sDescripcion)
                            select new
                            {
                                range.ProductoID,
                                range.Descripcion,
                                range.PrecioCompra,
                                range.PrecioVenta,
                            }).ToList();

            dt.Columns.Add("ProductoID");
            dt.Columns.Add("Descripcion");
            dt.Columns.Add("PrecioCompra");
            dt.Columns.Add("PrecioVenta");

            foreach (var item in consulta)
            {
                DataRow row = dt.NewRow();
                row["ProductoID"] = item.ProductoID;
                row["Descripcion"] = item.Descripcion;
                row["PrecioCompra"] = item.PrecioCompra;
                row["PrecioVenta"] = item.PrecioVenta;
                dt.Rows.Add(row);
            }

            return dt;
        }

        public Int32 ExisteProducto(String sProducto)
        {
            Int32 o = 0; 
            DataTable dt = new DataTable();
            InventariosEntities1 oInvEnt = new InventariosEntities1();
            o = (from range in oInvEnt.Productos
                            where range.ProductoID.Contains(sProducto)
                            select new
                            {
                                range.ProductoID,
                                range.Descripcion,
                                range.PrecioCompra,
                                range.PrecioVenta,
                            }).ToList().Count;

            return o;

        }

        public DataTable FiltrarPorClave(String Clave)
        {
            DataTable dt = new DataTable();
            InventariosEntities1 oInvEnt = new InventariosEntities1();
            var consulta = (from range in oInvEnt.Productos
                            where range.ProductoID.Contains(Clave)
                            select new
                            {
                                range.ProductoID,
                                range.Descripcion,
                                range.PrecioCompra,
                                range.PrecioVenta,
                            }).ToList();

            dt.Columns.Add("ProductoID");
            dt.Columns.Add("Descripcion");
            dt.Columns.Add("PrecioCompra");
            dt.Columns.Add("PrecioVenta");

            foreach (var item in consulta)
            {
                DataRow row = dt.NewRow();
                row["ProductoID"] = item.ProductoID;
                row["Descripcion"] = item.Descripcion;
                row["PrecioCompra"] = item.PrecioCompra;
                row["PrecioVenta"] = item.PrecioVenta;
                dt.Rows.Add(row);
            }

            return dt;
        }


        public DataTable RellenarTextBox(String ProductoID)
        {
            DataTable data = new DataTable();

            using (SqlConnection conn = new SqlConnection($"Server={Conexion.servidor}; Database=Inventarios; User Id = {Conexion.usuario}; Password = {Conexion.password}"))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = $"select ProductoID, Descripcion, PrecioCompra, PrecioVenta from Productos where ProductoID = '{ProductoID}'";
                    data.Load(cmd.ExecuteReader());
                }
                catch(Exception eerr)
                {
                    sLastError= eerr.Message;
                }

            }
            return data;

        }

        public Boolean AgregarProducto(String ProductoID, String Descripcion, String PreciCompra, String PrecioVenta)
        {
            Boolean bALLOk = true;  
            try
            {
                using (SqlConnection conn = new SqlConnection($"Server={Conexion.servidor}; Database=Inventarios; User Id = {Conexion.usuario}; Password = {Conexion.password}"))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    
                    cmd.Connection = conn;
                    cmd.CommandText = $"insert into Productos (ProductoID, Descripcion, PrecioCompra, PrecioVenta) values ('{ProductoID}', '{Descripcion}', '{PreciCompra}', '{PrecioVenta}')";
                    cmd.ExecuteNonQuery();
                }

                }catch(Exception error)
            {

                bALLOk = false;
                sLastError = error.Message;
            }
            return bALLOk;

        }

        public Boolean EliminarProducto(String ProductoID)
        {
            Boolean bALLOk = true;
            try
            {
                using (SqlConnection conn = new SqlConnection($"Server={Conexion.servidor}; Database=Inventarios; User Id = {Conexion.usuario}; Password = {Conexion.password}"))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = conn;
                    cmd.CommandText = $"delete from Productos where ProductoID = '{ProductoID}'";
                    cmd.ExecuteNonQuery();

                }

            }
            catch (Exception error)
            {

                bALLOk = false;
                sLastError = error.Message;
            }
            return bALLOk;

        }

        public Int32 ObtenerMax()
        {
            int Max = 0;
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection($"Server={Conexion.servidor}; Database=Inventarios; User Id = {Conexion.usuario}; Password = {Conexion.password}"))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = conn;
                    cmd.CommandText = $"select Max(Folio) from Inventarios";
                    dt.Load(cmd.ExecuteReader());
                    Max= Convert.ToInt32(dt.Rows[0][0].ToString());
                    conn.Close();
                }

            }
            catch (Exception error)
            {              
                sLastError = error.Message;
            }
            return Max;
        }

        public Boolean ValidarCampos(String sProducto, String sPrecioVenta, String sPrecioCompra, String sDescripcion)
        {
            Boolean bALLOk = false;

            if (sProducto == "" || sPrecioVenta == "" || sPrecioCompra == "" || sDescripcion == "")
            {
                bALLOk = true;
            }
           

            return bALLOk;
        }


    }
}