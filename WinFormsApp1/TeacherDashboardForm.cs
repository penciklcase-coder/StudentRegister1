using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class TeacherDashboardForm : Form
    {
        private readonly Panel mainWorkspace = new();
        private readonly Dictionary<string, Label> navButtons = new();
        private Label lblPageTitle = new();

        // Дүн оруулах талбарууд
        private readonly TextBox txtGradeStuId = new();
        private readonly TextBox txtGradeCourse = new();
        private readonly TextBox txtGradeCredit = new();
        private readonly TextBox txtGradeScore = new();
        private readonly DataGridView gridGrades = new();

        // Хичээл нэмэх талбарууд
        private readonly TextBox txtCourseName = new();
        private readonly TextBox txtCourseCredit = new();
        private readonly TextBox txtCourseDept = new();
        private readonly DataGridView gridCourses = new();

        // Оюутны жагсаалт
        private readonly DataGridView gridStudents = new();

        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public TeacherDashboardForm()
        {
            Text = "KOSEN LMS - Багшийн Удирдлагын Систем";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(1040, 650);
            MinimumSize = new Size(920, 580);

            // 1. ЗҮҮН ТАЛЫН БОСОО ЦЭС (Sidebar)
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 245,
                BackColor = HaaIsTheme.Green
            };

            sidebar.Controls.Add(new Label
            {
                Text = "KOSEN LMS\nБагшийн Портал",
                Location = new Point(25, 22),
                AutoSize = true,
                ForeColor = HaaIsTheme.Yellow,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            });

            var badgeBox = new Panel
            {
                Location = new Point(15, 85),
                Size = new Size(215, 60),
                BackColor = Color.FromArgb(20, 75, 48)
            };
            badgeBox.Controls.Add(new Label
            {
                Text = "Сургалтын Удирдлага",
                Location = new Point(12, 10),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            });
            badgeBox.Controls.Add(new Label
            {
                Text = "Дүн ба Хичээлийн систем",
                Location = new Point(12, 32),
                AutoSize = true,
                ForeColor = Color.FromArgb(190, 230, 205),
                Font = new Font("Segoe UI", 8.5F)
            });
            sidebar.Controls.Add(badgeBox);

            string[] menus = { "Дүн оруулах", "Хичээл удирдах", "Оюутны жагсаалт" };
            int navY = 170;
            foreach (var menu in menus)
            {
                var lblNav = new Label
                {
                    Text = "    " + menu,
                    Location = new Point(0, navY),
                    Size = new Size(245, 48),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                lblNav.Click += (s, e) => SwitchPage(menu);
                sidebar.Controls.Add(lblNav);
                navButtons[menu] = lblNav;
                navY += 50;
            }

            var btnLogout = HaaIsTheme.CreateButton("Системээс гарах", "btnLogout", 22, sidebar.Height - 65, primary: false);
            btnLogout.Size = new Size(200, 42);
            btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogout.Click += (s, e) => Close();
            sidebar.Controls.Add(btnLogout);

            // 2. ДЭЭД МЭДЭЭЛЛИЙН МӨР
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White
            };

            lblPageTitle = new Label
            {
                Text = "Дүн оруулах",
                Location = new Point(30, 18),
                AutoSize = true,
                ForeColor = HaaIsTheme.Green,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            };
            topBar.Controls.Add(lblPageTitle);

            var lblSchool = new Label
            {
                Text = "Дархан Коосэн Технологийн Коллеж",
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            topBar.Controls.Add(lblSchool);
            topBar.Resize += (s, e) =>
            {
                lblSchool.Location = new Point(topBar.Width - lblSchool.Width - 35, 23);
            };

            // 3. ГОЛЫН АЖЛЫН ТАЛБАР
            mainWorkspace.Dock = DockStyle.Fill;
            mainWorkspace.BackColor = Color.FromArgb(245, 248, 246);

            Controls.Add(mainWorkspace);
            Controls.Add(topBar);
            Controls.Add(sidebar);

            SwitchPage("Дүн оруулах");
        }

        private void SwitchPage(string pageName)
        {
            lblPageTitle.Text = pageName;
            foreach (var pair in navButtons)
            {
                bool active = pair.Key == pageName;
                pair.Value.BackColor = active ? Color.FromArgb(40, 110, 85) : HaaIsTheme.Green;
                pair.Value.ForeColor = active ? HaaIsTheme.Yellow : Color.White;
            }

            mainWorkspace.SuspendLayout();
            mainWorkspace.Controls.Clear();

            int pad = 25;
            int cardW = Math.Max(640, mainWorkspace.Width - pad * 2);
            int cardH = Math.Max(440, mainWorkspace.Height - pad * 2);

            var card = HaaIsTheme.CreateCard(pad, pad, cardW, cardH);
            card.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            card.Resize += (s, e) => card.Invalidate();

            if (pageName == "Дүн оруулах")
            {
                card.Controls.Add(HaaIsTheme.CreateLabel("Оюутны код:", 25, 20));
                HaaIsTheme.StyleTextBox(txtGradeStuId, 25, 45, 150);
                card.Controls.Add(txtGradeStuId);

                card.Controls.Add(HaaIsTheme.CreateLabel("Хичээлийн нэр:", 190, 20));
                HaaIsTheme.StyleTextBox(txtGradeCourse, 190, 45, 220);
                card.Controls.Add(txtGradeCourse);

                card.Controls.Add(HaaIsTheme.CreateLabel("Кредит:", 425, 20));
                txtGradeCredit.Text = "3";
                HaaIsTheme.StyleTextBox(txtGradeCredit, 425, 45, 80);
                card.Controls.Add(txtGradeCredit);

                card.Controls.Add(HaaIsTheme.CreateLabel("Оноо (0-100):", 520, 20));
                HaaIsTheme.StyleTextBox(txtGradeScore, 520, 45, 110);
                card.Controls.Add(txtGradeScore);

                var btnSaveGrade = HaaIsTheme.CreateButton("+ Дүн хадгалах", "btnSaveGrade", 25, 90, primary: true);
                btnSaveGrade.Size = new Size(210, 40);
                btnSaveGrade.Click += BtnSaveGrade_Click;
                card.Controls.Add(btnSaveGrade);

                var btnDeleteGrade = HaaIsTheme.CreateButton("Сонгосон дүнг устгах", "btnDeleteGrade", 250, 90, primary: false);
                btnDeleteGrade.Size = new Size(220, 40);
                btnDeleteGrade.Click += BtnDeleteGrade_Click;
                card.Controls.Add(btnDeleteGrade);

                gridGrades.Location = new Point(25, 145);
                gridGrades.Size = new Size(cardW - 50, cardH - 170);
                gridGrades.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                gridGrades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                gridGrades.ReadOnly = true;
                gridGrades.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                LoadAllGrades();
                card.Controls.Add(gridGrades);
            }
            else if (pageName == "Хичээл удирдах")
            {
                card.Controls.Add(HaaIsTheme.CreateLabel("Хичээлийн нэр:", 25, 20));
                HaaIsTheme.StyleTextBox(txtCourseName, 25, 45, 250);
                card.Controls.Add(txtCourseName);

                card.Controls.Add(HaaIsTheme.CreateLabel("Кредит:", 290, 20));
                txtCourseCredit.Text = "3";
                HaaIsTheme.StyleTextBox(txtCourseCredit, 290, 45, 90);
                card.Controls.Add(txtCourseCredit);

                card.Controls.Add(HaaIsTheme.CreateLabel("Тэнхим:", 395, 20));
                HaaIsTheme.StyleTextBox(txtCourseDept, 395, 45, 230);
                card.Controls.Add(txtCourseDept);

                var btnAddCourse = HaaIsTheme.CreateButton("+ Хичээл нэмэх", "btnAddCourse", 25, 90, primary: true);
                btnAddCourse.Size = new Size(210, 40);
                btnAddCourse.Click += BtnAddCourse_Click;
                card.Controls.Add(btnAddCourse);

                var btnDeleteCourse = HaaIsTheme.CreateButton("Сонгосон хичээл устгах", "btnDeleteCourse", 250, 90, primary: false);
                btnDeleteCourse.Size = new Size(230, 40);
                btnDeleteCourse.Click += BtnDeleteCourse_Click;
                card.Controls.Add(btnDeleteCourse);

                gridCourses.Location = new Point(25, 145);
                gridCourses.Size = new Size(cardW - 50, cardH - 170);
                gridCourses.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                gridCourses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                gridCourses.ReadOnly = true;
                gridCourses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                LoadAllCourses();
                card.Controls.Add(gridCourses);
            }
            else if (pageName == "Оюутны жагсаалт")
            {
                card.Controls.Add(new Label
                {
                    Text = "Бүртгэлтэй нийт оюутнууд болон хичээл сонголтын мэдээлэл:",
                    Location = new Point(25, 20),
                    AutoSize = true,
                    ForeColor = HaaIsTheme.Green,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
                });

                gridStudents.Location = new Point(25, 55);
                gridStudents.Size = new Size(cardW - 50, cardH - 80);
                gridStudents.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                gridStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                gridStudents.ReadOnly = true;
                gridStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                LoadStudentsList();
                card.Controls.Add(gridStudents);
            }

            mainWorkspace.Controls.Add(card);
            mainWorkspace.ResumeLayout();
        }

        private void BtnSaveGrade_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGradeStuId.Text) ||
                string.IsNullOrWhiteSpace(txtGradeCourse.Text) ||
                !int.TryParse(txtGradeCredit.Text.Trim(), out int credit) ||
                !int.TryParse(txtGradeScore.Text.Trim(), out int score))
            {
                MessageBox.Show("Оюутны код, хичээлийн нэр болон тоон оноог зөв оруулна уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string letter = score >= 90 ? "A" : score >= 80 ? "B" : score >= 70 ? "C" : score >= 60 ? "D" : "F";

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = @"INSERT INTO student_grades (student_id, course_name, credit, score, letter_grade) 
                                 VALUES (@id, @course, @credit, @score, @letter)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtGradeStuId.Text.Trim());
                cmd.Parameters.AddWithValue("@course", txtGradeCourse.Text.Trim());
                cmd.Parameters.AddWithValue("@credit", credit);
                cmd.Parameters.AddWithValue("@score", score);
                cmd.Parameters.AddWithValue("@letter", letter);
                cmd.ExecuteNonQuery();

                MessageBox.Show($"Дүн амжилттай хадгалагдлаа! (Үнэлгээ: {letter})", "Коосэн LMS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtGradeScore.Clear();
                LoadAllGrades();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Дүн хадгалахад алдаа гарлаа: " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteGrade_Click(object? sender, EventArgs e)
        {
            if (gridGrades.SelectedRows.Count == 0)
            {
                MessageBox.Show("Устгах дүнгийн мөрийг сонгоно уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string stuId = gridGrades.SelectedRows[0].Cells["Оюутны код"].Value?.ToString() ?? "";
                string course = gridGrades.SelectedRows[0].Cells["Хичээлийн нэр"].Value?.ToString() ?? "";

                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM student_grades WHERE student_id = @id AND course_name = @course LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@id", stuId);
                cmd.Parameters.AddWithValue("@course", course);
                cmd.ExecuteNonQuery();
                LoadAllGrades();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Устгахад алдаа гарлаа: " + ex.Message);
            }
        }

        private void BtnAddCourse_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourseName.Text) ||
                string.IsNullOrWhiteSpace(txtCourseDept.Text) ||
                !int.TryParse(txtCourseCredit.Text.Trim(), out int credit))
            {
                MessageBox.Show("Хичээлийн мэдээллийг бүрэн оруулна уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO available_courses (course_name, credit, department) VALUES (@name, @credit, @dept)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", txtCourseName.Text.Trim());
                cmd.Parameters.AddWithValue("@credit", credit);
                cmd.Parameters.AddWithValue("@dept", txtCourseDept.Text.Trim());
                cmd.ExecuteNonQuery();

                MessageBox.Show("Шинэ хичээл амжилттай нэмэгдлээ!", "Коосэн LMS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCourseName.Clear();
                txtCourseDept.Clear();
                LoadAllCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Хичээл нэмэхэд алдаа гарлаа: " + ex.Message);
            }
        }

        private void BtnDeleteCourse_Click(object? sender, EventArgs e)
        {
            if (gridCourses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Устгах хичээлээ сонгоно уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string course = gridCourses.SelectedRows[0].Cells["Хичээлийн нэр"].Value?.ToString() ?? "";
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM available_courses WHERE course_name = @name LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@name", course);
                cmd.ExecuteNonQuery();
                LoadAllCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Устгахад алдаа гарлаа: " + ex.Message);
            }
        }

        private void LoadAllGrades()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT student_id AS 'Оюутны код', course_name AS 'Хичээлийн нэр', credit AS 'Кредит', score AS 'Оноо', letter_grade AS 'Үсгэн үнэлгээ' FROM student_grades";
                using var adapter = new MySqlDataAdapter(query, conn);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                gridGrades.DataSource = table;
            }
            catch { }
        }

        private void LoadAllCourses()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT course_name AS 'Хичээлийн нэр', credit AS 'Кредит', department AS 'Тэнхим' FROM available_courses";
                using var adapter = new MySqlDataAdapter(query, conn);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                gridCourses.DataSource = table;
            }
            catch { }
        }

        private void LoadStudentsList()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT student_id AS 'Оюутны код', last_name AS 'Овог', first_name AS 'Нэр', major AS 'Мэргэжил', email AS 'Имэйл' FROM students";
                using var adapter = new MySqlDataAdapter(query, conn);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                gridStudents.DataSource = table;
            }
            catch { }
        }
    }
}