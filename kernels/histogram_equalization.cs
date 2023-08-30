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
    public partial class histogram_equalization : Form
    {
        Bitmap bg;
        Timer t = new Timer();
        ImageFilters filters;
        Bitmap H1, H2;
        int[] H,H_NEW,H_TMP;
        int[,] IMG2;
        Bitmap outp;
        bool start=false;
        public histogram_equalization(ref ImageFilters F)
        {
            filters = F;
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            t.Interval = 30;
            t.Tick += T_Tick;
            this.Load += Histogram_equalization_Load;
            this.Paint += Histogram_equalization_Paint;
        }

        private void Histogram_equalization_Paint(object sender, PaintEventArgs e)
        {
            bg.Dispose();
            bg = new Bitmap(ClientSize.Width, ClientSize.Height);
            draw(e.Graphics);
        }

        private void Histogram_equalization_Load(object sender, EventArgs e)
        {
            bg = new Bitmap(ClientSize.Width, ClientSize.Height);
            this.Text = "histogram form";
            if(filters.outp != null)
            {
                H2 = filters.draw_hisotgram(filters.outp, ref H_NEW);
                start = true;
            }
            if(filters.inp != null)
            {
                H1 = filters.draw_hisotgram(filters.inp, ref H);
            }
            button1.Location = new Point(ClientSize.Width-10-button1.Width,button1.Location.Y);
            button2.Location = new Point(ClientSize.Width - 10 - button2.Width, button2.Location.Y);
            t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            draw(CreateGraphics());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            filters.last = (int)Form1.F.histogtam_equalization;
            filters.outp = outp;
            filters.IMG2 = IMG2;
            filters.last_size = -1;
            start = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            filters.histogram_equalization(ref H, ref H_TMP, ref outp, ref IMG2);
            H2 = filters.draw_hisotgram(outp, ref H_TMP);
            start = false;
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
            if(H1 != null)
            {
                g.DrawImage(filters.inp, new Rectangle(0,0,ClientSize.Width/3,ClientSize.Height/2), new Rectangle(0,0,filters.inp.Width,filters.inp.Height), GraphicsUnit.Pixel);
                g.DrawImage(H1, new Rectangle(ClientSize.Width / 3, 0, ClientSize.Width/3,ClientSize.Height/2), new Rectangle(0,0,H1.Width,H1.Height), GraphicsUnit.Pixel);
            }
            if (H2 != null)
            {
                if(start && filters.outp != null)
                {
                    g.DrawImage(filters.outp, new Rectangle(0, ClientSize.Height/2, ClientSize.Width / 3, ClientSize.Height / 2), new Rectangle(0, 0, filters.outp.Width, filters.outp.Height), GraphicsUnit.Pixel);
                }
                else if(!start && outp != null) 
                {
                    g.DrawImage(outp, new Rectangle(0, ClientSize.Height / 2, ClientSize.Width / 3, ClientSize.Height / 2), new Rectangle(0, 0, outp.Width, outp.Height), GraphicsUnit.Pixel);
                }
                g.DrawImage(H2, new Rectangle(ClientSize.Width / 3, ClientSize.Height/2, ClientSize.Width / 3, ClientSize.Height / 2), new Rectangle(0, 0, H2.Width, H2.Height), GraphicsUnit.Pixel);
            }
        }
    }
}
