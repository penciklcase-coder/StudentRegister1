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
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public TeacherLoginForm()
        {
            Text = "Коосэн - Нэвтрэх";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(450, 400);

            var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = HaaIsTheme.Green };
            header.Controls.Add(new Label
            {
                Text = "Нэвтрэх",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });
            Controls.Add(header);

            Controls.Add(HaaIsTheme.CreateLabel("ID эсвэл Нэр", 50, 115));
            HaaIsTheme.StyleTextBox(txtTeacherID, 50, 145, 330);
            Controls.Add(txtTeacherID);

            Controls.Add(HaaIsTheme.CreateLabel("Нууц үг", 50, 195));
            HaaIsTheme.StyleTextBox(txtPassword, 50, 225, 330);
            txtPassword.PasswordChar = '●';
            Controls.Add(txtPassword);

            var btnLogin = HaaIsTheme.CreateButton("Нэвтрэх", "btnLogin", 50, 300, primary: true);
            btnLogin.Size = new Size(330, 44);
            btnLogin.Click += BtnLogin_Click;
            Controls.Add(btnLogin);
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
                MessageBox.Show("Админ эрхээр амжилттай нэвтэрлээ!", "Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
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