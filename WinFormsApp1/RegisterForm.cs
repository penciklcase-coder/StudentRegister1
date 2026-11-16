using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Mail;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class RegisterForm : Form
    {
        private readonly TextBox txtLastName = new();
        private readonly TextBox txtFirstName = new();
        private readonly TextBox txtStudentID = new();
        private readonly TextBox txtEmail = new();
        private readonly ComboBox cboMajor = new();
        private readonly TextBox txtPassword = new();
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public RegisterForm()
        {
            Text = "Шинэ Оюутны Бүртгэл";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(640, 740);
            MinimumSize = new Size(640, 740);
            MaximumSize = new Size(640, 740);
            BackColor = HaaIsTheme.White;
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 115,
                BackColor = HaaIsTheme.Green
            };
            header.Controls.Add(new Label
            {
                Text = "Шинэ Оюутны Бүртгэл",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold)
            });
            Controls.Add(header);

            AddTextField("Овог", txtLastName, "txtLastName", 140);
            AddTextField("Нэр", txtFirstName, "txtFirstName", 215);
            AddTextField("Оюутны код", txtStudentID, "txtStudentID", 290);
            AddTextField("Имэйл", txtEmail, "txtEmail", 365);

            Controls.Add(HaaIsTheme.CreateLabel("Мэргэжил", 100, 440));
            cboMajor.Name = "cboMajor";
            cboMajor.Location = new Point(100, 467);
            cboMajor.Size = new Size(440, 34);
            cboMajor.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMajor.Font = new Font("Segoe UI", 11F);
            cboMajor.Items.AddRange(new object[] { "Агроэкологи", "Эдийн засаг", "Мал эмнэлэг", "Инженер" });
            Controls.Add(cboMajor);

            AddTextField("Нууц үг", txtPassword, "txtPassword", 515);
            txtPassword.PasswordChar = '●';

            var btnRegister = HaaIsTheme.CreateButton("Бүртгүүлэх", "btnRegister", 100, 600, primary: true);
            btnRegister.Click += BtnRegister_Click;
            Controls.Add(btnRegister);

            var btnBack = HaaIsTheme.CreateButton("Буцах", "btnBack", 350, 600, primary: false);
            btnBack.Click += BtnBack_Click;
            Controls.Add(btnBack);
        }

        private void AddTextField(string label, TextBox textBox, string name, int y)
        {
            Controls.Add(HaaIsTheme.CreateLabel(label, 100, y));
            textBox.Name = name;
            HaaIsTheme.StyleTextBox(textBox, 100, y + 27, 440);
            Controls.Add(textBox);
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtStudentID.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                cboMajor.SelectedIndex < 0 ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Бүх талбарыг бөглөж, мэргэжлээ сонгоно уу.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _ = new MailAddress(txtEmail.Text.Trim());
            }
            catch (FormatException)
            {
                MessageBox.Show("Имэйл хаягаа зөв оруулна уу.", "Имэйл буруу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                string query = "INSERT INTO students (last_name, first_name, student_id, email, major, password) VALUES (@last, @first, @id, @email, @major, @pwd)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@last", txtLastName.Text.Trim());
                cmd.Parameters.AddWithValue("@first", txtFirstName.Text.Trim());
                cmd.Parameters.AddWithValue("@id", txtStudentID.Text.Trim());
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@major", cboMajor.SelectedItem?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@pwd", txtPassword.Text.Trim());

                cmd.ExecuteNonQuery();

                MessageBox.Show("Бүртгэл амжилттай хадгалагдлаа! Одоо нэвтэрч орно уу.", "ХААИС", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Бүртгэх явцад алдаа гарлаа (Оюутны код давхардсан байж магадгүй): " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}