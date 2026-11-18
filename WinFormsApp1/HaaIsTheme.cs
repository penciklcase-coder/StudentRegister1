using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public static class HaaIsTheme
    {
        public static readonly Color Green = Color.FromArgb(22, 68, 54);
        public static readonly Color GreenLight = Color.FromArgb(34, 98, 78);
        public static readonly Color MintBg = Color.FromArgb(236, 246, 242);
        public static readonly Color Yellow = Color.FromArgb(255, 193, 7);
        public static readonly Color Amber = Yellow;
        public static readonly Color White = Color.FromArgb(255, 255, 255);
        public static readonly Color BgSoft = Color.FromArgb(245, 247, 246);
        public static readonly Color BorderGray = Color.FromArgb(215, 222, 218);
        public static readonly Color MutedText = Color.FromArgb(100, 112, 106);

        public static void StyleForm(Form form)
        {
            form.ShowIcon = false;
            form.BackColor = BgSoft;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }

        public static GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (d <= 0 || rect.Width <= 0 || rect.Height <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void ApplyRoundedCorners(Control ctrl, int radius)
        {
            void UpdateRegion()
            {
                if (ctrl.Width > 0 && ctrl.Height > 0)
                {
                    using var path = GetRoundPath(new Rectangle(0, 0, ctrl.Width, ctrl.Height), radius);
                    ctrl.Region = new Region(path);
                }
            }
            UpdateRegion();
            ctrl.Resize += (s, e) => UpdateRegion();
        }

        public static Panel CreateCard(int x, int y, int w, int h, int radius = 14)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = White
            };
            ApplyRoundedCorners(card, radius);
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GetRoundPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), radius);
                using var pen = new Pen(BorderGray, 1.2f);
                e.Graphics.DrawPath(pen, path);
            };
            return card;
        }

        public static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Green
            };
        }

        // Хуучин кодтой нийцтэй байдлыг хадгалах функц
        public static void StyleTextBox(TextBox txt, int x, int y, int width)
        {
            txt.Location = new Point(x, y);
            txt.Size = new Size(width, 32);
            txt.Font = new Font("Segoe UI", 10.5F);
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        // Орчин үеийн дугуй булантай TextBox контейнер
        public static Panel CreateStyledInput(TextBox txt, int x, int y, int width, string placeholder = "", bool isPassword = false)
        {
            var wrapper = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 40),
                BackColor = White,
                Cursor = Cursors.IBeam
            };
            ApplyRoundedCorners(wrapper, 8);

            bool focused = false;
            wrapper.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GetRoundPath(new Rectangle(0, 0, wrapper.Width - 1, wrapper.Height - 1), 8);
                using var pen = new Pen(focused ? Green : BorderGray, focused ? 2f : 1.2f);
                e.Graphics.DrawPath(pen, path);
            };

            txt.BorderStyle = BorderStyle.None;
            txt.Font = new Font("Segoe UI", 10.5F);
            txt.Location = new Point(12, 9);
            txt.Width = width - 24;
            txt.BackColor = White;
            if (!string.IsNullOrEmpty(placeholder)) txt.PlaceholderText = placeholder;
            if (isPassword) txt.PasswordChar = '●';

            txt.Enter += (s, e) => { focused = true; wrapper.Invalidate(); };
            txt.Leave += (s, e) => { focused = false; wrapper.Invalidate(); };
            wrapper.Click += (s, e) => txt.Focus();

            wrapper.Controls.Add(txt);
            return wrapper;
        }

        public static Button CreateButton(string text, string name, int x, int y, bool primary)
        {
            var btn = new Button
            {
                Text = text,
                Name = name,
                Location = new Point(x, y),
                Size = new Size(190, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            Color baseColor = primary ? Green : Yellow;
            Color hoverColor = primary ? GreenLight : Color.FromArgb(255, 208, 45);

            btn.BackColor = baseColor;
            btn.ForeColor = primary ? White : Green;
            btn.FlatAppearance.BorderSize = 0;

            ApplyRoundedCorners(btn, 10);
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = baseColor;

            return btn;
        }

        // Дарахад дугуй (Bubble / Pill) болж идэвхждэг мэргэжлийн сонголтын товч
        public static Label CreateBubbleOption(string text, int x, int y, int width, int height, bool selected)
        {
            var pill = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = selected
            };

            ApplyRoundedCorners(pill, height / 2);

            pill.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool isSel = pill.Tag is bool b && b;
                using var path = GetRoundPath(new Rectangle(0, 0, pill.Width - 1, pill.Height - 1), height / 2);
                using var pen = new Pen(isSel ? Green : BorderGray, isSel ? 1.8f : 1.2f);
                e.Graphics.DrawPath(pen, path);
            };

            SetBubbleState(pill, selected);
            return pill;
        }

        public static void SetBubbleState(Label pill, bool selected)
        {
            pill.Tag = selected;
            pill.BackColor = selected ? Green : Color.FromArgb(242, 245, 243);
            pill.ForeColor = selected ? White : Color.FromArgb(60, 72, 66);
            pill.Invalidate();
        }
    }
}