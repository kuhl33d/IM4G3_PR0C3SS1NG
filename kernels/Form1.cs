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

/*
median--
adaptive_median--
adaptive_min--
adaptive_max--
averaging--
gaussian--
laplacian--
unsharp
robert--
sobel_top--
sobel_down--
sobel_left--
sobel_right--
custom.
impulse_noise--
gaussian_noise--
uniform_noise--
Histogram Equalization
Histogram Specification
Fourier transform (forward and Inverse)--
Interpolation (nearest neighbor, bilinear, or bicubic)
*/
namespace kernels
{
    public partial class Form1 : Form
    {
        Bitmap bg;
        ImageFilters filters;
        Graphics G;
        string fname = "";
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        float disp = 20;
        Point mouse = new Point();
        Point pixel = new Point();
        Point old_pixel = new Point(-1,-1);
        bool toggle_value = false;
        int[,] kernel;
        double[,] d_kernel;
        enum F{
            median,
            adaptive_median,
            adaptive_min,
            adaptive_max,
            averaging,
            gaussian,
            laplacian,
            unsharp,
            highboost,
            robert,
            sobel_horizontal,
            sobel_vertical,
            sobel_operators,
            impulse_noise,
            gaussian_noise,
            uniform_noise
        };
        StringFormat stringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        public Form1()
        {
            this.BackColor = Color.FromArgb(30,30,30);
            InitializeComponent();
            this.KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
            this.MouseDown += Form1_MouseDown;
            this.KeyDown += Form1_KeyDown;
            this.MouseMove += Form1_MouseMove;
            this.listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
            t.Tick += T_Tick;
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (filters == null)
                return;
            switch (listBox1.SelectedIndex)
            {
                case ((int)F.median):
                    this.Text = "applying median";
                    filters.median((int)numericUpDown1.Value);
                    filters.last = (int)F.median;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "median applied";
                    break;
                case ((int)F.adaptive_median):
                    this.Text = "applying adaptive median";
                    filters.adaptive_median((int)numericUpDown1.Value);
                    filters.last = (int)F.adaptive_median;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "adaptive median applied";
                    break;
                case ((int)F.adaptive_min):
                    this.Text = "applying adaptive min";
                    filters.adaptive_min((int)numericUpDown1.Value);
                    filters.last = (int)F.adaptive_min;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "adaptive min applied";
                    break;
                case ((int)F.adaptive_max):
                    this.Text = "applying adaptive max";
                    filters.adaptive_max((int)numericUpDown1.Value);
                    filters.last = (int)F.adaptive_max;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "adaptive max applied";
                    break;
                case ((int)F.averaging):
                    this.Text = "applying averaging";
                    filters.averaging((int)numericUpDown1.Value);
                    filters.last = (int)F.averaging;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "averaging applied";
                    break;
                case ((int)F.gaussian):
                    if ((double)numericUpDown2.Value == 0)
                    {
                        this.Text = "sigma cannot be 0";
                        return;
                    }
                    this.Text = "applying gaussian blur";
                    filters.gauss((int)numericUpDown1.Value,(double)numericUpDown2.Value);
                    filters.last = (int)F.gaussian;
                    filters.last_size = (int)numericUpDown1.Value;
                    filters.last_sigma = (double)numericUpDown2.Value;
                    this.Text = "gaussian blured applied";
                    break;
                case ((int)F.laplacian):
                    this.Text = "applying laplacian operator";
                    filters.laplacian((int)numericUpDown1.Value);
                    filters.last = (int)F.laplacian;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "laplacian operator applied";
                    break;
                case ((int)F.robert):
                    this.Text = "applying robert cross";
                    filters.robert_cross();
                    filters.last = (int)F.robert;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "robert cross applied";
                    break;
                case ((int)F.sobel_horizontal):
                    this.Text = "applying sobel horizontal";
                    filters.sobel((int)numericUpDown1.Value,true);
                    filters.last = (int)F.sobel_horizontal;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "sobel horizontal applied";
                    break;
                case ((int)F.sobel_vertical):
                    this.Text = "applying sobel vertical";
                    filters.sobel((int)numericUpDown1.Value, false);
                    filters.last = (int)F.sobel_vertical;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "sobel vertical applied";
                    break;
                case ((int)F.sobel_operators):
                    this.Text = "applying sobel operators";
                    filters.sobel_operators((int)numericUpDown1.Value);
                    filters.last = (int)F.sobel_operators;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "sobel operators applied";
                    break;
                case ((int)F.impulse_noise):
                    this.Text = "applying impulse noise";
                    filters.impluse_noise((double)numericUpDown2.Value);
                    filters.last = (int)F.impulse_noise;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "impulse noise applied";
                    break;
                case ((int)F.gaussian_noise):
                    this.Text = "applying gaussian noise";
                    filters.gaussian_noise((double)numericUpDown2.Value,(double)numericUpDown3.Value);
                    filters.last = (int)F.gaussian_noise;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "gaussian noise applied";
                    break;
                case ((int)F.uniform_noise):
                    this.Text = "applying uniform noise";
                    filters.uniform_noise((byte)numericUpDown2.Value, (byte)numericUpDown3.Value);
                    filters.last = (int)F.gaussian_noise;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "uniform noise applied";
                    break;
                case ((int)F.unsharp):
                    this.Text = "applying unsharp";
                    unsharp F2 = new unsharp(filters,true,(int)numericUpDown1.Value,(double)numericUpDown2.Value,(double)numericUpDown3.Value);
                    F2.ShowDialog();
                    filters.last = (int)F.unsharp;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "unsharp applied";
                    break;
                case ((int)F.highboost):
                    this.Text = "applying highboost";
                    F2 = new unsharp(filters, false, (int)numericUpDown1.Value, (double)numericUpDown2.Value, (double)numericUpDown3.Value);
                    F2.ShowDialog();
                    filters.last = (int)F.highboost;
                    filters.last_size = (int)numericUpDown1.Value;
                    this.Text = "highboost applied";
                    break;
            }
            old_pixel = new Point(-1, -1);
            listBox1.ClearSelected();
            listBox1.Enabled = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.X <= ClientSize.Height/2 && e.X >= 0 &&  e.Y <= ClientSize.Height/2 && e.Y >= 0 && filters != null)
            {
                mouse.X = e.X; mouse.Y = e.Y;
                // e.x / (display_width) = index / image_width
                // index = e.x*image_width / display_width
                pixel.X = (mouse.X * filters.width) / (ClientSize.Height / 2);
                pixel.Y = (mouse.Y * filters.height) / (ClientSize.Height / 2);

                this.Text = pixel.Y + " " + pixel.X + " image "+filters.height+" "+filters.width;
            }
            listBox1.Enabled = false;
            numericUpDown3.Enabled = false;
            numericUpDown2.Enabled = false;
            numericUpDown1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            if(e.X >= listBox1.Location.X && e.X <= listBox1.Width + listBox1.Location.X && filters != null)
            {
                if (e.Y >= listBox1.Location.Y && e.Y <= listBox1.Height + listBox1.Location.Y)
                {
                    listBox1.Enabled = true;
                }
            }
            if (e.X >= numericUpDown2.Location.X && e.X <= numericUpDown2.Width + numericUpDown2.Location.X)
            {
                if (e.Y >= numericUpDown2.Location.Y && e.Y <= numericUpDown2.Height + numericUpDown2.Location.Y)
                {
                    numericUpDown2.Enabled = true;
                }
            }
            if (e.X >= numericUpDown1.Location.X && e.X <= numericUpDown1.Width + numericUpDown1.Location.X)
            {
                if (e.Y >= numericUpDown1.Location.Y && e.Y <= numericUpDown1.Height + numericUpDown1.Location.Y)
                {
                    numericUpDown1.Enabled = true;
                }
            }
            if (e.X >= numericUpDown3.Location.X && e.X <= numericUpDown3.Width + numericUpDown3.Location.X)
            {
                if (e.Y >= numericUpDown3.Location.Y && e.Y <= numericUpDown3.Height + numericUpDown3.Location.Y)
                {
                    numericUpDown3.Enabled = true;
                }
            }
            if (e.X >= button3.Location.X && e.X <= button3.Width + button3.Location.X)
            {
                if (e.Y >= button3.Location.Y && e.Y <= button3.Height + button3.Location.Y)
                {
                    button3.Enabled = true;
                }
            }
            if (e.X >= button2.Location.X && e.X <= button2.Width + button2.Location.X)
            {
                if (e.Y >= button2.Location.Y && e.Y <= button2.Height + button2.Location.Y)
                {
                    button2.Enabled = true;
                }
            }
            if (e.X >= button1.Location.X && e.X <= button1.Width + button1.Location.X)
            {
                if (e.Y >= button1.Location.Y && e.Y <= button1.Height + button1.Location.Y)
                {
                    button1.Enabled = true;
                }
            }
        }
        
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (filters != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.W:
                        if(pixel.Y > 0) 
                        {
                            pixel.Y--;
                        }
                        break;
                    case Keys.S:
                        if(pixel.Y < filters.height-1)
                        {
                            pixel.Y++;
                        }
                        break;
                    case Keys.A:
                        if (pixel.X > 0)
                        {
                            pixel.X--;
                        }
                        break;
                    case Keys.D:
                        if (pixel.X < filters.width - 1)
                        {
                            pixel.X++;
                        }
                        break;
                    case Keys.Subtract:
                        if (disp < filters.width && disp < filters.height)
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
                    case Keys.Escape:
                        toggle_value = !toggle_value;
                        break;
                }
                this.Text = pixel.Y + " " + pixel.X + " image " + filters.height + " " + filters.width;
            }
            /*if(Math.Abs(mouse.X-numericUpDown2.Location.X)<=numericUpDown2.Width && Math.Abs(mouse.Y - numericUpDown2.Location.Y) <= numericUpDown2.Width)
            {
                e.Handled = false;
                e.SuppressKeyPress = false;
            }
            else
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }*/
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (filters != null)
            {
                switch (e.Button)
                {
                    case MouseButtons.Right:
                        if (disp < filters.width && disp < filters.height)
                        {
                            disp++;
                        }
                        break;
                    case MouseButtons.Left:
                        if (disp > 10)
                        {
                            disp--;
                        }
                        break;
                }
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            draw(CreateGraphics());
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            draw(e.Graphics);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Location = new Point(ClientSize.Width/2+button1.Width/2, 10);
            button2.Location = new Point(button1.Location.X - 10 - button2.Width, button1.Location.Y);
            button3.Location = new Point(button2.Location.X - 10 - button3.Width, button1.Location.Y);
            listBox1.Location = new Point(ClientSize.Width/2-listBox1.Width/2, button1.Location.Y+button1.Height+10);
            label1.Location = new Point(listBox1.Location.X,listBox1.Location.Y+listBox1.Height+10);
            numericUpDown1.Location = new Point(listBox1.Location.X+listBox1.Width - numericUpDown1.Width, label1.Location.Y);
            label2.Location = new Point(label1.Location.X,label1.Location.Y+label1.Height+10);
            numericUpDown2.Location = new Point(listBox1.Location.X+listBox1.Width-numericUpDown2.Width, label2.Location.Y);
            label3.Location = new Point(numericUpDown2.Location.X+numericUpDown2.Width+10, label2.Location.Y);
            numericUpDown3.Location = new Point(label3.Location.X + label3.Width + 10, label3.Location.Y);
            listBox1.Enabled = false;
            numericUpDown3.Enabled = false;
            numericUpDown2.Enabled = false;
            numericUpDown1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            //pictureBox1.Width = ClientSize.Height/2;
            //pictureBox1.Height = ClientSize.Height / 2;
            //pictureBox1.Location = new Point(0, 0);
            bg = new Bitmap(ClientSize.Width, ClientSize.Height);
            G = Graphics.FromImage(bg);
            mouse.X = 0;
            mouse.Y = 0;
            pixel.X = 0;
            pixel.Y = 0;
            t.Start();
        }
        void draw(Graphics g)
        {
            if(ClientSize.Width == 0 || ClientSize.Height == 0) return;
            G.Clear(BackColor);
            if (filters != null)
            {
                //pictureBox1.Image = filters.inp;
                G.DrawImage(filters.inp, new Rectangle(0, 0, ClientSize.Height / 2, ClientSize.Height / 2), new Rectangle(0, 0, filters.inp.Width, filters.inp.Height), GraphicsUnit.Pixel);
                if(filters.outp != null)
                {
                    G.DrawImage(filters.outp, new Rectangle(ClientSize.Width- ClientSize.Height / 2, 0, ClientSize.Height / 2, ClientSize.Height / 2), new Rectangle(0, 0, filters.inp.Width, filters.inp.Height), GraphicsUnit.Pixel);
                }
                int st_y = pixel.Y-(int)(disp/2);
                int st_x = pixel.X-(int)(disp/2);
                if( st_y < 0)
                {
                    st_y = 0;
                }
                if( st_y+(int)disp > filters.height-1) 
                {
                    st_y = filters.height-(int)disp;
                }
                if( st_x < 0)
                {
                    st_x = 0;
                }
                if (st_x + (int)disp > filters.width-1)
                {
                    st_x = filters.width - (int)disp;
                }
                G.DrawRectangle(Pens.Red, new Rectangle((st_x*(ClientSize.Height/2))/filters.width,(st_y*(ClientSize.Height/2))/filters.height, ((int)disp*(ClientSize.Height/2))/filters.width, ((int)disp * (ClientSize.Height / 2)) / filters.height));
                if(filters.outp != null)
                {
                    G.DrawRectangle(Pens.Red, new Rectangle((st_x * (ClientSize.Height / 2)) / filters.width + ClientSize.Width-ClientSize.Height/2, (st_y * (ClientSize.Height / 2)) / filters.height, ((int)disp * (ClientSize.Height / 2)) / filters.width, ((int)disp * (ClientSize.Height / 2)) / filters.height));
                }
                int tmp_i=0, tmp_j=0;
                for (int i = 0; i < disp; i++)
                {
                    for (int j = 0; j < disp; j++)
                    {
                        RectangleF rect = new RectangleF(j * (ClientSize.Height / 2 / disp), i * (ClientSize.Height / 2 / disp) + ClientSize.Height / 2, ClientSize.Height / 2 / disp, ClientSize.Height / 2 / disp);
                        G.FillRectangle(new SolidBrush(Color.FromArgb(filters.IMG[i+st_y, j+st_x], filters.IMG[i + st_y, j + st_x], filters.IMG[i + st_y, j + st_x])),rect);
                        RectangleF rect2;
                        if (filters.outp != null)
                        {
                            rect2 = new RectangleF(j * (ClientSize.Height / 2 / disp)+ClientSize.Width-ClientSize.Height/2, i * (ClientSize.Height / 2 / disp) + ClientSize.Height / 2, ClientSize.Height / 2 / disp, ClientSize.Height / 2 / disp);
                            G.FillRectangle(new SolidBrush(Color.FromArgb(filters.IMG2[i + st_y, j + st_x], filters.IMG2[i + st_y, j + st_x], filters.IMG2[i + st_y, j + st_x])), rect2);
                            if (toggle_value)
                            {
                                SizeF s = G.MeasureString(filters.IMG2[i + st_y, j + st_x] + "", this.Font);
                                float fontScale = Math.Max(s.Width / rect.Width, s.Height / rect.Height);
                                using (Font font = new Font(this.Font.FontFamily, this.Font.SizeInPoints / fontScale, GraphicsUnit.Point))
                                {
                                    Color c = Color.FromArgb(255 - filters.IMG2[i + st_y, j + st_x], 255 - filters.IMG2[i + st_y, j + st_x], 255 - filters.IMG2[i + st_y, j + st_x]);
                                    G.DrawString(filters.IMG2[i + st_y, j + st_x] + "", font, new SolidBrush(c), rect2, stringFormat);
                                }

                            }
                        }
                        if(i+st_y == pixel.Y && j+st_x == pixel.X) 
                        {
                            tmp_i=i;
                            tmp_j=j;
                        }
                        //G.DrawRectangle(Pens.White,j*(ClientSize.Height/2/disp),i*(ClientSize.Height/2/disp)+ClientSize.Height/2, ClientSize.Height / 2 / disp, ClientSize.Height / 2 / disp);
                        if(toggle_value)
                        {
                            SizeF s = G.MeasureString(filters.IMG[i + st_y, j + st_x]+"", this.Font);
                            float fontScale = Math.Max(s.Width / rect.Width, s.Height / rect.Height);
                            using (Font font = new Font(this.Font.FontFamily, this.Font.SizeInPoints / fontScale, GraphicsUnit.Point))
                            {
                                Color c = Color.FromArgb(255 - filters.IMG[i + st_y, j + st_x], 255 - filters.IMG[i + st_y, j + st_x], 255 - filters.IMG[i + st_y, j + st_x]);
                                G.DrawString(filters.IMG[i + st_y, j + st_x]+"", font, new SolidBrush(c), rect, stringFormat);
                            }

                        }
                    }
                }
                G.DrawRectangle(Pens.Blue,tmp_j * (ClientSize.Height / 2 / disp), tmp_i * (ClientSize.Height / 2 / disp) + ClientSize.Height / 2, ClientSize.Height / 2 / disp, ClientSize.Height / 2 / disp);
                if(filters.outp != null)
                {
                    G.DrawRectangle(Pens.Blue, tmp_j * (ClientSize.Height / 2 / disp) + ClientSize.Width-ClientSize.Height/2, tmp_i * (ClientSize.Height / 2 / disp) + ClientSize.Height / 2, ClientSize.Height / 2 / disp, ClientSize.Height / 2 / disp);
                    if(filters.last_size != 0)
                    {
                        G.DrawRectangle(Pens.Green, (tmp_j-filters.last_size/2) * (ClientSize.Height / 2 / disp), (tmp_i - filters.last_size / 2) * (ClientSize.Height / 2 / disp) + ClientSize.Height / 2, (ClientSize.Height / 2 / disp)*filters.last_size , (ClientSize.Height / 2 / disp)*filters.last_size);
                    }
                    if (old_pixel != pixel)
                    {
                        if (filters.last == (int)F.median || filters.last == (int)F.averaging || filters.last == (int)F.gaussian || filters.last==(int)F.laplacian || filters.last==(int)F.robert || filters.last==(int)F.sobel_horizontal || filters.last == (int)F.sobel_vertical || filters.last == (int)F.sobel_operators)
                        {
                            kernel = filters.median_at(pixel.Y, pixel.X);
                            if (filters.last == (int)F.sobel_vertical)
                            {
                                d_kernel = filters.sobel_kernel(filters.last_size, false);
                            }
                            else if(filters.last == (int)F.sobel_horizontal)
                            {
                                d_kernel = filters.sobel_kernel(filters.last_size, true);
                            }
                            else if (filters.last == (int)F.laplacian)
                            {
                                d_kernel = filters.laplacian_kernel(filters.last_size);
                            }
                            else if(filters.last == (int)F.gaussian)
                            {
                                d_kernel = filters.gauss_kernel(filters.last_size, filters.last_sigma);
                            }
                            else
                            {
                                d_kernel = null;
                            }
                        }
                        else
                        {
                            kernel = null;
                        }
                        old_pixel = pixel;
                    }

                    for(int i = 0; i <= filters.last_size && kernel!=null; i++)
                    {
                        if(filters.last_size == i)
                        {
                            G.DrawRectangle(Pens.Blue, ClientSize.Width / 2 - ClientSize.Height / 8,ClientSize.Height - ClientSize.Height / 4 / filters.last_size, ClientSize.Height / 4, ClientSize.Height / (4 *filters.last_size));
                            RectangleF rect = new RectangleF(ClientSize.Width / 2 - ClientSize.Height / 8, ClientSize.Height - ClientSize.Height / 4 / filters.last_size, ClientSize.Height / 4, ClientSize.Height / (4 * filters.last_size));
                            SizeF s;
                            if (pixel.Y < filters.height && pixel.X < filters.width)
                            {
                                s = G.MeasureString(filters.IMG2[pixel.Y, pixel.X] + "", this.Font);
                            }
                            else
                            {
                                s = G.MeasureString("0", this.Font);
                            }
                            float fontScale = Math.Max(s.Width / rect.Width, s.Height / rect.Height);
                            using (Font font = new Font(this.Font.FontFamily, this.Font.SizeInPoints / fontScale, GraphicsUnit.Point))
                            {
                                Color c = Color.FromArgb(255 - filters.IMG2[pixel.Y, pixel.X], 255 - filters.IMG2[pixel.Y, pixel.X], 255 - filters.IMG2[pixel.Y, pixel.X]);
                                G.DrawString(filters.IMG2[pixel.Y, pixel.X] + "", font, new SolidBrush(c), rect, stringFormat);
                            }
                            G.DrawRectangle(Pens.Blue, (int)(filters.last_size/2) * (ClientSize.Height / 4 / filters.last_size) + (ClientSize.Width / 2 - ClientSize.Height / 8), (int)(filters.last_size/2) * (ClientSize.Height / 4 / filters.last_size) + ClientSize.Height - (filters.last_size + 1) * (ClientSize.Height / 4 / filters.last_size), ClientSize.Height / 4 / filters.last_size, ClientSize.Height / 4 / filters.last_size);
                        }
                        else
                        {
                            for(int j=0;j< filters.last_size;j++)
                            {
                                if(d_kernel != null)
                                {
                                    RectangleF rect2 = new RectangleF(j * (ClientSize.Height / 4 / filters.last_size) + (ClientSize.Width / 2 - ClientSize.Height / 8), i * (ClientSize.Height / 4 / filters.last_size) + numericUpDown2.Location.Y+numericUpDown2.Height+10 , ClientSize.Height / 4 / filters.last_size, ClientSize.Height / 4 / filters.last_size);
                                    G.FillRectangle(new SolidBrush(Color.Gray), j * (ClientSize.Height / 4 / filters.last_size) + (ClientSize.Width / 2 - ClientSize.Height / 8), i * (ClientSize.Height / 4 / filters.last_size) + numericUpDown2.Location.Y + numericUpDown2.Height + 10, ClientSize.Height / 4 / filters.last_size, ClientSize.Height / 4 / filters.last_size);
                                    G.DrawRectangle(Pens.Black, j * (ClientSize.Height / 4 / filters.last_size) + (ClientSize.Width / 2 - ClientSize.Height / 8), i * (ClientSize.Height / 4 / filters.last_size) + numericUpDown2.Location.Y + numericUpDown2.Height + 10, ClientSize.Height / 4 / filters.last_size, ClientSize.Height / 4 / filters.last_size);
                                    SizeF s2 = G.MeasureString(string.Format("{0:.00}", d_kernel[i, j]) + " * " + kernel[i, j], this.Font);
                                    float fontScale2 = Math.Max(s2.Width / rect2.Width, s2.Height / rect2.Height);
                                    using (Font font = new Font(this.Font.FontFamily, this.Font.SizeInPoints / fontScale2, GraphicsUnit.Point))
                                    {
                                        //Color c = Color.FromArgb((int)(d_kernel[i,j]*kernel[i, j]), (int)(d_kernel[i, j] * kernel[i, j]), (int)(d_kernel[i, j] * kernel[i, j]));
                                        Color c = Color.White;
                                        G.DrawString(string.Format("{0:.00}", d_kernel[i, j])+" * "+ kernel[i, j], font, new SolidBrush(c), rect2, stringFormat);
                                    }
                                }
                                RectangleF rect= new RectangleF(j * (ClientSize.Height / 4 / filters.last_size) + (ClientSize.Width / 2 - ClientSize.Height / 8), i * (ClientSize.Height / 4 / filters.last_size) + ClientSize.Height - (filters.last_size + 1) * (ClientSize.Height / 4 / filters.last_size), ClientSize.Height / 4 / filters.last_size, ClientSize.Height / 4 / filters.last_size);
                                G.FillRectangle(new SolidBrush(Color.FromArgb(255, kernel[i, j], kernel[i, j], kernel[i, j])), j * (ClientSize.Height / 4 / filters.last_size) + (ClientSize.Width / 2 - ClientSize.Height / 8), i * (ClientSize.Height / 4 / filters.last_size)+ClientSize.Height-(filters.last_size+1)*(ClientSize.Height / 4 / filters.last_size), ClientSize.Height / 4 / filters.last_size, ClientSize.Height / 4 / filters.last_size);
                                SizeF s = G.MeasureString(kernel[i,j]+"", this.Font);
                                float fontScale = Math.Max(s.Width / rect.Width, s.Height / rect.Height);
                                using (Font font = new Font(this.Font.FontFamily, this.Font.SizeInPoints / fontScale, GraphicsUnit.Point))
                                {
                                    Color c = Color.FromArgb(255 - kernel[i, j], 255 - kernel[i, j], 255 - kernel[i, j]);
                                    G.DrawString(kernel[i, j] + "", font, new SolidBrush(c), rect, stringFormat);
                                }
                            }
                        }
                    }
                }
            }
            g.DrawImage(bg, 0, 0);

        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "jpg files(.*jpg)|*.jpg| PNG files(.*png)|*.png| All Files(*.*)|*.*";
                dialog.Title = "Choose Image File";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fname = dialog.FileName;
                    filters = new ImageFilters(new Bitmap(fname));
                    pixel = new Point(0, 0);
                    mouse = new Point(0, 0);
                    //this.Text = ClientSize.Height / 2 / filters.width + " " + ClientSize.Height / 2 / filters.height;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occured " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (filters == null || filters.outp == null)
                return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                filters.outp.Save(filePath);
                MessageBox.Show(filePath + " saved successfully!!");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(filters == null || filters.outp == null)
                return;
            Bitmap tmp = filters.outp;
            filters = new ImageFilters(tmp);
            pixel = new Point(0, 0);
        }
    }
}