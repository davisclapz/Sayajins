using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Jump_N_Run_Game1
{
    public partial class Form1 : Form
    {
        private enum GameState { StartScreen, Playing, GameOver }

        private GameState currentState = GameState.StartScreen;

        private System.Windows.Forms.Timer gameTimer;
        private Rectangle player = new Rectangle(100, 300, 40, 40);
        private int playerSpeedX = 0;
        private int playerSpeedY = 0;
        private int gravity = 2;
        private int jumpStrength = -20;
        private bool isJumping = false;
        private bool onGround = false;
        private int score = 0;
        private int lives = 3;
        private int selectedLevel = 1;

        private List<Rectangle> platforms = new List<Rectangle>();
        private List<MovingPlatform> movingPlatforms = new List<MovingPlatform>();
        private List<LavaBall> lavaBalls = new List<LavaBall>();
        private Rectangle lavaZone;

        private Random rand = new Random();

        public Form1()
        {
            

            this.DoubleBuffered = true;
            this.Width = 800;
            this.Height = 600;
            this.Text = "Survival Jumper";

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            SetupStartScreen();
        }

        private void SetupStartScreen()
        {
            currentState = GameState.StartScreen;
        }

        private void StartLevel(int level)
        {
            currentState = GameState.Playing;
            selectedLevel = level;
            lives = 3;
            score = 0;
            player = new Rectangle(100, 300, 40, 40);
            playerSpeedX = 0;
            playerSpeedY = 0;
            isJumping = false;
            onGround = false;

            // Plattformen
            platforms.Clear();
            platforms.Add(new Rectangle(0, 500, 200, 20));
            platforms.Add(new Rectangle(600, 500, 200, 20));

            if (level == 1)
            {
                platforms.Add(new Rectangle(300, 420, 120, 20));
                platforms.Add(new Rectangle(100, 340, 120, 20));
            }
            else if (level == 2)
            {
                platforms.Add(new Rectangle(300, 420, 120, 20));
                platforms.Add(new Rectangle(500, 360, 120, 20));
                platforms.Add(new Rectangle(200, 290, 120, 20));
            }

            // Lavafläche
            lavaZone = new Rectangle(0, 520, this.ClientSize.Width, this.ClientSize.Height - 520);

            // Bewegliche Plattformen
            movingPlatforms.Clear();
            movingPlatforms.Add(new MovingPlatform(new Rectangle(400, 250, 100, 20), 2, true));
            movingPlatforms.Add(new MovingPlatform(new Rectangle(200, 180, 100, 20), 2, false));

            // Lava-Kugeln
            lavaBalls.Clear();
            for (int i = 0; i < 5; i++)
            {
                int x = rand.Next(0, this.ClientSize.Width);
                lavaBalls.Add(new LavaBall(new Point(x, lavaZone.Y + lavaZone.Height - 10)));
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (currentState == GameState.Playing)
            {
                playerSpeedY += gravity;
                player.X += playerSpeedX;
                player.Y += playerSpeedY;
                onGround = false;

                // Plattform-Kollision
                foreach (var plat in platforms)
                {
                    if (player.IntersectsWith(plat) && playerSpeedY >= 0)
                    {
                        player.Y = plat.Y - player.Height;
                        playerSpeedY = 0;
                        onGround = true;
                        isJumping = false;
                    }
                }

                // Bewegliche Plattformen
                foreach (var mplat in movingPlatforms)
                {
                    mplat.Update();
                    if (player.IntersectsWith(mplat.Bounds) && playerSpeedY >= 0)
                    {
                        player.Y = mplat.Bounds.Y - player.Height;
                        playerSpeedY = 0;
                        onGround = true;
                        isJumping = false;
                    }
                }

                // Lava Berührung
                if (player.IntersectsWith(lavaZone))
                {
                    LoseLife();
                }

                // Lava-Kugel Bewegung & Kollision
                foreach (var ball in lavaBalls)
                {
                    ball.Update();
                    if (player.IntersectsWith(ball.Bounds))
                    {
                        LoseLife();
                        break;
                    }
                }

                // Spielfeldgrenzen
                if (player.X < 0) player.X = 0;
                if (player.X + player.Width > this.ClientSize.Width)
                    player.X = this.ClientSize.Width - player.Width;

                if (player.Y > this.ClientSize.Height)
                {
                    LoseLife();
                }
            }

            Invalidate();
        }

        private void LoseLife()
        {
            lives--;
            if (lives <= 0)
            {
                currentState = GameState.GameOver;
            }
            else
            {
                player = new Rectangle(100, 300, 40, 40);
                playerSpeedX = 0;
                playerSpeedY = 0;
                isJumping = false;
                onGround = false;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentState == GameState.StartScreen)
            {
                if (e.KeyCode == Keys.D1) StartLevel(1);
                if (e.KeyCode == Keys.D2) StartLevel(2);
                return;
            }

            if (currentState == GameState.GameOver)
            {
                if (e.KeyCode == Keys.Enter)
                    SetupStartScreen();
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
            g.Clear(Color.SkyBlue);

            if (currentState == GameState.StartScreen)
            {
                string title = "Survival Jumper";
                string instructions = "Drücke 1 oder 2 um ein Level zu starten";

                var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(title, new Font("Arial", 32, FontStyle.Bold), Brushes.DarkRed, ClientRectangle, sf);

                Rectangle instructionArea = new Rectangle(0, ClientSize.Height / 2 + 50, ClientSize.Width, 100);
                g.DrawString(instructions, new Font("Arial", 20), Brushes.Black, instructionArea, sf);
                return;
            }

            // Lava
            g.FillRectangle(Brushes.OrangeRed, lavaZone);

            // Plattformen
            foreach (var plat in platforms)
                g.FillRectangle(Brushes.SaddleBrown, plat);

            // Bewegliche Plattformen
            foreach (var mplat in movingPlatforms)
                g.FillRectangle(Brushes.Peru, mplat.Bounds);

            // Lava-Kugeln
            foreach (var ball in lavaBalls)
                g.FillEllipse(Brushes.DarkRed, ball.Bounds);

            // Spieler
            g.FillRectangle(Brushes.Red, player);

            // UI
            g.DrawString("Leben: " + lives, new Font("Arial", 16), Brushes.Black, 10, 10);

            if (currentState == GameState.GameOver)
            {
                string msg = "GAME OVER\nDrücke ENTER zum Neustart";
                var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, Color.Black)), ClientRectangle);
                g.DrawString(msg, new Font("Arial", 28, FontStyle.Bold), Brushes.White, ClientRectangle, sf);
            }
        }

        // Bewegliche Plattform
        public class MovingPlatform
        {
            public Rectangle Bounds;
            private int speed = 2;
            private bool vertical;
            private int direction = 1;

            public MovingPlatform(Rectangle bounds, int speed, bool vertical)
            {
                this.Bounds = bounds;
                this.speed = speed;
                this.vertical = vertical;
            }

            public void Update()
            {
                if (vertical)
                {
                    Bounds.Y += speed * direction;
                    if (Bounds.Y < 100 || Bounds.Y > 400)
                        direction *= -1;
                }
                else
                {
                    Bounds.X += speed * direction;
                    if (Bounds.X < 100 || Bounds.X > 600)
                        direction *= -1;
                }
            }
        }

        // Lava-Kugel
        public class LavaBall
        {
            public Rectangle Bounds;
            private int speed = -10;
            private int resetDelay = 100;
            private int delayCounter = 0;
            private Random rand = new Random();

            public LavaBall(Point start)
            {
                this.Bounds = new Rectangle(start.X, start.Y, 20, 20);
            }

            public void Update()
            {
                if (delayCounter > 0)
                {
                    delayCounter--;
                    return;
                }

                Bounds.Y += speed;
                if (Bounds.Y < 0)
                {
                    Bounds.Y = 580;
                    Bounds.X = rand.Next(0, 760);
                    delayCounter = rand.Next(30, 100);
                }
            }
        }
    }
}
