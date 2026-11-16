using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class StudentProfileForm : Form
    {
        public StudentProfileForm(string lastName, string firstName, string studentId, string email, string major)
        {
            Text = "ХААИС - Оюутны Профайл";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(550, 520);
            BackColor = HaaIsTheme.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = HaaIsTheme.Green
            };
            header.Controls.Add(new Label
            {
                Text = "Хөдөө Аж Ахуйн Их Сургууль\nОюутны Хувийн Хуудас",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });
            Controls.Add(header);

            int startY = 140;
            AddInfoRow("Овог, Нэр:", $"{lastName} {firstName}", startY);
            AddInfoRow("Оюутны код:", studentId, startY + 60);
            AddInfoRow("Цахим шуудан:", email, startY + 120);
            AddInfoRow("Мэргэжил:", major, startY + 180);
            AddInfoRow("Сургууль:", "ХААИС (MULS)", startY + 240);

            var btnLogout = HaaIsTheme.CreateButton("Гарах (Log out)", "btnLogout", 180, 430, primary: false);
            btnLogout.Click += (s, e) => Close();
            Controls.Add(btnLogout);
        }

        private void AddInfoRow(string labelText, string valueText, int y)
        {
            var lblTitle = new Label
            {
                Text = labelText,
                Location = new Point(50, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = HaaIsTheme.Green
            };
            Controls.Add(lblTitle);

            var lblValue = new Label
            {
                Text = valueText,
                Location = new Point(200, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.Black
            };
            Controls.Add(lblValue);
        }
    }
}