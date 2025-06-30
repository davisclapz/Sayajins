namespace Jump_N_Run_Game
{
    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();

        Rectangle player = new Rectangle(100, 300, 40, 40);
        int playerSpeedX = 0;
        int playerSpeedY = 0;
        int gravity = 2;
        int jumpStrength = -20;
        bool isJumping = false;
        bool onGround = false;

        List<Rectangle> platforms = new List<Rectangle>();
        List<Rectangle> coins = new List<Rectangle>();
        List<Enemy> enemies = new List<Enemy>();

        int score = 0;
        int lives = 3;
        bool gameOver = false;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Width = 800;
            this.Height = 600;
            this.Text = "Jump and Run";

            SetupLevel();

            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void SetupLevel()
        {
            // Spieler Startposition
            player.X = 100;
            player.Y = 300;
            playerSpeedX = 0;
            playerSpeedY = 0;
            isJumping = false;
            onGround = false;
            score = 0;
            lives = 3;
            gameOver = false;

            // Plattformen
            platforms.Clear();
            platforms.Add(new Rectangle(0, 550, 800, 50));
            platforms.Add(new Rectangle(200, 450, 150, 20));
            platforms.Add(new Rectangle(400, 350, 150, 20));
            platforms.Add(new Rectangle(600, 250, 150, 20));

            // Münzen
            coins.Clear();
            coins.Add(new Rectangle(220, 410, 20, 20));
            coins.Add(new Rectangle(430, 310, 20, 20));
            coins.Add(new Rectangle(620, 210, 20, 20));

            // Gegner
            enemies.Clear();
            enemies.Add(new Enemy(new Rectangle(300, 510, 40, 40), 3, 200, 400));
            enemies.Add(new Enemy(new Rectangle(500, 310, 40, 40), 2, 400, 550));
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameOver)
            {
                Invalidate();
                return;
            }

            // Schwerkraft
            playerSpeedY += gravity;

            // Spieler Bewegung
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
                    onGround = true;
                    isJumping = false;
                }
            }

            // Münzen sammeln
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                if (player.IntersectsWith(coins[i]))
                {
                    coins.RemoveAt(i);
                    score += 10;
                }
            }

            // Gegner bewegen und Kollisionscheck
            foreach (var enemy in enemies)
            {
                enemy.Move();

                if (player.IntersectsWith(enemy.Bounds))
                {
                    lives--;
                    if (lives <= 0)
                    {
                        gameOver = true;
                    }
                    else
                    {
                        // Spieler zurücksetzen
                        player.X = 100;
                        player.Y = 300;
                        playerSpeedX = 0;
                        playerSpeedY = 0;
                        isJumping = false;
                        onGround = false;
                    }
                    break;
                }
            }

            // Spielfeld Grenzen (links/rechts)
            if (player.X < 0) player.X = 0;
            if (player.X + player.Width > this.ClientSize.Width) player.X = this.ClientSize.Width - player.Width;

            // Unten raus = leben verlieren + reset
            if (player.Y > this.ClientSize.Height)
            {
                lives--;
                if (lives <= 0)
                {
                    gameOver = true;
                }
                else
                {
                    player.X = 100;
                    player.Y = 300;
                    playerSpeedX = 0;
                    playerSpeedY = 0;
                    isJumping = false;
                    onGround = false;
                }
            }

            Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameOver)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SetupLevel();
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
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) playerSpeedX = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.SkyBlue);

            // Plattformen
            foreach (var plat in platforms)
                g.FillRectangle(Brushes.SaddleBrown, plat);

            // Münzen
            foreach (var coin in coins)
                g.FillEllipse(Brushes.Gold, coin);

            // Gegner
            foreach (var enemy in enemies)
                g.FillRectangle(Brushes.Black, enemy.Bounds);

            // Spieler
            g.FillRectangle(Brushes.Red, player);

            // UI: Punkte und Leben
            g.DrawString("Punkte: " + score, new Font("Arial", 16), Brushes.Black, 10, 10);
            g.DrawString("Leben: " + lives, new Font("Arial", 16), Brushes.Black, 10, 40);

            // Game Over Screen
            if (gameOver)
            {
                string msg = "GAME OVER\nDrücke ENTER zum Neustart";
                var sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, Color.Black)), ClientRectangle);
                g.DrawString(msg, new Font("Arial", 32, FontStyle.Bold), Brushes.White, ClientRectangle, sf);
            }
        }
    }

    // Hilfsklasse für Gegner
    public class Enemy
    {
        public Rectangle Bounds;
        private int speed;
        private int minX;
        private int maxX;
        private int direction = 1;

        public Enemy(Rectangle rect, int speed, int minX, int maxX)
        {
            this.Bounds = rect;
            this.speed = speed;
            this.minX = minX;
            this.maxX = maxX;
        }

        public void Move()
        {
            Bounds.X += speed * direction;
            if (Bounds.X < minX || Bounds.X + Bounds.Width > maxX)
                direction *= -1;
        }
    }
}
