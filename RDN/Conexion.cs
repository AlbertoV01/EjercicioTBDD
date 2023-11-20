using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RDN
{
    public class Conexion
    {   
        
        public static String sLastError = string.Empty;
        public static String servidor = string.Empty;
        public static String baseDeDatos = "Inventarios";
        public static String usuario = string.Empty;
        public static String password = string.Empty;
        public static String cadenaConexion = string.Empty;
        public static SqlConnection conn = new SqlConnection();

        public static Boolean AbrirConexion()
        {
            Boolean bALLOk = true;
            try
            {
                cadenaConexion = $"Data Source = {servidor}; User Id = {usuario}; password = {password}; Initial Catalog= {baseDeDatos}";
                conn.ConnectionString = cadenaConexion;
                conn.Open();
            }
            catch (Exception e)
            {
                bALLOk = false;
                sLastError = e.Message;
            }
            finally
            {
                conn.Close();
            }
            return bALLOk;
        }

        public Boolean GuardarProducto(String producto, String descripcion, String precioCompra, String precioVenta)
        {
            Boolean bALLOk = true;

            try
            {
                conn.Open();
                SqlTransaction transaccion = conn.BeginTransaction();
                SqlCommand comand = new SqlCommand();
                comand.Connection = conn;
                comand.CommandText = $"Insert into Productos Values {producto}, {descripcion}, {precioCompra}, {precioVenta}";
                comand.Transaction = transaccion;
                comand.ExecuteNonQuery();
            }
            catch(Exception error)
            {
                sLastError = error.Message;
                bALLOk = false;
            }
            finally
            {
                conn.Close();
            }
            return bALLOk;
        }




    }
}
