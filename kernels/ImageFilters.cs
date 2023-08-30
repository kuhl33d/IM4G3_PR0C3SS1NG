using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing.Imaging;
using System.Xml;

namespace kernels
{
    public class ImageFilters
    {
        public Bitmap inp=null;
        public int[,] IMG=null;
        public Bitmap outp = null;
        public int[,] IMG2 = null;
        public int width=0, height=0;
        public int last = -1;
        public int last_size = -1;
        public double last_sigma = -1;
        public int[,] KK;
        public ImageFilters(Bitmap img)//gray scaling 
        {
            inp = new Bitmap(img);
            width = img.Width;
            height = img.Height;
            IMG = new int[inp.Height, inp.Width];
            for(int i=0;i<inp.Height; i++)
            {
                for(int j = 0; j < inp.Width; j++)
                {
                    Color pixel = inp.GetPixel(j,i);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    IMG[i,j] = gray;
                    inp.SetPixel(j,i,Color.FromArgb(255,gray,gray,gray));
                }
            }
            
        }
        public void unsharp_masking(int size,double sigma, double strength,ref Bitmap blurred,ref Bitmap mask)
        {
            gauss(size, sigma);
            blurred = new Bitmap(outp);
            mask = operation_image(inp, blurred,'-');
            mul_mask(ref mask, strength);
            //enhanced
            outp = operation_image(inp, mask, '+');
            IMG2 = new int[inp.Height, inp.Width];
            for(int i = 0; i < outp.Height; i++)
            {
                for(int j=0;j< inp.Width; j++)
                {
                    IMG2[i,j] = (outp.GetPixel(j, i).R + outp.GetPixel(j, i).B + outp.GetPixel(j, i).G)/ 3;
                }
            }
        }
        public void thershold(int thres)
        {
            for (int i = 0; i < outp.Height; i++)
            {
                for (int j = 0; j < outp.Width; j++)
                {
                    Color pixel = outp.GetPixel(j, i);
                    if (pixel.R < thres)
                    {
                        outp.SetPixel(j, i, Color.Black);
                        IMG2[i, j] = 0;
                    }
                }
            }
        }
        public void apply_thershold(int thres)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    Color pixel = inp.GetPixel(j, i);
                    if (pixel.R < thres)
                    {
                        outp.SetPixel(j, i, Color.Black);
                        IMG2[i, j] = 0;
                    }
                    else
                    {
                        outp.SetPixel(j, i, Color.White);
                        IMG2[i, j] = 255;
                    }
                }
            }
        }
        public void mul_mask(ref Bitmap mask,double strength)
        {
            for(int i = 0; i < mask.Height; i++)
            {
                for(int j=0;j< mask.Width; j++)
                {
                    int p1 = ((mask.GetPixel(j, i).R + mask.GetPixel(j, i).B + mask.GetPixel(j, i).G) / 3);
                    if(p1*strength > 255)
                    {
                        p1 = 255;
                    }
                    else
                    {
                        p1 = (int)(p1*strength);
                    }
                    mask.SetPixel(j, i, Color.FromArgb(p1, p1, p1));
                }
            }
        }
        public Bitmap operation_image(Bitmap img1,Bitmap img2,char op)
        {
            Bitmap result = new Bitmap(img1.Width,img1.Height);
            int res;
            byte p1;
            byte p2;
            for (int i = 0; i < result.Height; i++)
            {
                for(int j=0;j<result.Width; j++)
                {
                    p1 = (byte)((img1.GetPixel(j, i).R + img1.GetPixel(j, i).B+ img1.GetPixel(j, i).G) / 3);
                    p2 = (byte)((img2.GetPixel(j, i).R + img2.GetPixel(j, i).B + img2.GetPixel(j, i).G) / 3);
                    switch (op)
                    {
                        case '+':
                            if(p1+p2>255)
                                res = 255;
                            else
                                res = p1+p2;
                            result.SetPixel(j, i, Color.FromArgb(res, res, res));
                            break;
                        case '-':
                            if (p1 - p2 < 0)
                                res = 0;
                            else
                                res = p1 - p2;
                            result.SetPixel(j, i, Color.FromArgb(res, res, res));
                            break;
                    }
                }
            }
            return result;
        }
        public void uniform_noise(byte minInt,byte maxInt)
        {
            IMG2 = new int[height, width];
            outp = new Bitmap(width, height);
            Random R = new Random();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    byte noise = (byte)R.Next(minInt, maxInt+ 1);
                    int newValue = (inp.GetPixel(j, i).R + noise) / 2;
                    IMG2[i,j] = newValue;
                    outp.SetPixel(j, i, Color.FromArgb(newValue, newValue, newValue));
                }
            }
        }
        public void gaussian_noise(double mean,double std)
        {
            outp = new Bitmap(width,height);
            IMG2 = new int[height,width];
            Random R = new Random();
            for(int i = 0; i < height; i++)
            {
                for(int j=0;j<width; j++)
                {
                    double gaussianValue = R.NextDouble();
                    double noise = mean + std* Math.Sqrt(-2 * Math.Log(gaussianValue)) * Math.Cos(2 * Math.PI * R.NextDouble());
                    int newValue = (int)Math.Round(inp.GetPixel(j, i).R + noise);

                    //trimming
                    newValue = Math.Max(0, Math.Min(255, newValue));

                    outp.SetPixel(j, i, Color.FromArgb(newValue, newValue, newValue));
                    IMG2[i, j] = newValue;
                }
            }
        }
        public void impluse_noise(double noise_ratio=0.05,byte minInt=0,byte maxInt=255)
        {
            IMG2 = new int[height,width];
            outp = new Bitmap(width,height);
            Random R = new Random();
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    if (R.NextDouble() < noise_ratio)
                    {
                        byte val = R.Next(2) == 0 ? minInt:maxInt;
                        outp.SetPixel(j, i, Color.FromArgb(val, val, val));
                        IMG2[i, j] = val;
                    }
                    else
                    {
                        outp.SetPixel(j, i, inp.GetPixel(j,i));
                        IMG2[i, j] = IMG[i,j];
                    }
                }
            }
        }
        public void median(int size,int padding=0)
        {
            if (size % 2 == 0)
                return;
            int[,] kernel = new int[size, size];
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            List<int> ints = new List<int>();
            for(int i = 0; i < inp.Height; i++)
            {
                for(int j=0;j< inp.Width; j++)
                {
                    ints.Clear();
                    for(int r = 0; r < size; r++)
                    {
                        for(int c = 0; c < size; c++)
                        {
                            if(i-(size/2)+r<0 || i- (size / 2) + r >= inp.Height || j- (size / 2) + c<0 || j- (size / 2) + c>=inp.Width)
                            {
                                kernel[r, c] = padding;
                                ints.Add(padding);
                            }
                            else
                            {
                                kernel[r, c] = IMG[i - (size / 2) + r, j - (size / 2) + c];
                                ints.Add(IMG[i - (size / 2) + r, j - (size / 2) + c]);
                            }
                        }
                    }
                    ints.Sort();
                    IMG2[i,j] = ints[(int)(ints.Count/2)];
                    outp.SetPixel(j,i,Color.FromArgb(255, IMG2[i, j], IMG2[i, j], IMG2[i, j]));
                }
            }
        }
        public void sobel_operators(int size,int padding = 0)
        {
            outp = new Bitmap(width, height);
            IMG2 = new int[height, width];
            double[,] sobel_h = conv_sobel(sobel_kernel(size, true), size);
            double[,] sobel_v = conv_sobel(sobel_kernel(size, false), size);

            double[,] S = new double[height, width];
            int max = int.MinValue;
            int min = int.MaxValue;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    S[i, j] = Math.Sqrt(Math.Pow(sobel_h[i, j], 2) + Math.Pow(sobel_v[i, j], 2));
                    IMG2[i, j] = (int)S[i, j];
                    if ((int)S[i, j] > max)
                        max = (int)S[i, j];
                    if ((int)S[i, j] < min)
                        min = (int)S[i, j];
                }
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    IMG2[i, j] = 255 * (IMG2[i, j] - min) / (max - min);
                    outp.SetPixel(j, i, Color.FromArgb(IMG2[i, j], IMG2[i, j], IMG2[i, j]));
                }
            }
        }
        public double[,] conv_sobel(double[,] kernel,int size,int padding=0)
        {
            double[,] val = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double sum = 0;
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            if (i - (size/2) + r < 0 || i - (size/2) + r >= inp.Height || j - (size/2) + c < 0 || j - (size/2) + c >= inp.Width)
                            {
                                sum += 0;
                            }
                            else
                            {
                                sum += (int)(IMG[i - (size/2) + r, j - (size/2) + c] * kernel[r, c]);
                            }
                        }
                    }
                    val[i, j] = sum;
                }
            }
            return val;
        }
        public double[,] sobel_kernel(int size, bool horizontal)
        {
            double[,] kernel = new double[size, size];
            int center = size / 2;
            int value = -center;
            if (horizontal)
            {

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        kernel[i, j] = value;
                        value++;
                    }
                    value = -center;
                }

            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        kernel[i, j] = value;
                    }
                    value++;
                }
            }
            return kernel;
        }
        public void sobel(int size, bool is_horizaontal, int padding = 0)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            double[,] kernel = sobel_kernel(size, is_horizaontal);
            int min = int.MaxValue;
            int max = int.MinValue;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int sum = 0;
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            if (i - (size / 2) + r < 0 || i - (size / 2) + r >= inp.Height || j - (size / 2) + c < 0 || j - (size / 2) + c >= inp.Width)
                            {
                                sum += (int)(padding * kernel[r, c]);
                            }
                            else
                            {
                                sum += (int)(IMG[i - (size / 2) + r, j - (size / 2) + c] * kernel[r, c]); ;
                            }
                        }
                    }
                    IMG2[i, j] = sum;
                    if (sum > max)
                        max = sum;
                    if (sum < min)
                        min = sum;
                }
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    IMG2[i, j] = 255 * (IMG2[i, j] - min) / (max - min);
                    outp.SetPixel(j, i, Color.FromArgb(IMG2[i, j], IMG2[i, j], IMG2[i, j]));
                }
            }
        }
        double[,] conv_robert(bool is_X)
        {
            double[,] kernel = new double[2,2];
            if (is_X)
            {
                kernel[0, 0] = -1;
                kernel[0, 1] = 0;
                kernel[1, 0] = 0;
                kernel[1, 1] = 1;
            }
            else
            {
                kernel[0, 0] = 0;
                kernel[0, 1] = -1;
                kernel[1, 0] = 1;
                kernel[1, 1] = 0;
            }
            double[,] val = new double[height,width];
            for(int i=0;i< height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    double sum = 0;
                    for (int r = 0; r < 2; r++)
                    {
                        for (int c = 0; c < 2; c++)
                        {
                            if (i - 1 + r < 0 || i - 1 + r >= inp.Height || j - 1 + c < 0 || j - 1 + c >= inp.Width)
                            {
                                sum += 0;
                            }
                            else
                            {
                                sum+= (int)(IMG[i - 1 + r, j - 1 + c] * kernel[r, c]);
                            }
                        }
                    }
                    val[i, j] = sum;
                }
            }

            return val;
        }
        public void robert_cross(int padding = 0)
        {
            double[,] gradX = conv_robert(true);
            double[,] gradY = conv_robert(false);

            double[,] cross = new double[height,width];
            outp = new Bitmap(width,height);
            IMG2 = new int[height,width];
            int max = int.MinValue;
            int min = int.MaxValue;
            for(int i = 0; i < height; i++)
            {
                for(int j=0;j<width; j++)
                {
                    cross[i,j] = Math.Sqrt(Math.Pow(gradX[i,j],2)+ Math.Pow(gradY[i, j], 2));
                    IMG2[i,j] = (int)cross[i,j];
                    if ((int)cross[i, j] > max)
                        max = (int)cross[i, j];
                    if((int)cross[i, j] < min)
                        min = (int)cross[i, j];
                }
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    IMG2[i, j] = 255 * (IMG2[i, j] - min) / (max - min);
                    outp.SetPixel(j, i, Color.FromArgb(IMG2[i,j], IMG2[i, j], IMG2[i, j]));
                }
            }
        }
        public double[,] laplacian_kernel(int size,int padding = 0)
        {
            if (size < 3 || size % 2 == 0)
                return null;
            double[,] kernel = new double[size, size];
            int center = size / 2;

            int centerValue = size * size - 1;
            int edgeValue = -1;
            int cornerValue = -2;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == center && j == center)
                        kernel[i, j] = centerValue;
                    else if (i == center - 1 || i == center + 1)
                    {
                        if (j == center - 1 || j == center + 1)
                            kernel[i, j] = cornerValue;
                        else
                            kernel[i, j] = edgeValue;
                    }
                    else if (j == center - 1 || j == center + 1)
                        kernel[i, j] = edgeValue;
                    else
                        kernel[i, j] = 0;
                }
            }

            return kernel;
        }
        public void laplacian(int size,int padding = 0)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            double[,] mask = laplacian_kernel(size);
            int[,] kernel = new int[size, size];
            int max = int.MinValue;
            int min = int.MaxValue;
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            if (i - (size / 2) + r < 0 || i - (size / 2) + r >= inp.Height || j - (size / 2) + c < 0 || j - (size / 2) + c >= inp.Width)
                            {
                                kernel[r, c] = (int)(padding * mask[r, c]);
                            }
                            else
                            {
                                kernel[r, c] = (int)(IMG[i - (size / 2) + r, j - (size / 2) + c] * mask[r, c]);
                            }
                        }
                    }
                    int sum = 0;
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            sum += kernel[r, c];
                        }
                    }
                    IMG2[i, j] = sum;
                    if (sum > max)
                        max = sum;
                    if (sum < min)
                        min = sum;
                }
            }
            for(int  i = 0; i < inp.Height; i++)
            {
                for(int j=0;j< inp.Width; j++)
                {
                    IMG2[i, j] = 255 * (IMG2[i,j]-min)/(max-min);
                    outp.SetPixel(j, i, Color.FromArgb(IMG2[i, j], IMG2[i, j], IMG2[i, j]));
                }
            }
        }
        public double[,] gauss_kernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double kernelSum = 0;
            int foff = (size - 1) / 2;
            double distance = 0;
            double constant = 1d / (2.0f * Math.PI * sigma * sigma);
            for (int y = -foff; y <= foff; y++)
            {
                for (int x = -foff; x <= foff; x++)
                {
                    distance = ((y * y) + (x * x)) / (2 * sigma * sigma);
                    kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
                    kernelSum += kernel[y + foff, x + foff];
                }
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] = kernel[y, x] * 1d / kernelSum;
                }
            }
            return kernel;
        }
        public void gauss(int size,double sigma,int padding=0)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            double[,] mask = gauss_kernel(size, sigma);
            int[,] kernel = new int[size, size];
            for(int i=0;i<inp.Height; i++)
            {
                for(int j=0;j<inp.Width; j++)
                {
                    for(int r = 0; r < size; r++)
                    {
                        for(int c=0; c < size; c++)
                        {
                            if (i - (size / 2) + r < 0 || i - (size / 2) + r >= inp.Height || j - (size / 2) + c < 0 || j - (size / 2) + c >= inp.Width)
                            {
                                kernel[r, c] = (int)(padding * mask[r,c]); 
                            }
                            else
                            {
                                kernel[r, c] = (int)(IMG[i - (size / 2) + r, j - (size / 2) + c] * mask[r, c]);
                            }
                        }
                    }
                    int sum = 0;
                    for(int r = 0; r < size; r++)
                    {
                        for(int c=0;c<size; c++)
                        {
                            sum += kernel[r, c];
                        }
                    }
                    IMG2[i, j] = sum;
                    outp.SetPixel(j,i,Color.FromArgb(255,sum,sum,sum));
                }
            }
        }
        public void adaptive_median(int s_max,int padding = 0)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            List<int> ints = new List<int>();
            int size = 3;
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    A:
                    ints.Clear();
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            if (i - (size / 2) + r < 0 || i - (size / 2) + r >= inp.Height || j - (size / 2) + c < 0 || j - (size / 2) + c >= inp.Width)
                            {
                                ints.Add(padding);
                            }
                            else
                            {
                                ints.Add(IMG[i - (size / 2) + r, j - (size / 2) + c]);
                            }
                        }
                    }
                    for(int x = 0; x < ints.Count; x++)
                    {
                        bool f = false;
                        for(int y = 0; y < ints.Count - x - 1; y++)
                        {
                            if (ints[y] > ints[y + 1])
                            {
                                f = true;
                                int t = ints[y];
                                ints[y] = ints[y + 1];
                                ints[y + 1] = t;
                            }
                        }
                        if (f == false)
                        {
                            break;
                        }
                    }
                    int min = ints[0];
                    int max = ints[ints.Count - 1];
                    int med = ints[(int)(ints.Count / 2)];
                    if(med-min > 0 && med-max < 0)
                    {
                        goto B;
                    }
                    else
                    {
                        size++;
                        if(size <= s_max)
                        {
                            goto A;
                        }
                        else
                        {
                            IMG2[i, j] = med;
                            outp.SetPixel(j, i, Color.FromArgb(255, med, med, med));
                            size = 3;
                            continue;
                        }
                    }
                    B:
                    if (IMG[i,j]-min > 0 && IMG2[i,j]-max < 0)
                    {
                        IMG2[i, j] = IMG[i, j];
                        outp.SetPixel(j, i, Color.FromArgb(255, IMG[i,j], IMG[i,j], IMG[i,j]));
                    }
                    else
                    {
                        IMG2[i, j] = med;
                        outp.SetPixel(j, i, Color.FromArgb(255, med, med, med));
                    }
                    size = 3;
                }
            }

        }
        public void adaptive_min(int s_max, int padding = 0)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            List<int> ints = new List<int>();
            int size = 3;
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    size = 3;
                    A:
                    ints.Clear();
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            if (i - (size / 2) + r < 0 || i - (size / 2) + r >= inp.Height || j - (size / 2) + c < 0 || j - (size / 2) + c >= inp.Width)
                            {
                                ints.Add(padding);
                            }
                            else
                            {
                                ints.Add(IMG[i - (size / 2) + r, j - (size / 2) + c]);
                            }
                        }
                    }
                    int min = ints[1];
                    for(int k = 1; k < ints.Count; k++)
                    {
                        if (ints[k]< min)
                        {
                            min = ints[k];
                        }
                    }
                    if (IMG[i, j] != min)
                    {
                        if(size != s_max)
                        {
                            size += 2;
                            goto A;
                        }
                    }
                    IMG2[i, j] = min;
                    outp.SetPixel(j, i, Color.FromArgb(255, min, min, min));
                }
            }
        }
        public void adaptive_max(int s_max, int padding = 0)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            List<int> ints = new List<int>();
            int size = 3;
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    size = 3;
                A:
                    ints.Clear();
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            if (i - (size / 2) + r < 0 || i - (size / 2) + r >= inp.Height || j - (size / 2) + c < 0 || j - (size / 2) + c >= inp.Width)
                            {
                                ints.Add(padding);
                            }
                            else
                            {
                                ints.Add(IMG[i - (size / 2) + r, j - (size / 2) + c]);
                            }
                        }
                    }
                    int max = ints[0];
                    for(int k = 1; k < ints.Count; k++)
                    {
                        if (ints[k]> max)
                        {
                            max = ints[k];
                        }
                    }
                    if (IMG[i, j] != max)
                    {
                        if (size != s_max)
                        {
                            size += 2;
                            goto A;
                        }
                    }
                    IMG2[i, j] = max;
                    outp.SetPixel(j, i, Color.FromArgb(255, max, max, max));
                }
            }
        }
        public void averaging(int size, int padding = 0)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    int ct = 0;
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            if (i - (size / 2) + r < 0 || i - (size / 2) + r >= inp.Height || j - (size / 2) + c < 0 || j - (size / 2) + c >= inp.Width)
                            {
                                ct += padding;
                            }
                            else
                            {
                                ct += IMG[i - (size / 2) + r, j - (size / 2) + c];
                            }
                        }
                    }
                    IMG2[i, j] = (int)(ct / (size * size));
                    outp.SetPixel(j, i, Color.FromArgb(255, IMG2[i,j], IMG2[i, j], IMG2[i, j]));
                }
            }
        }
        public int[,] median_at(int i,int j,int padding=0)
        {
            int[,] kernel = new int[last_size, last_size];
            for(int r=0;r<last_size; r++)
            {
                for(int c = 0; c < last_size; c++)
                {
                    if (i - (last_size/2) + r < 0 || i - (last_size / 2) + r >= inp.Height || j - (last_size / 2) + c < 0 || j - (last_size / 2) + c >= inp.Width)
                    {
                        kernel[r, c] = padding;
                    }
                    else
                    {
                        kernel[r, c] = IMG[i - (last_size / 2) + r, j - (last_size / 2) + c];
                    }
                }
            }
            return kernel;
        }

        public void conv(int[,] kernel,int size,int padding=0)
        {
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            int max = int.MinValue;
            int min = int.MaxValue;
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    int S = 0;
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                        {
                            if (i - (size / 2) + r < 0 || i - (size / 2) + r >= inp.Height || j - (size / 2) + c < 0 || j - (size / 2) + c >= inp.Width)
                            {
                                
                            }
                            else
                            {
                                S += kernel[r, c] * IMG[i - (size / 2) + r, j - (size / 2) + c];
                            }
                        }
                    }
                    if(S > max)
                    {
                        max = S;
                    }
                    if(S < min)
                    {
                        min = S;
                    }
                    IMG2[i, j] = S;
                }
            }
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    IMG2[i, j] = 255 * (IMG2[i, j] - min) / (max - min);
                    outp.SetPixel(j, i, Color.FromArgb(IMG2[i, j], IMG2[i, j], IMG2[i, j]));
                }
            }
        }
        public int[,] rotate_kernel_clock(int[,] kernel,int size)
        {
            int[,] new_k = new int[size, size];
            for(int i = 0; i < size; i++)
            {
                for(int j=0;j< size; j++)
                {
                    new_k[j,size-1-i] = kernel[i,j];
                }
            }
            return new_k;
        }
        public void histogram_equalization(ref int []H,ref int[] H_NEW,ref Bitmap outp,ref int[,] IMG2)
        {
            if(H == null)
                return;
            outp = new Bitmap(inp.Width, inp.Height);
            IMG2 = new int[inp.Height, inp.Width];
            float[] h_i_hat = new float[256];
            float[] CDF = new float[256];
            H_NEW = new int[256];
            for(int i = 0; i < 256; i++)
            {
                H_NEW[i] = 0;
                h_i_hat[i] = (H[i] / (float)(inp.Width*inp.Height));
                if(i != 0)
                {
                    CDF[i] = CDF[i - 1] + h_i_hat[i];
                }
                else
                {
                    CDF[i] = h_i_hat[i];
                }
            }
            for (int i = 0; i < inp.Height; i++)
            {
                for (int j = 0; j < inp.Width; j++)
                {
                    int p = (int)(Math.Round(CDF[IMG[i, j]] * 255));
                    H_NEW[p]++;
                    IMG2[i, j] = p;
                    //IMG2[i, j] = (int)(ct / (size * size));
                    outp.SetPixel(j, i, Color.FromArgb(255, IMG2[i, j], IMG2[i, j], IMG2[i, j]));
                }
            }
        }
        public Bitmap draw_hisotgram(Bitmap inp,ref int[] H)
        {
            Bitmap HIS = new Bitmap(1000, 1000);
            Graphics G = Graphics.FromImage(HIS);
            int L = 16;
            //int seg = width / 16;
            G.Clear(Color.White);
            int max = -1;
            H = new int[256];
            for (int i = 0; i < 256; H[i] = 0, i++) ;
            for(int i=0;i< inp.Height; i++)
            {
                for(int j=0;j<inp.Width; j++)
                {
                    Color c = inp.GetPixel(j, i);
                    int p = (c.R+c.G+c.B)/3;
                    H[p]++;
                }
            }
            for (int i = 0; i < 256; i++)
            {
                if (H[i] > max)
                    max = H[i];
            }
            for (int i = 0; i < 256 / L; i++)
            {
                for (int j = i * L; j < (i + 1) * L; j++)
                {
                    int x = j * (width / 256);
                    int y = height - (H[j] * height) / max;
                    G.FillRectangle(Brushes.Black, x, y, width / 256, (H[j] * height) / max);
                }
            }
            return HIS;
        }

        public Bitmap interpolation_nearest(Bitmap inp)
        {
            Bitmap outp = new Bitmap(inp.Width * 2, inp.Height * 2);
            IMG2 = new int[inp.Height *2, inp.Width * 2];
            for(int i=0,r=0;i< inp.Height; i++,r+=2)
            {
                for(int j = 0,c=0; j < inp.Width; j++,c+=2)
                {
                    Color C = inp.GetPixel(j, i);
                    int p = (C.R + C.G + C.B) / 3;
                    IMG2[r, c] = p;
                    IMG2[r, c+1] = p;
                    IMG2[r+1, c] = p;
                    IMG2[r+1, c + 1] = p;
                    outp.SetPixel(c,r,Color.FromArgb(p,p,p));
                    outp.SetPixel(c+1, r, Color.FromArgb(p, p, p));
                    outp.SetPixel(c, r+1, Color.FromArgb(p, p, p));
                    outp.SetPixel(c + 1, r+1, Color.FromArgb(p, p, p));
                }
            }
            return outp;
        }
        public Bitmap interpolation_bilinear(Bitmap inp)
        {
            Bitmap outp = new Bitmap(inp.Width * 2, inp.Height * 2);

            for (int r = 0; r < outp.Height; r++)
            {
                for (int c = 0; c < outp.Width; c++)
                {
                    float origX = c / 2.0f;
                    float origY = r / 2.0f;

                    int x1 = (int)Math.Floor(origX);
                    int x2 = Math.Min(x1 + 1, inp.Width - 1);
                    int y1 = (int)Math.Floor(origY);
                    int y2 = Math.Min(y1 + 1, inp.Height - 1);

                    Color C1 = inp.GetPixel(x1, y1);
                    Color C2 = inp.GetPixel(x2, y1);
                    Color C3 = inp.GetPixel(x1, y2);
                    Color C4 = inp.GetPixel(x2, y2);

                    float alpha = origX - x1;
                    float beta = origY - y1;

                    int R = (int)((1 - alpha) * (1 - beta) * C1.R + alpha * (1 - beta) * C2.R + (1 - alpha) * beta * C3.R + alpha * beta * C4.R);
                    int G = (int)((1 - alpha) * (1 - beta) * C1.G + alpha * (1 - beta) * C2.G + (1 - alpha) * beta * C3.G + alpha * beta * C4.G);
                    int B = (int)((1 - alpha) * (1 - beta) * C1.B + alpha * (1 - beta) * C2.B + (1 - alpha) * beta * C3.B + alpha * beta * C4.B);

                    outp.SetPixel(c, r, Color.FromArgb(R, G, B));
                }
            }

            return outp;
        }

    }
    struct COMPLEX
    {
        public double real, imag;
        public COMPLEX(double x, double y)
        {
            real = x;
            imag = y;
        }
        public float Magnitude()
        {
            return ((float)Math.Sqrt(real * real + imag * imag));
        }
        public float Phase()
        {
            return ((float)Math.Atan(imag / real));
        }
    }
    class FFT
    {
        public Bitmap Obj;
        public Bitmap FourierPlot;
        public Bitmap PhasePlot;

        public int[,] GreyImage;
        public double[,] FourierMagnitude;
        public float[,] FourierPhase;

        public double[,] FFTLog;
        public float[,] FFTPhaseLog;
        public int nx, ny;
        public int Width, Height;
        public COMPLEX[,] Fourier;
        public COMPLEX[,] FFTShifted;
        public COMPLEX[,] Output;
        public COMPLEX[,] FFTNormal;
        public double[,] FFTNormalized;
        public int[,] FFTPhaseNormalized;

        /*
        fft = new FFT(test);
        fft.ForwardFFT();
        fft.FFTShift();
        for(int i = 0; i < 50; i++)
        {
            for(int j = 0; j < 50; j++)
            {
                fft.Fourier[fft.nx / 2 - 25 + i, fft.ny / 2 - 25 + j] = new COMPLEX(0, 0);
            }
        }
        fft.FFTPlot(fft.Fourier, fft.Obj.Width);
        outputImage = fft.FourierPlot;
        for(int i=0;i<fft.ny;i++)
        {
            for(int j=0;j<fft.nx;j++)
            {
                fft.Fourier[j, i].real /= fft.Obj.Width;
            }
        }
        fft.Fourier = fft.rev(fft.Fourier);
        fft.InverseFFT();
        recon = fft.Obj;
        */
        public FFT(Bitmap Input)
        {
            int Max = Math.Max(Input.Width, Input.Height);
            int size = 1;
            while (size < Max)
            {
                size <<= 1;
            }
            Bitmap paddedImage = PadImage(Input, size, size);

            Obj = paddedImage;
            Width = nx = size;
            Height = ny = size;
            ReadImage();
        }
        /*public void inverse_from_image(Bitmap Input, double cons)
        {
            if (Input.Width != Input.Height)
            {
                int Max = Math.Max(Input.Width, Input.Height);
                int size = 1;
                while (size < Max)
                {
                    size <<= 1;
                }
                Input = PadImage(Input, size, size);
            }
            Obj = Input;
            Width = nx = Input.Width;
            Height = ny = Input.Height;
            ReadImage();
            int i, j;
            Output = new COMPLEX[Width, Height];
            for (i = 0; i <= Width - 1; i++)
            {
                for (j = 0; j <= Height - 1; j++)
                {
                    Fourier[i, j].real /= 1 / cons;
                }
            }
            RemoveFFTShift();
            ForwardFFT(); // Compute forward Fourier transformation
            Fourier = FFTNormal;
            InverseFFT(); // Compute inverse Fourier transformation
        }*/

        Bitmap PadImage(Bitmap image, int paddedWidth, int paddedHeight)
        {
            Bitmap paddedImage = new Bitmap(paddedWidth, paddedHeight);
            using (Graphics graphics = Graphics.FromImage(paddedImage))
            {
                graphics.DrawImage(image, new Rectangle(0, 0, paddedWidth, paddedHeight), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            return paddedImage;
        }
        private void ReadImage()
        {
            int i, j;
            GreyImage = new int[Width, Height];  //[Col,Row]
            Bitmap image = Obj;
            Fourier = new COMPLEX[Width, Height];
            for (i = 0; i < Height; i++)
            {
                for (j = 0; j < Width; j++)
                {
                    Color pixel = image.GetPixel(j, i);
                    GreyImage[j, i] = (pixel.R + pixel.G + pixel.B) / 3;
                    Fourier[j, i].real = (double)GreyImage[j, i];
                    Fourier[j, i].imag = 0;
                }
            }
            /*
            BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;

                for (i = 0; i < bitmapData1.Height; i++)
                {
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        GreyImage[j, i] = (int)((imagePointer1[0] + imagePointer1[1] + imagePointer1[2]) / 3.0);
                        //4 bytes per pixel
                        imagePointer1 += 4;
                    }//end for j
                    //4 bytes per pixel
                    imagePointer1 += bitmapData1.Stride - (bitmapData1.Width * 4);
                }//end for i
            }//end unsafe
            image.UnlockBits(bitmapData1);
            */
            return;
        }
        public Bitmap Displayimage()
        {
            int i, j;
            Bitmap image = new Bitmap(Width, Height);
            /*
            for (i = 0; i < Height; i++)
            {
                for (j = 0; j < Width; j++)
                {
                    image.SetPixel(j, i, Color.FromArgb(GreyImage[j, i], GreyImage[j, i], GreyImage[j, i]));
                }
            }
            */
            BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {

                byte* imagePointer1 = (byte*)bitmapData1.Scan0;

                for (i = 0; i < bitmapData1.Height; i++)
                {
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        // write the logic implementation here
                        imagePointer1[0] = (byte)GreyImage[j, i];
                        imagePointer1[1] = (byte)GreyImage[j, i];
                        imagePointer1[2] = (byte)GreyImage[j, i];
                        imagePointer1[3] = (byte)255;
                        //4 bytes per pixel
                        imagePointer1 += 4;
                    }//end for j

                    //4 bytes per pixel
                    imagePointer1 += (bitmapData1.Stride - (bitmapData1.Width * 4));
                }//end for i
            }//end unsafe
            image.UnlockBits(bitmapData1);
            return image;// col;
        }
        public Bitmap Displayimage(int[,] image)
        {
            int i, j;
            Bitmap output = new Bitmap(image.GetLength(0), image.GetLength(1));
            /*
            int max = int.MinValue;
            int min = int.MaxValue;

            // Find the maximum and minimum values in the image array
            for (i = 0; i < output.Height; i++)
            {
                for (j = 0; j < output.Width; j++)
                {
                    int pixelValue = image[j, i];
                    if (pixelValue > max)
                        max = pixelValue;
                    if (pixelValue < min)
                        min = pixelValue;
                }
            }

            // Map the pixel values to the 0-255 range
            for (i = 0; i < output.Height; i++)
            {
                for (j = 0; j < output.Width; j++)
                {
                    int pixelValue = image[j, i];
                    int mappedValue = (int)((pixelValue - min) * 255.0 / (max - min));

                    output.SetPixel(j, i, Color.FromArgb(mappedValue, mappedValue, mappedValue));
                }
            }
            int i, j;
            Bitmap output = new Bitmap(image.GetLength(0), image.GetLength(1));
            int max = int.MinValue;
            int min = int.MaxValue;
            for(i = 0; i < output.Height; i++)
            {
                for(j=0;j<output.Width; j++)
                {
                    output.SetPixel(j, i, Color.FromArgb(image[j, i], image[j, i], image[j, i]));
                }
            }*/
            BitmapData bitmapData1 = output.LockBits(new Rectangle(0, 0, image.GetLength(0), image.GetLength(1)), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;
                for (i = 0; i < bitmapData1.Height; i++)
                {
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        imagePointer1[0] = (byte)image[j, i];
                        imagePointer1[1] = (byte)image[j, i];
                        imagePointer1[2] = (byte)image[j, i];
                        imagePointer1[3] = 255;
                        //4 bytes per pixel
                        imagePointer1 += 4;
                    }//end for j
                    //4 bytes per pixel
                    imagePointer1 += (bitmapData1.Stride - (bitmapData1.Width * 4));
                }//end for i
            }//end unsafe
            output.UnlockBits(bitmapData1);
            return output;// col;

        }
        public Bitmap Displayimage(double[,] image)
        {
            int i, j;
            Bitmap output = new Bitmap(image.GetLength(0), image.GetLength(1));
            BitmapData bitmapData1 = output.LockBits(new Rectangle(0, 0, image.GetLength(0), image.GetLength(1)), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;
                for (i = 0; i < bitmapData1.Height; i++)
                {
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        imagePointer1[0] = (byte)image[j, i];
                        imagePointer1[1] = (byte)image[j, i];
                        imagePointer1[2] = (byte)image[j, i];
                        imagePointer1[3] = 255;
                        //4 bytes per pixel
                        imagePointer1 += 4;
                    }//end for j
                    //4 bytes per pixel
                    imagePointer1 += (bitmapData1.Stride - (bitmapData1.Width * 4));
                }//end for i
            }//end unsafe
            output.UnlockBits(bitmapData1);
            return output;// col;

        }
        public void ForwardFFT()
        {
            //int i, j;
            //Fourier = new COMPLEX[Width, Height];
            Output = new COMPLEX[Width, Height];
            /*for (i = 0; i <= Width - 1; i++)
                for (j = 0; j <= Height - 1; j++)
                {
                    Fourier[i, j].real = (double)GreyImage[i, j];
                    Fourier[i, j].imag = 0;
                }
            */
            //FFT_forward
            Output = FFT2D(Fourier, nx, ny, 1);
            return;
        }
        public void FFTShift()
        {
            int i, j;
            FFTShifted = new COMPLEX[nx, ny];
            for (i = 0; i <= (nx / 2) - 1; i++)
            {
                for (j = 0; j <= (ny / 2) - 1; j++)
                {
                    FFTShifted[i + (nx / 2), j + (ny / 2)] = Fourier[i, j];
                    FFTShifted[i, j] = Fourier[i + (nx / 2), j + (ny / 2)];
                    FFTShifted[i + (nx / 2), j] = Fourier[i, j + (ny / 2)];
                    FFTShifted[i, j + (nx / 2)] = Fourier[i + (nx / 2), j];
                }
            }
            Fourier = FFTShifted;
        }
        public Bitmap FFTPlot(COMPLEX[,] Output, double cons = 1.0f)
        {
            int i, j;
            FFTLog = new double[nx, ny];
            FourierMagnitude = new double[nx, ny];
            FFTNormalized = new double[nx, ny];
            double maxMagnitude = Double.MinValue;
            double minMagnitude = Double.MaxValue;
            //remove scaling
            for (j = 0; j < Width; j++)
            {
                for (i = 0; i < Height; i++)
                {
                    Output[j, i].real *= cons;
                }
            }

            for (j = 0; j < Width; j++)
            {
                for (i = 0; i < Height; i++)
                {
                    FourierMagnitude[j, i] = Output[j, i].Magnitude();
                }
            }

            for (j = 0; j < Width; j++)
            {
                for (i = 0; i < Height; i++)
                {
                    FFTLog[j, i] = cons * Math.Log10(1.0 + FourierMagnitude[j, i]);

                    maxMagnitude = Math.Max(maxMagnitude, FFTLog[j, i]);
                    minMagnitude = Math.Min(minMagnitude, FFTLog[j, i]);
                }
            }

            double range = maxMagnitude - minMagnitude;
            for (j = 0; j < Width; j++)
            {
                for (i = 0; i < Height; i++)
                {
                    FFTNormalized[j, i] = 255.0 * ((FFTLog[j, i] - minMagnitude) / range);
                }
            }

            return Displayimage(FFTNormalized);
        }
        public void InverseFFT()
        {
            int i, j;
            Output = new COMPLEX[nx, ny];
            Output = FFT2D(Fourier, nx, ny, -1);
            Obj = null;
            for (i = 0; i <= Width - 1; i++)
                for (j = 0; j <= Height - 1; j++)
                {
                    GreyImage[i, j] = (int)Output[i, j].Magnitude();

                }
            Obj = Displayimage(GreyImage);
        }
        public COMPLEX[,] FFT2D(COMPLEX[,] c, int nx, int ny, int dir)//1 forward,-1 inverse
        {
            int i, j;
            int m;
            double[] real;
            double[] imag;
            COMPLEX[,] output;//=new COMPLEX [nx,ny];
            output = c;
            real = new double[nx];
            imag = new double[nx];

            for (j = 0; j < ny; j++)
            {
                for (i = 0; i < nx; i++)
                {
                    real[i] = c[i, j].real;
                    imag[i] = c[i, j].imag;
                }
                //FFT foreach row
                m = (int)Math.Log((double)nx, 2);
                FFT1D(dir, m, ref real, ref imag);

                for (i = 0; i < nx; i++)
                {
                    output[i, j].real = real[i];
                    output[i, j].imag = imag[i];
                }
            }
            //transform cols  
            real = new double[ny];
            imag = new double[ny];

            for (i = 0; i < nx; i++)
            {
                for (j = 0; j < ny; j++)
                {
                    real[j] = output[i, j].real;
                    imag[j] = output[i, j].imag;
                }
                //FFT foreach col
                m = (int)Math.Log((double)ny, 2);//find 2^K = ny
                FFT1D(dir, m, ref real, ref imag);
                for (j = 0; j < ny; j++)
                {
                    output[i, j].real = real[j];
                    output[i, j].imag = imag[j];
                }
            }
            return (output);
        }
        private void FFT1D(int dir, int m, ref double[] x, ref double[] y)
        {
            long nn, i, i1, j, k, i2, l, l1, l2;
            double c1, c2, tx, ty, t1, t2, u1, u2, z;
            //new size
            nn = 1;
            for (i = 0; i < m; i++)
                nn *= 2;
            //new stretched size
            i2 = nn >> 1;
            j = 0;
            for (i = 0; i < nn - 1; i++)
            {
                if (i < j)
                {
                    tx = x[i];
                    ty = y[i];
                    x[i] = x[j];
                    y[i] = y[j];
                    x[j] = tx;
                    y[j] = ty;
                }
                k = i2;
                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }
            //FFT
            c1 = -1.0;
            c2 = 0.0;
            l2 = 1;
            for (l = 0; l < m; l++)
            {
                l1 = l2;
                l2 <<= 1;
                u1 = 1.0;
                u2 = 0.0;
                for (j = 0; j < l1; j++)
                {
                    for (i = j; i < nn; i += l2)
                    {
                        i1 = i + l1;
                        t1 = u1 * x[i1] - u2 * y[i1];
                        t2 = u1 * y[i1] + u2 * x[i1];
                        x[i1] = x[i] - t1;
                        y[i1] = y[i] - t2;
                        x[i] += t1;
                        y[i] += t2;
                    }
                    z = u1 * c1 - u2 * c2;
                    u2 = u1 * c2 + u2 * c1;
                    u1 = z;
                }
                c2 = Math.Sqrt((1.0 - c1) / 2.0);
                if (dir == 1)
                    c2 = -c2;
                c1 = Math.Sqrt((1.0 + c1) / 2.0);
            }
            //scaling
            if (dir == 1)
            {
                for (i = 0; i < nn; i++)
                {
                    x[i] /= (double)nn;
                    y[i] /= (double)nn;

                }
            }
            return;
        }
        public COMPLEX[,] rev(COMPLEX[,] spectrum)
        {
            int N1 = spectrum.GetLength(0); // Number of rows
            int N2 = spectrum.GetLength(1); // Number of columns
            int rowShift = (N1 - 1) / 2;
            int colShift = (N2 - 1) / 2;

            // Reverse the shift in rows and columns
            COMPLEX[,] shiftedSpectrum = new COMPLEX[N1, N2];
            for (int i = 0; i < N1; i++)
            {
                int rowIndex = (i + rowShift) % N1;
                for (int j = 0; j < N2; j++)
                {
                    int colIndex = (j + colShift) % N2;
                    shiftedSpectrum[rowIndex, colIndex] = spectrum[i, j];
                }
            }

            return shiftedSpectrum;
        }
    }
}
