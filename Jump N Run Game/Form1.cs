using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Jump_N_Run_Game
{
    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();
        Rectangle player = new Rectangle(100, 440, 40, 40);
        int playerSpeedX = 0;
        int playerSpeedY = 0;
        int gravity = 2;
        int jumpStrength = -25;
        bool isJumping = false;
        bool onGround = false;

        List<Rectangle> platforms = new List<Rectangle>();
        Rectangle lava; // groﬂe zusammenh‰ngende Lavafl‰che
        Rectangle goal = new Rectangle(1400, 360, 40, 40);

        int lives = 3;
        bool gameOver = false;
        bool gameWon = false;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Width = 900;
            this.Height = 600;
            this.Text = "?? Lava Level";

            SetupLevel();

            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void SetupLevel()
        {
            player.X = 100;
            player.Y = 440;
            playerSpeedX = 0;
            playerSpeedY = 0;
            isJumping = false;
            onGround = false;
            gameOver = false;
            gameWon = false;

            platforms.Clear();

            // Startplattform (kurz)
            platforms.Add(new Rectangle(100, 480, 120, 20));

            // Level-Design mit Plattformen
            platforms.Add(new Rectangle(300, 420, 100, 20));
            platforms.Add(new Rectangle(450, 360, 80, 20));
            platforms.Add(new Rectangle(580, 300, 100, 20));
            platforms.Add(new Rectangle(720, 360, 80, 20));
            platforms.Add(new Rectangle(850, 420, 100, 20));
            platforms.Add(new Rectangle(1000, 360, 80, 20));
            platforms.Add(new Rectangle(1120, 300, 120, 20));
            platforms.Add(new Rectangle(1300, 360, 100, 20)); // Zielplattform

            // Groﬂe zusammenh‰ngende Lavafl‰che unten (fast ganz unten)
            lava = new Rectangle(0, 520, 1600, 80);

            goal = new Rectangle(1400, 320, 40, 40); // Ziel am Ende
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameOver || gameWon)
            {
                Invalidate();
                return;
            }

            playerSpeedY += gravity;
            player.X += playerSpeedX;
            player.Y += playerSpeedY;

            // Kollision mit Plattformen
            onGround = false;
            foreach (var plat in platforms)
            {
                if (player.IntersectsWith(plat) && playerSpeedY >= 0)
                {
                    player.Y = plat.Y - player.Height;
                    playerSpeedY = 0;
                    isJumping = false;
                    onGround = true;
                }
            }

            // Lava-Kollision (groﬂe Fl‰che)
            if (player.IntersectsWith(lava))
            {
                lives--;
                if (lives <= 0)
                {
                    gameOver = true;
                }
                else
                {
                    ResetPlayer();
                }
            }

            // Ziel erreicht
            if (player.IntersectsWith(goal))
            {
                gameWon = true;
                gameTimer.Stop();
            }

            // Bildschirmgrenzen
            if (player.X < 0) player.X = 0;
            if (player.X + player.Width > 1600) player.X = 1600 - player.Width;

            if (player.Y > this.ClientSize.Height)
            {
                lives--;
                if (lives <= 0)
                {
                    gameOver = true;
                }
                else
                {
                    ResetPlayer();
                }
            }

            Invalidate();
        }

        private void ResetPlayer()
        {
            player.X = 100;
            player.Y = 440;
            playerSpeedX = 0;
            playerSpeedY = 0;
            isJumping = false;
            onGround = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameOver || gameWon)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SetupLevel();
                    gameTimer.Start();
                }
                return;
            }

            if (e.KeyCode == Keys.Left) playerSpeedX = -5;
            if (e.KeyCode == Keys.Right) playerSpeedX = 5;
            if (e.KeyCode == Keys.Space && onGround && !isJumping)
            {
                playerSpeedY = jumpStrength;
                isJumping = true;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                playerSpeedX = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Hintergrund mit Farbverlauf (warm, feurig)
            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle,
                Color.OrangeRed, Color.DarkOrange, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // Plattformen
            foreach (var plat in platforms)
                g.FillRectangle(Brushes.SaddleBrown, plat);

            // Groﬂe Lavafl‰che (gl¸hendes Orange)
            using (SolidBrush lavaBrush = new SolidBrush(Color.FromArgb(220, 255, 69, 0))) // leicht transparentes Feuer-Orange
            {
                g.FillRectangle(lavaBrush, lava);
            }

            // Ziel (gr¸n)
            g.FillRectangle(Brushes.LimeGreen, goal);

            // Spieler (rot)
            g.FillRectangle(Brushes.Blue, player);

            // UI: Leben
            g.DrawString("Leben: " + lives, new Font("Arial", 16, FontStyle.Bold), Brushes.White, 10, 10);

            if (gameOver)
            {
                string msg = "?? GAME OVER ??\nDr¸cke ENTER zum Neustart";
                var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, Color.Black)), ClientRectangle);
                g.DrawString(msg, new Font("Arial", 32, FontStyle.Bold), Brushes.White, ClientRectangle, sf);
            }

            if (gameWon)
            {
                string msg = "?? LEVEL GESCHAFFT! ??\nDr¸cke ENTER zum Neustart";
                var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, Color.Green)), ClientRectangle);
                g.DrawString(msg, new Font("Arial", 32, FontStyle.Bold), Brushes.White, ClientRectangle, sf);
            }
        }
    }
}
