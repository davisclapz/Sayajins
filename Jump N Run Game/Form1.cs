using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LavaJumper
{
    public partial class GameForm : Form
    {
        System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();


        Rectangle player = new Rectangle(100, 400, 40, 40);
        int playerSpeedX = 0;
        int playerSpeedY = 0;
        int gravity = 2;
        int jumpStrength = -24;
        bool isJumping = false;
        bool onGround = false;

        List<Rectangle> platforms = new List<Rectangle>();
        List<MovingPlatform> movingPlatforms = new List<MovingPlatform>();
        List<Rectangle> lavaBalls = new List<Rectangle>();
        Rectangle lava = new Rectangle(0, 550, 1600, 50);

        int score = 0;
        int lives = 3;
        bool gameOver = false;
        bool showMenu = true;

        int lavaBallTimer = 0;
        Random rand = new Random();

        public GameForm()
        {
            
            this.DoubleBuffered = true;
            this.Width = 1000;
            this.Height = 600;
            this.Text = "Lava Jumper";

            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;

            this.KeyDown += Form_KeyDown;
            this.KeyUp += Form_KeyUp;
        }

        private void StartGame(int level)
        {
            showMenu = false;
            SetupLevel(level);
            gameTimer.Start();
        }

        private void SetupLevel(int level)
        {
            player.X = 100;
            player.Y = 400;
            playerSpeedX = 0;
            playerSpeedY = 0;
            isJumping = false;
            onGround = false;
            score = 0;
            lives = 3;
            gameOver = false;

            platforms.Clear();
            movingPlatforms.Clear();
            lavaBalls.Clear();

            platforms.Add(new Rectangle(100, 500, 100, 20));

            if (level >= 1)
            {
                platforms.Add(new Rectangle(250, 450, 100, 20));
                platforms.Add(new Rectangle(400, 400, 80, 20));
            }
            if (level >= 2)
            {
                platforms.Add(new Rectangle(550, 350, 100, 20));
                movingPlatforms.Add(new MovingPlatform(700, 300, 80, 20, true));
            }
            if (level == 3)
            {
                platforms.Add(new Rectangle(900, 250, 100, 20));
                movingPlatforms.Add(new MovingPlatform(1100, 200, 80, 20, false));
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameOver)
            {
                Invalidate();
                return;
            }

            // Bewegung
            playerSpeedY += gravity;
            player.X += playerSpeedX;
            player.Y += playerSpeedY;

            // Plattform-Kollision
            onGround = false;
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

            foreach (var mp in movingPlatforms)
            {
                mp.Update();
                if (player.IntersectsWith(mp.Bounds) && playerSpeedY >= 0)
                {
                    player.Y = mp.Bounds.Y - player.Height;
                    playerSpeedY = 0;
                    onGround = true;
                    isJumping = false;
                }
            }

            // Lava-Kollision
            if (player.IntersectsWith(lava))
            {
                lives--;
                if (lives <= 0) gameOver = true;
                else ResetPlayer();
            }

            // Lava-Kugeln
            lavaBallTimer++;
            if (lavaBallTimer > 60)
            {
                lavaBallTimer = 0;
                int x = rand.Next(0, this.ClientSize.Width - 20);
                lavaBalls.Add(new Rectangle(x, lava.Y, 20, 20));
            }

            for (int i = lavaBalls.Count - 1; i >= 0; i--)
            {
                lavaBalls[i] = new Rectangle(lavaBalls[i].X, lavaBalls[i].Y - 8, 20, 20);
                if (lavaBalls[i].Y < 0)
                    lavaBalls.RemoveAt(i);
                else if (player.IntersectsWith(lavaBalls[i]))
                {
                    lives--;
                    lavaBalls.RemoveAt(i);
                    if (lives <= 0) gameOver = true;
                    else ResetPlayer();
                }
            }

            // Spielfeldgrenzen
            if (player.X < 0) player.X = 0;
            if (player.X + player.Width > this.ClientSize.Width) player.X = this.ClientSize.Width - player.Width;
            if (player.Y > this.ClientSize.Height)
            {
                lives--;
                if (lives <= 0) gameOver = true;
                else ResetPlayer();
            }

            Invalidate();
        }

        private void ResetPlayer()
        {
            player.X = 100;
            player.Y = 400;
            playerSpeedY = 0;
            playerSpeedX = 0;
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (showMenu)
            {
                if (e.KeyCode == Keys.D1) StartGame(1);
                if (e.KeyCode == Keys.D2) StartGame(2);
                if (e.KeyCode == Keys.D3) StartGame(3);
                return;
            }

            if (gameOver && e.KeyCode == Keys.Enter)
            {
                showMenu = true;
                gameOver = false;
                Invalidate();
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

        private void Form_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) playerSpeedX = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (showMenu)
            {
                g.Clear(Color.OrangeRed);
                string msg = "LAVA JUMPER\nDrücke 1-3 zum Starten eines Levels";
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(msg, new Font("Arial", 28, FontStyle.Bold), Brushes.White, ClientRectangle, sf);
                return;
            }

            g.Clear(Color.LightSalmon);

            // Plattformen
            foreach (var plat in platforms)
                g.FillRectangle(Brushes.SaddleBrown, plat);

            // Bewegliche Plattformen
            foreach (var mp in movingPlatforms)
                g.FillRectangle(Brushes.Peru, mp.Bounds);

            // Lava
            g.FillRectangle(Brushes.Red, lava);

            // Lava-Kugeln
            foreach (var ball in lavaBalls)
                g.FillEllipse(Brushes.OrangeRed, ball);

            // Spieler
            g.FillRectangle(Brushes.Blue, player);

            g.DrawString($"Leben: {lives}", new Font("Arial", 16), Brushes.Black, 10, 10);

            if (gameOver)
            {
                string msg = "GAME OVER\nDrücke ENTER";
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, Color.Black)), ClientRectangle);
                g.DrawString(msg, new Font("Arial", 32, FontStyle.Bold), Brushes.White, ClientRectangle, sf);
            }
        }
    }

    public class MovingPlatform
    {
        public Rectangle Bounds;
        private int range;
        private int speed;
        private int direction = 1;
        private int start;
        private bool vertical;

        public MovingPlatform(int x, int y, int w, int h, bool vertical)
        {
            this.Bounds = new Rectangle(x, y, w, h);
            this.vertical = vertical;
            this.range = 100;
            this.speed = 2;
            this.start = vertical ? y : x;
        }

        public void Update()
        {
            if (vertical)
            {
                Bounds.Y += speed * direction;
                if (Bounds.Y > start + range || Bounds.Y < start - range)
                    direction *= -1;
            }
            else
            {
                Bounds.X += speed * direction;
                if (Bounds.X > start + range || Bounds.X < start - range)
                    direction *= -1;
            }
        }
    }
}
