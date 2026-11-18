using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class ManagerPanelForm : Form
    {
        private readonly DataGridView gridStudents = new();
        private readonly DataGridView gridTeachers = new();

        // Оюутан нэмэх талбарууд
        private readonly TextBox txtStuLastName = new();
        private readonly TextBox txtStuFirstName = new();
        private readonly TextBox txtStuId = new();
        private readonly TextBox txtStuEmail = new();
        private readonly ComboBox cmbStuMajor = new();
        private readonly TextBox txtStuPass = new();

        // Багш нэмэх талбарууд
        private readonly TextBox txtTeacherId = new();
        private readonly TextBox txtTeacherName = new();
        private readonly TextBox txtDepartment = new();
        private readonly TextBox txtTeacherPass = new();

        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public ManagerPanelForm()
        {
            Text = "Коосэн - Систем Удирдлагын Самбар (Manager Panel)";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(920, 640);
            MinimumSize = new Size(860, 580);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(120, 40, 40)
            };
            header.Controls.Add(new Label
            {
                Text = "Систем Менежерийн Удирдлагын Самбар",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });

            var tabControl = new TabControl
            {
                Location = new Point(20, 85),
                Size = new Size(ClientSize.Width - 40, ClientSize.Height - 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            // =================================================================
            // ТАБ 1: ОЮУТНЫ ХАЯГ ҮҮСГЭХ & УДИРДАХ
            // =================================================================
            var tabStudents = new TabPage("Оюутны бүртгэл үүсгэх") { BackColor = HaaIsTheme.White };

            tabStudents.Controls.Add(HaaIsTheme.CreateLabel("Овог:", 15, 12));
            HaaIsTheme.StyleTextBox(txtStuLastName, 15, 35, 170);
            tabStudents.Controls.Add(txtStuLastName);

            tabStudents.Controls.Add(HaaIsTheme.CreateLabel("Нэр:", 200, 12));
            HaaIsTheme.StyleTextBox(txtStuFirstName, 200, 35, 170);
            tabStudents.Controls.Add(txtStuFirstName);

            tabStudents.Controls.Add(HaaIsTheme.CreateLabel("Оюутны код:", 385, 12));
            HaaIsTheme.StyleTextBox(txtStuId, 385, 35, 170);
            tabStudents.Controls.Add(txtStuId);

            tabStudents.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг:", 570, 12));
            HaaIsTheme.StyleTextBox(txtStuPass, 570, 35, 170);
            tabStudents.Controls.Add(txtStuPass);

            tabStudents.Controls.Add(HaaIsTheme.CreateLabel("Имэйл:", 15, 72));
            HaaIsTheme.StyleTextBox(txtStuEmail, 15, 95, 240);
            tabStudents.Controls.Add(txtStuEmail);

            tabStudents.Controls.Add(HaaIsTheme.CreateLabel("Мэргэжил / Анги:", 270, 72));
            cmbStuMajor.Location = new Point(270, 95);
            cmbStuMajor.Width = 285;
            cmbStuMajor.Font = new Font("Segoe UI", 10.5F);
            cmbStuMajor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStuMajor.Items.AddRange(new object[]
            {
                "Мэдээллийн технологи",
                "Програм хангамж",
                "Цахилгаан, электроник",
                "Механик инженерчлэл",
                "Барилгын инженерчлэл",
                "Био-инженерчлэл"
            });
            cmbStuMajor.SelectedIndex = 0;
            tabStudents.Controls.Add(cmbStuMajor);

            var btnAddStudent = HaaIsTheme.CreateButton("+ Оюутны хаяг нэмэх", "btnAddStudent", 570, 90, primary: true);
            btnAddStudent.Size = new Size(240, 38);
            btnAddStudent.Click += BtnAddStudent_Click;
            tabStudents.Controls.Add(btnAddStudent);

            gridStudents.Location = new Point(15, 145);
            gridStudents.Size = new Size(tabStudents.Width - 30, tabStudents.Height - 210);
            gridStudents.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridStudents.ReadOnly = true;
            gridStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            LoadStudentData();
            tabStudents.Controls.Add(gridStudents);

            var btnDeleteStudent = HaaIsTheme.CreateButton("Сонгосон оюутныг устгах", "btnDeleteStudent", 15, tabStudents.Height - 52, primary: false);
            btnDeleteStudent.Size = new Size(250, 40);
            btnDeleteStudent.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteStudent.Click += (s, e) =>
            {
                if (gridStudents.SelectedRows.Count > 0)
                {
                    string studentId = gridStudents.SelectedRows[0].Cells["student_id"].Value?.ToString() ?? "";
                    DeleteRecord("students", "student_id", studentId);
                    LoadStudentData();
                }
                else
                {
                    MessageBox.Show("Устгах оюутнаа мөрөөр нь сонгоно уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            tabStudents.Controls.Add(btnDeleteStudent);
            tabControl.TabPages.Add(tabStudents);

            // =================================================================
            // ТАБ 2: БАГШИЙН ХАЯГ ҮҮСГЭХ & УДИРДАХ
            // =================================================================
            var tabTeachers = new TabPage("Багшийн бүртгэл үүсгэх") { BackColor = HaaIsTheme.White };

            tabTeachers.Controls.Add(HaaIsTheme.CreateLabel("Багшийн ID:", 15, 15));
            HaaIsTheme.StyleTextBox(txtTeacherId, 15, 40, 160);
            tabTeachers.Controls.Add(txtTeacherId);

            tabTeachers.Controls.Add(HaaIsTheme.CreateLabel("Овог, Нэр:", 190, 15));
            HaaIsTheme.StyleTextBox(txtTeacherName, 190, 40, 210);
            tabTeachers.Controls.Add(txtTeacherName);

            tabTeachers.Controls.Add(HaaIsTheme.CreateLabel("Тэнхим:", 415, 15));
            HaaIsTheme.StyleTextBox(txtDepartment, 415, 40, 200);
            tabTeachers.Controls.Add(txtDepartment);

            tabTeachers.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг:", 630, 15));
            HaaIsTheme.StyleTextBox(txtTeacherPass, 630, 40, 180);
            tabTeachers.Controls.Add(txtTeacherPass);

            var btnAddTeacher = HaaIsTheme.CreateButton("+ Багшийн хаяг нэмэх", "btnAddTeacher", 15, 85, primary: true);
            btnAddTeacher.Size = new Size(230, 38);
            btnAddTeacher.Click += BtnAddTeacher_Click;
            tabTeachers.Controls.Add(btnAddTeacher);

            gridTeachers.Location = new Point(15, 140);
            gridTeachers.Size = new Size(tabTeachers.Width - 30, tabTeachers.Height - 205);
            gridTeachers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridTeachers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridTeachers.ReadOnly = true;
            gridTeachers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            LoadTeacherData();
            tabTeachers.Controls.Add(gridTeachers);

            var btnDeleteTeacher = HaaIsTheme.CreateButton("Сонгосон багшийг устгах", "btnDeleteTeacher", 15, tabTeachers.Height - 52, primary: false);
            btnDeleteTeacher.Size = new Size(250, 40);
            btnDeleteTeacher.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteTeacher.Click += (s, e) =>
            {
                if (gridTeachers.SelectedRows.Count > 0)
                {
                    string teacherId = gridTeachers.SelectedRows[0].Cells["teacher_id"].Value?.ToString() ?? "";
                    DeleteRecord("teachers", "teacher_id", teacherId);
                    LoadTeacherData();
                }
                else
                {
                    MessageBox.Show("Устгах багшаа мөрөөр нь сонгоно уу.", "Анхааруулга", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            tabTeachers.Controls.Add(btnDeleteTeacher);
            tabControl.TabPages.Add(tabTeachers);

            var btnClose = HaaIsTheme.CreateButton("Гарах", "btnClose", ClientSize.Width - 220, ClientSize.Height - 58, primary: true);
            btnClose.BackColor = Color.FromArgb(120, 40, 40);
            btnClose.Size = new Size(200, 42);
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Click += (s, e) => Close();

            Controls.Add(btnClose);
            Controls.Add(tabControl);
            Controls.Add(header);
        }

        private void BtnAddStudent_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStuLastName.Text) ||
                string.IsNullOrWhiteSpace(txtStuFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtStuId.Text) ||
                string.IsNullOrWhiteSpace(txtStuEmail.Text) ||
                string.IsNullOrWhiteSpace(txtStuPass.Text))
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
                cmd.Parameters.AddWithValue("@last", txtStuLastName.Text.Trim());
                cmd.Parameters.AddWithValue("@first", txtStuFirstName.Text.Trim());
                cmd.Parameters.AddWithValue("@id", txtStuId.Text.Trim());
                cmd.Parameters.AddWithValue("@email", txtStuEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@major", cmbStuMajor.SelectedItem?.ToString() ?? "Мэдээллийн технологи");
                cmd.Parameters.AddWithValue("@pwd", txtStuPass.Text.Trim());
                cmd.ExecuteNonQuery();

                MessageBox.Show("Оюутны хаяг амжилттай үүслээ!", "Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtStuLastName.Clear();
                txtStuFirstName.Clear();
                txtStuId.Clear();
                txtStuEmail.Clear();
                txtStuPass.Clear();
                LoadStudentData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Оюутан нэмэхэд алдаа гарлаа (Оюутны код давхардсан байж магадгүй): " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddTeacher_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTeacherId.Text) ||
                string.IsNullOrWhiteSpace(txtTeacherName.Text) ||
                string.IsNullOrWhiteSpace(txtDepartment.Text) ||
                string.IsNullOrWhiteSpace(txtTeacherPass.Text))
            {
                MessageBox.Show("Багшийн бүх мэдээллийг бүрэн бөглөнө үү.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO teachers (teacher_id, full_name, department, password) VALUES (@id, @name, @dept, @pwd)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtTeacherId.Text.Trim());
                cmd.Parameters.AddWithValue("@name", txtTeacherName.Text.Trim());
                cmd.Parameters.AddWithValue("@dept", txtDepartment.Text.Trim());
                cmd.Parameters.AddWithValue("@pwd", txtTeacherPass.Text.Trim());
                cmd.ExecuteNonQuery();

                MessageBox.Show("Багшийн хаяг амжилттай үүслээ!", "Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTeacherId.Clear();
                txtTeacherName.Clear();
                txtDepartment.Clear();
                txtTeacherPass.Clear();
                LoadTeacherData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Багш нэмэхэд алдаа гарлаа (ID давхардсан байж магадгүй): " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudentData()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT id, last_name AS 'Овог', first_name AS 'Нэр', student_id, email AS 'Имэйл', major AS 'Мэргэжил' FROM students";
                using var adapter = new MySqlDataAdapter(query, conn);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                gridStudents.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Оюутны мэдээлэл татахад алдаа гарлаа: " + ex.Message);
            }
        }

        private void LoadTeacherData()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT id, teacher_id, full_name AS 'Багшийн нэр', department AS 'Тэнхим' FROM teachers";
                using var adapter = new MySqlDataAdapter(query, conn);
                var table = new System.Data.DataTable();
                adapter.Fill(table);
                gridTeachers.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Багшийн мэдээлэл татахад алдаа гарлаа: " + ex.Message);
            }
        }

        private void DeleteRecord(string tableName, string columnName, string idValue)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = $"DELETE FROM {tableName} WHERE {columnName} = @id";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", idValue);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Бүртгэл амжилттай устгагдлаа.", "Мэдээлэл", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Устгахад алдаа гарлаа: " + ex.Message);
            }
        }
    }
}