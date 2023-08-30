using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kernels
{
    public partial class interpolation : Form
    {
        Timer t = new Timer();
        Bitmap bg;
        Bitmap outp;
        ImageFilters filters;
        public interpolation(ref ImageFilters F)
        {
            filters = F;
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.Load += interpolation_Load;
            t.Tick += T_Tick;
            this.Paint += Interpolation_Paint;
        }

        private void Interpolation_Paint(object sender, PaintEventArgs e)
        {
            bg.Dispose();
            bg = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            draw(e.Graphics);
        }

        private void T_Tick(object sender, EventArgs e)
        {
            draw(CreateGraphics());
        }

        private void interpolation_Load(object sender, EventArgs e)
        {
            bg = new Bitmap(ClientSize.Width, ClientSize.Height);

            t.Start();
        }
        void draw(Graphics g)
        {
            if(bg != null)
            {
                buffer(Graphics.FromImage(bg));
                g.DrawImage(bg, 0, 0);
            }
        }
        void buffer(Graphics g)
        {
            g.Clear(BackColor);
            g.DrawImage(filters.inp, new Rectangle(0,numericUpDown1.Location.Y+10 + numericUpDown1.Height, ClientSize.Width/2,ClientSize.Height- numericUpDown1.Location.Y + numericUpDown1.Height + 10), new Rectangle(0,0,filters.inp.Width,filters.inp.Height), GraphicsUnit.Pixel);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Text = "applying nearest interpolation";
            outp = new Bitmap(filters.inp);
            for(int i = 0; i < (int)numericUpDown1.Value; i++)
            {
                outp = filters.interpolation_nearest(outp);
            }
            this.Text = "nearest interpolation applied";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                outp.Save(filePath);
                MessageBox.Show(filePath + " saved successfully!!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Text = "applying bilinear interpolation";
            outp = new Bitmap(filters.inp);
            for (int i = 0; i < (int)numericUpDown1.Value; i++)
            {
                outp = filters.interpolation_bilinear(outp);
            }
            this.Text = "bilinear interpolation applied";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                outp.Save(filePath);
                MessageBox.Show(filePath + " saved successfully!!");
            }
        }
    }
}
