using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class ManagerLoginForm : Form
    {
        private readonly TextBox txtUser = new();
        private readonly TextBox txtPass = new();

        public ManagerLoginForm()
        {
            Text = "КООСЭН - Менежерийн нэвтрэх";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(450, 400);

            var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = HaaIsTheme.Green };
            header.Controls.Add(new Label
            {
                Text = "Систем Менежер Нэвтрэх",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            });
            Controls.Add(header);

            Controls.Add(HaaIsTheme.CreateLabel("Менежер нэр", 50, 120));
            Controls.Add(HaaIsTheme.CreateStyledInput(txtUser, 50, 148, 330, "Нэвтрэх нэр"));

            Controls.Add(HaaIsTheme.CreateLabel("Нууц үг", 50, 200));
            Controls.Add(HaaIsTheme.CreateStyledInput(txtPass, 50, 228, 330, "Нууц үг", isPassword: true));

            var btnLogin = HaaIsTheme.CreateButton("Нэвтрэх", "btnLogin", 50, 300, primary: true);
            btnLogin.Size = new Size(330, 44);
            btnLogin.Click += (s, e) =>
            {
                if (txtUser.Text.Trim() == "admin" && txtPass.Text.Trim() == "123")
                {
                    Hide();
                    new ManagerPanelForm().ShowDialog();
                    Close();
                }
                else
                {
                    MessageBox.Show("Хэрэглэгчийн нэр эсвэл нууц үг буруу байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Controls.Add(btnLogin);
        }
    }
}