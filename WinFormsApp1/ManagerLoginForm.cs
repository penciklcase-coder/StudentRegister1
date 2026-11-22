using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class ManagerLoginForm : Form
    {
        private readonly TextBox txtAdminUser = new();
        private readonly TextBox txtAdminPass = new();
        private readonly Panel centerCard;

        private static readonly Color AdminRed = Color.FromArgb(120, 40, 40);

        public ManagerLoginForm()
        {
            Text = "Коосэн - Систем Менежер Нэвтрэх";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(760, 520);
            MinimumSize = new Size(520, 460);
            BackColor = Color.FromArgb(248, 245, 245);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = AdminRed
            };
            header.Controls.Add(new Label
            {
                Text = "Систем Менежер (Админ) Нэвтрэх",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });

            centerCard = HaaIsTheme.CreateCard(165, 115, 430, 350);

            centerCard.Controls.Add(new Label
            {
                Text = "Зөвхөн сургалтын алба болон системийн админ нэвтэрнэ.",
                Location = new Point(35, 20),
                Size = new Size(360, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.8F)
            });

            var lblUser = HaaIsTheme.CreateLabel("Админ нэвтрэх нэр:", 50, 60);
            lblUser.ForeColor = AdminRed;
            centerCard.Controls.Add(lblUser);

            HaaIsTheme.StyleTextBox(txtAdminUser, 50, 88, 330);
            centerCard.Controls.Add(txtAdminUser);

            var lblPass = HaaIsTheme.CreateLabel("Нууц үг:", 50, 145);
            lblPass.ForeColor = AdminRed;
            centerCard.Controls.Add(lblPass);

            HaaIsTheme.StyleTextBox(txtAdminPass, 50, 173, 330);
            txtAdminPass.PasswordChar = '●';
            centerCard.Controls.Add(txtAdminPass);

            var btnLogin = HaaIsTheme.CreateButton("Админ эрхээр нэвтрэх", "btnAdminLogin", 50, 235, primary: true);
            btnLogin.BackColor = AdminRed;
            btnLogin.Size = new Size(330, 44);
            btnLogin.Click += BtnLogin_Click;
            centerCard.Controls.Add(btnLogin);

            var btnBack = HaaIsTheme.CreateButton("Буцах", "btnBack", 50, 290, primary: false);
            btnBack.Size = new Size(330, 38);
            btnBack.Click += (s, e) => Close();
            centerCard.Controls.Add(btnBack);

            Controls.Add(centerCard);
            Controls.Add(header);

            Resize += (s, e) => CenterLoginCard();
            Load += (s, e) => CenterLoginCard();
        }

        private void CenterLoginCard()
        {
            centerCard.Location = new Point(
                Math.Max(20, (ClientSize.Width - centerCard.Width) / 2),
                Math.Max(95, 80 + (ClientSize.Height - 80 - centerCard.Height) / 2)
            );
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string user = txtAdminUser.Text.Trim();
            string pass = txtAdminPass.Text.Trim();

            if (user == "admin" && pass == "123")
            {
                Hide();
                using var panel = new ManagerPanelForm();
                panel.ShowDialog();
                Close();
            }
            else
            {
                MessageBox.Show("Админ нэвтрэх нэр эсвэл нууц үг буруу байна!", "Нэвтрэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}