using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MagyarorszagTerkepe
{
    public partial class Mainem : Window
    {
        double[] xKoordinatak = new double[3137];
        double[] yKoordinatak = new double[3137];
        List<(int, int)> utHalozat = new List<(int, int)>();

        public Mainem()
        {
            InitializeComponent();
            BeolvasKoordinatak("helysegek_koordinatai.csv");
            SzamolUtHalozat();
        }

        private void BeolvasKoordinatak(string fajlUtvonal)
        {
            using (StreamReader file = new StreamReader(fajlUtvonal))
            {
                file.ReadLine(); // Fejléc átugrása
                int db = 0;

                while (!file.EndOfStream)
                {
                    string sor = file.ReadLine();
                    string[] reszek = sor.Split(';'); // név;x;y

                    double x = ParseKoordinata(reszek[1]);
                    double y = ParseKoordinata(reszek[2]);

                    xKoordinatak[db] = x;
                    yKoordinatak[db] = y;
                    db++;
                }
            }
        }

        private double ParseKoordinata(string koordinata)
        {
            string[] reszek = koordinata.Split(new char[] { ':', '.' });
            double fok = int.Parse(reszek[0]);
            double perc = double.Parse(reszek[1]) / 60;
            double masodperc = double.Parse(reszek[2]) / 6000;
            return fok + perc + masodperc;
        }

        private void SzamolUtHalozat()
        {
            int db = xKoordinatak.Length;

            for (int i = 0; i < db; i++)
            {
                List<(int, double)> tavolsagok = new List<(int, double)>();

                for (int j = 0; j < db; j++)
                {
                    if (i != j)
                    {
                        double tavolsag = SzamolTavolsag(xKoordinatak[i], yKoordinatak[i], xKoordinatak[j], yKoordinatak[j]);
                        tavolsagok.Add((j, tavolsag));
                    }
                }

                tavolsagok.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                for (int k = 0; k < 4; k++)
                {
                    utHalozat.Add((i, tavolsagok[k].Item1));
                }
            }
        }

        private double SzamolTavolsag(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        private void Terkep_Click(object sender, MouseButtonEventArgs e)
        {
            RajzolTerkep();
        }

        private void Teglalap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show($"{((Rectangle)sender).Margin}");
        }

        private void Ablak_MeretValtozas(object sender, SizeChangedEventArgs e)
        {
            RajzolTerkep();
        }

        private void RajzolTerkep()
        {
            Rajzlap.Children.Clear();
            Rajzlap.Width = this.ActualWidth;
            Rajzlap.Height = this.ActualHeight - 20;
            double xMeret = Rajzlap.Width / 7;
            double yMeret = Rajzlap.Height / 3;

            RajzolTeglalapok(xMeret, yMeret);
            RajzolVonalak(xMeret, yMeret);
        }

        private void RajzolTeglalapok(double xMeret, double yMeret)
        {
            for (int i = 0; i < xKoordinatak.Length; i++)
            {
                Rectangle teglalap = new Rectangle
                {
                    Width = 7,
                    Height = 7,
                    Margin = new Thickness((xKoordinatak[i] - 16) * xMeret + 3, (48.7 - yKoordinatak[i]) * yMeret, 0, 0),
                    Fill = SzerezzSzinBySzelesseg(yKoordinatak[i])
                };
                teglalap.MouseUp += Teglalap_MouseUp;
                Rajzlap.Children.Add(teglalap);
            }
        }

        private void RajzolVonalak(double xMeret, double yMeret)
        {
            foreach (var ut in utHalozat)
            {
                int index1 = ut.Item1;
                int index2 = ut.Item2;

                Line vonal = new Line
                {
                    X1 = (xKoordinatak[index1] - 16) * xMeret + 3 + 3.5,
                    Y1 = (48.7 - yKoordinatak[index1]) * yMeret + 3.5,
                    X2 = (xKoordinatak[index2] - 16) * xMeret + 3 + 3.5,
                    Y2 = (48.7 - yKoordinatak[index2]) * yMeret + 3.5,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Rajzlap.Children.Add(vonal);
            }
        }

        private Brush SzerezzSzinBySzelesseg(double szelesseg)
        {
            if (szelesseg > 47.5)
            {
                return Brushes.Red;
            }
            else if (szelesseg < 46.3)
            {
                return Brushes.Green;
            }
            else
            {
                return Brushes.White;
            }
        }
    }
}
