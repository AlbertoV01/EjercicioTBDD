using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RDN;

namespace Transacciones_AVR_501
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void tb_Password_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_Conectar_Click(object sender, EventArgs e)
        {

            

            Conexion.servidor = tb_Servidor.Text;
            Conexion.usuario = tb_Usuario.Text;
            Conexion.password = tb_Password.Text;


            if(Conexion.AbrirConexion())
            {

                MessageBox.Show("Conexion Exitosa","Succes", MessageBoxButtons.OK,MessageBoxIcon.Information);
                ProductosVentas pV = new ProductosVentas();
                this.Hide();
                pV.ShowDialog();
            }
            else
            {
                MessageBox.Show("Ha ocurrido un error" + Conexion.sLastError);
            }
            
        }
    }
}
