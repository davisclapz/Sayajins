using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Himmels_Spring_Spiel
{
    public class HauptFenster : Form
    {
        // Spielfiguren und Steuerung
        private readonly Timer spielUhr;
        private Rectangle spieler;
        private int geschwindigkeitY;
        private bool springtGerade;
        private bool gehtLinks;
        private bool gehtRechts;

        // Level und Elemente
        private readonly List<Rectangle> plattformen;
        private readonly List<Point> wolkenPositionen;
        private readonly Rectangle ziel;
        private readonly int levelBreite = 2000;
        private int kameraPosition;

        // Konstanten
        private const int Schwerkraft = 1;
        private const int SprungKraft = -18;
        private const int LaufGeschwindigkeit = 7;
        private const int BodenHoehe = 550;
        private int leben = 3;

        // Form-Komponenten
        private IContainer components = null;

        public HauptFenster()
        {
            components = new Container();
            spielUhr = new Timer(components);
            plattformen = new List<Rectangle>();
            wolkenPositionen = new List<Point>();
            ziel = new Rectangle(levelBreite - 100, 50, 60, 60);
            spieler = new Rectangle(100, 300, 30, 50);
            geschwindigkeitY = 0;
            springtGerade = false;
            gehtLinks = false;
            gehtRechts = false;
            kameraPosition = 0;

            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(800, 600);
            this.Name = "HauptFenster";
            this.Text = "Himmels-Spring-Spiel";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ResumeLayout(false);
        }

        private void InitializeGame()
        {
            spielUhr.Interval = 20;
            spielUhr.Tick += SpielSchleife;

            this.KeyDown += HauptFenster_KeyDown;
            this.KeyUp += HauptFenster_KeyUp;
            this.KeyPreview = true;

            BaueLevel();
            spielUhr.Start();
        }

        private void BaueLevel()
        {
            plattformen.Clear();
            wolkenPositionen.Clear();
            Random zufall = new Random();

            for (int i = 0; i < 20; i++)
            {
                wolkenPositionen.Add(new Point(
                    zufall.Next(0, levelBreite),
                    zufall.Next(50, 300)));
            }

            plattformen.Add(new Rectangle(100, BodenHoehe, 120, 20)); // Start
            plattformen.Add(new Rectangle(300, 450, 120, 20));
            plattformen.Add(new Rectangle(500, 400, 120, 20));
            plattformen.Add(new Rectangle(700, 350, 120, 20));
            plattformen.Add(new Rectangle(900, 300, 120, 20));
            plattformen.Add(new Rectangle(1100, 250, 120, 20));
            plattformen.Add(new Rectangle(1300, 200, 120, 20));
            plattformen.Add(new Rectangle(1500, 150, 120, 20));
            plattformen.Add(new Rectangle(1700, 100, 120, 20));
        }

        private void SpielSchleife(object sender, EventArgs e)
        {
            if (gehtLinks && spieler.X > 0) spieler.X -= LaufGeschwindigkeit;
            if (gehtRechts && spieler.X < levelBreite - spieler.Width) spieler.X += LaufGeschwindigkeit;

            if (spieler.X > this.ClientSize.Width / 2 &&
                spieler.X < levelBreite - this.ClientSize.Width / 2)
            {
                kameraPosition = spieler.X - this.ClientSize.Width / 2;
            }

            geschwindigkeitY += Schwerkraft;
            spieler.Y += geschwindigkeitY;

            foreach (var plattform in plattformen)
            {
                if (spieler.Bottom >= plattform.Top &&
                    spieler.Bottom <= plattform.Top + 15 &&
                    spieler.Right > plattform.Left + 10 &&
                    spieler.Left < plattform.Right - 10 &&
                    geschwindigkeitY >= 0)
                {
                    spieler.Y = plattform.Top - spieler.Height;
                    geschwindigkeitY = 0;
                    springtGerade = false;
                    break;
                }
            }

            if (spieler.Y > this.ClientSize.Height)
            {
                leben--;
                if (leben <= 0)
                {
                    spielUhr.Stop();
                    MessageBox.Show("💀 Game Over!", "Verloren");
                    this.Close();
                    return;
                }

                spieler.X = 100;
                spieler.Y = 300;
                geschwindigkeitY = 0;
                springtGerade = false;
                kameraPosition = 0;
            }

            if (spieler.IntersectsWith(ziel))
            {
                spielUhr.Stop();
                MessageBox.Show("🎉 Geschafft! Du hast die Sonne erreicht!", "Gewonnen");
                this.Close();
            }

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var himmel = new LinearGradientBrush(
                new Point(0, 0),
                new Point(0, this.ClientSize.Height),
                Color.LightSkyBlue,
                Color.DeepSkyBlue))
            {
                g.FillRectangle(himmel, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            g.TranslateTransform(-kameraPosition, 0);

            foreach (var pos in wolkenPositionen)
            {
                ZeichneWolke(g, pos.X, pos.Y, 100, 40);
            }

            foreach (var plattform in plattformen)
            {
                ZeichneWolke(g, plattform.X, plattform.Y, plattform.Width, plattform.Height);
            }

            ZeichneSonne(g, ziel);
            g.FillEllipse(Brushes.Red, spieler);

            g.ResetTransform();

            using (var schrift = new Font("Arial", 14, FontStyle.Bold))
            {
                g.DrawString("Springe zur Sonne! (Pfeiltasten)", schrift, Brushes.White, 20, 20);
                g.DrawString("Leben: " + new string('❤', Math.Max(0, Math.Min(leben, 10))), schrift, Brushes.White, 20, 40);

            }

        }

        private void ZeichneWolke(Graphics g, int x, int y, int breite, int hoehe)
        {
            using (SolidBrush wolkenPinsel = new SolidBrush(Color.WhiteSmoke))
            {
                g.FillEllipse(wolkenPinsel, x + breite * 0.2f, y, breite * 0.6f, hoehe);
                g.FillEllipse(wolkenPinsel, x, y + hoehe * 0.3f, breite, hoehe * 0.7f);
                g.FillEllipse(wolkenPinsel, x + breite * 0.3f, y + hoehe * 0.2f, breite * 0.5f, hoehe);
            }
        }

        private void ZeichneSonne(Graphics g, Rectangle bereich)
        {
            int centerX = bereich.X + bereich.Width / 2;
            int centerY = bereich.Y + bereich.Height / 2;
            int radius = bereich.Width / 2;

            using (Pen strahl = new Pen(Color.Gold, 2))
            {
                for (int i = 0; i < 12; i++)
                {
                    double winkel = i * Math.PI / 6;
                    int x1 = centerX + (int)(radius * 1.2 * Math.Cos(winkel));
                    int y1 = centerY + (int)(radius * 1.2 * Math.Sin(winkel));
                    int x2 = centerX + (int)(radius * 1.8 * Math.Cos(winkel));
                    int y2 = centerY + (int)(radius * 1.8 * Math.Sin(winkel));
                    g.DrawLine(strahl, x1, y1, x2, y2);
                }
            }

            using (Brush sonneKern = new LinearGradientBrush(
                bereich, Color.Yellow, Color.Orange, 45f))
            {
                g.FillEllipse(sonneKern, bereich);
            }
        }

        private void HauptFenster_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left: gehtLinks = true; break;
                case Keys.Right: gehtRechts = true; break;
                case Keys.Up:
                case Keys.Space:
                    if (!springtGerade)
                    {
                        geschwindigkeitY = SprungKraft;
                        springtGerade = true;
                    }
                    break;
                case Keys.Escape: this.Close(); break;
            }
        }

        private void HauptFenster_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) gehtLinks = false;
            if (e.KeyCode == Keys.Right) gehtRechts = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }



    }
}
