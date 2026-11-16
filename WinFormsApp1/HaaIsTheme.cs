using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public static class HaaIsTheme
    {
        public static readonly Color Green = Color.FromArgb(27, 77, 62);
        public static readonly Color Yellow = Color.FromArgb(255, 193, 7);
        public static readonly Color Amber = Yellow;
        public static readonly Color White = Color.FromArgb(255, 255, 255);
        public static readonly Color MutedText = Color.FromArgb(100, 110, 105);

        public static void StyleForm(Form form)
        {
            form.ShowIcon = false; // Энэ нь гарчиг дээрх хар жижиг дүрслэлийг арилгаж цэвэрхэн болгоно
            form.BackColor = White;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }

        public static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Green
            };
        }

        public static void StyleTextBox(TextBox txt, int x, int y, int width)
        {
            txt.Location = new Point(x, y);
            txt.Size = new Size(width, 34);
            txt.Font = new Font("Segoe UI", 11F);
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        public static Button CreateButton(string text, string name, int x, int y, bool primary)
        {
            var btn = new Button
            {
                Text = text,
                Name = name,
                Location = new Point(x, y),
                Size = new Size(190, 44),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            if (primary)
            {
                btn.BackColor = Green;
                btn.ForeColor = White;
            }
            else
            {
                btn.BackColor = Yellow;
                btn.ForeColor = Green;
            }

            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}