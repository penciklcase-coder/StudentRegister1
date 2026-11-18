using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class StudentProfileForm : Form
    {
        private readonly string lastName;
        private readonly string firstName;
        private readonly string studentId;
        private readonly string email;
        private readonly string major;

        private readonly Panel mainWorkspace = new();
        private readonly DataGridView gridEnrolled = new();
        private readonly Dictionary<string, Label> navButtons = new();
        private Label lblPageTitle = new();

        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public StudentProfileForm(string lastName, string firstName, string studentId, string email, string major)
        {
            this.lastName = lastName;
            this.firstName = firstName;
            this.studentId = studentId;
            this.email = email;
            this.major = major;

            Text = $"KOSEN SIS - Оюутны Систем ({studentId})";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(940, 600);

            // 1. ЗҮҮН ТАЛЫН БОСОО ЦЭС (Web Dashboard Sidebar)
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = HaaIsTheme.Green
            };

            sidebar.Controls.Add(new Label
            {
                Text = "KOSEN SIS\nОюутны Портал",
                Location = new Point(25, 22),
                AutoSize = true,
                ForeColor = HaaIsTheme.Yellow,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            });

            // Оюутны товч карт (Sidebar дотор)
            var userBox = new Panel
            {
                Location = new Point(15, 85),
                Size = new Size(200, 65),
                BackColor = Color.FromArgb(20, 75, 48)
            };
            userBox.Controls.Add(new Label
            {
                Text = $"{lastName} {firstName}",
                Location = new Point(12, 10),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            });
            userBox.Controls.Add(new Label
            {
                Text = $"Код: {studentId}",
                Location = new Point(12, 34),
                AutoSize = true,
                ForeColor = Color.FromArgb(190, 230, 205),
                Font = new Font("Segoe UI", 9F)
            });
            sidebar.Controls.Add(userBox);

            // Sidebar цэснүүд
            string[] menus = { "Хувийн мэдээлэл", "Хичээл сонголт", "Дүнгийн жагсаалт" };
            int navY = 175;
            foreach (var menu in menus)
            {
                var lblNav = new Label
                {
                    Text = "   " + menu,
                    Location = new Point(0, navY),
                    Size = new Size(230, 46),
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

            // Гарах товч зүүн доод буланд
            var btnLogout = HaaIsTheme.CreateButton("Системээс гарах", "btnLogout", 20, 535, primary: false);
            btnLogout.Size = new Size(190, 42);
            btnLogout.Click += (s, e) => Close();
            sidebar.Controls.Add(btnLogout);

            // 2. ДЭЭД МЭДЭЭЛЛИЙН МӨР (Top Header Bar)
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White
            };
            lblPageTitle = new Label
            {
                Text = "Хувийн мэдээлэл",
                Location = new Point(30, 18),
                AutoSize = true,
                ForeColor = HaaIsTheme.Green,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };
            topBar.Controls.Add(lblPageTitle);

            topBar.Controls.Add(new Label
            {
                Text = $"Мэргэжил: {major}",
                Location = new Point(450, 23),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            });

            // 3. ГОЛЫН АЖЛЫН ТАЛБАР (Main Workspace)
            mainWorkspace.Dock = DockStyle.Fill;
            mainWorkspace.BackColor = Color.FromArgb(245, 248, 246);

            Controls.Add(mainWorkspace);
            Controls.Add(topBar);
            Controls.Add(sidebar);

            SwitchPage("Хувийн мэдээлэл");
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

            if (pageName == "Хувийн мэдээлэл")
            {
                var card = HaaIsTheme.CreateCard(30, 25, 650, 340);
                int startY = 30;
                AddInfoRow(card, "Овог, Нэр:", $"{lastName} {firstName}", startY);
                AddInfoRow(card, "Оюутны код:", studentId, startY + 55);
                AddInfoRow(card, "Цахим шуудан:", email, startY + 110);
                AddInfoRow(card, "Мэргэжил / Анги:", major, startY + 165);
                AddInfoRow(card, "Сургууль:", "Коосэн Технологийн Коллеж", startY + 220);
                AddInfoRow(card, "Төлөв:", "Идэвхтэй суралцаж буй", startY + 275);
                mainWorkspace.Controls.Add(card);
            }
            else if (pageName == "Хичээл сонголт")
            {
                var card = HaaIsTheme.CreateCard(25, 20, 660, 490);

                card.Controls.Add(new Label
                {
                    Text = "Боломжтой хичээлүүдээс сонгож бүртгүүлнэ үү:",
                    Location = new Point(20, 15),
                    AutoSize = true,
                    ForeColor = HaaIsTheme.Green,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold)
                });

                var gridAvailable = CreateGrid(20, 45, 620, 165);
                LoadAvailableCourses(gridAvailable);
                card.Controls.Add(gridAvailable);

                var btnEnroll = HaaIsTheme.CreateButton("+ Хичээл сонгох (Enroll)", "btnEnroll", 20, 220, primary: true);
                btnEnroll.Size = new Size(230, 40);
                btnEnroll.Click += (s, e) =>
                {
                    if (gridAvailable.SelectedRows.Count > 0)
                    {
                        string courseName = gridAvailable.SelectedRows[0].Cells["Хичээлийн нэр"].Value?.ToString() ?? "";
                        int credit = Convert.ToInt32(gridAvailable.SelectedRows[0].Cells["Кредит"].Value ?? 3);
                        EnrollCourse(courseName, credit);
                        LoadEnrolledCourses(gridEnrolled);
                    }
                    else
                    {
                        MessageBox.Show("Жагсаалтаас хичээлээ сонгоно уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };
                card.Controls.Add(btnEnroll);

                card.Controls.Add(new Label
                {
                    Text = "Миний сонгосон хичээлүүд:",
                    Location = new Point(20, 275),
                    AutoSize = true,
                    ForeColor = HaaIsTheme.Green,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold)
                });

                gridEnrolled.Location = new Point(20, 305);
                gridEnrolled.Size = new Size(620, 160);
                gridEnrolled.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                gridEnrolled.ReadOnly = true;
                gridEnrolled.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                LoadEnrolledCourses(gridEnrolled);
                card.Controls.Add(gridEnrolled);

                mainWorkspace.Controls.Add(card);
            }
            else if (pageName == "Дүнгийн жагсаалт")
            {
                var card = HaaIsTheme.CreateCard(25, 20, 660, 480);
                var gridGrades = CreateGrid(20, 20, 620, 440);
                LoadGradesData(gridGrades);
                card.Controls.Add(gridGrades);
                mainWorkspace.Controls.Add(card);
            }

            mainWorkspace.ResumeLayout();
        }

        private void AddInfoRow(Control parent, string labelText, string valueText, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = labelText,
                Location = new Point(35, y),
                AutoSize = true,
                ForeColor = HaaIsTheme.Green,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            });
            parent.Controls.Add(new Label
            {
                Text = valueText,
                Location = new Point(220, y),
                AutoSize = true,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11F)
            });
        }

        private DataGridView CreateGrid(int x, int y, int w, int h)
        {
            return new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
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
                MessageBox.Show("Хичээлийг амжилттай сонголоо!", "Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
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