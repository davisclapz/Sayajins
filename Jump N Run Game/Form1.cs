namespace Jump_N_Run_Game
{
    public partial class Form1 : Form
    {
        bool moveLeft = false;
        bool moveRight = false;
        bool isJumping = false;
        int velocityY = 0;

        const int jumpStrength = -20;

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                moveLeft = true;

            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                moveRight = true;

            if (e.KeyCode == Keys.Space && !isJumping)
            {
                velocityY = jumpStrength;
                isJumping = true;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                moveLeft = false;

            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                moveRight = false;
        }
        
           

            
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            Rectangle player = new Rectangle(50, 300, 40, 40);
            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
            this.KeyPreview = true;
            // wichtig: damit die Form die Tasten erkennt

        }
      
    }
}
