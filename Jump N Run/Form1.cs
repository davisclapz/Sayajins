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
        // Game components and variables
        private readonly Timer spielUhr;
        private Rectangle spieler;
        private int geschwindigkeitY;
        private bool springtGerade;
        private bool gehtLinks;
        private bool gehtRechts;

        // Level elements
        private readonly List<Rectangle> plattformen;
        private readonly List<Rectangle> wolkenHintergrund;
        private readonly Rectangle ziel;
        private readonly int levelBreite = 2000;
        private int kameraPosition;

        // Game constants
        private const int Schwerkraft = 1;
        private const int SprungKraft = -18;
        private const int LaufGeschwindigkeit = 7;
        private const int BodenHoehe = 550;

        // Form components
        private IContainer components = null;

        public HauptFenster()
        {
            // Initialize components container
            components = new Container();

            // Initialize readonly fields
            spielUhr = new Timer(components);
            plattformen = new List<Rectangle>();
            wolkenHintergrund = new List<Rectangle>();
            ziel = new Rectangle(levelBreite - 100, 50, 60, 60);

            // Initialize other fields
            spieler = new Rectangle(100, 300, 30, 50);
            geschwindigkeitY = 0;
            springtGerade = false;
            gehtLinks = false;
            gehtRechts = false;
            kameraPosition = 0;

            // Initialize form properties
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Basic form settings
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 600);
            this.Name = "HauptFenster";
            this.Text = "Himmels-Spring-Spiel";

            // Custom form settings
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.ResumeLayout(false);
        }

        private void InitializeGame()
        {
            // Set up game timer
            spielUhr.Interval = 20; // 50 FPS
            spielUhr.Tick += SpielSchleife;

            // Set up keyboard events
            this.KeyDown += HauptFenster_KeyDown;
            this.KeyUp += HauptFenster_KeyUp;
            this.KeyPreview = true;

            // Build level
            BaueLevel();

            // Start game
            spielUhr.Start();
        }

        private void BaueLevel()
        {
            plattformen.Clear();
            wolkenHintergrund.Clear();

            // Create background clouds
            Random zufall = new Random();
            for (int i = 0; i < 20; i++)
            {
                wolkenHintergrund.Add(new Rectangle(
                    zufall.Next(0, levelBreite),
                    zufall.Next(50, 300),
                    100, 40));
            }

            // Create platforms (jumpable clouds)
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
            // Handle movement
            if (gehtLinks && spieler.X > 0) spieler.X -= LaufGeschwindigkeit;
            if (gehtRechts && spieler.X < levelBreite - spieler.Width) spieler.X += LaufGeschwindigkeit;

            // Update camera
            if (spieler.X > this.ClientSize.Width / 2 &&
                spieler.X < levelBreite - this.ClientSize.Width / 2)
            {
                kameraPosition = spieler.X - this.ClientSize.Width / 2;
            }

            // Apply gravity
            geschwindigkeitY += Schwerkraft;
            spieler.Y += geschwindigkeitY;

            // Check platform collisions
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

            // Check if player fell
            if (spieler.Y > this.ClientSize.Height)
            {
                spieler.X = 100;
                spieler.Y = 300;
                geschwindigkeitY = 0;
                springtGerade = false;
                kameraPosition = 0;
            }

            // Check if player reached goal
            if (spieler.IntersectsWith(ziel))
            {
                spielUhr.Stop();
                MessageBox.Show("🎉 Geschafft! Du hast die Sonne erreicht!", "Gewonnen");
                this.Close();
            }

            // Redraw game
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Apply camera offset
            g.TranslateTransform(-kameraPosition, 0);

            // Draw sky background
            using (var himmelPinsel = new LinearGradientBrush(
                new Point(0, 0),
                new Point(0, this.ClientSize.Height),
                Color.LightSkyBlue,
                Color.DeepSkyBlue))
            {
                g.FillRectangle(himmelPinsel, 0, 0, levelBreite, this.ClientSize.Height);
            }

            // Draw background clouds
            foreach (var wolke in wolkenHintergrund)
            {
                g.FillEllipse(Brushes.WhiteSmoke, wolke);
            }

            // Draw platforms
            foreach (var plattform in plattformen)
            {
                g.FillEllipse(Brushes.WhiteSmoke, plattform);
            }

            // Draw goal (sun)
            g.FillEllipse(Brushes.Gold, ziel);
            g.DrawString("Ziel", this.Font, Brushes.Black,
                ziel.X + ziel.Width / 2 - 15,
                ziel.Y + ziel.Height / 2 - 8);

            // Draw player
            g.FillEllipse(Brushes.Red, spieler);

            // Reset transform
            g.ResetTransform();

            // Draw UI
            using (var schrift = new Font("Arial", 14, FontStyle.Bold))
            {
                g.DrawString("Springe zur Sonne! (Pfeiltasten)", schrift, Brushes.White, 20, 20);
            }
        }

        private void HauptFenster_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    gehtLinks = true;
                    break;

                case Keys.Right:
                    gehtRechts = true;
                    break;

                case Keys.Up:
                case Keys.Space:
                    if (!springtGerade)
                    {
                        geschwindigkeitY = SprungKraft;
                        springtGerade = true;
                    }
                    break;

                case Keys.Escape:
                    this.Close();
                    break;
            }
        }

        private void HauptFenster_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    gehtLinks = false;
                    break;

                case Keys.Right:
                    gehtRechts = false;
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HauptFenster());
        }
    }
}