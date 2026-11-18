using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class TeacherLoginForm : Form
    {
        private readonly TextBox txtTeacherID = new();
        private readonly TextBox txtPassword = new();
        private readonly Panel centerCard;
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public TeacherLoginForm()
        {
            Text = "Коосэн - Багш / Админ Нэвтрэх";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(760, 520);
            MinimumSize = new Size(520, 460);
            BackColor = Color.FromArgb(245, 248, 246);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = HaaIsTheme.Green
            };
            header.Controls.Add(new Label
            {
                Text = "Багш / Админ Системд Нэвтрэх",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });

            // Голын карт (Дэлгэц томроход яг голд нь байрлана)
            centerCard = HaaIsTheme.CreateCard(165, 115, 430, 340);

            centerCard.Controls.Add(HaaIsTheme.CreateLabel("ID эсвэл Нэр:", 50, 35));
            HaaIsTheme.StyleTextBox(txtTeacherID, 50, 62, 330);
            centerCard.Controls.Add(txtTeacherID);

            centerCard.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг:", 50, 120));
            HaaIsTheme.StyleTextBox(txtPassword, 50, 147, 330);
            txtPassword.PasswordChar = '●';
            centerCard.Controls.Add(txtPassword);

            var btnLogin = HaaIsTheme.CreateButton("Нэвтрэх", "btnLogin", 50, 215, primary: true);
            btnLogin.Size = new Size(330, 44);
            btnLogin.Click += BtnLogin_Click;
            centerCard.Controls.Add(btnLogin);

            Controls.Add(centerCard);
            Controls.Add(header);

            Resize += (s, e) => CenterLoginCard();
            Load += (s, e) => CenterLoginCard();
        }

        private void CenterLoginCard()
        {
            centerCard.Location = new Point(
                Math.Max(20, (ClientSize.Width - centerCard.Width) / 2),
                Math.Max(95, 80 + (ClientSize.Height - 80 - centerCard.Height) / 2)
            );
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string user = txtTeacherID.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("ID болон нууц үгээ оруулна уу.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (user == "admin" && pass == "123")
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
                cmd.Parameters.AddWithValue("@id", user);
                cmd.Parameters.AddWithValue("@pwd", pass);

                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    MessageBox.Show($"Тавтай морилно уу, {result} багш аа!", "Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Hide();
                    new TeacherDashboardForm().ShowDialog();
                    Close();
                }
                else
                {
                    MessageBox.Show("ID эсвэл нууц үг буруу байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Өгөгдлийн сантай холбогдоход алдаа гарлаа: " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}