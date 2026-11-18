using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class LoginForm : Form
    {
        private readonly TextBox txtUserID = new();
        private readonly TextBox txtPassword = new();
        private readonly Label lblUserPrompt = new();
        private Button btnRoleStudent = null!;
        private Button btnRoleTeacher = null!;
        private bool isStudentMode = true;

        private static readonly Color LmsGreen = Color.FromArgb(14, 77, 45);
        private static readonly Color LimeAccent = Color.FromArgb(168, 224, 52);
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public LoginForm()
        {
            Text = "Сайтад нэвтрэх | LMS";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(980, 580);

            // =================================================================
            // 1. ЗҮҮН ТАЛЫН НОГООН САМБАР (Зураг 1-ийн зүүн тал яг хэвээрээ)
            // =================================================================
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 490,
                BackColor = LmsGreen
            };

            leftPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Арын том бүдэг цагираг болон долгион зураас (Зураг 1 шиг)
                using var circlePen = new Pen(Color.FromArgb(25, 255, 255, 255), 1.5f);
                g.DrawEllipse(circlePen, 90, -110, 320, 320);
                g.DrawEllipse(circlePen, 40, -160, 420, 420);

                // Доод долгионтой шугамнууд
                for (int i = 0; i < 6; i++)
                {
                    int y = 430 + i * 24;
                    g.DrawBezier(circlePen, new Point(0, y), new Point(180, y - 20), new Point(320, y + 20), new Point(490, y - 5));
                }

                // "СУРГАЛТЫН ДИЖИТАЛ ОРЧИН" дугуй хүрээтэй шошго
                Rectangle badgeRect = new Rectangle(35, 145, 240, 32);
                using (GraphicsPath badgePath = HaaIsTheme.GetRoundPath(badgeRect, 15))
                using (Pen badgePen = new Pen(Color.FromArgb(120, 255, 255, 255), 1.2f))
                {
                    g.DrawPath(badgePen, badgePath);
                }
                TextRenderer.DrawText(g, "СУРГАЛТЫН ДИЖИТАЛ ОРЧИН", new Font("Segoe UI", 9F, FontStyle.Regular),
                    badgeRect, Color.FromArgb(220, 245, 230), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // Гол гарчигнууд
                g.DrawString("ХӨДӨӨ АЖ АХУЙН ИХ СУРГУУЛИЙН", new Font("Segoe UI", 14F, FontStyle.Bold),
                    Brushes.White, new PointF(32, 198));

                // "ДАРХАН-УУЛ" (Ногоон-шар) + "АЙМАГ ДАХЬ" (Цагаан)
                g.DrawString("ДАРХАН-УУЛ", new Font("Segoe UI", 22F, FontStyle.Bold),
                    new SolidBrush(LimeAccent), new PointF(30, 232));
                g.DrawString("АЙМАГ ДАХЬ", new Font("Segoe UI", 22F, FontStyle.Bold),
                    Brushes.White, new PointF(238, 232));

                g.DrawString("САЛБАР СУРГУУЛЬ • КООСЭН", new Font("Segoe UI", 18F, FontStyle.Bold),
                    Brushes.White, new PointF(32, 276));

                // Доод 3 зууван (pill) тэмдэглэгээ: ХИЧЭЭЛ | ДААЛГАВАР | ДҮН
                DrawFeaturePill(g, 35, 345, 95, "📖 ХИЧЭЭЛ");
                DrawFeaturePill(g, 140, 345, 115, "☑ ДААЛГАВАР");
                DrawFeaturePill(g, 265, 345, 80, "📈 ДҮН");
            };

            // =================================================================
            // 2. БАРУУН ТАЛЫН ЦАГААН НЭВТРЭХ ХЭСЭГ (Зураг 1-ийн баруун тал)
            // =================================================================
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Дээд дугуй ХААИС/Коосэн лого
            var logoPanel = new Panel
            {
                Location = new Point(195, 45),
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
            rightPanel.Controls.Add(logoPanel);

            rightPanel.Controls.Add(new Label
            {
                Text = "Сургалтын удирдлагын системд нэвтрэх ↴",
                Location = new Point(95, 160),
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 90, 85),
                Font = new Font("Segoe UI", 9.5F)
            });

            // Оюутан эсвэл Багш/Админ сонгох
            btnRoleStudent = HaaIsTheme.CreateButton("Оюутан", "btnRoleStudent", 95, 190, primary: true);
            btnRoleStudent.Size = new Size(145, 36);
            btnRoleStudent.Click += (s, e) => SwitchRole(true);
            rightPanel.Controls.Add(btnRoleStudent);

            btnRoleTeacher = HaaIsTheme.CreateButton("Багш / Админ", "btnRoleTeacher", 250, 190, primary: false);
            btnRoleTeacher.Size = new Size(145, 36);
            btnRoleTeacher.Click += (s, e) => SwitchRole(false);
            rightPanel.Controls.Add(btnRoleTeacher);

            // Нэвтрэх нэр / Код болон Нууц үг
            lblUserPrompt = HaaIsTheme.CreateLabel("Оюутны код (OpenID):", 95, 240);
            rightPanel.Controls.Add(lblUserPrompt);

            HaaIsTheme.StyleTextBox(txtUserID, 95, 265, 300);
            rightPanel.Controls.Add(txtUserID);

            rightPanel.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг:", 95, 308));
            txtPassword.PasswordChar = '●';
            HaaIsTheme.StyleTextBox(txtPassword, 95, 333, 300);
            rightPanel.Controls.Add(txtPassword);

            // Нэвтрэх товч
            var btnLogin = HaaIsTheme.CreateButton("Системд нэвтрэх", "btnLogin", 95, 380, primary: true);
            btnLogin.Size = new Size(300, 42);
            btnLogin.Click += BtnLogin_Click;
            rightPanel.Controls.Add(btnLogin);

            // Зураг 1 дээрх доод тайлбар текстүүд
            rightPanel.Controls.Add(new Label
            {
                Text = "Нууц үгээ мартсан бол academic@sab.edu.mn хаягаар холбогдоно уу.",
                Location = new Point(55, 440),
                Size = new Size(380, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(110, 120, 115),
                Font = new Font("Segoe UI", 8.2F)
            });

            rightPanel.Controls.Add(new Label
            {
                Text = "Монгол ⌄",
                Location = new Point(195, 475),
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = LmsGreen,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            });

            rightPanel.Controls.Add(new Label
            {
                Text = "Күүки санах ойн мэдэгдэл",
                Location = new Point(145, 510),
                Size = new Size(200, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.2F)
            });

            Controls.Add(rightPanel);
            Controls.Add(leftPanel);
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

        private void SwitchRole(bool studentMode)
        {
            isStudentMode = studentMode;
            lblUserPrompt.Text = studentMode ? "Оюутны код (OpenID):" : "Багшийн ID эсвэл Админ нэр:";
            txtUserID.Clear();
            txtPassword.Clear();

            Control rightPanel = Controls[0];
            rightPanel.Controls.Remove(btnRoleStudent);
            rightPanel.Controls.Remove(btnRoleTeacher);

            btnRoleStudent = HaaIsTheme.CreateButton("Оюутан", "btnRoleStudent", 95, 190, primary: studentMode);
            btnRoleStudent.Size = new Size(145, 36);
            btnRoleStudent.Click += (s, e) => SwitchRole(true);

            btnRoleTeacher = HaaIsTheme.CreateButton("Багш / Админ", "btnRoleTeacher", 250, 190, primary: !studentMode);
            btnRoleTeacher.Size = new Size(145, 36);
            btnRoleTeacher.Click += (s, e) => SwitchRole(false);

            rightPanel.Controls.Add(btnRoleStudent);
            rightPanel.Controls.Add(btnRoleTeacher);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Код болон нууц үгээ оруулна уу.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isStudentMode)
            {
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
                    MessageBox.Show("Өгөгдлийн сантай холбогдоход алдаа гарлаа: " + ex.Message);
                }
            }
            else
            {
                if (userId == "admin" && password == "123")
                {
                    Hide();
                    new ManagerPanelForm().ShowDialog();
                    Close();
                    return;
                }

                try
                {
                    using var conn = new MySqlConnection(ConnectionString);
                    conn.Open();
                    string query = "SELECT full_name FROM teachers WHERE teacher_id = @id AND password = @pwd";
                    using var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@pwd", password);

                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        Hide();
                        new TeacherDashboardForm().ShowDialog();
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Багшийн ID эсвэл нууц үг буруу байна.", "LMS Нэвтрэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Өгөгдлийн сантай холбогдоход алдаа гарлаа: " + ex.Message);
                }
            }
        }
    }
}