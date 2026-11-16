using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class HomeForm : Form
    {
        public HomeForm()
        {
            Text = "ХААИС - Оюутны Нэгдсэн Систем";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(500, 450);
            BackColor = HaaIsTheme.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = HaaIsTheme.Green
            };

            header.Controls.Add(new Label
            {
                Text = "Хөдөө Аж Ахуйн Их Сургууль\nОюутны Портал",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });
            Controls.Add(header);

            var lblWelcome = new Label
            {
                Text = "Системд тавтай морилно уу!",
                Location = new Point(110, 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = HaaIsTheme.Green
            };
            Controls.Add(lblWelcome);

            var btnLogin = HaaIsTheme.CreateButton("Нэвтрэх", "btnLogin", 155, 230, primary: true);
            btnLogin.Click += (s, e) =>
            {
                var loginForm = new LoginForm();
                Hide();
                loginForm.ShowDialog();
                Show();
            };
            Controls.Add(btnLogin);

            var btnRegister = HaaIsTheme.CreateButton("Бүртгүүлэх", "btnRegister", 155, 300, primary: false);
            btnRegister.Click += (s, e) =>
            {
                var regForm = new RegisterForm();
                Hide();
                regForm.ShowDialog();
                Show();
            };
            Controls.Add(btnRegister);
        }
    }
}