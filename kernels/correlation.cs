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
    public partial class correlation : Form
    {
        StringFormat stringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        string buff = "";
        int curr = 0;
        bool calculated = false;
        Bitmap bg;
        Timer t = new Timer();
        ImageFilters filters;
        int[,] K;
        int size;
        public correlation(ref ImageFilters F)
        {
            filters = F;
            this.WindowState = FormWindowState.Maximized;
            InitializeComponent();
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Load += Correlation_Load;
            this.MouseMove += Correlation_MouseMove;
            numericUpDown1.ValueChanged += NumericUpDown1_ValueChanged;
            this.KeyDown += Correlation_KeyDown;
            t.Interval = 100;
            t.Tick += T_Tick;
        }

        private void Correlation_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Up:
                    if(curr-size >= 0)
                    {
                        curr -= size;
                    }
                    buff = K[curr / size, curr % size] + "";
                    break;
                case Keys.Down:
                    if(curr+size <= size * size - 1)
                    {
                        curr += size;
                    }
                    buff = K[curr / size, curr % size] + "";
                    break;
                case Keys.Right:
                    if(curr+1 <= size * size - 1)
                    {
                        curr++;
                    }
                    buff = K[curr / size, curr % size] + "";
                    break;
                case Keys.Left:
                    if (curr - 1 >= 0)
                    {
                        curr--;
                    }
                    buff = K[curr / size, curr % size] + "";
                    break;
                case Keys.D0:
                case Keys.NumPad0:
                    if (int.Parse(buff + "0") <= int.MaxValue)
                    {
                        buff += "0";
                    }
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    if (int.Parse(buff + "1") <= int.MaxValue)
                    {
                        buff += "1";
                    }
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    if (int.Parse(buff + "2") <= int.MaxValue)
                    {
                        buff += "2";
                    }
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    if (int.Parse(buff + "3") <= int.MaxValue)
                    {
                        buff += "3";
                    }
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    if (int.Parse(buff + "4") <= int.MaxValue)
                    {
                        buff += "4";
                    }
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    if (int.Parse(buff + "5") <= int.MaxValue)
                    {
                        buff += "5";
                    }
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    if (int.Parse(buff + "6") <= int.MaxValue)
                    {
                        buff += "6";
                    }
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    if (int.Parse(buff + "7") <= int.MaxValue)
                    {
                        buff += "7";
                    }
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    if (int.Parse(buff + "8") <= int.MaxValue)
                    {
                        buff += "8";
                    }
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    if (int.Parse(buff + "9") <= int.MaxValue)
                    {
                        buff += "9";
                    }
                    break;
                case Keys.Back:
                    if(buff != "")
                    {
                        buff = buff.Substring(0, buff.Length - 1);
                    }
                    break;
                case Keys.OemMinus:
                    K[curr / size, curr % size] *= -1;
                    buff = "-"+buff;
                    break;
            }
            if(buff != "" && buff[0]!='-')
            {
                K[curr/size,curr%size]=int.Parse(buff);
            }
            else if(buff != "" && buff[0] == '-')
            {
                buff = buff.Substring(1, buff.Length - 1);
            }
            else
            {
                buff = "0";
                K[curr / size, curr % size] = 0;
            }
            draw(CreateGraphics());
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            size = (int)numericUpDown1.Value;
            K = new int[size, size];
            for(int i = 0; i < size; i++)
            {
                for(int j = 0; j < size; j++)
                {
                    K[i, j] = 0;
                }
            }
        }

        private void Correlation_MouseMove(object sender, MouseEventArgs e)
        {
            numericUpDown1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            if (e.X >= numericUpDown1.Location.X && e.X <= numericUpDown1.Location.X + numericUpDown1.Width)
            {
                if (e.Y >= numericUpDown1.Location.Y && e.Y <= numericUpDown1.Location.Y + numericUpDown1.Height)
                {
                    numericUpDown1.Enabled = true;
                }
            }
            if (e.X >= button1.Location.X && e.X <= button1.Location.X + button1.Width)
            {
                if (e.Y >= button1.Location.Y && e.Y <= button1.Location.Y + button1.Height)
                {
                    button1.Enabled = true;
                }
            }
            if (e.X >= button2.Location.X && e.X <= button2.Location.X + button2.Width)
            {
                if (e.Y >= button2.Location.Y && e.Y <= button2.Location.Y + button2.Height)
                {
                    button2.Enabled = true;
                }
            }
        }

        private void Correlation_Load(object sender, EventArgs e)
        {
            bg = new Bitmap(ClientSize.Width, ClientSize.Height);
            numericUpDown1.Location = new Point(ClientSize.Width - numericUpDown1.Width - 10,numericUpDown1.Location.Y);
            button1.Text = "conv";
            button2.Text = "corr";
            button1.Location = new Point(numericUpDown1.Location.X, button1.Location.Y);
            button2.Location = new Point(numericUpDown1.Location.X, button2.Location.Y);
            numericUpDown1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            size = (int)numericUpDown1.Value;
            K = new int[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    K[i, j] = 0;
                }
            }
            t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            draw(this.CreateGraphics());
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
            g.DrawLine(Pens.Black, ClientSize.Width / 2, 0, ClientSize.Width / 2, ClientSize.Height);
            g.DrawLine(Pens.Black, ClientSize.Width / 2, ClientSize.Height/2, ClientSize.Width, ClientSize.Height / 2);
            g.DrawImage(filters.inp, new Rectangle(0,0,ClientSize.Width/2,ClientSize.Height/2), new Rectangle(0,0,filters.inp.Width,filters.inp.Height), GraphicsUnit.Pixel);
            if (calculated)
            {
                g.DrawImage(filters.outp, new Rectangle(0, ClientSize.Height / 2, ClientSize.Width/2, ClientSize.Height/2), new Rectangle(0, 0, filters.outp.Width, filters.outp.Height), GraphicsUnit.Pixel);
            }
            if(K != null)
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        RectangleF rect = new RectangleF(j*(ClientSize.Width/2/size)+ClientSize.Width/2,i*(ClientSize.Height/2/size)+ClientSize.Height/2,ClientSize.Width/2/size,ClientSize.Height/2/size);
                        g.FillRectangle((i*size+j == curr)?Brushes.Blue:Brushes.Black, rect);
                        g.DrawRectangle(Pens.White, rect.X,rect.Y,rect.Width,rect.Height);
                        SizeF s = g.MeasureString(K[i, j]+"", this.Font);
                        float fontScale = Math.Max(s.Width / rect.Width, s.Height / rect.Height);
                        using (Font font = new Font(this.Font.FontFamily, this.Font.SizeInPoints / fontScale, GraphicsUnit.Point))
                        {
                            Color c = Color.White;
                            g.DrawString(K[i, j] + "", font, new SolidBrush(c), rect, stringFormat);
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Text = "applying convolution";
            filters.conv(K, size);
            filters.last_size = size;
            filters.KK = K;
            this.Text = "convolution applied";
            calculated = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Text = "applying correlation";
            K = filters.rotate_kernel_clock(K,size);
            K = filters.rotate_kernel_clock(K, size);
            filters.conv(K, size);
            filters.last_size = size;
            filters.KK = K;
            this.Text = "correlation applied";
            calculated = true;
        }
    }
}
