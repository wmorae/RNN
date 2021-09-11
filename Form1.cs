using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Windows;
using System.Globalization;
using System.Threading;
using rtChart;


namespace RNN
{
   

    public partial class Form1 : Form
    {
        string str;
        string synapse;
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string path = "";
        private Thread tarefa;



        public static double sig(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        public static double dsig(double x)
        {
            return sig(x) * (1 - sig(x));
        }

        public static double tanh(double x)
        {
           //  return Math.Tanh(x);
            // return (LRLU(x));
            return (sig(x));

        }

        public static double dtanh(double x)
        {
          // return (1 - (tanh(x) * tanh(x)));
           // return (dLRLU(x));
          return (dsig(x));

        }
        public static double LRLU(double x)
        {
            if (x < 0)
                return (x*0.3);
            else
                return (x);
        }
        public static double dLRLU(double x)
        {
            if (x < 0)
                return (0.3);
            else
                return (1.0);
        }
        public static double ELU(double x)
        {
            if (x < 0)
                return (Math.Exp(x) - 1);
            else
                return (x);
        }
        public static double dELU(double x)
        {            
            if (x < 0)
                return Math.Exp(x);
            else
                return (1);
        }


        public Form1()
        {
         
            InitializeComponent();

        }
        // this.Invoke((MethodInvoker)delegate { UpdateChart(Math.Round(cpuPerfCounter.NextValue(), 0)); });
        //  tarefa= new Thread(new ThreadStart(this.getPerformanceCounters));
        //tarefa.IsBackground = true;
          //  tarefa.Start();
        private void UpdateChart( string x)
        {           
            chart1.Series["Series2"].Points.AddY(x);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            chart1.Titles.Add("Cost(Ciclo)");

            path = Path.Combine(docPath, "Synapse.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                synapse = sr.ReadToEnd();
            }
            textBox1.Text = synapse.Substring(0, synapse.IndexOf("\n"));
        }

        //CreateRNN
        private void button1_Click(object sender, EventArgs e)
        {
            String Synapse;
            chart1.Series[0].Points.Clear();
            Synapse = textBox1.Text + "\n" + "\n";
            str = Synapse;
            int L = str.IndexOf("\n") - (str.Replace("-", "")).IndexOf("\n") + 1;

            int l, i, j, t, ex;
            int x1 = 0;
            int x2 ;
            int[] nl = new int[L];
            for (l = 0; l < L; l++)
            {
                x2 = str.IndexOf("-", x1);
                if (l + 1 == L)
                    x2 = str.IndexOf("\n");
                nl[l] = Int32.Parse(str.Substring(x1, (x2 - x1)));
                x1 = x2 + 1;
            }

            str = "";
            for (i = 0; i < L; i++)
                str += " " + nl[i];

            str += "\n";

            int nlmax = nl[0];
            for (l = 1; l < L; l++)
            {
                if (nl[l] > nlmax)
                    nlmax = nl[l];
            }
            // System.Windows.Forms.MessageBox.Show("Arquitetura importada com sucesso\n"+str);

            double[,,] WXu = new double[L, nlmax, nlmax];
            double[,,] WXc = new double[L, nlmax, nlmax];


            double[,] WTu = new double[L, nlmax];
            double[,] WTc = new double[L, nlmax];
            double[,] Bu = new double[L, nlmax];
            double[,] Bc = new double[L, nlmax];

            double[,,] dWXu = new double[L, nlmax, nlmax];
            double[,,] dWXc = new double[L, nlmax, nlmax];

            double[,] dWTu = new double[L, nlmax];
            double[,] dWTc = new double[L, nlmax];
            double[,] dBu = new double[L, nlmax];
            double[,] dBc = new double[L, nlmax];


            // System.Windows.Forms.MessageBox.Show("Parametros criados com sucesso");

            path = Path.Combine(docPath, "Livro.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }
            int ne = str.Length - ((str.Replace("\n", "")).Length);

            //System.Windows.Forms.MessageBox.Show("DataSet com " + ne.ToString() + " Atendimentos");

            //int T = str.IndexOf("/")-(str.Replace(" ","")).IndexOf("/")+1;
            x1 = 0;
            int[] T = new int[ne];
            int Tmax = 1;
            for (ex = 0; ex < ne; ex++)
            {
                T[ex] = str.Substring(x1, str.IndexOf("/", x1) - x1).Length - str.Substring(x1, str.IndexOf("/", x1) - x1).Replace(" ", "").Length + 1;
                if (T[ex] > Tmax)
                    Tmax = T[ex];
                x1 = str.IndexOf("\n", x1) + 1;

            }
            //System.Windows.Forms.MessageBox.Show("Passos de tempo importado com sucesso, maximo "+Tmax.ToString());


            double[,,] inputs = new double[ne, Tmax, nl[0]];
            double[,,] outputs = new double[ne, Tmax, nl[L - 1]];
            string aux ;

            for (ex = 0; ex < ne; ex++)
                for (t = 0; t < T[ex]; t++)
                {
                    for (j = 0; j < nl[0]; j++)
                        inputs[ex, t, j] = 0.0;
                    for (j = 0; j < nl[L - 1]; j++)
                        outputs[ex, t, j] = 0.0;
                }

            x1 = 0;
            for (ex = 0; ex < ne; ex++)
            {
                for (t = 1; t < T[ex]; t++)
                {
                    x2 = str.IndexOf(" ", x1);
                    j = Int32.Parse(str.Substring(x1, (x2 - x1)));
                    if (j > 0)
                    {
                        inputs[ex, t, j - 1] = 1.0;
                    }
                    x1 = x2 + 1;
                }

                x1 = str.IndexOf("/", x1) + 2;
                x2 = str.IndexOf(" ", x1);
                aux = str.Substring(x1, (x2 - x1));
                if (aux.IndexOf(",") > 0)
                {
                    int x3 = aux.IndexOf(",");
                    while (x3 > 0)
                    {
                        j = Int32.Parse(aux.Substring(0, x3));
                        outputs[ex, T[ex] - 1, j - 1] = 1.0;
                        aux = aux.Substring(x3+1);
                        x3 = aux.IndexOf(",");
                    }
                }
               
                j = Int32.Parse(aux);
                if (j > 0)
                {
                // for(t=1;t<T[ex];t++)
                outputs[ex, T[ex] - 1, j - 1] = 1.0;
                }
                


                x1 = x2 + 1;
                x1 = str.IndexOf("\n", x1) + 1;
            }
            //System.Windows.Forms.MessageBox.Show("DataSet Importado com sucesso");


            //M_dz_zero
            for (l = 0; l < L; l++)
            {
                for (i = 0; i < nlmax; i++)
                {
                    for (j = 0; j < nlmax; j++)
                    {
                        WXc[l, i, j] = 0.0;
                        WXu[l, i, j] = 0.0;
                        dWXc[l, i, j] = 0.0;
                        dWXu[l, i, j] = 0.0;
                    }
                    Bu[l, i] = 0.0;
                    Bc[l, i] = 0.0;
                    WTc[l, i] = 0.0;
                    WTu[l, i] = 0.0;
                    dWTc[l, i] = 0.0;
                    dWTu[l, i] = 0.0;
                    dBu[l, i] = 0.0;
                    dBc[l, i] = 0.0;
                }
            }
            Random rnd = new Random();
            //MFill
            for (l = 0; l < L - 1; l++)
            {
                
                for (i = 0; i < nl[l + 1]; i++)
                {
                    Bu[l + 1, i] = rnd.Next(Int32.Parse(textBox24.Text), Int32.Parse(textBox23.Text));
                    Bc[l + 1, i] = rnd.Next(Int32.Parse(textBox22.Text), Int32.Parse(textBox21.Text));
                    WTc[l + 1, i] = rnd.Next(Int32.Parse(textBox8.Text), Int32.Parse(textBox7.Text)) ;
                    WTu[l + 1, i] = rnd.Next(Int32.Parse(textBox20.Text), Int32.Parse(textBox19.Text));
                    for (j = 0; j < nl[l]; j++)
                    {
                        WXc[l, i, j] = rnd.Next(Int32.Parse(textBox6.Text), Int32.Parse(textBox5.Text)) * Math.Sqrt(2.0/(nl[l]) );
                        WXu[l, i, j] = rnd.Next(Int32.Parse(textBox3.Text), Int32.Parse(textBox4.Text)) * Math.Sqrt(2.0/(nl[l]) );
                    }

                }
            }


            double C = 0 ;
            label11.Text = "-";
            int ciclo, rr;

            int CIC = Int32.Parse(textBox9.Text);
            int RR = Int32.Parse(textBox10.Text);
            double gWXc = Double.Parse(textBox17.Text);
            double gWXu = Double.Parse(textBox18.Text);
            double gWTc = Double.Parse(textBox15.Text);
            double gWTu = Double.Parse(textBox16.Text);
            double gBu = Double.Parse(textBox14.Text);
            double gBc = Double.Parse(textBox13.Text);


            double[,,] a = new double[Tmax, L, nlmax];
            double[,,] da = new double[Tmax, L, nlmax];

            double[,,] zu = new double[Tmax, L, nlmax];
            double[,,] zc = new double[Tmax, L, nlmax];
            double[,,] Γ = new double[Tmax, L, nlmax];
            double[,,] ã = new double[Tmax, L, nlmax];

            for (ciclo = 0; ciclo < CIC; ciclo++)
            {
                C = 0;
                for (rr = 0; rr < RR; rr++)
                {
                    ex = rnd.Next(0, ne);

                    //Reset_Net
                    for (t = 0; t < T[ex]; t++)
                    {
                        for (l = 0; l < L; l++)
                        {
                            for (i = 0; i < nlmax; i++)
                            {
                                a[t, l, i] = 0.0;
                                da[t, l, i] = 0.0;
                                zu[t, l, i] = 0.0;
                                zc[t, l, i] = 0.0;
                                Γ[t, l, i] = 0.0;
                                ã[t, l, i] = 0.0;
                            }
                        }
                    }

                    //Inputs
                    for (t = 1; t < T[ex]; t++)
                        for (i = 0; i < nl[0]; i++)
                            a[t, 0, i] = inputs[ex, t, i];

                    //FeedFoward
                    for (t = 1; t < T[ex]; t++)
                    {
                        for (l = 1; l < L; l++)
                        {
                            for (i = 0; i < nl[l]; i++)
                            {
                                for (j = 0; j < nl[l - 1]; j++)
                                {
                                    zc[t, l, i] += WXc[l - 1, i, j] * a[t, l - 1, j];
                                    zu[t, l, i] += WXu[l - 1, i, j] * a[t, l - 1, j];
                                }
                                zc[t, l, i] += WTc[l, i] * a[t - 1, l, i] + Bc[l, i];
                                zu[t, l, i] += WTu[l, i] * a[t - 1, l, i] + Bu[l, i];
                                Γ[t, l, i] = sig(zu[t, l, i]);
                                ã[t, l, i] = tanh(zc[t, l, i]);
                                a[t, l, i] = Γ[t, l, i] * ã[t, l, i] + (1 - Γ[t, l, i]) * a[t - 1, l, i];
                            }
                        }
                    }


                    //Back_prop
                   // for (t = 1; t < T[ex]; t++)
                        for (i = 0; i < nl[L - 1]; i++)
                         {
                        da[T[ex] - 1, L - 1, i] = 2.0 * (a[T[ex] - 1, L - 1, i] - outputs[ex, T[ex] - 1, i]);
                        C += Math.Pow((a[T[ex] - 1, L - 1, i] - outputs[ex, T[ex] - 1, i]), 2);
                         }
                           

                    for (t = T[ex] - 1; t > 0; t--)
                    {
                        for (l = L - 1; l > 0; l--)
                        {
                            for (i = 0; i < nl[l - 1]; i++)
                            {
                                for (j = 0; j < nl[l]; j++)
                                {
                                    da[t, l - 1, i] += da[t, l, j] * (Γ[t, l, j] * dtanh(zc[t, l, j]) * WXc[l - 1, j, i] + (ã[t, l, j] - a[t - 1, l, j]) * dsig(zu[t, l, j]) * WXu[l - 1, j, i]);
                                }
                                da[t - 1, l, i] += da[t, l, i] * ((1 - Γ[t, l, i]) + Γ[t, l, i] * dtanh(zc[t, l, i]) * WTc[l, i] + (ã[t, l, i] - a[t - 1, l, i]) * dsig(zu[t, l, i]) * WTu[l, i]);
                            }

                        }
                    }

                    for (t = T[ex] - 1; t > 0; t--)
                    {
                        for (l = L - 1; l > 0; l--)
                        {
                            for (j = 0; j < nl[l]; j++)
                            {
                                dBc[l, j] += da[t, l, j] * Γ[t, l, j] * dtanh(zc[t, l, j]);
                                dBu[l, j] += da[t, l, j] * (ã[t, l, j] - a[t - 1, l, j]) * dsig(zu[t, l, j]);
                                dWTc[l, j] += da[t, l, j] * Γ[t, l, j] * dtanh(zc[t, l, j]) * a[t - 1, l, j];
                                dWTu[l, j] += da[t, l, j] * (ã[t, l, j] - a[t - 1, l, j]) * dsig(zu[t, l, j]) * a[t - 1, l, j];
                                for (i = 0; i < nl[l - 1]; i++)
                                {
                                    dWXc[l - 1, j, i] += da[t, l, j] * Γ[t, l, j] * dtanh(zc[t, l, j]) * a[t, l - 1, i];
                                    dWXu[l - 1, j, i] += da[t, l, j] * (ã[t, l, j] - a[t - 1, l, j]) * dsig(zu[t, l, j]) * a[t, l - 1, i];
                                }
                            }
                        }
                    }

                }//endEx

                //Apply_backpropagation
                for (l = 0; l < L; l++)
                {
                    for (i = 0; i < nlmax; i++)
                    {
                        for (j = 0; j < nlmax; j++)
                        {
                            WXc[l, i, j] -= dWXc[l, i, j] * gWXc;
                            WXu[l, i, j] -= dWXu[l, i, j] * gWXu;
                        }
                        Bu[l, i] -= dBu[l, i] * gBu;
                        Bc[l, i] -= dBc[l, i] * gBc;
                        WTc[l, i] -= dWTc[l, i] * gWTc;
                        WTu[l, i] -= dWTu[l, i] * gWTu;
                    }
                }


                //M_dz_zero
                for (l = 0; l < L; l++)
                {
                    for (i = 0; i < nlmax; i++)
                    {
                        for (j = 0; j < nlmax; j++)
                        {
                            dWXc[l, i, j] = 0.0;
                            dWXu[l, i, j] = 0.0;
                        }
                        dWTc[l, i] = 0.0;
                        dWTu[l, i] = 0.0;
                        dBu[l, i] = 0.0;
                        dBc[l, i] = 0.0;
                    }
                }
                //chart1.Series["Series2"].Points.AddXY(ciclo.ToString(), (C/RR).ToString().Replace(",", "."));
                //UpdateChart((C / RR).ToString().Replace(",", "."));
                this.Invoke((MethodInvoker)delegate {  UpdateChart((C / RR).ToString().Replace(",", ".")); });

            }//endCiclo   

            //chart1.Series["Series2"].Points.AddXY(ciclo.ToString(), C.ToString().Replace(",", "."));
            label11.Text = (C/RR).ToString();


            path = Path.Combine(docPath, "Synapse.txt");
            //using (StreamReader sr = new StreamReader(path))
            //{
              //  Synapse = sr.ReadToEnd();
           // }
            //x1 = Synapse.IndexOf("\n") + 1;
            //Synapse = synapse + "\n";
            for (l = 0; l < (L - 1); l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        Synapse += WXc[l, i, j].ToString() + " ";
                    }
                    Synapse += "\n";
                }
                Synapse += "\n";
            }
            for (l = 0; l < (L - 1); l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        Synapse += WXu[l, i, j].ToString() + " ";
                    }
                    Synapse += "\n";
                }
                Synapse += "\n";
            }


