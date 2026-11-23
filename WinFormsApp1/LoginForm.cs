#nullable enable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
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
        private Button? btnLogin;

        private static readonly Color LmsGreen = Color.FromArgb(14, 77, 45);
        private static readonly Color LimeAccent = Color.FromArgb(168, 224, 52);

        // АНХААР: root + хоосон нууц үг нь зөвхөн хөгжүүлэлтэд тохиромжтой.
        // Бодит ашиглалтад тусдаа хязгаарлагдсан эрхтэй DB хэрэглэгч үүсгээд
        // app.config / орчны хувьсагчаас унш.
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        // Фонтуудыг нэг удаа үүсгэнэ (Paint бүрд шинээр үүсгэхгүй)
        private static readonly Font FontBadge = new("Segoe UI", 9F, FontStyle.Regular);
        private static readonly Font FontHeading1 = new("Segoe UI", 14.5F, FontStyle.Bold);
        private static readonly Font FontHeading2 = new("Segoe UI", 22F, FontStyle.Bold);
        private static readonly Font FontHeading3 = new("Segoe UI", 18F, FontStyle.Bold);
        private static readonly Font FontPill = new("Segoe UI", 8.2F, FontStyle.Bold);
        private static readonly Font FontLogo = new("Segoe UI", 9.5F, FontStyle.Bold);
        private static readonly Font FontNote = new("Segoe UI", 9.5F);
        private static readonly Font FontSmall = new("Segoe UI", 8.2F);
        private static readonly Font FontAdmin = new("Segoe UI", 8.8F, FontStyle.Bold);

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
            HaaIsTheme.SetDoubleBuffered(leftPanel); // resize үеийн анивчилтыг арилгана
            leftPanel.Resize += (s, e) => leftPanel.Invalidate();
            leftPanel.Paint += LeftPanel_Paint;

            // 2. БАРУУН ТАЛЫН ЦАГААН САМБАР (50%)
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.White;

            // Хамгийн бага цонхонд (клиент ~520px) багтахын тулд 500 болгов
            loginBox.Size = new Size(420, 500);
            loginBox.BackColor = Color.White;

            var logoPanel = new Panel
            {
                Location = new Point(160, 10),
                Size = new Size(100, 100),
                BackColor = Color.White
            };
            logoPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen1 = new Pen(LmsGreen, 3.5f);
                using var pen2 = new Pen(LmsGreen, 1.5f);
                g.DrawEllipse(pen1, 5, 5, 88, 88);
                g.DrawEllipse(pen2, 15, 15, 68, 68);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var brush = new SolidBrush(LmsGreen);
                g.DrawString("ХААИС\nКООСЭН", FontLogo, brush, new RectangleF(15, 15, 68, 68), sf);
            };
            loginBox.Controls.Add(logoPanel);

            loginBox.Controls.Add(new Label
            {
                Text = "Сургалтын удирдлагын системд нэвтрэх ↴",
                Location = new Point(50, 122),
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 90, 85),
                Font = FontNote
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
                try { tLogin.ShowDialog(); }
                finally { Show(); Activate(); }
            };
            loginBox.Controls.Add(btnTeacherPortal);

            loginBox.Controls.Add(HaaIsTheme.CreateLabel("Оюутны код (OpenID):", 50, 208));
            HaaIsTheme.StyleTextBox(txtUserID, 50, 233, 320);
            loginBox.Controls.Add(txtUserID);

            loginBox.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг:", 50, 278));
            txtPassword.PasswordChar = '●';
            HaaIsTheme.StyleTextBox(txtPassword, 50, 303, 320);
            loginBox.Controls.Add(txtPassword);

            btnLogin = HaaIsTheme.CreateButton("Системд нэвтрэх", "btnLogin", 50, 355, primary: true);
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
                Font = FontSmall
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
                Font = FontAdmin,
                Cursor = Cursors.Hand
            };
            btnAdminPortal.FlatAppearance.BorderSize = 0;
            btnAdminPortal.Click += (s, e) =>
            {
                using var adminLogin = new ManagerLoginForm();
                Hide();
                try { adminLogin.ShowDialog(); }
                finally { Show(); Activate(); }
            };
            loginBox.Controls.Add(btnAdminPortal);

            rightPanel.Controls.Add(loginBox);

            // Enter дарахад нэвтэрнэ
            AcceptButton = btnLogin;

            Resize += (s, e) => AdjustResponsiveLayout();
            Load += (s, e) =>
            {
                AdjustResponsiveLayout();
                txtUserID.Focus();
            };

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

        private void LeftPanel_Paint(object? sender, PaintEventArgs e)
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
            TextRenderer.DrawText(g, "СУРГАЛТЫН ДИЖИТАЛ ОРЧИН", FontBadge,
                badgeRect, Color.FromArgb(220, 245, 230), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            g.DrawString("ХӨДӨӨ АЖ АХУЙН ИХ СУРГУУЛИЙН", FontHeading1,
                Brushes.White, new PointF(baseX - 3, baseY + 52));

            // "ДАРХАН-УУЛ" ба "АЙМАГ ДАХЬ"-ийн хоорондох зайг хатуу тоогоор биш, текстийн өргөнөөр тооцно
            using var fmt = StringFormat.GenericTypographic;
            float limeWidth = g.MeasureString("ДАРХАН-УУЛ", FontHeading2, PointF.Empty, fmt).Width;
            using (var limeBrush = new SolidBrush(LimeAccent))
            {
                g.DrawString("ДАРХАН-УУЛ", FontHeading2, limeBrush, new PointF(baseX - 5, baseY + 86), fmt);
            }
            g.DrawString("АЙМАГ ДАХЬ", FontHeading2, Brushes.White,
                new PointF(baseX - 5 + limeWidth + 14, baseY + 86), fmt);

            g.DrawString("САЛБАР СУРГУУЛЬ • КООСЭН", FontHeading3,
                Brushes.White, new PointF(baseX - 3, baseY + 132));

            DrawFeaturePill(g, baseX, baseY + 200, 95, "📖 ХИЧЭЭЛ");
            DrawFeaturePill(g, baseX + 105, baseY + 200, 115, "☑ ДААЛГАВАР");
            DrawFeaturePill(g, baseX + 230, baseY + 200, 80, "📈 ДҮН");
        }

        private static void DrawFeaturePill(Graphics g, int x, int y, int w, string text)
        {
            Rectangle pillRect = new Rectangle(x, y, w, 28);
            using GraphicsPath path = HaaIsTheme.GetRoundPath(pillRect, 13);
            using Pen pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1.2f);
            g.DrawPath(pen, path);
            TextRenderer.DrawText(g, text, FontPill,
                pillRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static string ReadStr(MySqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? "" : r.GetString(i);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string password = txtPassword.Text; // нууц үгийг Trim хийхгүй (зайтай нууц үг зөв ажиллана)

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Оюутны код болон нууц үгээ оруулна уу.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string firstName, lastName, email, major, storedPassword;

            try
            {
                // Холболтыг зөвхөн өгөгдөл унших хүртэл нээлттэй байлгана.
                // (Өмнө нь profile цонх нээлттэй байх бүх хугацаанд DB холболт хаагдахгүй байсан)
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    const string query =
                        "SELECT first_name, last_name, email, major, password FROM students WHERE student_id = @id LIMIT 1";
                    using var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", userId);

                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        FailLogin();
                        return;
                    }

                    firstName = ReadStr(reader, "first_name");
                    lastName = ReadStr(reader, "last_name");
                    email = ReadStr(reader, "email");
                    major = ReadStr(reader, "major");
                    storedPassword = ReadStr(reader, "password");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Өгөгдлийн сантай холбогдоход алдаа гарлаа: " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Алдаа гарлаа: " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!PasswordHasher.Verify(password, storedPassword))
            {
                FailLogin();
                return;
            }

            Hide();
            using var profileForm = new StudentProfileForm(lastName, firstName, userId, email, major);
            profileForm.ShowDialog(this);
            Close();
        }

        private void FailLogin()
        {
            MessageBox.Show("Оюутны код эсвэл нууц үг буруу байна.", "LMS Нэвтрэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }

    /// <summary>
    /// Нууц үгийн шалгалт. DB-д "PBKDF2$..." форматтай hash байвал hash-аар шалгана.
    /// Хуучин plain-text нууц үг байвал (шилжилтийн хугацаанд) тогтмол хугацаатай харьцуулна.
    /// Hash() ашиглан students.password баганыг шилжүүлнэ (багана VARCHAR(255) байх ёстой).
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            if (stored.StartsWith(Prefix + "$", StringComparison.Ordinal))
            {
                var parts = stored.Split('$');
                if (parts.Length != 4 || !int.TryParse(parts[1], out int iter)) return false;
                try
                {
                    byte[] salt = Convert.FromBase64String(parts[2]);
                    byte[] expected = Convert.FromBase64String(parts[3]);
                    byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, expected.Length);
                    return CryptographicOperations.FixedTimeEquals(actual, expected);
                }
                catch (FormatException) { return false; }
            }

            // Хуучин plain-text
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(stored));
        }
    }
}