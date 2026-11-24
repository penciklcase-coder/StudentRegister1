using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFormsApp1.DAL;
using WinFormsApp1.Models;

namespace WinFormsApp1
{
    /// <summary>
    /// Presentation Layer (UI): Стандарт Windows Forms контролууд (DataGridView, CRUD, Хайлт,
    /// Анхааруулгын улаан мөр, StreamWriter CSV Экспорт, Статистик График)
    /// </summary>
    public class WinFormsReportForm : Form
    {
        private readonly StudentRepository studentRepo = new();

        private readonly TabControl tabMain = new();

        // Таб 1: Стандарт нэршилтэй CRUD ба Хайлтын элементүүд
        private readonly TextBox txtStudentId = new();
        private readonly TextBox txtLastName = new();
        private readonly TextBox txtFirstName = new();
        private readonly TextBox txtEmail = new();
        private readonly ComboBox cmbMajor = new();
        private readonly DateTimePicker dtpDate = new();
        private readonly TextBox txtSearch = new();
        private readonly ComboBox cmbCategory = new();
        private readonly Button btnSave = new();
        private readonly Button btnUpdate = new();
        private readonly Button btnDelete = new();
        private readonly Button btnClear = new();
        private readonly Button btnExportCsv = new();
        private readonly DataGridView dgvData = new();

        // Таб 2: Анхааруулгын систем (Alert)
        private readonly Label lblAlertBanner = new();
        private readonly DataGridView dgvAlerts = new();
        private readonly Button btnExportAlertsCsv = new();

        // Таб 3: Статистик График (Chart)
        private readonly Panel pnlChart = new();
        private Dictionary<string, int> chartMajors = new();
        private Dictionary<string, double> chartGrades = new();

        public WinFormsReportForm(int initialTabIndex = 0)
        {
            Text = "Дархан Коосэн — WinForms Удирдлага, Анхааруулга ба Тайлан";
            ClientSize = new Size(1120, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5f);

            BuildTabs();
            if (initialTabIndex >= 0 && initialTabIndex < tabMain.TabPages.Count)
                tabMain.SelectedIndex = initialTabIndex;

            Load += (_, _) => LoadAllData();
        }

        private void BuildTabs()
        {
            tabMain.Dock = DockStyle.Fill;

            var tabCrud = new TabPage("Оюутны бүртгэл (CRUD & Хайлт)");
            var tabAlerts = new TabPage("Анхааруулга (Дүн & Төлбөр)");
            var tabCharts = new TabPage("Статистик График (Chart)");

            BuildCrudTab(tabCrud);
            BuildAlertTab(tabAlerts);
            BuildChartTab(tabCharts);

            tabMain.TabPages.AddRange(new[] { tabCrud, tabAlerts, tabCharts });
            Controls.Add(tabMain);
        }

        // ================= ТАБ 1: CRUD + ХАЙЛТ + CSV =================
        private void BuildCrudTab(TabPage page)
        {
            page.BackColor = Color.FromArgb(248, 250, 252);
            page.Padding = new Padding(12);

            // Дээд талын хайлт & шүүлтүүрийн мөр
            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 48 };
            var lblSearch = new Label { Text = "Хайх (Код/Нэр):", AutoSize = true, Location = new Point(4, 14) };
            txtSearch.Location = new Point(112, 10);
            txtSearch.Width = 220;
            txtSearch.TextChanged += (_, _) => LoadStudentsGrid();

            var lblCat = new Label { Text = "Мэргэжил:", AutoSize = true, Location = new Point(350, 14) };
            cmbCategory.Location = new Point(428, 10);
            cmbCategory.Width = 210;
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Items.AddRange(new object[]
            {
                "Бүгд", "Мэдээллийн технологи", "Механик инженерчлэл",
                "Цахилгаан инженерчлэл", "Барилгын инженерчлэл"
            });
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (_, _) => LoadStudentsGrid();

