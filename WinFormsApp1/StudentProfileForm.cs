using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class StudentProfileForm : Form
    {
        private readonly string studentId;
        private readonly string major;
        private readonly DataGridView gridEnrolled = new();
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public StudentProfileForm(string lastName, string firstName, string studentId, string email, string major)
        {
            this.studentId = studentId;
            this.major = major;

            Text = $"ХААИС - Оюутны Кабинет ({firstName} {lastName})";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(780, 580);

            var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = HaaIsTheme.Green };
            header.Controls.Add(new Label { Text = $"Оюутны Мэдээллийн Самбар - {lastName} {firstName}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = HaaIsTheme.White, Font = new Font("Segoe UI", 16F, FontStyle.Bold) });
            Controls.Add(header);

            var tabControl = new TabControl
            {
                Location = new Point(20, 95),
                Size = new Size(730, 410),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            // TAB 1: Profile Info
            var tabProfile = new TabPage("Хувийн мэдээлэл") { BackColor = HaaIsTheme.White };
            int startY = 30;
            AddInfoRow(tabProfile, "Овог, Нэр:", $"{lastName} {firstName}", startY);
            AddInfoRow(tabProfile, "Оюутны код:", studentId, startY + 50);
            AddInfoRow(tabProfile, "Цахим шуудан:", email, startY + 100);
            AddInfoRow(tabProfile, "Мэргэжил:", major, startY + 150);
            AddInfoRow(tabProfile, "Сургууль:", "Хөдөө Аж Ахуйн Их Сургууль (MULS)", startY + 200);
            tabControl.TabPages.Add(tabProfile);

            // TAB 2: Course Selection (Хичээл сонголт)
            var tabEnroll = new TabPage("Хичээл сонголт") { BackColor = HaaIsTheme.White };

            tabEnroll.Controls.Add(new Label { Text = "Боломжтой хичээлүүдээс сонгож бүртгүүлнэ үү:", Location = new Point(20, 15), AutoSize = true, ForeColor = HaaIsTheme.Green });

            var gridAvailable = CreateGrid(20, 45, 680, 140);
            LoadAvailableCourses(gridAvailable);
            tabEnroll.Controls.Add(gridAvailable);

            var btnEnroll = HaaIsTheme.CreateButton("Сонгох (Enroll)", "btnEnroll", 20, 195, primary: true);
            btnEnroll.Size = new Size(200, 38);
            btnEnroll.Click += (s, e) => {
                if (gridAvailable.SelectedRows.Count > 0)
                {
                    string courseName = gridAvailable.SelectedRows[0].Cells["Хичээлийн нэр"].Value?.ToString() ?? "";
                    int credit = Convert.ToInt32(gridAvailable.SelectedRows[0].Cells["Кредит"].Value ?? 3);
                    EnrollCourse(courseName, credit);
                    LoadEnrolledCourses(gridEnrolled);
                }
                else
                {
                    MessageBox.Show("Жагсаалтаас сонгоно уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            tabEnroll.Controls.Add(btnEnroll);

            tabEnroll.Controls.Add(new Label
            {
                Text = "Миний сонгосон хичээлүүд:", Location = new Point(20, 240), AutoSize = true, ForeColor = HaaIsTheme.Green });


            gridEnrolled.Location = new Point(20, 270);
                gridEnrolled.Size = new Size(680, 95);
                gridEnrolled.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                gridEnrolled.ReadOnly = true;
                LoadEnrolledCourses(gridEnrolled);
                tabEnroll.Controls.Add(gridEnrolled);

                tabControl.TabPages.Add(tabEnroll);

                // TAB 3: Grades
                var tabGrades = new TabPage("Дүнгийн жагсаалт") { BackColor = HaaIsTheme.White };
                var gridGrades = CreateGrid(20, 20, 680, 330);
                LoadGradesData(gridGrades);
                tabGrades.Controls.Add(gridGrades);
                tabControl.TabPages.Add(tabGrades);

                Controls.Add(tabControl);

                var btnLogout = HaaIsTheme.CreateButton("Гарах (Log out)", "btnLogout", 290, 520, primary: false);
                btnLogout.Click += (s, e) => Close();
                Controls.Add(btnLogout);
            }

        private void AddInfoRow(TabPage tab, string labelText, string valueText, int y)
        {
            tab.Controls.Add(new Label { Text = labelText, Location = new Point(40, y), AutoSize = true, ForeColor = HaaIsTheme.Green, Font = new Font("Segoe UI", 11F, FontStyle.Bold) });
            tab.Controls.Add(new Label { Text = valueText, Location = new Point(220, y), AutoSize = true, ForeColor = Color.Black, Font = new Font("Segoe UI", 11F) });
        }

        private DataGridView CreateGrid(int x, int y, int w, int h)
        {
            return new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                BackgroundColor = Color.White
            };
        }

        private void LoadAvailableCourses(DataGridView grid)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT course_name AS 'Хичээлийн нэр', credit AS 'Кредит', department AS 'Тэнхим' FROM available_courses";
                using var adapter = new MySqlDataAdapter(query, conn);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                grid.DataSource = table;
            }
            catch { }
        }

        private void LoadEnrolledCourses(DataGridView grid)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT course_name AS 'Хичээлийн нэр', credit AS 'Кредит' FROM student_enrolled_courses WHERE student_id = @id";
                using var adapter = new MySqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@id", studentId);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                grid.DataSource = table;
            }
            catch { }
        }

        private void EnrollCourse(string courseName, int credit)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO student_enrolled_courses (student_id, course_name, credit) VALUES (@id, @course, @credit)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@course", courseName);
                cmd.Parameters.AddWithValue("@credit", credit);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Хичээлийг амжилттай сонголоо!", "ХААИС", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Энэ хичээлийг өмнө нь сонгосон байна эсвэл алдаа гарлаа: " + ex.Message, "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadGradesData(DataGridView grid)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT course_name AS 'Хичээлийн нэр', credit AS 'Кредит', score AS 'Оноо', letter_grade AS 'Үсгэн үнэлгээ' FROM student_grades WHERE student_id = @id";
                using var adapter = new MySqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@id", studentId);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                grid.DataSource = table;
            }
            catch { }
        }
    }
}