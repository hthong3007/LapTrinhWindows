using System;
using System.Drawing;
using System.Windows.Forms;

namespace LTWD
{
    public partial class Form13 : Form
    {
        PictureBox pbBasket = new PictureBox();
        PictureBox pbEgg = new PictureBox();
        PictureBox pbChicken = new PictureBox();
        PictureBox pbBomb = new PictureBox();

        Timer tmEgg = new Timer();
        Timer tmChicken = new Timer();
        Timer tmBomb = new Timer();
        Timer tmGameTime = new Timer(); // Timer cho thời gian chơi

        int xBasket = 300;
        int yBasket = 550;
        int xDeltaBasket = 30;

        int xChicken = 300;
        int yChicken = 10;
        int xDeltaChicken = 5;

        int xEgg;
        int yEgg = 10;
        int yDeltaEgg = 3;

        int xBomb;
        int yBomb = 10;
        int yDeltaBomb = 5;

        Random rand = new Random();

        bool isEggBroken = false;
        bool isDroppingBomb = false;

        int diem = 0;
        int thoigian = 60; // Thời gian chơi 60 giây
        int eggCount = 0;

        private int currentLevel = 1; // Thêm biến theo dõi cấp độ
        private int targetScore = 5; // Số điểm cần đạt để qua màn
        private int bombCooldown = 0; // Thời gian chờ giữa các lần thả bom
        private bool hasHitBomb = false; // Trạng thái đã hứng trúng bom

        public Form13()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Giảm hiện tượng nhấp nháy
        }

