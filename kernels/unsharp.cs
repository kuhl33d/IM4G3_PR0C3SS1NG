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
    public partial class unsharp : Form
    {
        ImageFilters filters;
        bool is_unsharp;
        bool done = false;
        int size;
        double sigma;
        double strength;
        Bitmap bg;
        Bitmap blurred;
        Bitmap mask;
        Bitmap old;
        Graphics G;
        Timer t = new Timer();
        public unsharp(ImageFilters filters,bool is_unsharp,int size,double sigma,double strength)
        {
            this.filters = filters;
            this.is_unsharp= is_unsharp;
            WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(30, 30, 30);
            InitializeComponent();
            this.Paint += Unsharp_Paint;
            this.Load += Unsharp_Load;
            t.Tick += T_Tick;
            this.size = size;
            this.sigma = sigma;
            this.strength = strength;
        }

        private void T_Tick(object sender, EventArgs e)
        {
            draw(CreateGraphics());
        }

        private void Unsharp_Load(object sender, EventArgs e)
        {
            bg = new Bitmap(ClientSize.Width, ClientSize.Height);
            G = Graphics.FromImage(bg);
            this.Text = "generating...";
            filters.unsharp_masking(size, sigma, strength, ref blurred, ref mask);
            this.Text = "now showing input/blurred/mask/unsharpen";
            if(!is_unsharp)
            {
                old = new Bitmap(filters.outp);
                this.Text += " please select highboost value 0-255";
                label1.Location = new Point(ClientSize.Width / 2 - (label1.Width + 10 + numericUpDown1.Width)/2, 10);
                numericUpDown1.Location = new Point(label1.Location.X + 10 + label1.Width, 10);
                button1.Location = new Point(label1.Location.X, label1.Location.Y + label1.Height + 10);
                label1.Visible = true;
                numericUpDown1.Visible = true;
                button1.Visible = true;
            }
            t.Start();
        }

        private void Unsharp_Paint(object sender, PaintEventArgs e)
        {
            draw(e.Graphics);
        }
        void draw(Graphics g)
        {
            G.Clear(this.BackColor);
            G.DrawImage(filters.inp, new Rectangle(0, ClientSize.Height / 2, ClientSize.Height / 2, ClientSize.Height / 2), new Rectangle(0, 0, filters.inp.Width, filters.inp.Height), GraphicsUnit.Pixel);
            G.DrawImage(blurred, new Rectangle(ClientSize.Height / 2, ClientSize.Height / 2, ClientSize.Height / 2, ClientSize.Height / 2), new Rectangle(0, 0, blurred.Width, blurred.Height), GraphicsUnit.Pixel);
            G.DrawImage(mask, new Rectangle(ClientSize.Height, ClientSize.Height / 2, ClientSize.Height / 2, ClientSize.Height / 2), new Rectangle(0, 0, mask.Width, mask.Height), GraphicsUnit.Pixel);
            if (is_unsharp)
            {
                G.DrawImage(filters.outp, new Rectangle((3*ClientSize.Height) / 2, ClientSize.Height / 2, ClientSize.Height / 2, ClientSize.Height / 2), new Rectangle(0, 0, filters.outp.Width, filters.outp.Height), GraphicsUnit.Pixel);
            }
            else
            {
                G.DrawImage(old, new Rectangle((3 * ClientSize.Height) / 2, ClientSize.Height / 2, ClientSize.Height / 2, ClientSize.Height / 2), new Rectangle(0, 0, old.Width, old.Height), GraphicsUnit.Pixel);
                if (done)
                {
                    G.DrawImage(filters.outp, new Rectangle((3 * ClientSize.Height) / 2, 0, ClientSize.Height / 2, ClientSize.Height / 2), new Rectangle(0, 0, filters.outp.Width, filters.outp.Height), GraphicsUnit.Pixel);
                }
            }
            g.DrawImage(bg, 0, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(done == true)
            {
                filters.outp = new Bitmap(old);
                for(int i = 0; i < old.Height; i++)
                {
                    for(int j=0; j < old.Width; j++)
                    {
                        filters.IMG2[i, j] = old.GetPixel(j, i).R;
                    }
                }
            }
            this.Text = "applying threshold";
            filters.thershold((int)numericUpDown1.Value);
            this.Text = "done";
            done = true;
        }
    }
}
