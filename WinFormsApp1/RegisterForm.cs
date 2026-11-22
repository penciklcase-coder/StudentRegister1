using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class RegisterForm : Form
    {
        private readonly TextBox txtLastName = new();
        private readonly TextBox txtFirstName = new();
        private readonly TextBox txtStudentId = new();
        private readonly TextBox txtEmail = new();
        private readonly ComboBox cmbMajor = new();
        private readonly TextBox txtPassword = new();
        private readonly Panel centerCard;

        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public RegisterForm()
        {
            Text = "Коосэн - Шинэ оюутан бүртгэх";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(760, 620);
            MinimumSize = new Size(540, 560);
            BackColor = Color.FromArgb(245, 248, 246);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = HaaIsTheme.Green
            };
            header.Controls.Add(new Label
            {
                Text = "Шинэ Оюутны Бүртгэл Үүсгэх",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });

            // Голын бүртгэлийн карт
            centerCard = HaaIsTheme.CreateCard(140, 95, 480, 480);

            int x = 45;
            int w = 390;

            // Овог & Нэр (нэг мөрөнд зэрэгцээ)
            centerCard.Controls.Add(HaaIsTheme.CreateLabel("Овог:", x, 25));
            HaaIsTheme.StyleTextBox(txtLastName, x, 50, 185);
            centerCard.Controls.Add(txtLastName);

            centerCard.Controls.Add(HaaIsTheme.CreateLabel("Нэр:", x + 205, 25));
            HaaIsTheme.StyleTextBox(txtFirstName, x + 205, 50, 185);
            centerCard.Controls.Add(txtFirstName);

            // Оюутны код
            centerCard.Controls.Add(HaaIsTheme.CreateLabel("Оюутны код (Нэвтрэх ID):", x, 98));
            HaaIsTheme.StyleTextBox(txtStudentId, x, 123, w);
            centerCard.Controls.Add(txtStudentId);

            // Цахим шуудан
            centerCard.Controls.Add(HaaIsTheme.CreateLabel("Цахим шуудан (Имэйл):", x, 170));
            HaaIsTheme.StyleTextBox(txtEmail, x, 195, w);
            centerCard.Controls.Add(txtEmail);

            // Мэргэжил
            centerCard.Controls.Add(HaaIsTheme.CreateLabel("Мэргэжил / Анги:", x, 242));
            cmbMajor.Location = new Point(x, 267);
            cmbMajor.Width = w;
            cmbMajor.Font = new Font("Segoe UI", 10.5F);
            cmbMajor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMajor.Items.AddRange(new object[]
            {
                "Мэдээллийн технологи",
                "Програм хангамж",
                "Цахилгаан, электроник",
                "Механик инженерчлэл",
                "Барилгын инженерчлэл",
                "Био-инженерчлэл"
            });
            cmbMajor.SelectedIndex = 0;
            centerCard.Controls.Add(cmbMajor);

            // Нууц үг
            centerCard.Controls.Add(HaaIsTheme.CreateLabel("Анхны нууц үг:", x, 314));
            HaaIsTheme.StyleTextBox(txtPassword, x, 339, w);
            centerCard.Controls.Add(txtPassword);

            // Хадгалах ба Цуцлах товч
            var btnSave = HaaIsTheme.CreateButton("Бүртгэл хадгалах", "btnSave", x, 398, primary: true);
            btnSave.Size = new Size(235, 44);
            btnSave.Click += BtnSave_Click;
            centerCard.Controls.Add(btnSave);

            var btnCancel = HaaIsTheme.CreateButton("Цуцлах", "btnCancel", x + 250, 398, primary: false);
            btnCancel.Size = new Size(140, 44);
            btnCancel.Click += (s, e) => Close();
            centerCard.Controls.Add(btnCancel);

            Controls.Add(centerCard);
            Controls.Add(header);

            Resize += (s, e) => CenterCard();
            Load += (s, e) => CenterCard();
        }

        private void CenterCard()
        {
            centerCard.Location = new Point(
                Math.Max(20, (ClientSize.Width - centerCard.Width) / 2),
                Math.Max(90, 75 + (ClientSize.Height - 75 - centerCard.Height) / 2)
            );
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtStudentId.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Оюутны бүх мэдээллийг бүрэн бөглөнө үү.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                string query = @"INSERT INTO students (last_name, first_name, student_id, email, major, password) 
                                 VALUES (@last, @first, @id, @email, @major, @pwd)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@last", txtLastName.Text.Trim());
                cmd.Parameters.AddWithValue("@first", txtFirstName.Text.Trim());
                cmd.Parameters.AddWithValue("@id", txtStudentId.Text.Trim());
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@major", cmbMajor.SelectedItem?.ToString() ?? "Мэдээллийн технологи");
                cmd.Parameters.AddWithValue("@pwd", txtPassword.Text.Trim());
                cmd.ExecuteNonQuery();

                MessageBox.Show("Шинэ оюутны бүртгэл амжилттай үүслээ!", "Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Бүртгэхэд алдаа гарлаа (Оюутны код давхардсан байж магадгүй): " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}