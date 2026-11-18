using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class LoginForm : Form
    {
        private readonly TextBox txtStudentID = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly Label lblIdTitle;
        private readonly Button btnLogin;
        private readonly Button btnRegister;
        private readonly CheckBox chkRemember = new CheckBox();
        private readonly List<Label> roleBubbles = new List<Label>();
        private string selectedRole = "Оюутан";
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public LoginForm()
        {
            Text = "КООСЭН - Системд Нэвтрэх";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(980, 600);
            BackColor = Color.White;

            // =========================================================================
            // 1. ЗҮҮН ТАЛЫН НОГООН БАННЕР
            // =========================================================================
            var leftHero = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(490, 600),
                BackColor = Color.FromArgb(14, 58, 36)
            };

            leftHero.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var circleBrush = new SolidBrush(Color.FromArgb(20, 72, 46)))
                {
                    g.FillEllipse(circleBrush, 270, -70, 260, 260);
                }

                using (var ringPen = new Pen(Color.FromArgb(30, 92, 58), 1.5f))
                {
                    g.DrawEllipse(ringPen, 220, -120, 360, 360);
                    g.DrawEllipse(ringPen, 165, -175, 470, 470);
                    g.DrawEllipse(ringPen, 110, -230, 580, 580);
                }

                using (var wavePen = new Pen(Color.FromArgb(24, 80, 50), 1.2f))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        int yOff = 420 + i * 28;
                        g.DrawBezier(
                            wavePen,
                            new Point(0, yOff),
                            new Point(170, yOff - 18),
                            new Point(320, yOff + 18),
                            new Point(490, yOff - 8)
                        );
                    }
                }

                var badgeRect = new Rectangle(38, 150, 245, 34);
                using (var badgePath = HaaIsTheme.GetRoundPath(badgeRect, 17))
                using (var badgePen = new Pen(Color.FromArgb(132, 204, 22), 1.4f))
                {
                    g.DrawPath(badgePen, badgePath);
                }

                TextRenderer.DrawText(
                    g,
                    "СУРГАЛТЫН ДИЖИТАЛ ОРЧИН",
                    new Font("Segoe UI", 9.5F, FontStyle.Regular),
                    badgeRect,
                    Color.FromArgb(163, 230, 53),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );

                using (var subFont = new Font("Segoe UI", 13.5F, FontStyle.Bold))
                using (var subBrush = new SolidBrush(Color.FromArgb(205, 222, 212)))
                {
                    g.DrawString("ДАРХАН-УУЛ АЙМАГ ДАХЬ", subFont, subBrush, new PointF(36, 205));
                }

                using (var highlightFont = new Font("Segoe UI", 24F, FontStyle.Bold))
                using (var limeBrush = new SolidBrush(Color.FromArgb(154, 230, 48)))
                {
                    g.DrawString("ДАРХАН КООСЭН", highlightFont, limeBrush, new PointF(34, 238));
                }

                using (var whiteTitleFont = new Font("Segoe UI", 21F, FontStyle.Bold))
                {
                    g.DrawString("ТЕХНОЛОГИЙН КОЛЛЕЖ", whiteTitleFont, Brushes.White, new PointF(35, 286));
                }

                string[] pills = { "📖 ХИЧЭЭЛ", "☑️ ДААЛГАВАР", "📈 ДҮН" };
                int px = 38;
                foreach (var pText in pills)
                {
                    int pw = pText.Length > 10 ? 128 : 102;
                    var pRect = new Rectangle(px, 355, pw, 32);
                    using (var pPath = HaaIsTheme.GetRoundPath(pRect, 16))
                    using (var pFill = new SolidBrush(Color.FromArgb(18, 68, 42)))
                    using (var pBorder = new Pen(Color.FromArgb(115, 185, 35), 1.2f))
                    {
                        g.FillPath(pFill, pPath);
                        g.DrawPath(pBorder, pPath);
                    }

                    TextRenderer.DrawText(
                        g,
                        pText,
                        new Font("Segoe UI", 8.5F, FontStyle.Bold),
                        pRect,
                        Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                    );
                    px += pw + 12;
                }
            };
            Controls.Add(leftHero);

            // =========================================================================
            // 2. БАРУУН ТАЛЫН НЭВТРЭХ ФОРМ
            // =========================================================================
            var rightPanel = new Panel
            {
                Location = new Point(490, 0),
                Size = new Size(490, 600),
                BackColor = Color.White
            };
            Controls.Add(rightPanel);

            var emblem = new Panel
            {
                Location = new Point(197, 26),
                Size = new Size(96, 96),
                BackColor = Color.White
            };
            emblem.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var outerPen = new Pen(HaaIsTheme.Green, 3f))
                using (var innerPen = new Pen(Color.FromArgb(132, 204, 22), 1.8f))
                using (var fillBrush = new SolidBrush(Color.FromArgb(242, 249, 245)))
                {
                    g.FillEllipse(fillBrush, 3, 3, 89, 89);
                    g.DrawEllipse(outerPen, 3, 3, 89, 89);
                    g.DrawEllipse(innerPen, 10, 10, 75, 75);
                }

                TextRenderer.DrawText(
                    g,
                    "ДАРХАН\nКООСЭН",
                    new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    new Rectangle(0, 0, 96, 96),
                    HaaIsTheme.Green,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            };
            rightPanel.Controls.Add(emblem);

            var lblSystemTitle = new Label
            {
                Text = "Сургалтын удирдлагын системд нэвтрэх ↳",
                AutoSize = false,
                Location = new Point(45, 130),
                Size = new Size(400, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 68, 62)
            };
            rightPanel.Controls.Add(lblSystemTitle);

            // Оюутан болон Багш дугуй Bubble сонголт
            string[] roles = { "👨‍🎓 Оюутан", "👨‍🏫 Багш" };
            int bx = 65;
            foreach (var role in roles)
            {
                bool isDefault = role.Contains("Оюутан");
                var bubble = HaaIsTheme.CreateBubbleOption(role, bx, 164, 172, 36, isDefault);
                string currentRole = role;
                bubble.Click += (s, e) => SelectRoleBubble(bubble, currentRole);
                rightPanel.Controls.Add(bubble);
                roleBubbles.Add(bubble);
                bx += 186;
            }

            lblIdTitle = HaaIsTheme.CreateLabel("Оюутны код", 65, 214);
            rightPanel.Controls.Add(lblIdTitle);

            txtStudentID.Name = "txtStudentID";
            rightPanel.Controls.Add(HaaIsTheme.CreateStyledInput(txtStudentID, 65, 238, 360, "Оюутны код"));

            rightPanel.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг", 65, 290));

            var lblForgot = new Label
            {
                Text = "Нууц үг мартсан?",
                AutoSize = true,
                Location = new Point(322, 292),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = HaaIsTheme.MutedText,
                Cursor = Cursors.Hand
            };
            lblForgot.Click += (s, e) => MessageBox.Show("Нууц үгээ сэргээлгэх бол сургалтын албанд (academic@kosen.edu.mn) хандана уу.", "Дархан Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
            rightPanel.Controls.Add(lblForgot);

            txtPassword.Name = "txtPassword";
            rightPanel.Controls.Add(HaaIsTheme.CreateStyledInput(txtPassword, 65, 314, 360, "Нууц үг оруулах", isPassword: true));

            chkRemember.Text = "Бүртгэл санах";
            chkRemember.AutoSize = true;
            chkRemember.Location = new Point(65, 365);
            chkRemember.Font = new Font("Segoe UI", 9F);
            chkRemember.ForeColor = Color.FromArgb(75, 85, 80);
            rightPanel.Controls.Add(chkRemember);

            btnLogin = HaaIsTheme.CreateButton("⇥  Нэвтрэх", "btnLogin", 65, 398, primary: true);
            btnLogin.Size = new Size(360, 42);
            HaaIsTheme.ApplyRoundedCorners(btnLogin, 8);
            btnLogin.Click += (s, e) => PerformLogin();
            rightPanel.Controls.Add(btnLogin);

            btnRegister = HaaIsTheme.CreateButton("📝  Шинэ оюутны бүртгэл", "btnRegister", 65, 450, primary: false);
            btnRegister.Size = new Size(360, 42);
            HaaIsTheme.ApplyRoundedCorners(btnRegister, 8);
            btnRegister.Click += (s, e) =>
            {
                Hide();
                using (var registerForm = new RegisterForm())
                {
                    registerForm.ShowDialog(this);
                }
                Show();
            };
            rightPanel.Controls.Add(btnRegister);

            var lblBack = new Label
            {
                Text = "← Нүүр хуудас руу буцах",
                AutoSize = false,
                Location = new Point(65, 508),
                Size = new Size(360, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.Green,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            lblBack.Click += (s, e) => Close();
            rightPanel.Controls.Add(lblBack);

            var lblCopyright = new Label
            {
                Text = "Дархан Коосэн Технологийн Коллеж ©2026",
                AutoSize = false,
                Location = new Point(65, 558),
                Size = new Size(360, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(150, 160, 155),
                Font = new Font("Segoe UI", 8.5F)
            };
            rightPanel.Controls.Add(lblCopyright);
        }

        private void SelectRoleBubble(Label clickedBubble, string roleText)
        {
            foreach (var b in roleBubbles)
            {
                HaaIsTheme.SetBubbleState(b, b == clickedBubble);
            }

            if (roleText.Contains("Оюутан"))
            {
                selectedRole = "Оюутан";
                lblIdTitle.Text = "Оюутны код";
                txtStudentID.PlaceholderText = "Оюутны код";
                btnRegister.Visible = true;
            }
            else
            {
                selectedRole = "Багш";
                lblIdTitle.Text = "Багшийн ID код";
                txtStudentID.PlaceholderText = "Багшийн код оруулах";
                btnRegister.Visible = false;
            }
        }

        private void PerformLogin()
        {
            string userId = txtStudentID.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Нэвтрэх код болон нууц үгээ оруулна уу.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Хэрэв "Багш" хэсгийг сонгосон бол (Админ мөн эндээс нэвтэрнэ)
            if (selectedRole == "Багш")
            {
                if (userId == "admin" && password == "123")
                {
                    MessageBox.Show("Систем админ эрхээр амжилттай нэвтэрлээ!", "Дархан Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Hide();
                    using (var mgr = new ManagerPanelForm())
                    {
                        mgr.ShowDialog(this);
                    }
                    Close();
                    return;
                }

                try
                {
                    using (var conn = new MySqlConnection(ConnectionString))
                    {
                        conn.Open();
                        string tQuery = "SELECT full_name FROM teachers WHERE teacher_id = @id AND password = @pwd";
                        using (var cmd = new MySqlCommand(tQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.Parameters.AddWithValue("@pwd", password);
                            var result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                MessageBox.Show($"Тавтай морилно уу, {result} багш аа!", "Дархан Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Hide();
                                using (var tDash = new TeacherDashboardForm())
                                {
                                    tDash.ShowDialog(this);
                                }
                                Close();
                            }
                            else
                            {
                                MessageBox.Show("Багшийн ID эсвэл нууц үг буруу байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Өгөгдлийн сантай холбогдоход алдаа гарлаа: " + ex.Message, "Системийн алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // 2. Оюутан нэвтрэх
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT first_name, last_name, email, major FROM students WHERE student_id = @id AND password = @pwd";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.Parameters.AddWithValue("@pwd", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader.GetString("first_name");
                                string lastName = reader.GetString("last_name");
                                string email = reader.GetString("email");
                                string major = reader.GetString("major");

                                MessageBox.Show($"Тавтай морилно уу, {lastName} овогтой {firstName}!", "Дархан Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                Hide();
                                using (var profileForm = new StudentProfileForm(lastName, firstName, userId, email, major))
                                {
                                    profileForm.ShowDialog(this);
                                }
                                Close();
                            }
                            else
                            {
                                MessageBox.Show("Оюутны код эсвэл нууц үг буруу байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Өгөгдлийн сантай холбогдоход алдаа гарлаа: " + ex.Message, "Системийн алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}