            for (l = 1; l < (L); l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    Synapse += Bc[l, i].ToString() + "\n";

                }
                Synapse += "\n";
            }
            for (l = 1; l < (L); l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    Synapse += Bu[l, i].ToString() + "\n";

                }
                Synapse += "\n";
            }
            for (l = 1; l < (L); l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    Synapse += WTc[l, i].ToString() + "\n";

                }
                Synapse += "\n";
            }
            for (l = 1; l < (L); l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    Synapse += WTu[l, i].ToString() + "\n";

                }
                Synapse += "\n";
            }


            using (StreamWriter outputFile = new StreamWriter(path))
            {
                outputFile.Write(Synapse);
            }
        }

        //TrainRNN
        private void train()
        {
            //GC.Collect();
            System.Globalization.CultureInfo cultureinfo = new CultureInfo("pt-BR", false);
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR", false);
            string Synapse;
            path = Path.Combine(docPath, "Synapse.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                Synapse = sr.ReadToEnd();
            }
            //Thread.Sleep(1000);
            // chart1.Series["Series2"].Points.AddXY(kkk.ToString(), (kkk * kkk - 2 * kkk - 10).ToString());

            str = Synapse;
            int L = Synapse.IndexOf("\n") - (Synapse.Replace("-", "")).IndexOf("\n") + 1;

            int l, i, j, t, ex;
            int x1 = 0;
            int x2 ;
            int[] nl = new int[L];
            for (l = 0; l < L; l++)
            {
                x2 = Synapse.IndexOf("-", x1);
                if (l + 1 == L)
                    x2 = Synapse.IndexOf("\n");
                nl[l] = Int32.Parse(Synapse.Substring(x1, (x2 - x1)));
                x1 = x2 + 1;
            }
            

            int nlmax = nl[0];
            for (l = 1; l < L; l++)
            {
                if (nl[l] > nlmax)
                    nlmax = nl[l];
            }
            //System.Windows.Forms.MessageBox.Show("Arquitetura importada com sucesso\n"+stro);

            double[,,] WXu = new double[L, nlmax, nlmax];
            double[,,] WXc = new double[L, nlmax, nlmax];


            double[,] WTu = new double[L, nlmax];
            double[,] WTc = new double[L, nlmax];
            double[,] Bu = new double[L, nlmax];
            double[,] Bc = new double[L, nlmax];

            double[,,] dWXu = new double[L, nlmax, nlmax];
            double[,,] dWXc = new double[L, nlmax, nlmax];

            double[,] dWTu = new double[L, nlmax];
            double[,] dWTc = new double[L, nlmax];
            double[,] dBu = new double[L, nlmax];
            double[,] dBc = new double[L, nlmax];

            string aux ;

            //M_dz_zero
            for (l = 0; l < L; l++)
            {
                for (i = 0; i < nlmax; i++)
                {
                    for (j = 0; j < nlmax; j++)
                    {
                        WXc[l, i, j] = 0.0;
                        WXu[l, i, j] = 0.0;
                        dWXc[l, i, j] = 0.0;
                        dWXu[l, i, j] = 0.0;
                    }
                    Bu[l, i] = 0.0;
                    Bc[l, i] = 0.0;
                    WTc[l, i] = 0.0;
                    WTu[l, i] = 0.0;
                    dWTc[l, i] = 0.0;
                    dWTu[l, i] = 0.0;
                    dBu[l, i] = 0.0;
                    dBc[l, i] = 0.0;
                }
            }
            x1 = Synapse.IndexOf("\n") + 2;

            //MFill
            for (l = 0; l < L - 1; l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        x2 = Synapse.IndexOf(" ", x1);
                        WXc[l, i, j] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                        x1 = x2 + 1;
                    }
                    x1 = Synapse.IndexOf("\n", x1) + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WXc Imprtado com sucesso.");
            for (l = 0; l < L - 1; l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        x2 = Synapse.IndexOf(" ", x1);
                        WXu[l, i, j] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                        x1 = x2 + 1;
                    }
                    x1 = Synapse.IndexOf("\n", x1) + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WXu Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = Synapse.IndexOf("\n", x1);
                    Bc[l, i] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("Bc Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = Synapse.IndexOf("\n", x1);
                    Bu[l, i] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            //System.Windows.Forms.MessageBox.Show("Bu Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = Synapse.IndexOf("\n", x1);
                    WTc[l, i] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WTc Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = Synapse.IndexOf("\n", x1);
                    WTu[l, i] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WTu Imprtado com sucesso.");


            //System.Windows.Forms.MessageBox.Show("Parametros Importados com sucesso");

            path = Path.Combine(docPath, "Livro.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }
            int ne = str.Length - ((str.Replace("\n", "")).Length);

            //System.Windows.Forms.MessageBox.Show("DataSet com "+ne.ToString()+" Atendimentos");

            x1 = 0;
            int[] T = new int[ne];
            int Tmax = 1;
            for (ex = 0; ex < ne; ex++)
            {
                T[ex] = str.Substring(x1, str.IndexOf("/", x1) - x1).Length - str.Substring(x1, str.IndexOf("/", x1) - x1).Replace(" ", "").Length + 1;
                if (T[ex] > Tmax)
                    Tmax = T[ex];
                x1 = str.IndexOf("\n", x1) + 1;

            }
            //System.Windows.Forms.MessageBox.Show("DataSet com "+T.ToString()+" passos de tempo");

            double[,,] inputs = new double[ne, Tmax, nl[0]];
            double[,,] outputs = new double[ne, Tmax, nl[L - 1]];

            for (ex = 0; ex < ne; ex++)
                for (t = 0; t < T[ex]; t++)
                {
                    for (j = 0; j < nl[0]; j++)
                        inputs[ex, t, j] = 0.0;
                    for (j = 0; j < nl[L - 1]; j++)
                        outputs[ex, t, j] = 0.0;
                }

            x1 = 0;
            for (ex = 0; ex < ne; ex++)
            {
                for (t = 1; t < T[ex]; t++)
                {
                    x2 = str.IndexOf(" ", x1);
                    j = Int32.Parse(str.Substring(x1, (x2 - x1)));
                    if (j > 0)
                    {
                        inputs[ex, t, j - 1] = 1.0;
                    }
                    x1 = x2 + 1;
                }

                x1 = str.IndexOf("/", x1) + 2;
                x2 = str.IndexOf(" ", x1);
                aux = str.Substring(x1, (x2 - x1));
                if (aux.IndexOf(",") > 0)
                {
                    
                    int x3 = aux.IndexOf(",");
                    while (x3 > 0)
                    {
                        j = Int32.Parse(aux.Substring(0, x3));
                        outputs[ex, T[ex] - 1, j - 1] = 1.0;
                        aux = aux.Substring(x3 + 1);
                        x3 = aux.IndexOf(",");

                    }


                }
             
                j = Int32.Parse(aux);
                if (j > 0)
                {
                // for(t=1;t<T[ex];t++)
                outputs[ex, T[ex] - 1, j - 1] = 1.0;
                }
                


                x1 = x2 + 1;
                x1 = str.IndexOf("\n", x1) + 1;
            }
            //System.Windows.Forms.MessageBox.Show("DataSet Importado com sucesso");
            Random rnd = new Random();
            
            double C=0;
            int ciclo = 0, rr;
            

            int CIC=Int32.Parse(textBox9.Text);
            int RR= Int32.Parse(textBox10.Text);
            DateTime when_started = DateTime.Now;
            DateTime when_stop = DateTime.Now.AddMinutes(CIC);
            if (checkBox1.Checked)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label11.Text = "-";
                    progressBar1.Maximum = (int)(when_stop - when_started).TotalSeconds;
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate { label11.Text = "-"; 
                progressBar1.Maximum = CIC;
                });
            }
            double gWXc = Double.Parse(textBox17.Text);
            double gWXu = Double.Parse(textBox18.Text);
            double gWTc = Double.Parse(textBox15.Text);
            double gWTu = Double.Parse(textBox16.Text);
            double gBu  = Double.Parse(textBox14.Text);
            double gBc  = Double.Parse(textBox13.Text);


            double[,,] a = new double[Tmax, L, nlmax];
            double[,,] da = new double[Tmax, L, nlmax];

            double[,,] zu = new double[Tmax, L, nlmax];
            double[,,] zc = new double[Tmax, L, nlmax];
            double[,,] Γ = new double[Tmax, L, nlmax];
            double[,,] ã = new double[Tmax, L, nlmax];
            bool condition = true;
            while (condition)
            {
                
                C = 0;
                for (rr = 0; rr < RR; rr++)
                {
                    ex = rnd.Next(0, ne);

                    //Reset_Net
                    for (t = 0; t < T[ex]; t++)
                    {
                        for (l = 0; l < L; l++)
                        {
                            for (i = 0; i < nlmax; i++)
                            {
                                a[t, l, i] = 0.0;
                                da[t, l, i] = 0.0;
                                zu[t, l, i] = 0.0;
                                zc[t, l, i] = 0.0;
                                Γ[t, l, i] = 0.0;
                                ã[t, l, i] = 0.0;
                            }
                        }
                    }

                    //Inputs
                    for (t = 1; t < T[ex]; t++)
                        for (i = 0; i < nl[0]; i++)
                            a[t, 0, i] = inputs[ex, t, i];

                    //FeedFoward
                    for (t = 1; t < T[ex]; t++)
                    {
                        for (l = 1; l < L; l++)
                        {
                            for (i = 0; i < nl[l]; i++)
                            {
                                for (j = 0; j < nl[l - 1]; j++)
                                {
                                    zc[t, l, i] += WXc[l - 1, i, j] * a[t, l - 1, j];
                                    zu[t, l, i] += WXu[l - 1, i, j] * a[t, l - 1, j];
                                }
                                zc[t, l, i] += WTc[l, i] * a[t - 1, l, i] + Bc[l, i];
                                zu[t, l, i] += WTu[l, i] * a[t - 1, l, i] + Bu[l, i];
                                Γ[t, l, i] = sig(zu[t, l, i]);
                                ã[t, l, i] = tanh(zc[t, l, i]);
                                a[t, l, i] = Γ[t, l, i] * ã[t, l, i] + (1 - Γ[t, l, i]) * a[t - 1, l, i];
                            }
                        }
                    }



                    //Back_prop
                    // for (t = 1; t < T[ex]; t++)
                    for (i = 0; i < nl[L - 1]; i++)
                    {
                        da[T[ex] - 1, L - 1, i] = 2.0 * (a[T[ex] - 1, L - 1, i] - outputs[ex, T[ex] - 1, i]);
                        C += Math.Pow((a[T[ex] - 1, L - 1, i] - outputs[ex, T[ex] - 1, i]), 2);
                    }
                       

                    for (t = T[ex] - 1; t > 0; t--)
                    {
                        for (l = L - 1; l > 0; l--)
                        {
                            for (i = 0; i < nl[l - 1]; i++)
                            {
                                for (j = 0; j < nl[l]; j++)
                                {
                                    da[t, l - 1, i] += da[t, l, j] * (Γ[t, l, j] * dtanh(zc[t, l, j]) * WXc[l - 1, j, i] + (ã[t, l, j] - a[t - 1, l, j]) * dsig(zu[t, l, j]) * WXu[l - 1, j, i]);
                                    //System.Windows.Forms.MessageBox.Show(da[t,l-1,i].ToString(),"da["+t.ToString()+(l-1).ToString()+i.ToString()+"]");
                                }
                                da[t - 1, l, i] += da[t, l, i] * ((1 - Γ[t, l, i]) + Γ[t, l, i] * dtanh(zc[t, l, i]) * WTc[l, i] + (ã[t, l, i] - a[t - 1, l, i]) * dsig(zu[t, l, i]) * WTu[l, i]);
                            }

                        }
                    }

                    for (t = T[ex] - 1; t > 0; t--)
                    {
                        for (l = L - 1; l > 0; l--)
                        {
                            for (j = 0; j < nl[l]; j++)
                            {
                                dBc[l, j] += da[t, l, j] * Γ[t, l, j] * dtanh(zc[t, l, j]);
                                dBu[l, j] += da[t, l, j] * (ã[t, l, j] - a[t - 1, l, j]) * dsig(zu[t, l, j]);
                                dWTc[l, j] += da[t, l, j] * Γ[t, l, j] * dtanh(zc[t, l, j]) * a[t - 1, l, j];
                                dWTu[l, j] += da[t, l, j] * (ã[t, l, j] - a[t - 1, l, j]) * dsig(zu[t, l, j]) * a[t - 1, l, j];
                                for (i = 0; i < nl[l - 1]; i++)
                                {
                                    dWXc[l - 1, j, i] += da[t, l, j] * Γ[t, l, j] * dtanh(zc[t, l, j]) * a[t, l - 1, i];
                                    dWXu[l - 1, j, i] += da[t, l, j] * (ã[t, l, j] - a[t - 1, l, j]) * dsig(zu[t, l, j]) * a[t, l - 1, i];
                                }
                            }
                        }
                    }


                }//endEx

                //Apply_backpropagation
                for (l = 0; l < L; l++)
                {
                    for (i = 0; i < nlmax; i++)
                    {
                        for (j = 0; j < nlmax; j++)
                        {
                            WXc[l, i, j] -= dWXc[l, i, j] * gWXc;
                            WXu[l, i, j] -= dWXu[l, i, j] * gWXu;
                        }
                        Bu[l, i] -= dBu[l, i] * gBu;
                        Bc[l, i] -= dBc[l, i] * gBc;
                        WTc[l, i] -= dWTc[l, i] * gWTc;
                        WTu[l, i] -= dWTu[l, i] * gWTu;
                    }
                }


                //M_dz_zero
                for (l = 0; l < L; l++)
                {
                    for (i = 0; i < nlmax; i++)
                    {
                        for (j = 0; j < nlmax; j++)
                        {
                            dWXc[l, i, j] = 0.0;
                            dWXu[l, i, j] = 0.0;
                        }
                        dWTc[l, i] = 0.0;
                        dWTu[l, i] = 0.0;
                        dBu[l, i] = 0.0;
                        dBc[l, i] = 0.0;
                    }

                }
                // chart1.Series["Series2"].Points.AddXY(ciclo.ToString(), (C / RR).ToString().Replace(",", "."));
                // UpdateChart((C / RR).ToString().Replace(",", "."));
                ciclo++;
                
                
                //this.Invoke((MethodInvoker)delegate { progressBar1.Value = (ciclo / CIC * 100); });
               
                if (checkBox1.Checked)
                {
                    if (DateTime.Now > when_stop)
                        condition = false;
                    this.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = (int)(DateTime.Now - when_started).TotalSeconds;
                        UpdateChart((C / RR).ToString().Replace(",", "."));
                    });
                }
                else
                {
                    if(!(ciclo<CIC))
                        condition = false;

                    this.Invoke((MethodInvoker)delegate {
                        progressBar1.Value = ciclo;
                        UpdateChart((C / RR).ToString().Replace(",", "."));
                    });
                }
                

            }//endCiclo              


            this.Invoke((MethodInvoker)delegate { label11.Text = (C / RR).ToString(); });

            x1 = Synapse.IndexOf("\n") + 1;
            Synapse = Synapse.Substring(0, x1) + "\n";
            for (l = 0; l < (L - 1); l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        Synapse += WXc[l, i, j].ToString() + " ";
                    }
                    Synapse += "\n";
                }
                Synapse += "\n";
            }
            for (l = 0; l < (L - 1); l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        Synapse += WXu[l, i, j].ToString() + " ";
                    }
                    Synapse += "\n";
                }
                Synapse += "\n";
            }


            for (l = 1; l < (L); l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    Synapse += Bc[l, i].ToString() + "\n";

                }
                Synapse += "\n";
            }
            for (l = 1; l < (L); l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    Synapse += Bu[l, i].ToString() + "\n";

                }
                Synapse += "\n";
            }
            for (l = 1; l < (L); l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    Synapse += WTc[l, i].ToString() + "\n";

                }
                Synapse += "\n";
            }
            for (l = 1; l < (L); l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    Synapse += WTu[l, i].ToString() + "\n";

                }
                Synapse += "\n";
            }

            path = Path.Combine(docPath, "Synapse.txt");
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                outputFile.Write(Synapse);
            }
        }
        private void train_rnn_Click(object sender, EventArgs e)
        {

           tarefa = new Thread(new ThreadStart(this.train));
            tarefa.IsBackground = true;
            tarefa.Start();
        }
            //PlayRNN
            private void button3_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(docPath, "Synapse.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }


            int L = str.IndexOf("\n") - (str.Replace("-", "")).IndexOf("\n") + 1;
            int l, i, j, t;
            int x1 = 0;
            int x2;
            int[] nl = new int[L];
            for (l = 0; l < L; l++)
            {
                x2 = str.IndexOf("-", x1);
                if (l + 1 == L)
                    x2 = str.IndexOf("\n");
                nl[l] = Int32.Parse(str.Substring(x1, (x2 - x1)));
                x1 = x2 + 1;
            }

            int nlmax = nl[0];
            for (l = 1; l < L; l++)
            {
                if (nl[l] > nlmax)
                    nlmax = nl[l];
            }
            // System.Windows.Forms.MessageBox.Show("Arquitetura importada com sucesso");

            double[,,] WXu = new double[L, nlmax, nlmax];
            double[,,] WXc = new double[L, nlmax, nlmax];

            double[,] WTu = new double[L, nlmax];
            double[,] WTc = new double[L, nlmax];
            double[,] Bu = new double[L, nlmax];
            double[,] Bc = new double[L, nlmax];

            //M_dz_zero
            for (l = 0; l < L; l++)
            {
                for (i = 0; i < nlmax; i++)
                {
                    for (j = 0; j < nlmax; j++)
                    {
                        WXc[l, i, j] = 0.0;
                        WXu[l, i, j] = 0.0;
                    }
                    Bu[l, i] = 0.0;
                    Bc[l, i] = 0.0;
                    WTc[l, i] = 0.0;
                    WTu[l, i] = 0.0;
                }
            }
            // System.Windows.Forms.MessageBox.Show("Parametros criados com sucesso");

            x1 = str.IndexOf("\n") + 2;

            //MFill
            for (l = 0; l < L - 1; l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        x2 = str.IndexOf(" ", x1);
                        WXc[l, i, j] = Double.Parse(str.Substring(x1, (x2 - x1)));
                        x1 = x2 + 1;
                    }
                    x1 = str.IndexOf("\n", x1) + 1;
                }
                x1 = str.IndexOf("\n", x1) + 1;
            }
            //System.Windows.Forms.MessageBox.Show("WXc Imprtado com sucesso.");
            for (l = 0; l < L - 1; l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        x2 = str.IndexOf(" ", x1);
                        WXu[l, i, j] = Double.Parse(str.Substring(x1, (x2 - x1)));
                        x1 = x2 + 1;
                    }
                    x1 = str.IndexOf("\n", x1) + 1;
                }
                x1 = str.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WXu Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = str.IndexOf("\n", x1);
                    Bc[l, i] = Double.Parse(str.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = str.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("Bc Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = str.IndexOf("\n", x1);
                    Bu[l, i] = Double.Parse(str.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = str.IndexOf("\n", x1) + 1;
            }
            //System.Windows.Forms.MessageBox.Show("Bu Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = str.IndexOf("\n", x1);
                    WTc[l, i] = Double.Parse(str.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = str.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WTc Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = str.IndexOf("\n", x1);
                    WTu[l, i] = Double.Parse(str.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = str.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WTu Imprtado com sucesso.");



            path = Path.Combine(docPath, "Tratativas.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }
            string[] tratativas = new string[nl[L - 1]];
            x1 = 0;
            for (i = 0; i < nl[L - 1]; i++)
            {
                x2 = str.IndexOf("\n", x1);
                tratativas[i] = str.Substring(x1, x2 - x1 - 1).ToLower();
                x1 = x2 + 1;
            }

            path = Path.Combine(docPath, "Words.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }
            string[] words = new string[nl[0]];
            x1 = 0;
            for (i = 0; i < nl[0]; i++)
            {
                x2 = str.IndexOf("\n", x1);
                words[i] = " " + str.Substring(x1, x2 - x1 - 1).ToLower();
                x1 = x2 + 1;
            }




            string clip =" "+ textBox2.Text + " ";
            string output = "";
            string aux, aux2;
            clip = clip.Replace("  ", " ").Replace("\n", "").ToLower();
            clip += "\n";


            x1 = 0;
            x2 = clip.IndexOf("\n", x1);
            aux = clip.Substring(x1, x2 - x1 + 1);
            int x3 = 0;
            int x4 = 0;
            string output2 = "";
            while (aux.IndexOf("\n", x4 + 2) > 0)
            {
                x4 = aux.IndexOf(" ", x3 + 1);
                aux2 = aux.Substring(x3, x4 - x3+1);
                for (i = 0; i < nl[0]; i++)
                {
                    if (aux2.IndexOf(words[i]) > -1)
                    {
                        output += (i + 1).ToString() + " ";
                        output2 += words[i] + " ";
                        break;
                    }
                }
                x3 = x4;
            }
            if (output == "")
                clip = "0";
            else
                clip = output.Substring(0, output.Length - 1);
            output ="Words Nº's : " + output2+"\n\t"+clip+"\n";

            int T = clip.Length - (clip.Replace(" ", "")).Length + 1;
            //System.Windows.Forms.MessageBox.Show("Exemplo com "+T.ToString()+" passos de tempo");

            double[,,] a = new double[T, L, nlmax];
            double[,,] da = new double[T, L, nlmax];

            double[,,] zu = new double[T, L, nlmax];
            double[,,] zc = new double[T, L, nlmax];
            double[,,] Γ = new double[T, L, nlmax];
            double[,,] ã = new double[T, L, nlmax];

            //Reset_Net
            for (t = 0; t < T; t++)
            {
                for (l = 0; l < L; l++)
                {
                    for (i = 0; i < nlmax; i++)
                    {
                        a[t, l, i] = 0.0;
                        da[t, l, i] = 0.0;
                        zu[t, l, i] = 0.0;
                        zc[t, l, i] = 0.0;
                        Γ[t, l, i] = 0.0;
                        ã[t, l, i] = 0.0;
                    }
                }
            }

            //Inputs
            x1 = 0;
            for (t = 1; t < T; t++)
            {
                x2 = clip.IndexOf(" ", x1);
                j = Int32.Parse(clip.Substring(x1, (x2 - x1)));
                if (j > 0 && j <= nl[0])
                {
                    a[t, 0, j - 1] = 1.0;
                }
                x1 = x2 + 1;
            }

            //FeedFoward
            for (t = 1; t < T; t++)
            {
                for (l = 1; l < L; l++)
                {
                    for (i = 0; i < nl[l]; i++)
                    {
                        for (j = 0; j < nl[l - 1]; j++)
                        {
                            zc[t, l, i] += WXc[l - 1, i, j] * a[t, l - 1, j];
                            zu[t, l, i] += WXu[l - 1, i, j] * a[t, l - 1, j];
                        }
                        zc[t, l, i] += WTc[l, i] * a[t - 1, l, i] + Bc[l, i];
                        zu[t, l, i] += WTu[l, i] * a[t - 1, l, i] + Bu[l, i];
                        Γ[t, l, i] = sig(zu[t, l, i]);
                        ã[t, l, i] = tanh(zc[t, l, i]);
                        a[t, l, i] = Γ[t, l, i] * ã[t, l, i] + (1 - Γ[t, l, i]) * a[t - 1, l, i];
                    }
                }
            }
            str = "";
            //for(t=1;t<T;t++)
            //str+=t.ToString()+" ";
            //str+="\n"; 
            for (i = 0; i < nl[L - 1]; i++)
            {
                t = T - 1;
                if(a[t, L - 1, i] >= 0.2)
                {
                    str += tratativas[i] + "\t \t \t : \t \t";
                    str += (Math.Round(a[t, L - 1, i], 1, MidpointRounding.AwayFromZero)).ToString() + " ";
                    str += "\n";
                }
                
               // for (t = T-1; t < T; t++)
            }
            output += str;
            Output3.Text = output;

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
                Output3.Text = "-";
            else
                button3_Click(sender, e);
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            textBox24.Text = "-" + textBox11.Text;
            textBox23.Text = textBox11.Text;
            textBox22.Text = "-" + textBox11.Text;
            textBox21.Text = textBox11.Text;
            textBox8.Text  = "-" + textBox11.Text;
            textBox7.Text  = textBox11.Text;
            textBox20.Text = "-" + textBox11.Text;
            textBox19.Text = textBox11.Text;
            textBox6.Text  = "-" + textBox11.Text;
            textBox5.Text  = textBox11.Text;
            textBox3.Text  = "-" + textBox11.Text;
            textBox4.Text  =  textBox11.Text;
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            textBox17.Text = textBox12.Text;
            textBox18.Text = textBox12.Text;
            textBox15.Text = textBox12.Text;
            textBox16.Text = textBox12.Text;
            textBox14.Text = textBox12.Text;
            textBox13.Text = textBox12.Text;
        }

        private void textBox25_TextChanged(object sender, EventArgs e)
        {
            textBox24.Text = "-" + textBox25.Text;
            textBox23.Text = textBox25.Text;
            textBox22.Text = "-" + textBox25.Text;
            textBox21.Text = textBox25.Text;
        }

        private void OpenBook_Click(object sender, EventArgs e)
        {
            path = Path.Combine(docPath, "Livro.txt");
            System.Diagnostics.Process.Start(path);
        }

        private void OpenSynapse_Click(object sender, EventArgs e)
        {
            path = Path.Combine(docPath, "Synapse.txt");
            System.Diagnostics.Process.Start(path);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

            string Synapse;
            path = Path.Combine(docPath, "Synapse.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                Synapse = sr.ReadToEnd();
            }
            //Thread.Sleep(1000);
            // chart1.Series["Series2"].Points.AddXY(kkk.ToString(), (kkk * kkk - 2 * kkk - 10).ToString());

            str = Synapse;
            int L = Synapse.IndexOf("\n") - (Synapse.Replace("-", "")).IndexOf("\n") + 1;

            int l, i, j, t, ex;
            int x1 = 0;
            int x2;
            int[] nl = new int[L];
            for (l = 0; l < L; l++)
            {
                x2 = Synapse.IndexOf("-", x1);
                if (l + 1 == L)
                    x2 = Synapse.IndexOf("\n");
                nl[l] = Int32.Parse(Synapse.Substring(x1, (x2 - x1)));
                x1 = x2 + 1;
            }


            int nlmax = nl[0];
            for (l = 1; l < L; l++)
            {
                if (nl[l] > nlmax)
                    nlmax = nl[l];
            }
            //System.Windows.Forms.MessageBox.Show("Arquitetura importada com sucesso\n"+stro);

            double[,,] WXu = new double[L, nlmax, nlmax];
            double[,,] WXc = new double[L, nlmax, nlmax];


            double[,] WTu = new double[L, nlmax];
            double[,] WTc = new double[L, nlmax];
            double[,] Bu = new double[L, nlmax];
            double[,] Bc = new double[L, nlmax];

            double[,,] dWXu = new double[L, nlmax, nlmax];
            double[,,] dWXc = new double[L, nlmax, nlmax];

            double[,] dWTu = new double[L, nlmax];
            double[,] dWTc = new double[L, nlmax];
            double[,] dBu = new double[L, nlmax];
            double[,] dBc = new double[L, nlmax];

            string aux;

            //M_dz_zero
            for (l = 0; l < L; l++)
            {
                for (i = 0; i < nlmax; i++)
                {
                    for (j = 0; j < nlmax; j++)
                    {
                        WXc[l, i, j] = 0.0;
                        WXu[l, i, j] = 0.0;
                        dWXc[l, i, j] = 0.0;
                        dWXu[l, i, j] = 0.0;
                    }
                    Bu[l, i] = 0.0;
                    Bc[l, i] = 0.0;
                    WTc[l, i] = 0.0;
                    WTu[l, i] = 0.0;
                    dWTc[l, i] = 0.0;
                    dWTu[l, i] = 0.0;
                    dBu[l, i] = 0.0;
                    dBc[l, i] = 0.0;
                }
            }
            x1 = Synapse.IndexOf("\n") + 2;

            //MFill
            for (l = 0; l < L - 1; l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        x2 = Synapse.IndexOf(" ", x1);
                        WXc[l, i, j] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                        x1 = x2 + 1;
                    }
                    x1 = Synapse.IndexOf("\n", x1) + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WXc Imprtado com sucesso.");
            for (l = 0; l < L - 1; l++)
            {
                for (i = 0; i < nl[l + 1]; i++)
                {
                    for (j = 0; j < nl[l]; j++)
                    {
                        x2 = Synapse.IndexOf(" ", x1);
                        WXu[l, i, j] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                        x1 = x2 + 1;
                    }
                    x1 = Synapse.IndexOf("\n", x1) + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WXu Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = Synapse.IndexOf("\n", x1);
                    Bc[l, i] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("Bc Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = Synapse.IndexOf("\n", x1);
                    Bu[l, i] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            //System.Windows.Forms.MessageBox.Show("Bu Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = Synapse.IndexOf("\n", x1);
                    WTc[l, i] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WTc Imprtado com sucesso.");
            for (l = 1; l < L; l++)
            {
                for (i = 0; i < nl[l]; i++)
                {
                    x2 = Synapse.IndexOf("\n", x1);
                    WTu[l, i] = Double.Parse(Synapse.Substring(x1, (x2 - x1)));
                    x1 = x2 + 1;
                }
                x1 = Synapse.IndexOf("\n", x1) + 1;
            }
            // System.Windows.Forms.MessageBox.Show("WTu Imprtado com sucesso.");


            //System.Windows.Forms.MessageBox.Show("Parametros Importados com sucesso");

            path = Path.Combine(docPath, "Livro.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }
            int ne = str.Length - ((str.Replace("\n", "")).Length);

            //System.Windows.Forms.MessageBox.Show("DataSet com "+ne.ToString()+" Atendimentos");

            x1 = 0;
            int[] T = new int[ne];
            int Tmax = 1;
            for (ex = 0; ex < ne; ex++)
            {
                T[ex] = str.Substring(x1, str.IndexOf("/", x1) - x1).Length - str.Substring(x1, str.IndexOf("/", x1) - x1).Replace(" ", "").Length + 1;
                if (T[ex] > Tmax)
                    Tmax = T[ex];
                x1 = str.IndexOf("\n", x1) + 1;

            }
            //System.Windows.Forms.MessageBox.Show("DataSet com "+T.ToString()+" passos de tempo");

            double[,,] inputs = new double[ne, Tmax, nl[0]];
            double[,,] outputs = new double[ne, Tmax, nl[L - 1]];

            for (ex = 0; ex < ne; ex++)
                for (t = 0; t < T[ex]; t++)
                {
                    for (j = 0; j < nl[0]; j++)
                        inputs[ex, t, j] = 0.0;
                    for (j = 0; j < nl[L - 1]; j++)
                        outputs[ex, t, j] = 0.0;
                }

            x1 = 0;
            for (ex = 0; ex < ne; ex++)
            {
                for (t = 1; t < T[ex]; t++)
                {
                    x2 = str.IndexOf(" ", x1);
                    j = Int32.Parse(str.Substring(x1, (x2 - x1)));
                    if (j > 0)
                    {
                        inputs[ex, t, j - 1] = 1.0;
                    }
                    x1 = x2 + 1;
                }

                x1 = str.IndexOf("/", x1) + 2;
                x2 = str.IndexOf(" ", x1);
                aux = str.Substring(x1, (x2 - x1));
                if (aux.IndexOf(",") > 0)
                {

                    int x3 = aux.IndexOf(",");
                    while (x3 > 0)
                    {
                        j = Int32.Parse(aux.Substring(0, x3));
                        outputs[ex, T[ex] - 1, j - 1] = 1.0;
                        aux = aux.Substring(x3 + 1);
                        x3 = aux.IndexOf(",");

                    }


                }

                j = Int32.Parse(aux);
                if (j > 0)
                {
                    // for(t=1;t<T[ex];t++)
                    outputs[ex, T[ex] - 1, j - 1] = 1.0;
                }



                x1 = x2 + 1;
                x1 = str.IndexOf("\n", x1) + 1;
            }
            //System.Windows.Forms.MessageBox.Show("DataSet Importado com sucesso");

            double C0;
            double C = 0;
            string erro = "";

            double[,,] a = new double[Tmax, L, nlmax];
            double[,,] da = new double[Tmax, L, nlmax];

            double[,,] zu = new double[Tmax, L, nlmax];
            double[,,] zc = new double[Tmax, L, nlmax];
            double[,,] Γ = new double[Tmax, L, nlmax];
            double[,,] ã = new double[Tmax, L, nlmax];

            for (ex = 0; ex < ne; ex++)
            {
                C0= 0;

                    //Reset_Net
                    for (t = 0; t < T[ex]; t++)
                    {
                        for (l = 0; l < L; l++)
                        {
                            for (i = 0; i < nlmax; i++)
                            {
                                a[t, l, i] = 0.0;
                                da[t, l, i] = 0.0;
                                zu[t, l, i] = 0.0;
                                zc[t, l, i] = 0.0;
                                Γ[t, l, i] = 0.0;
                                ã[t, l, i] = 0.0;
                            }
                        }
                    }

                    //Inputs
                    for (t = 1; t < T[ex]; t++)
                        for (i = 0; i < nl[0]; i++)
                            a[t, 0, i] = inputs[ex, t, i];

                    //FeedFoward
                    for (t = 1; t < T[ex]; t++)
                    {
                        for (l = 1; l < L; l++)
                        {
                            for (i = 0; i < nl[l]; i++)
                            {
                                for (j = 0; j < nl[l - 1]; j++)
                                {
                                    zc[t, l, i] += WXc[l - 1, i, j] * a[t, l - 1, j];
                                    zu[t, l, i] += WXu[l - 1, i, j] * a[t, l - 1, j];
                                }
                                zc[t, l, i] += WTc[l, i] * a[t - 1, l, i] + Bc[l, i];
                                zu[t, l, i] += WTu[l, i] * a[t - 1, l, i] + Bu[l, i];
                                Γ[t, l, i] = sig(zu[t, l, i]);
                                ã[t, l, i] = tanh(zc[t, l, i]);
                                a[t, l, i] = Γ[t, l, i] * ã[t, l, i] + (1 - Γ[t, l, i]) * a[t - 1, l, i];
                            }
                        }
                    }



                //ComputeC
                for (i = 0; i < nl[L - 1]; i++)
                    {
                        C0 += Math.Pow((a[T[ex] - 1, L - 1, i] - outputs[ex, T[ex] - 1, i]), 2);
                    }
                C += C0;
                erro += ex.ToString()+"\t"+C0.ToString()+"\n";



            }//endCiclo     
            path = Path.Combine(docPath, "Erro.txt");
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                outputFile.Write(erro);
            }
            label24.Text = C.ToString();
            System.Diagnostics.Process.Start(path);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            path = Path.Combine(docPath, "DB.txt");
            System.Diagnostics.Process.Start(path);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            path = Path.Combine(docPath, "Words.txt");
            System.Diagnostics.Process.Start(path);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            path = Path.Combine(docPath, "Tratativas.txt");
            System.Diagnostics.Process.Start(path);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int x1, x2;
            int i;
            string aux = "";
            string aux2 = "";
            string str = "";
            string chat = "";
            string trat = "";
            string livro = "";

            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string path = Path.Combine(docPath, "Words.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }

            int li = (str.Length - ((str.Replace("\n", "")).Length));

            System.Windows.Forms.MessageBox.Show("Dicionario com " + li.ToString() + " palavras");

            int[] inputs = new int[li];

            for (i = 0; i < li; i++)
                inputs[i] = 0;

            string[] words = new string[li];

            x1 = 0;
            for (i = 0; i < li; i++)
            {
                x2 = str.IndexOf("\n", x1);
                words[i] = " " + str.Substring(x1, x2 - x1 - 1).ToLower();
                x1 = x2 + 1;
            }


            path = Path.Combine(docPath, "Tratativas.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }

            int lo = (str.Length - ((str.Replace("\n", "")).Length));
            System.Windows.Forms.MessageBox.Show("Tratativas com " + lo.ToString() + " tratativas");

            string[] tratativas = new string[lo];

            string output;


            x1 = 0;
            for (i = 0; i < lo; i++)
            {
                x2 = str.IndexOf("\n", x1);
                tratativas[i] = str.Substring(x1, x2 - x1 - 1).ToLower();
                x1 = x2 + 1;
            }

            path = Path.Combine(docPath, "DB.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                str = sr.ReadToEnd();
            }
            x1 = 0;

            while (str.IndexOf("\t", x1) > 0)
            {
                output = "";
                x2 = str.IndexOf("\t", x1) + 1;
                x2 = str.IndexOf("\t", x2);
                aux = str.Substring(x1, x2 - x1).ToLower();

                for (i = 0; i < lo; i++)
                {
                    if (aux.IndexOf(tratativas[i]) > -1)
                        output += (i + 1).ToString() + ",";
                }
                if (output == "")
                    trat = "0";
                else
                    trat = output.Substring(0, output.Length - 1);


                //System.Windows.Forms.MessageBox.Show("tratat= "+trat);
                output = "";
                x1 = x2 + 1;
                x2 = str.IndexOf("\n", x1);
                aux = aux.Replace(".", " ").Replace(",", " ").Replace("  ", " ").Replace("  ", " ");
                aux = " " + str.Substring(x1, x2 - x1) + " \n";                
                int a = 0;
                int b = 0;
                while (aux.IndexOf("\n", b + 2) > 0)
                {
                    b = aux.IndexOf(" ", a + 1);
                    aux2 = aux.Substring(a, b - a + 1);
                    for (i = 0; i < li; i++)
                    {
                        if (aux2.IndexOf(words[i]) > -1)
                        {
                            output += (i + 1).ToString() + " ";
                            break;
                        }
                    }
                    a = b;
                }
                if (output == "")
                    chat = "0";
                else
                    chat = output.Substring(0, output.Length - 1);
                //System.Windows.Forms.MessageBox.Show("chat= "+chat);
                livro += chat + " / " + trat + " \n";


                x1 = str.IndexOf("\n", x1) + 1;

            }
            System.Windows.Forms.MessageBox.Show(livro);



            path = Path.Combine(docPath, "Livro.txt");
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                outputFile.Write(livro);
            }
            System.Diagnostics.Process.Start(path);
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo cultureinfo = new CultureInfo("pt-BR", false);
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR", false);
            if ( textBox9.Text!="")
            {
                if (checkBox1.Checked)
                    label24.Text = DateTime.Now.AddMinutes(Int32.Parse(textBox9.Text)).ToString();
                else
                    label24.Text = "";
            }
            
            
        }

        private void button11_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo cultureinfo = new CultureInfo("pt-BR", false);
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR", false);
            if (textBox9.Text != "")
            {
                if (checkBox1.Checked)
                    label24.Text = DateTime.Now.AddMinutes(Int32.Parse(textBox9.Text)).ToString();
                else
                    label24.Text = "";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            path = Path.Combine(docPath, "SynapseA.txt");
            System.Diagnostics.Process.Start(path);
        }
    }
}
