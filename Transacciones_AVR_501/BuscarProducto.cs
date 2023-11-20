using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RDN.Models;
using RDN;
namespace Transacciones_AVR_501
{
    public partial class BuscarProducto : Form
    {
        public BuscarProducto()
        {
            InitializeComponent();
        }

        public String ProductoId = String.Empty;
        public String PrecioCompra = String.Empty;
        public String PrecioVenta = String.Empty;
        public String Descripcion = String.Empty;
        public String Total = String.Empty;
        public Int32 Folio = 0;

        opInventarios opInventarios= new opInventarios();

        private void tb_Clave_TextChanged(object sender, EventArgs e)
        {

            opInventarios op = new opInventarios();
            dataGridView1.DataSource = op.FiltrarPorClave(tb_Clave.Text);
        }

    
        private void tb_Descripcion_TextChanged(object sender, EventArgs e)
        {
            opInventarios op = new opInventarios();
            dataGridView1.DataSource =  op.FiltrarPorDescripcion(tb_Descripcion.Text);
            
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            
            try
            {
                ProductoId = Convert.ToString(dataGridView1.SelectedRows[0].Cells[0].Value);
                Descripcion = Convert.ToString(dataGridView1.SelectedRows[0].Cells[1].Value);
                PrecioCompra = Convert.ToString(dataGridView1.SelectedRows[0].Cells[2].Value);
                PrecioVenta = Convert.ToString(dataGridView1.SelectedRows[0].Cells[3].Value);

                this.Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message); 
            }
        }

        private void BuscarProducto_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
