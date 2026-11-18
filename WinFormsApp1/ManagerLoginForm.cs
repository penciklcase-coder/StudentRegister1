using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class ManagerLoginForm : Form
    {
        private readonly TextBox txtUser = new();
        private readonly TextBox txtPass = new();
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        private void InitializeComponent()
        {

        }

        public ManagerLoginForm()
        {
            Text = "ХААИС - Менежерийн нэвтрэх";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(450, 400);
            BackColor = HaaIsTheme.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(120, 40, 40) };
            header.Controls.Add(new Label { Text = "Систем Менежер Нэвтрэх", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = HaaIsTheme.White, Font = new Font("Segoe UI", 16F, FontStyle.Bold) });
            Controls.Add(header);

            Controls.Add(HaaIsTheme.CreateLabel("Менежер нэр (admin)", 50, 120));
            HaaIsTheme.StyleTextBox(txtUser, 50, 150, 330);
            Controls.Add(txtUser);

            Controls.Add(HaaIsTheme.CreateLabel("Нууц үг (123)", 50, 200));
            HaaIsTheme.StyleTextBox(txtPass, 50, 230, 330);
            txtPass.PasswordChar = '●';
            Controls.Add(txtPass);

            var btnLogin = HaaIsTheme.CreateButton("Нэвтрэх", "btnLogin", 50, 300, primary: true);
            btnLogin.BackColor = Color.FromArgb(120, 40, 40);
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
                    MessageBox.Show("Хэрэглэгчийн нэр эсвэл нууц үг буруу байна. (admin / 123)", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Controls.Add(btnLogin);
        }
    }
}
