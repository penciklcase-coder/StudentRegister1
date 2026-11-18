using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Mail;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class RegisterForm : Form
    {
        private readonly TextBox txtLastName = new TextBox();
        private readonly TextBox txtFirstName = new TextBox();
        private readonly TextBox txtStudentID = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly ComboBox cboMajor = new ComboBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly List<Label> jobBubbles = new List<Label>();
        private string selectedMajor = "";
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public RegisterForm()
        {
            Text = "КООСЭН - Шинэ Оюутны Бүртгэл";
            HaaIsTheme.StyleForm(this);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(700, 650);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = HaaIsTheme.Green
            };
            header.Controls.Add(new Label
            {
                Text = "Дархан Коосэн - Шинэ Оюутны Бүртгэл",
                Location = new Point(40, 18),
                AutoSize = true,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold)
            });
            header.Controls.Add(new Label
            {
                Text = "Мэдээллээ бөглөж, инженерийн мэргэжлийн чиглэлээ дугуй товчоор сонгоно уу",
                Location = new Point(42, 54),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 225, 215),
                Font = new Font("Segoe UI", 9.5F)
            });
            Controls.Add(header);

            var card = HaaIsTheme.CreateCard(35, 110, 630, 510, 18);
            Controls.Add(card);

            card.Controls.Add(HaaIsTheme.CreateLabel("Овог", 35, 22));
            txtLastName.Name = "txtLastName";
            card.Controls.Add(HaaIsTheme.CreateStyledInput(txtLastName, 35, 48, 265, "Овог оруулах"));

            card.Controls.Add(HaaIsTheme.CreateLabel("Нэр", 330, 22));
            txtFirstName.Name = "txtFirstName";
            card.Controls.Add(HaaIsTheme.CreateStyledInput(txtFirstName, 330, 48, 265, "Нэр оруулах"));

            card.Controls.Add(HaaIsTheme.CreateLabel("Оюутны код", 35, 102));
            txtStudentID.Name = "txtStudentID";
            card.Controls.Add(HaaIsTheme.CreateStyledInput(txtStudentID, 35, 128, 265, "Жишээ: K2026001"));

            card.Controls.Add(HaaIsTheme.CreateLabel("Имэйл хаяг", 330, 102));
            txtEmail.Name = "txtEmail";
            card.Controls.Add(HaaIsTheme.CreateStyledInput(txtEmail, 330, 128, 265, "student@kosen.edu.mn"));

            // ЗӨВХӨН 3 Инженерийн мэргэжлийн дугуй Bubble сонголт
            card.Controls.Add(HaaIsTheme.CreateLabel("Инженерийн мэргэжил сонгох (Дарж идэвхжүүлнэ үү):", 35, 185));

            string[] jobDisplays = { "⚡ Цахилгааны инженер", "⚙️ Механик инженер", "💻 МТ-ийн инженер (IT)" };
            string[] jobValues = { "Цахилгааны инженер", "Механик инженер", "Мэдээллийн технологийн инженер" };

            cboMajor.Visible = false;
            foreach (var val in jobValues) cboMajor.Items.Add(val);
            card.Controls.Add(cboMajor);

            int bx = 35;
            for (int i = 0; i < jobDisplays.Length; i++)
            {
                string display = jobDisplays[i];
                string val = jobValues[i];

                var pill = HaaIsTheme.CreateBubbleOption(display, bx, 218, 180, 42, false);
                pill.Click += (s, e) =>
                {
                    foreach (var b in jobBubbles) HaaIsTheme.SetBubbleState(b, false);
                    HaaIsTheme.SetBubbleState(pill, true);
                    selectedMajor = val;
                    cboMajor.SelectedItem = val;
                };
                card.Controls.Add(pill);
                jobBubbles.Add(pill);
                bx += 190;
            }

            card.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг зохиох", 35, 285));
            txtPassword.Name = "txtPassword";
            card.Controls.Add(HaaIsTheme.CreateStyledInput(txtPassword, 35, 312, 560, "Хамгийн багадаа 4 тэмдэгт", isPassword: true));

            var btnRegister = HaaIsTheme.CreateButton("Бүртгүүлэх", "btnRegister", 35, 395, primary: true);
            btnRegister.Size = new Size(270, 46);
            btnRegister.Click += (s, e) => PerformRegistration();
            card.Controls.Add(btnRegister);

            var btnBack = HaaIsTheme.CreateButton("Буцах", "btnBack", 325, 395, primary: false);
            btnBack.Size = new Size(270, 46);
            btnBack.Click += (s, e) => Close();
            card.Controls.Add(btnBack);
        }

        private void PerformRegistration()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtStudentID.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(selectedMajor) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Бүх талбарыг бөглөж, инженерийн мэргэжлийн дугуй товчоос сонгоно уу.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var addr = new MailAddress(txtEmail.Text.Trim());
            }
            catch (FormatException)
            {
                MessageBox.Show("Имэйл хаягаа зөв оруулна уу.", "Имэйл буруу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    string query = "INSERT INTO students (last_name, first_name, student_id, email, major, password) VALUES (@last, @first, @id, @email, @major, @pwd)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@last", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@first", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@id", txtStudentID.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@major", selectedMajor);
                        cmd.Parameters.AddWithValue("@pwd", txtPassword.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Бүртгэл амжилттай хадгалагдлаа! Одоо нэвтэрч орно уу.", "Дархан Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Бүртгэх явцад алдаа гарлаа (Оюутны код давхардсан байж магадгүй): " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}