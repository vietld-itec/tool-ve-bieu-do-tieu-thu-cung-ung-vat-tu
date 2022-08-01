using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ACAD_BieuDoKeHoachVatTu
{
    public partial class frmNhapLieu : Form
    {
        public string tenvatlieu = "";
        public int ngaydutru = 0;
        public frmNhapLieu()
        {
            InitializeComponent();
        }

        private void frmNhapLieu_Load(object sender, EventArgs e)
        {

        }

        private void cmd_ThucHien_Click(object sender, EventArgs e)
        {
            try
            {
                ngaydutru = int.Parse(numeric.Value.ToString());
            }
            catch (Exception)
            {

                //throw;
            }
            try
            {
                tenvatlieu = txtTenVatLieu.Text;
            }
            catch (Exception)
            {

                //throw;
            }
            Data.TenVatLieu = tenvatlieu;
            Data.NgayDuTru = ngaydutru;
            string full = rich_inputdata.Text;
            try
            {
                Function.GetData(full);


            }
            catch (Exception)
            {
                MessageBox.Show("Error");
                //throw;
            }
            this.Hide();
        }

        private void txtTenVatLieu_TextChanged(object sender, EventArgs e)
        {
            try
            {
                tenvatlieu = txtTenVatLieu.Text;
            }
            catch (Exception)
            {

                //throw;
            }
        }

        private void txtNgayDuTru_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ngaydutru = int.Parse(numeric.Value.ToString());
            }
            catch (Exception)
            {

                //throw;
            }
        }

        private void cmd_huy_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string tmp = "";
            for (int i = 0; i < Data.input.ThoiGian.Count; i++)
            {
                tmp = tmp + Data.input.ThoiGian[i] + "\n";
            }

            richTextBox1.Text = tmp;
            MessageBox.Show("Success");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string tmp = "";
            for (int i = 0; i < Data.input.CongDonKeHoach.Count; i++)
            {
                tmp = tmp + Data.input.CongDonKeHoach[i] + "\n";
            }

            richTextBox1.Text = tmp;
            MessageBox.Show("Success");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string tmp = "";
            for (int i = 0; i < Data.input.CongDonThucTe.Count; i++)
            {
                tmp = tmp + Data.input.CongDonThucTe[i] + "\n";
            }

            richTextBox1.Text = tmp;
            MessageBox.Show("Success");
        }
    }
}
