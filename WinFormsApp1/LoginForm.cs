using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class LoginForm : Form
    {
        private readonly TextBox txtStudentID = new();
        private readonly TextBox txtPassword = new();
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public LoginForm()
        {
            Text = "ХААИС - Оюутны систем";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(720, 540);
            MinimumSize = new Size(720, 540);
            MaximumSize = new Size(720, 540);
            BackColor = HaaIsTheme.White;
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = HaaIsTheme.Green
            };
            Controls.Add(header);

            var logo = new PictureBox
            {
                Name = "picUniversityLogo",
                Size = new Size(82, 82),
                Location = new Point(48, 34),
                BackColor = HaaIsTheme.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            header.Controls.Add(logo);
            logo.Controls.Add(new Label
            {
                Text = "ХААИС",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.Green,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            });

            header.Controls.Add(new Label
            {
                Text = "Хөдөө Аж Ахуйн Их Сургууль\nОюутны Систем",
                AutoSize = false,
                Location = new Point(155, 37),
                Size = new Size(510, 75),
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            });

            var content = new Panel
            {
                Location = new Point(0, 150),
                Size = new Size(720, 390),
                BackColor = HaaIsTheme.White
            };
            Controls.Add(content);

            content.Controls.Add(HaaIsTheme.CreateLabel("Оюутны код", 160, 50));
            txtStudentID.Name = "txtStudentID";
            HaaIsTheme.StyleTextBox(txtStudentID, 160, 80, 400);
            content.Controls.Add(txtStudentID);

            content.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг", 160, 140));
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            HaaIsTheme.StyleTextBox(txtPassword, 160, 170, 400);
            content.Controls.Add(txtPassword);

            var btnLogin = HaaIsTheme.CreateButton("Нэвтрэх", "btnLogin", 160, 245, primary: true);
            btnLogin.Click += BtnLogin_Click;
            content.Controls.Add(btnLogin);

            var btnRegister = HaaIsTheme.CreateButton("Бүртгүүлэх", "btnRegister", 370, 245, primary: false);
            btnRegister.Click += BtnRegister_Click;
            content.Controls.Add(btnRegister);

            content.Controls.Add(new Label
            {
                Text = "Оюутны бүртгэл, нэвтрэх үйлчилгээ",
                AutoSize = true,
                Location = new Point(160, 315),
                ForeColor = HaaIsTheme.MutedText,
                Font = new Font("Segoe UI", 9F)
            });
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string studentId = txtStudentID.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(password))
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
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@pwd", password);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string firstName = reader.GetString("first_name");
                    string lastName = reader.GetString("last_name");
                    string email = reader.GetString("email");
                    string major = reader.GetString("major");

                    MessageBox.Show($"Тавтай морилно уу, {lastName} овогтой {firstName}!", "Амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Hide();
                    using var profileForm = new StudentProfileForm(lastName, firstName, studentId, email, major);
                    profileForm.ShowDialog(this);
                    Close();
                }
                else
                {
                    MessageBox.Show("Оюутны код эсвэл нууц үг буруу байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Өгөгдлийн сантай холбогдоход алдаа гарлаа: " + ex.Message, "Системийн алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            Hide();
            using var registerForm = new RegisterForm();
            registerForm.ShowDialog(this);
            Show();
        }
    }
}