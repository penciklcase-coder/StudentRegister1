using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class LoginForm : Form
    {
        private readonly Panel leftPanel = new();
        private readonly Panel rightPanel = new();
        private readonly Panel loginBox = new();

        private readonly TextBox txtUserID = new();
        private readonly TextBox txtPassword = new();

        private static readonly Color LmsGreen = Color.FromArgb(14, 77, 45);
        private static readonly Color LimeAccent = Color.FromArgb(168, 224, 52);
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public LoginForm()
        {
            Text = "Сайтад нэвтрэх | LMS";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(1020, 620);
            MinimumSize = new Size(900, 560);

            // 1. ЗҮҮН ТАЛЫН НОГООН САМБАР (50%)
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = ClientSize.Width / 2;
            leftPanel.BackColor = LmsGreen;

            leftPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int w = leftPanel.Width;
                int h = leftPanel.Height;
                int baseX = Math.Max(40, (w - 430) / 2);
                int baseY = Math.Max(120, (h - 260) / 2);

                using var circlePen = new Pen(Color.FromArgb(25, 255, 255, 255), 1.5f);
                g.DrawEllipse(circlePen, w / 2 - 140, -110, 340, 340);
                g.DrawEllipse(circlePen, w / 2 - 190, -160, 440, 440);

                for (int i = 0; i < 6; i++)
                {
                    int y = h - 160 + i * 24;
                    g.DrawBezier(circlePen, new Point(0, y), new Point(w / 3, y - 25), new Point(w * 2 / 3, y + 25), new Point(w, y - 5));
                }

                Rectangle badgeRect = new Rectangle(baseX, baseY, 240, 32);
                using (GraphicsPath badgePath = HaaIsTheme.GetRoundPath(badgeRect, 15))
                using (Pen badgePen = new Pen(Color.FromArgb(120, 255, 255, 255), 1.2f))
                {
                    g.DrawPath(badgePen, badgePath);
                }
                TextRenderer.DrawText(g, "СУРГАЛТЫН ДИЖИТАЛ ОРЧИН", new Font("Segoe UI", 9F, FontStyle.Regular),
                    badgeRect, Color.FromArgb(220, 245, 230), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                g.DrawString("ХӨДӨӨ АЖ АХУЙН ИХ СУРГУУЛИЙН", new Font("Segoe UI", 14.5F, FontStyle.Bold),
                    Brushes.White, new PointF(baseX - 3, baseY + 52));

                g.DrawString("ДАРХАН-УУЛ", new Font("Segoe UI", 22F, FontStyle.Bold),
                    new SolidBrush(LimeAccent), new PointF(baseX - 5, baseY + 86));
                g.DrawString("АЙМАГ ДАХЬ", new Font("Segoe UI", 22F, FontStyle.Bold),
                    Brushes.White, new PointF(baseX + 203, baseY + 86));

                g.DrawString("САЛБАР СУРГУУЛЬ • КООСЭН", new Font("Segoe UI", 18F, FontStyle.Bold),
                    Brushes.White, new PointF(baseX - 3, baseY + 132));

                DrawFeaturePill(g, baseX, baseY + 200, 95, "📖 ХИЧЭЭЛ");
                DrawFeaturePill(g, baseX + 105, baseY + 200, 115, "☑ ДААЛГАВАР");
                DrawFeaturePill(g, baseX + 230, baseY + 200, 80, "📈 ДҮН");
            };

            // 2. БАРУУН ТАЛЫН ЦАГААН САМБАР (50%)
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.White;

            loginBox.Size = new Size(420, 520);
            loginBox.BackColor = Color.White;

            var logoPanel = new Panel
            {
                Location = new Point(160, 10),
                Size = new Size(100, 100),
                BackColor = Color.White
            };
            logoPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen1 = new Pen(LmsGreen, 3.5f);
                using var pen2 = new Pen(LmsGreen, 1.5f);
                e.Graphics.DrawEllipse(pen1, 5, 5, 88, 88);
                e.Graphics.DrawEllipse(pen2, 15, 15, 68, 68);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString("ХААИС\nКООСЭН", new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    new SolidBrush(LmsGreen), new RectangleF(15, 15, 68, 68), sf);
            };
            loginBox.Controls.Add(logoPanel);

            loginBox.Controls.Add(new Label
            {
                Text = "Сургалтын удирдлагын системд нэвтрэх ↴",
                Location = new Point(50, 122),
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 90, 85),
                Font = new Font("Segoe UI", 9.5F)
            });

            // Оюутан (идэвхтэй) ба Багшийн тусдаа цонх руу үсрэх товч
            var btnStudentTab = HaaIsTheme.CreateButton("Оюутан", "btnStudentTab", 50, 152, primary: true);
            btnStudentTab.Size = new Size(155, 38);
            loginBox.Controls.Add(btnStudentTab);

            var btnTeacherPortal = HaaIsTheme.CreateButton("Багш нэвтрэх ➔", "btnTeacherPortal", 215, 152, primary: false);
            btnTeacherPortal.Size = new Size(155, 38);
            btnTeacherPortal.Click += (s, e) =>
            {
                using var tLogin = new TeacherLoginForm();
                Hide();
                tLogin.ShowDialog();
                Show();
            };
            loginBox.Controls.Add(btnTeacherPortal);

            loginBox.Controls.Add(HaaIsTheme.CreateLabel("Оюутны код (OpenID):", 50, 208));
            HaaIsTheme.StyleTextBox(txtUserID, 50, 233, 320);
            loginBox.Controls.Add(txtUserID);

            loginBox.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг:", 50, 278));
            txtPassword.PasswordChar = '●';
            HaaIsTheme.StyleTextBox(txtPassword, 50, 303, 320);
            loginBox.Controls.Add(txtPassword);

            var btnLogin = HaaIsTheme.CreateButton("Системд нэвтрэх", "btnLogin", 50, 355, primary: true);
            btnLogin.Size = new Size(320, 44);
            btnLogin.Click += BtnLogin_Click;
            loginBox.Controls.Add(btnLogin);

            loginBox.Controls.Add(new Label
            {
                Text = "Нууц үгээ мартсан бол academic@sab.edu.mn хаягаар холбогдоно уу.",
                Location = new Point(10, 415),
                Size = new Size(400, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(110, 120, 115),
                Font = new Font("Segoe UI", 8.2F)
            });

            // Админы тусгай нэвтрэх цонх (ManagerLoginForm) руу үсрэх холбоос
            var btnAdminPortal = new Button
            {
                Text = "⚙ Систем удирдлага (Админ нэвтрэх)",
                Location = new Point(85, 452),
                Size = new Size(250, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(120, 40, 40),
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdminPortal.FlatAppearance.BorderSize = 0;
            btnAdminPortal.Click += (s, e) =>
            {
                using var adminLogin = new ManagerLoginForm();
                Hide();
                adminLogin.ShowDialog();
                Show();
            };
            loginBox.Controls.Add(btnAdminPortal);

            rightPanel.Controls.Add(loginBox);

            Resize += (s, e) => AdjustResponsiveLayout();
            Load += (s, e) => AdjustResponsiveLayout();

            Controls.Add(rightPanel);
            Controls.Add(leftPanel);
        }

        private void AdjustResponsiveLayout()
        {
            leftPanel.Width = ClientSize.Width / 2;
            leftPanel.Invalidate();

            loginBox.Location = new Point(
                Math.Max(10, (rightPanel.Width - loginBox.Width) / 2),
                Math.Max(10, (rightPanel.Height - loginBox.Height) / 2)
            );
        }

        private void DrawFeaturePill(Graphics g, int x, int y, int w, string text)
        {
            Rectangle pillRect = new Rectangle(x, y, w, 28);
            using GraphicsPath path = HaaIsTheme.GetRoundPath(pillRect, 13);
            using Pen pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1.2f);
            g.DrawPath(pen, path);
            TextRenderer.DrawText(g, text, new Font("Segoe UI", 8.2F, FontStyle.Bold),
                pillRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Оюутны код болон нууц үгээ оруулна уу.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT first_name, last_name, email, major FROM students WHERE student_id = @id AND password = @pwd";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@pwd", password);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string firstName = reader.GetString("first_name");
                    string lastName = reader.GetString("last_name");
                    string email = reader.GetString("email");
                    string major = reader.GetString("major");

                    Hide();
                    using var profileForm = new StudentProfileForm(lastName, firstName, userId, email, major);
                    profileForm.ShowDialog(this);
                    Close();
                }
                else
                {
                    MessageBox.Show("Оюутны код эсвэл нууц үг буруу байна.", "LMS Нэвтрэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Өгөгдлийн сантай холбогдоход алдаа гарлаа: " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}