using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace kernels
{
    public partial class fourier_editor : Form
    {
        Timer t = new Timer();
        Bitmap bg;
        Bitmap outp,plot,recon;
        ImageFilters filters;
        FFT fft;
        Point mouse;
        Point pixel;
        int disp=20;
        int st_y, st_x;
        COMPLEX[,] tmp;
        public fourier_editor(ref ImageFilters F)
        {
            filters = F;
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.Load += interpolation_Load;
            t.Tick += T_Tick;
            this.Paint += Interpolation_Paint;
            this.KeyDown += Fourier_editor_KeyDown;
            this.MouseMove += Fourier_editor_MouseMove;
        }

        private void Fourier_editor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X <= ClientSize.Width / 2 && e.X >= 0 && e.Y <= ClientSize.Height && e.Y >= 0 && plot != null)
            {
                mouse.X = e.X; mouse.Y = e.Y;
                // e.x / (display_width) = index / image_width
                // index = e.x*image_width / display_width
                pixel.X = (mouse.X * plot.Width) / (ClientSize.Width / 2);
                pixel.Y = (mouse.Y * plot.Height) / (ClientSize.Height);

                this.Text = pixel.Y + " " + pixel.X + " image " + plot.Height + " " + plot.Width;
            }
        }

        private void Fourier_editor_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    for (int i = 0; i < fft.ny; i++)
                    {
                        for (int j = 0; j < fft.nx; j++)
                        {
                            fft.Fourier[j, i].real /= fft.Obj.Width;
                        }
                    }
                    fft.Fourier = fft.rev(fft.Fourier);
                    fft.InverseFFT();
                    outp = fft.Obj;
                    break;
                case Keys.Subtract:
                    if (disp < plot.Width && disp < plot.Height)
                    {
                        disp++;
                    }
                    break;
                case Keys.Add:
                    if (disp > 10)
                    {
                        disp--;
                    }
                    break;
                case Keys.Back:
                    for (int i = st_y; i < st_y+disp; i++)
                    {
                        for (int j = st_x; j < st_x+disp; j++)
                        {
                            fft.Fourier[j,i] = new COMPLEX(0, 0);
                        }
                    }
                    plot = fft.FFTPlot(fft.Fourier, fft.Obj.Width);
                    for (int i = 0; i < fft.ny; i++)
                    {
                        for (int j = 0; j < fft.nx; j++)
                        {
                            fft.Fourier[j, i].real /= fft.Obj.Width;
                        }
                    }
                    fft.FourierPlot = plot;
                    fft.Fourier = fft.rev(fft.Fourier);
                    fft.InverseFFT();
                    recon = fft.Obj;
                    break;
                case Keys.Escape:
                    for (int i = st_y; i < st_y + disp; i++)
                    {
                        for (int j = st_x; j < st_x + disp; j++)
                        {
                            fft.Fourier[j, i] = tmp[j,i];
                        }
                    }
                    plot = fft.FFTPlot(fft.Fourier, fft.Obj.Width);
                    for (int i = 0; i < fft.ny; i++)
                    {
                        for (int j = 0; j < fft.nx; j++)
                        {
                            fft.Fourier[j, i].real /= fft.Obj.Width;
                        }
                    }
                    fft.FourierPlot = plot;

                    break;
                case Keys.D1:
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            string filePath = saveFileDialog.FileName;
                            plot.Save(filePath);
                            MessageBox.Show(filePath + " saved successfully!!");
                        }
                    }
                    break;
                case Keys.D2:
                    if(recon != null)
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            string filePath = saveFileDialog.FileName;
                            recon.Save(filePath);
                            MessageBox.Show(filePath + " saved successfully!!");
                        }
                    }
                    break;
            }
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
            fft = new FFT(filters.inp);
            fft.ForwardFFT();
            fft.FFTShift();
            plot = fft.FFTPlot(fft.Fourier, fft.Obj.Width);
            for (int i = 0; i < fft.ny; i++)
            {
                for (int j = 0; j < fft.nx; j++)
                {
                    fft.Fourier[j, i].real /= fft.Obj.Width;
                }
            }
            fft.FourierPlot=plot;
            tmp = fft.Fourier;
            t.Start();
        }
        void draw(Graphics g)
        {
            if (bg != null)
            {
                buffer(Graphics.FromImage(bg));
                g.DrawImage(bg, 0, 0);
            }
        }
        void buffer(Graphics g)
        {
            g.Clear(BackColor);
            g.DrawImage(plot, new Rectangle(0,0,ClientSize.Width/2,ClientSize.Height), new Rectangle(0,0,plot.Width,plot.Height), GraphicsUnit.Pixel);
            g.DrawImage(filters.inp, new Rectangle(ClientSize.Width / 2, 0, ClientSize.Width / 2, ClientSize.Height / 2),new Rectangle(0,0,filters.inp.Width,filters.inp.Height), GraphicsUnit.Pixel);
            if(recon != null)
            {
                g.DrawImage(recon, new Rectangle(ClientSize.Width/2,ClientSize.Height/2,ClientSize.Width/2,ClientSize.Height/2), new Rectangle(0,0,recon.Width,recon.Height), GraphicsUnit.Pixel);
            }
            st_y = pixel.Y - (int)(disp / 2);
            st_x = pixel.X - (int)(disp / 2);
            if (st_y < 0)
            {
                st_y = 0;
            }
            if (st_y + (int)disp > plot.Width - 1)
            {
                st_y = plot.Height - (int)disp;
            }
            if (st_x < 0)
            {
                st_x = 0;
            }
            if (st_x + (int)disp > plot.Width - 1)
            {
                st_x = plot.Width - (int)disp;
            }
            g.DrawRectangle(Pens.Red, new Rectangle((st_x * (ClientSize.Width / 2)) / plot.Width, (st_y * (ClientSize.Height)) / plot.Height, ((int)disp * (ClientSize.Width / 2)) / plot.Width, ((int)disp * (ClientSize.Height)) / plot.Height));

        }
    }
}
