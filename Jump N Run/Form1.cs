using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkyJumpGame
{
    public class GameForm : Form
    {
        // Spielfigur und Steuerung
        private readonly Timer gameTimer;
        private Rectangle player;
        private int velocityY;
        private bool isJumping;
        private bool moveLeft;
        private bool moveRight;

        // Level-Elemente
        private readonly List<Rectangle> platforms;
        private readonly List<Point> cloudPositions; // Hintergrundwolken
        private readonly Rectangle goal;
        private readonly int levelWidth = 2000;
        private int cameraX;

        // Konstanten
        private const int Gravity = 1;
        private const int JumpStrength = -18;
        private const int MoveSpeed = 7;
        private const int GroundHeight = 550;
        private int lives = 3;

        private IContainer components = null;

        public GameForm()
        {
            components = new Container();
            gameTimer = new Timer(components);
            platforms = new List<Rectangle>();
            cloudPositions = new List<Point>();
            goal = new Rectangle(levelWidth - 100, 50, 60, 60);
            player = new Rectangle(100, 300, 30, 50);
            velocityY = 0;

            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(800, 600);
            this.Name = "GameForm";
            this.Text = "Sky Jump";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ResumeLayout(false);
        }

        private void InitializeGame()
        {
            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;

            this.KeyDown += MainForm_KeyDown;
            this.KeyUp += MainForm_KeyUp;
            this.KeyPreview = true;

            BuildLevel();
            gameTimer.Start();
        }

        private void BuildLevel()
        {
            platforms.Clear();
            cloudPositions.Clear();
            Random random = new Random();

            // Hintergrundwolken zufällig platzieren
            for (int i = 0; i < 20; i++)
            {
                cloudPositions.Add(new Point(
                    random.Next(0, levelWidth),
                    random.Next(50, 300)));
            }

            // Plattformen 
            platforms.Add(new Rectangle(100, GroundHeight, 120, 20)); // Startplattform
            platforms.Add(new Rectangle(300, 450, 120, 20));
            platforms.Add(new Rectangle(500, 400, 120, 20));
            platforms.Add(new Rectangle(700, 350, 120, 20));
            platforms.Add(new Rectangle(900, 300, 120, 20));
            platforms.Add(new Rectangle(1100, 250, 120, 20));
            platforms.Add(new Rectangle(1300, 200, 120, 20));
            platforms.Add(new Rectangle(1500, 150, 120, 20));
            platforms.Add(new Rectangle(1700, 100, 120, 20));
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Bewegung links/rechts
            if (moveLeft && player.X > 0)
                player.X -= MoveSpeed;
            if (moveRight && player.X < levelWidth - player.Width)
                player.X += MoveSpeed;

            // Kamera folgt Spieler
            if (player.X > this.ClientSize.Width / 2 &&
                player.X < levelWidth - this.ClientSize.Width / 2)
            {
                cameraX = player.X - this.ClientSize.Width / 2;
            }

            // Schwerkraft anwenden
            velocityY += Gravity;
            player.Y += velocityY;

            // Plattform-Kollision prüfen
            foreach (var platform in platforms)
            {
                if (player.Bottom >= platform.Top &&
                    player.Bottom <= platform.Top + 15 &&
                    player.Right > platform.Left + 10 &&
                    player.Left < platform.Right - 10 &&
                    velocityY >= 0)
                {
                    player.Y = platform.Top - player.Height;
                    velocityY = 0;
                    isJumping = false;
                    break;
                }
            }

            // Spieler fällt aus dem Level
            if (player.Y > this.ClientSize.Height)
            {
                lives--;
                if (lives <= 0)
                {
                    gameTimer.Stop();
                    MessageBox.Show("💀 Game Over!", "Wohin du Vogel");
                    this.Close();
                    return;
                }

                // Respawn
                player.X = 100;
                player.Y = 300;
                velocityY = 0;
                isJumping = false;
                cameraX = 0;
            }

            // Ziel erreicht
            if (player.IntersectsWith(goal))
            {
                gameTimer.Stop();
                MessageBox.Show("🎉 Bis zur Sonne!", "Victory Royale");
                this.Close();
            }

            this.Invalidate(); // Bildschirm neu zeichnen
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Himmel-Hintergrund
            using (var sky = new LinearGradientBrush(
                new Point(0, 0),
                new Point(0, this.ClientSize.Height),
                Color.LightSkyBlue,
                Color.DeepSkyBlue))
            {
                g.FillRectangle(sky, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            // Kamera-Verschiebung
            g.TranslateTransform(-cameraX, 0);

            // Hintergrundwolken
            foreach (var pos in cloudPositions)
            {
                DrawCloud(g, pos.X, pos.Y, 120, 50);
            }

            // Plattformen – gleiche Wolkenform wie Hintergrund
            foreach (var platform in platforms)
            {
                DrawCloud(g, platform.X, platform.Y, platform.Width, platform.Height);
            }

            // Ziel (Sonne) zeichnen
            DrawSun(g, goal);

            // Spieler
            g.FillEllipse(Brushes.Red, player);

            g.ResetTransform();

            // HUD: Leben und Anleitung
            using (var font = new Font("Arial", 14, FontStyle.Bold))
            {
                g.DrawString("Erreiche die Sonne! (Pfeil tasten)", font, Brushes.White, 20, 20);
                g.DrawString("Leben: " + new string('❤', Math.Max(0, Math.Min(lives, 10))), font, Brushes.White, 20, 40);
            }
        }

        // Einheitliche Wolkenzeichnung – für Plattform & Hintergrund
        private void DrawCloud(Graphics g, int x, int y, int width, int height)
        {
            using (SolidBrush cloudBrush = new SolidBrush(Color.WhiteSmoke))
            {
                g.FillEllipse(cloudBrush, x + width * 0.1f, y + height * 0.3f, width * 0.6f, height * 0.5f);
                g.FillEllipse(cloudBrush, x, y + height * 0.4f, width * 0.8f, height * 0.6f);
                g.FillEllipse(cloudBrush, x + width * 0.3f, y + height * 0.1f, width * 0.5f, height * 0.6f);
            }
        }

        // Sonne zeichnen (Ziel)
        private void DrawSun(Graphics g, Rectangle area)
        {
            int centerX = area.X + area.Width / 2;
            int centerY = area.Y + area.Height / 2;
            int radius = area.Width / 2;

            using (Pen rayPen = new Pen(Color.Gold, 2))
            {
                for (int i = 0; i < 12; i++)
                {
                    double angle = i * Math.PI / 6;
                    int x1 = centerX + (int)(radius * 1.2 * Math.Cos(angle));
                    int y1 = centerY + (int)(radius * 1.2 * Math.Sin(angle));
                    int x2 = centerX + (int)(radius * 1.8 * Math.Cos(angle));
                    int y2 = centerY + (int)(radius * 1.8 * Math.Sin(angle));
                    g.DrawLine(rayPen, x1, y1, x2, y2);
                }
            }

            using (Brush sunBrush = new LinearGradientBrush(
                area, Color.Yellow, Color.Orange, 45f))
            {
                g.FillEllipse(sunBrush, area);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left: moveLeft = true; break;
                case Keys.Right: moveRight = true; break;
                case Keys.Up:
                case Keys.Space:
                    if (!isJumping)
                    {
                        velocityY = JumpStrength;
                        isJumping = true;
                    }
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) moveLeft = false;
            if (e.KeyCode == Keys.Right) moveRight = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }
    }
}