        private void Form13_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right && (xBasket < this.ClientSize.Width - pbBasket.Width))
                xBasket += xDeltaBasket;
            if (e.KeyCode == Keys.Left && xBasket > 0)
                xBasket -= xDeltaBasket;
            pbBasket.Location = new Point(xBasket, yBasket);
        }

        private void Form13_Load(object sender, EventArgs e)
        {
            this.BackgroundImage = Image.FromFile("../../Images/nen.png");
            this.BackgroundImageLayout = ImageLayout.Stretch;

            tmEgg.Interval = 10;
            tmEgg.Tick += tmEgg_Tick;
            tmEgg.Start();

            tmChicken.Interval = 10;
            tmChicken.Tick += tmChicken_Tick;
            tmChicken.Start();

            tmBomb.Interval = 10;
            tmBomb.Tick += tmBomb_Tick;

            tmGameTime.Interval = 1000; // Cập nhật mỗi giây
            tmGameTime.Tick += tmGameTime_Tick;
            tmGameTime.Start(); // Bắt đầu đếm thời gian

            pbBasket.SizeMode = PictureBoxSizeMode.StretchImage;
            pbBasket.Size = new Size(70, 70);
            pbBasket.Location = new Point(xBasket, yBasket);
            pbBasket.BackColor = Color.Transparent;
            this.Controls.Add(pbBasket);
            pbBasket.Image = Image.FromFile("../../Images/gio.png");

            pbEgg.SizeMode = PictureBoxSizeMode.StretchImage;
            pbEgg.Size = new Size(50, 50);
            pbEgg.BackColor = Color.Transparent;
            this.Controls.Add(pbEgg);
            pbEgg.Image = Image.FromFile("../../Images/trung1.png");

            pbChicken.SizeMode = PictureBoxSizeMode.StretchImage;
            pbChicken.Size = new Size(100, 100);
            pbChicken.Location = new Point(xChicken, yChicken);
            pbChicken.BackColor = Color.Transparent;
            this.Controls.Add(pbChicken);
            pbChicken.Image = Image.FromFile("../../Images/ga2.png");

            pbBomb.SizeMode = PictureBoxSizeMode.StretchImage;
            pbBomb.Size = new Size(50, 50);
            pbBomb.BackColor = Color.Transparent;
            this.Controls.Add(pbBomb);
            pbBomb.Image = Image.FromFile("../../Images/bom.png");
            pbBomb.Visible = false;

            ResetGame(); // Khởi động lại trò chơi
        }

        private void tmEgg_Tick(object sender, EventArgs e)
        {
            yEgg += yDeltaEgg;

            if (yEgg > this.ClientSize.Height - pbEgg.Height)
            {
                pbEgg.Image = Image.FromFile("../../Images/trung2.png");
                tmEgg.Stop();

                // Hiển thị hộp thoại khi bỏ lỡ trứng
                if (MessageBox.Show("Bạn đã bỏ lỡ trứng! Trò chơi sẽ bắt đầu lại.", "Game Over", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    ResetGame(); // Khởi động lại trò chơi
                }
                else
                {
                    this.Close(); // Đóng ứng dụng
                }

                return;
            }

            Rectangle unionRect = Rectangle.Intersect(pbEgg.Bounds, pbBasket.Bounds);
            if (!unionRect.IsEmpty)
            {
                diem++;
                lblDiem.Text = "" + diem;
                yEgg = 30; // Đặt lại vị trí y của trứng
                xEgg = pbChicken.Location.X + pbChicken.Width / 2 - pbEgg.Width / 2;
                pbEgg.Image = Image.FromFile("../../Images/trung1.png");
                eggCount++;

                // Kiểm tra nếu đã đạt đủ điểm để qua màn
                if (diem >= targetScore)
                {
                    tmEgg.Stop();
                    tmChicken.Stop();
                    MessageBox.Show("Chúc mừng! Bạn đã đạt " + diem + " điểm. Chuyển sang màn tiếp theo!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    currentLevel++; // Tăng cấp độ
                    targetScore += 5; // Tăng số điểm yêu cầu cho cấp độ tiếp theo
                    ResetGame(); // Khởi động lại trò chơi
                    return; // Ra khỏi hàm sau khi thông báo
                }

                // Giới hạn thời gian giữa các lần thả bom
                if (eggCount >= 3 && bombCooldown == 0)
                {
                    eggCount = 0; // Reset đếm trứng
                    DropBomb(); // Thả bom
                    bombCooldown = 30; // Thời gian chờ giữa các lần thả bom
                }
            }

            pbEgg.Location = new Point(xEgg, yEgg);
        }

        private void tmChicken_Tick(object sender, EventArgs e)
        {
            xChicken += xDeltaChicken;
            if (xChicken > this.ClientSize.Width - pbChicken.Width || xChicken <= 0)
            {
                xDeltaChicken = -xDeltaChicken;
            }
            pbChicken.Location = new Point(xChicken, yChicken);
        }

        private void tmBomb_Tick(object sender, EventArgs e)
        {
            if (isDroppingBomb)
            {
                yBomb += yDeltaBomb;

                if (yBomb > this.ClientSize.Height - pbBomb.Height)
                {
                    yBomb = 10; // Đặt lại vị trí y của bom
                    xBomb = rand.Next(0, this.ClientSize.Width - pbBomb.Width);
                    pbBomb.Location = new Point(xBomb, yBomb);
                }

                Rectangle unionRectBomb = Rectangle.Intersect(pbBomb.Bounds, pbBasket.Bounds);
                if (!unionRectBomb.IsEmpty && !hasHitBomb) // Kiểm tra xem đã hứng bom chưa
                {
                    hasHitBomb = true; // Đánh dấu là đã hứng bom
                    if (MessageBox.Show("Bạn đã hứng trúng bom! Bạn có muốn chơi lại?",
                        "Game Over",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        ResetGame(); // Gọi hàm ResetGame để thiết lập lại trò chơi
                    }
                    else
                    {
                        this.Close(); // Đóng ứng dụng
                    }
                    return;
                }

                pbBomb.Location = new Point(xBomb, yBomb);
            }
        }

        private void DropBomb()
        {
            isDroppingBomb = true;
            yBomb = 10;
            xBomb = rand.Next(0, this.ClientSize.Width - pbBomb.Width);
            pbBomb.Location = new Point(xBomb, yBomb);
            pbBomb.Visible = true;
            tmBomb.Start();
        }

        private void ResetGame()
        {
            // Đặt lại vị trí giỏ
            xBasket = 300;
            yBasket = 550;
            pbBasket.Location = new Point(xBasket, yBasket);

            // Đặt lại vị trí gà
            xChicken = 300;
            pbChicken.Location = new Point(xChicken, yChicken);

            // Đặt lại vị trí trứng
            yEgg = 10; // Reset yEgg
            xEgg = pbChicken.Location.X + pbChicken.Width / 2 - pbEgg.Width / 2;
            pbEgg.Location = new Point(xEgg, yEgg);
            pbEgg.Image = Image.FromFile("../../Images/trung1.png");

            // Đặt lại vị trí bom
            pbBomb.Visible = false; // Ẩn bom
            isDroppingBomb = false; // Đặt lại trạng thái bom đang rơi
            hasHitBomb = false; // Đặt lại trạng thái đã hứng bom

            // Đặt lại điểm số và thời gian
            diem = 0;
            lblDiem.Text = "0";

            thoigian = 60; // Reset thời gian
            lblDisplay.Text = "60"; // Hiện thị lại thời gian

            // Dừng các timer
            tmEgg.Stop();
            tmChicken.Stop();
            tmBomb.Stop();
            tmGameTime.Stop();

            // Bắt đầu lại game
            tmEgg.Start();
            tmChicken.Start();
            tmBomb.Start();
            tmGameTime.Start();
        }

        private void tmGameTime_Tick(object sender, EventArgs e)
        {
            thoigian--; // Giảm thời gian
            lblDisplay.Text = thoigian.ToString(); // Cập nhật thời gian trên label

            if (thoigian <= 0)
            {
                tmEgg.Stop();
                tmChicken.Stop();
                tmBomb.Stop();
                tmGameTime.Stop();

                MessageBox.Show("Thời gian đã hết! Trò chơi sẽ bắt đầu lại.", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ResetGame(); // Khởi động lại trò chơi
            }
        }
    }
}