            btnExportCsv.Text = "CSV Экспорт (.csv)";
            btnExportCsv.BackColor = Color.FromArgb(14, 77, 45);
            btnExportCsv.ForeColor = Color.White;
            btnExportCsv.FlatStyle = FlatStyle.Flat;
            btnExportCsv.Size = new Size(160, 30);
            btnExportCsv.Location = new Point(660, 8);
            btnExportCsv.Click += (_, _) => ExportGridToCsv(dgvData, "Students_List.csv");

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblCat, cmbCategory, btnExportCsv });

            // Зүүн талын мэдээлэл оруулах хэсэг
            var pnlForm = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = Color.White,
                Padding = new Padding(14),
                BorderStyle = BorderStyle.FixedSingle
            };

            int y = 14;
            void AddField(string labelText, Control ctrl)
            {
                pnlForm.Controls.Add(new Label { Text = labelText, Location = new Point(14, y), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
                y += 22;
                ctrl.Location = new Point(14, y);
                ctrl.Width = 266;
                pnlForm.Controls.Add(ctrl);
                y += 36;
            }

            AddField("Оюутны код (Жнь: KIT23C008):", txtStudentId);
            AddField("Овог:", txtLastName);
            AddField("Нэр:", txtFirstName);
            AddField("Имэйл хаяг:", txtEmail);

            cmbMajor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMajor.Items.AddRange(new object[]
            {
                "Мэдээллийн технологи", "Механик инженерчлэл",
                "Цахилгаан инженерчлэл", "Барилгын инженерчлэл"
            });
            cmbMajor.SelectedIndex = 0;
            AddField("Мэргэжил:", cmbMajor);

            dtpDate.Format = DateTimePickerFormat.Short;
            AddField("Бүртгэлийн огноо:", dtpDate);

            // CRUD товчлуурууд
            btnSave.Text = "Нэмэх";
            btnSave.BackColor = Color.FromArgb(22, 163, 74);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.SetBounds(14, y + 6, 128, 34);
            btnSave.Click += BtnSave_Click;

            btnUpdate.Text = "Засах";
            btnUpdate.BackColor = Color.FromArgb(2, 132, 199);
            btnUpdate.ForeColor = Color.White;
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.SetBounds(152, y + 6, 128, 34);
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete.Text = "Устгах";
            btnDelete.BackColor = Color.FromArgb(220, 38, 38);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.SetBounds(14, y + 48, 128, 34);
            btnDelete.Click += BtnDelete_Click;

            btnClear.Text = "Цэвэрлэх";
            btnClear.BackColor = Color.FromArgb(100, 116, 139);
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.SetBounds(152, y + 48, 128, 34);
            btnClear.Click += (_, _) => ClearInputs();

            pnlForm.Controls.AddRange(new Control[] { btnSave, btnUpdate, btnDelete, btnClear });

            // Баруун талын DataGridView
            dgvData.Dock = DockStyle.Fill;
            dgvData.BackgroundColor = Color.White;
            dgvData.ReadOnly = true;
            dgvData.AllowUserToAddRows = false;
            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvData.RowHeadersVisible = false;
            dgvData.CellClick += DgvData_CellClick;

            // WinForms Docking Z-order: Fill эхэлж, Left болон Top дараа нь нэмэгдэнэ
            page.Controls.Add(dgvData);
            page.Controls.Add(pnlForm);
            page.Controls.Add(pnlSearch);
        }

        // ================= ТАБ 2: АНХААРУУЛГЫН СИСТЕМ (УЛААН МӨР) =================
        private void BuildAlertTab(TabPage page)
        {
            page.BackColor = Color.FromArgb(248, 250, 252);
            page.Padding = new Padding(12);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 52 };
            lblAlertBanner.AutoSize = false;
            lblAlertBanner.Dock = DockStyle.Left;
            lblAlertBanner.Width = 760;
            lblAlertBanner.TextAlign = ContentAlignment.MiddleLeft;
            lblAlertBanner.Font = new Font("Segoe UI", 10f, FontStyle.Bold);

            btnExportAlertsCsv.Text = "Тайлан экспортлох (.csv)";
            btnExportAlertsCsv.BackColor = Color.FromArgb(14, 77, 45);
            btnExportAlertsCsv.ForeColor = Color.White;
            btnExportAlertsCsv.FlatStyle = FlatStyle.Flat;
            btnExportAlertsCsv.Size = new Size(190, 32);
            btnExportAlertsCsv.Location = new Point(880, 10);
            btnExportAlertsCsv.Click += (_, _) => ExportGridToCsv(dgvAlerts, "Student_Alerts_Report.csv");

            pnlTop.Controls.AddRange(new Control[] { lblAlertBanner, btnExportAlertsCsv });

            dgvAlerts.Dock = DockStyle.Fill;
            dgvAlerts.BackgroundColor = Color.White;
            dgvAlerts.ReadOnly = true;
            dgvAlerts.AllowUserToAddRows = false;
            dgvAlerts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAlerts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAlerts.RowHeadersVisible = false;
            dgvAlerts.CellFormatting += DgvAlerts_CellFormatting;

            page.Controls.Add(dgvAlerts);
            page.Controls.Add(pnlTop);
        }

        // ================= ТАБ 3: СТАТИСТИК ГРАФИК (CHART) =================
        private void BuildChartTab(TabPage page)
        {
            page.BackColor = Color.White;
            page.Padding = new Padding(12);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 44 };
            var btnRefresh = new Button
            {
                Text = "График шинэчлэх",
                BackColor = Color.FromArgb(14, 77, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 30),
                Location = new Point(4, 6)
            };
            btnRefresh.Click += (_, _) => LoadChartData();
            pnlTop.Controls.Add(btnRefresh);

            pnlChart.Dock = DockStyle.Fill;
            pnlChart.BackColor = Color.White;
            pnlChart.Paint += PnlChart_Paint;
            pnlChart.Resize += (_, _) => pnlChart.Invalidate();

            page.Controls.Add(pnlChart);
            page.Controls.Add(pnlTop);
        }

        // ================= ӨГӨГДӨЛ АЧААЛАХ =================
        private void LoadAllData()
        {
            LoadStudentsGrid();
            LoadAlertsGrid();
            LoadChartData();
        }

        private void LoadStudentsGrid()
        {
            try
            {
                string maj = cmbCategory.SelectedItem?.ToString() ?? "Бүгд";
                var students = studentRepo.GetAllStudents(txtSearch.Text, maj);
                dgvData.DataSource = students;

                if (dgvData.Columns["Password"] != null)
                    dgvData.Columns["Password"].Visible = false;
                if (dgvData.Columns["StudentId"] != null) dgvData.Columns["StudentId"].HeaderText = "Оюутны код";
                if (dgvData.Columns["LastName"] != null) dgvData.Columns["LastName"].HeaderText = "Овог";
                if (dgvData.Columns["FirstName"] != null) dgvData.Columns["FirstName"].HeaderText = "Нэр";
                if (dgvData.Columns["Email"] != null) dgvData.Columns["Email"].HeaderText = "Имэйл";
                if (dgvData.Columns["Major"] != null) dgvData.Columns["Major"].HeaderText = "Мэргэжил";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Оюутны жагсаалт уншихад алдаа гарлаа:\n" + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAlertsGrid()
        {
            try
            {
                var alerts = studentRepo.GetStudentAlertSummaries();
                dgvAlerts.DataSource = alerts;

                if (dgvAlerts.Columns["HasAlert"] != null) dgvAlerts.Columns["HasAlert"].Visible = false;
                if (dgvAlerts.Columns["StudentId"] != null) dgvAlerts.Columns["StudentId"].HeaderText = "Оюутны код";
                if (dgvAlerts.Columns["FullName"] != null) dgvAlerts.Columns["FullName"].HeaderText = "Овог, Нэр";
                if (dgvAlerts.Columns["Major"] != null) dgvAlerts.Columns["Major"].HeaderText = "Мэргэжил";
                if (dgvAlerts.Columns["AvgScore"] != null) dgvAlerts.Columns["AvgScore"].HeaderText = "Дундаж оноо";
                if (dgvAlerts.Columns["UnpaidFee"] != null) dgvAlerts.Columns["UnpaidFee"].HeaderText = "Төлбөрийн үлдэгдэл (₮)";
                if (dgvAlerts.Columns["AbsentCount"] != null) dgvAlerts.Columns["AbsentCount"].HeaderText = "Тасалсан тоо";
                if (dgvAlerts.Columns["AlertReason"] != null) dgvAlerts.Columns["AlertReason"].HeaderText = "Анхааруулгын төлөв";

                int alertCount = alerts.Count(a => a.HasAlert);
                if (alertCount > 0)
                {
                    lblAlertBanner.Text = $"АНХААРУУЛГА: {alertCount} оюутанд төлбөрийн үлдэгдэл, муу дүн эсвэл таслалтын зөрчил илэрлээ (улаан мөр).";
                    lblAlertBanner.ForeColor = Color.FromArgb(185, 28, 28);
                }
                else
                {
                    lblAlertBanner.Text = "Бүх оюутны дүн, ирц болон төлбөрийн төлөв хэвийн байна.";
                    lblAlertBanner.ForeColor = Color.FromArgb(21, 128, 61);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Анхааруулгын тайлан унших алдаа:\n" + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvAlerts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvAlerts.Rows.Count) return;
            if (dgvAlerts.Rows[e.RowIndex].DataBoundItem is StudentAlertSummary item && item.HasAlert)
            {
                dgvAlerts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(254, 226, 226);
                dgvAlerts.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(153, 27, 27);
            }
        }

        private void LoadChartData()
        {
            try
            {
                chartMajors = studentRepo.GetMajorCounts();
                chartGrades = studentRepo.GetCourseAverageScores();
                pnlChart.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("График өгөгдөл татах алдаа:\n" + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= CRUD ТОВЧЛУУРУУД =================
        private void DgvData_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvData.Rows[e.RowIndex].DataBoundItem is Student s)
            {
                txtStudentId.Text = s.StudentId;
                txtLastName.Text = s.LastName;
                txtFirstName.Text = s.FirstName;
                txtEmail.Text = s.Email;
                cmbMajor.SelectedItem = s.Major;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentId.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Оюутны код болон нэрийг заавал бөглөнө үү.", "Анхаар", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                studentRepo.AddStudent(new Student
                {
                    StudentId = txtStudentId.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    FirstName = txtFirstName.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text)
                        ? $"{txtStudentId.Text.Trim().ToLower()}@kosen.edu.mn"
                        : txtEmail.Text.Trim(),
                    Major = cmbMajor.SelectedItem?.ToString() ?? "Мэдээллийн технологи",
                    Password = "1234"
                });

                MessageBox.Show("Шинэ оюутан амжилттай бүртгэгдлээ!", "Амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearInputs();
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Нэмэхэд алдаа гарлаа (код давхардсан байж магадгүй):\n" + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentId.Text))
            {
                MessageBox.Show("Засах оюутнаа хүснэгтээс сонгоно уу.", "Анхаар", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                studentRepo.UpdateStudent(new Student
                {
                    StudentId = txtStudentId.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    FirstName = txtFirstName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Major = cmbMajor.SelectedItem?.ToString() ?? "Мэдээллийн технологи"
                });

                MessageBox.Show("Оюутны мэдээлэл амжилттай шинэчлэгдлээ!", "Амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Засах үед алдаа гарлаа:\n" + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            string id = txtStudentId.Text.Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                MessageBox.Show("Устгах оюутнаа хүснэгтээс сонгоно уу.", "Анхаар", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"\"{id}\" кодтой оюутныг устгах уу?", "Баталгаажуулах",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                studentRepo.DeleteStudent(id);
                MessageBox.Show("Оюутны бүртгэл устгагдлаа.", "Амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearInputs();
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Устгах үед алдаа гарлаа:\n" + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearInputs()
        {
            txtStudentId.Clear();
            txtLastName.Clear();
            txtFirstName.Clear();
            txtEmail.Clear();
            if (cmbMajor.Items.Count > 0) cmbMajor.SelectedIndex = 0;
        }

        // ================= STREAMWRITER CSV ЭКСПОРТ =================
        public static void ExportGridToCsv(DataGridView dgv, string defaultFileName)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV файл (*.csv)|*.csv",
                FileName = defaultFileName
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                // UTF-8 BOM ашигласнаар Excel дээр монгол крилл үсэг алдаагүй нээгдэнэ
                using var sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true));
                var visibleCols = dgv.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();

                sw.WriteLine(string.Join(",", visibleCols.Select(c => $"\"{c.HeaderText.Replace("\"", "\"\"")}\"")));

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;
                    var vals = visibleCols.Select(c =>
                    {
                        string val = row.Cells[c.Index].Value?.ToString() ?? "";
                        return $"\"{val.Replace("\"", "\"\"")}\"";
                    });
                    sw.WriteLine(string.Join(",", vals));
                }

                MessageBox.Show($"CSV файл амжилттай хадгалагдлаа:\n{sfd.FileName}", "Экспорт амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV экспорт хийхэд алдаа гарлаа:\n" + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= СТАТИСТИК ГРАФИК ЗУРАХ (CHART) =================
        private void PnlChart_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int halfW = pnlChart.Width / 2;
            int h = pnlChart.Height;

            DrawBarChart(
                g,
                new Rectangle(20, 20, Math.Max(220, halfW - 35), Math.Max(260, h - 40)),
                "Мэргэжил бүрийн оюутны тоо",
                chartMajors.ToDictionary(k => k.Key, v => (double)v.Value),
                Color.FromArgb(14, 77, 45)
            );

            DrawBarChart(
                g,
                new Rectangle(halfW + 15, 20, Math.Max(220, halfW - 35), Math.Max(260, h - 40)),
                "Хичээл бүрийн дундаж оноо (0–100)",
                chartGrades,
                Color.FromArgb(2, 132, 199)
            );
        }

        private void DrawBarChart(Graphics g, Rectangle area, string title, Dictionary<string, double> data, Color barColor)
        {
            using var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1);
            g.DrawRectangle(borderPen, area);

            using var titleFont = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var labelFont = new Font("Segoe UI", 8.5f);
            using var valFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb(15, 23, 42));

            g.DrawString(title, titleFont, textBrush, area.X + 14, area.Y + 12);

            if (data.Count == 0)
            {
                g.DrawString("Өгөгдөл одоогоор байхгүй байна.", labelFont, Brushes.Gray, area.X + 20, area.Y + 60);
                return;
            }

            int plotLeft = area.X + 40;
            int plotTop = area.Y + 52;
            int plotWidth = area.Width - 60;
            int plotHeight = area.Height - 120;
            if (plotWidth <= 40 || plotHeight <= 40) return;

            double maxVal = Math.Max(1, data.Values.Max());
            int count = data.Count;
            int slotW = plotWidth / count;
            int barW = Math.Min(56, Math.Max(20, slotW - 18));

            using var axisPen = new Pen(Color.FromArgb(148, 163, 184), 1);
            g.DrawLine(axisPen, plotLeft, plotTop, plotLeft, plotTop + plotHeight);
            g.DrawLine(axisPen, plotLeft, plotTop + plotHeight, plotLeft + plotWidth, plotTop + plotHeight);

            using var barBrush = new SolidBrush(barColor);
            int idx = 0;
            foreach (var kv in data)
            {
                int barH = (int)Math.Round((kv.Value / maxVal) * (plotHeight - 24));
                int x = plotLeft + idx * slotW + (slotW - barW) / 2;
                int y = plotTop + plotHeight - barH;

                g.FillRectangle(barBrush, x, y, barW, barH);

                string valStr = kv.Value % 1 == 0 ? ((int)kv.Value).ToString() : kv.Value.ToString("0.0");
                var valSize = g.MeasureString(valStr, valFont);
                g.DrawString(valStr, valFont, textBrush, x + (barW - valSize.Width) / 2, y - 18);

                var labelRect = new RectangleF(plotLeft + idx * slotW + 2, plotTop + plotHeight + 6, slotW - 4, 52);
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(kv.Key, labelFont, textBrush, labelRect, sf);

                idx++;
            }
        }
    }
}