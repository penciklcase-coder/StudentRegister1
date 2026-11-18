using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class TeacherLoginForm : Form
    {
        private readonly TextBox txtAdminUser = new();
        private readonly TextBox txtPassword = new();

        public TeacherLoginForm()
        {
            Text = "КООСЭН - Систем Админ Нэвтрэх";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(500, 460);

            var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = HaaIsTheme.Green };
            header.Controls.Add(new Label
            {
                Text = "Дархан Коосэн • Админ Портал",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });
            Controls.Add(header);

            var card = HaaIsTheme.CreateCard(35, 110, 430, 320, 16);
            Controls.Add(card);

            var pillAdmin = HaaIsTheme.CreateBubbleOption("🛡️ Систем Админ / Менежер", 40, 24, 350, 38, true);
            card.Controls.Add(pillAdmin);

            // (admin) болон (123) гэсэн ил бичиглэлүүдийг арилгасан
            card.Controls.Add(HaaIsTheme.CreateLabel("Админ нэвтрэх нэр", 40, 82));
            card.Controls.Add(HaaIsTheme.CreateStyledInput(txtAdminUser, 40, 108, 350, "Нэвтрэх нэр оруулах"));

            card.Controls.Add(HaaIsTheme.CreateLabel("Нууц үг", 40, 162));
            card.Controls.Add(HaaIsTheme.CreateStyledInput(txtPassword, 40, 188, 350, "Нууц үг оруулах", isPassword: true));

            var btnLogin = HaaIsTheme.CreateButton("Нэвтрэх", "btnLogin", 40, 250, primary: true);
            btnLogin.Size = new Size(220, 44);
            btnLogin.Click += BtnLogin_Click;
            card.Controls.Add(btnLogin);

            var btnBack = HaaIsTheme.CreateButton("Буцах", "btnBack", 270, 250, primary: false);
            btnBack.Size = new Size(120, 44);
            btnBack.Click += (s, e) => Close();
            card.Controls.Add(btnBack);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string user = txtAdminUser.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Админ нэр болон нууц үгээ оруулна уу.", "Мэдээлэл дутуу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (user == "admin" && pass == "123")
            {
                MessageBox.Show("Админ эрхээр амжилттай нэвтэрлээ!", "Дархан Коосэн", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Hide();
                using var mgr = new ManagerPanelForm();
                mgr.ShowDialog(this);
                Close();
            }
            else
            {
                MessageBox.Show("Админ нэр эсвэл нууц үг буруу байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}