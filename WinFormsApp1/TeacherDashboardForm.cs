using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class TeacherDashboardForm : Form
    {
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public TeacherDashboardForm()
        {
            Text = "ХААИС - Багшийн Удирдлагын Самбар (Оюутнуудын Жагсаалт)";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(700, 480);
            BackColor = HaaIsTheme.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = HaaIsTheme.Green };
            header.Controls.Add(new Label { Text = "Миний Ангийн Оюутнууд", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = HaaIsTheme.White, Font = new Font("Segoe UI", 16F, FontStyle.Bold) });
            Controls.Add(header);

            var grid = new DataGridView
            {
                Location = new Point(30, 100),
                Size = new Size(630, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            Controls.Add(grid);

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT student_id AS 'Оюутны код', last_name AS 'Овог', first_name AS 'Нэр', email AS 'Имэйл', major AS 'Мэргэжил' FROM students";
                using var adapter = new MySqlDataAdapter(query, conn);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                grid.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Өгөгдөл татахад алдаа гарлаа: " + ex.Message);
            }

            var btnClose = HaaIsTheme.CreateButton("Буцах", "btnClose", 250, 415, primary: false);
            btnClose.Click += (s, e) => Close();
            Controls.Add(btnClose);
        }
    }
}
