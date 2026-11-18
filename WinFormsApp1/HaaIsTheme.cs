using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public static class HaaIsTheme
    {
        public static readonly Color Green = Color.FromArgb(27, 94, 60);
        public static readonly Color GreenHover = Color.FromArgb(38, 120, 78);
        public static readonly Color SubMenuGreen = Color.FromArgb(24, 90, 56);
        public static readonly Color White = Color.White;
        public static readonly Color SoftBg = Color.FromArgb(245, 248, 246);
        public static readonly Color Yellow = Color.FromArgb(255, 215, 0);
        public static readonly Color TextDark = Color.FromArgb(35, 45, 40);

        public static void StyleForm(Form form)
        {
            form.BackColor = White;
            form.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.Sizable; // Томруулж, багасгадаг болгох
            form.MaximizeBox = true;                      // Томруулах дөрвөлжин товчийг идэвхжүүлэх

            typeof(Form).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, form, new object[] { true });

            form.ControlAdded += (s, e) => StyleNestedControls(e.Control);
        }

        private static void StyleNestedControls(Control? parent)
        {
            if (parent == null) return;
            if (parent is DataGridView dgv) StyleGrid(dgv);
            foreach (Control child in parent.Controls) StyleNestedControls(child);
            parent.ControlAdded += (s, e) => StyleNestedControls(e.Control);
        }

        public static Button CreateButton(string text, string name, int x, int y, bool primary = true)
        {
            return new ModernButton(primary)
            {
                Text = text,
                Name = name,
                Location = new Point(x, y),
                Size = new Size(200, 44)
            };
        }

        public static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Green,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
        }

        public static void StyleTextBox(TextBox txt, int x, int y, int width)
        {
            txt.Location = new Point(x, y);
            txt.Width = width;
            txt.Font = new Font("Segoe UI", 11F);
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.BackColor = Color.White;
            txt.ForeColor = TextDark;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(225, 235, 228);
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;

            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Green;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 38;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = TextDark;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 240, 225);
            grid.DefaultCellStyle.SelectionForeColor = Green;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.RowTemplate.Height = 32;
        }

        public static Panel CreateCard(int x, int y, int w, int h)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.White
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using var path = GetRoundPath(rect, 12);
                using var pen = new Pen(Color.FromArgb(210, 225, 215), 1.5f);
                e.Graphics.DrawPath(pen, path);
            };
            return card;
        }

        public static GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class ModernButton : Button
    {
        private readonly bool isPrimary;
        private bool isHovered = false;
        private bool isPressed = false;
        private readonly int radius = 10;

        public ModernButton(bool primary)
        {
            isPrimary = primary;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.White;
            ForeColor = primary ? Color.White : HaaIsTheme.Green;
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { isHovered = false; isPressed = false; Invalidate(); };
            MouseDown += (s, e) => { isPressed = true; Invalidate(); };
            MouseUp += (s, e) => { isPressed = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color parentBg = (Parent != null && Parent.BackColor != Color.Transparent) ? Parent.BackColor : Color.White;
            g.Clear(parentBg);

            int offset = isPressed ? 1 : 0;
            Rectangle btnRect = new Rectangle(1, 1 + offset, Width - 3, Height - 3);

            using GraphicsPath btnPath = HaaIsTheme.GetRoundPath(btnRect, radius);
            Color topColor, bottomColor, borderColor;

            bool customColor = BackColor != Color.Transparent && BackColor != HaaIsTheme.Green && BackColor != Color.White;

            if (customColor)
            {
                topColor = isHovered ? ControlPaint.Light(BackColor, 0.15f) : BackColor;
                bottomColor = ControlPaint.Dark(BackColor, 0.1f);
                borderColor = bottomColor;
                ForeColor = Color.White;
            }
            else if (isPrimary)
            {
                topColor = isHovered ? Color.FromArgb(46, 138, 88) : Color.FromArgb(35, 115, 73);
                bottomColor = isHovered ? HaaIsTheme.Green : Color.FromArgb(22, 78, 49);
                borderColor = Color.FromArgb(18, 65, 40);
                ForeColor = Color.White;
            }
            else
            {
                topColor = isHovered ? Color.FromArgb(242, 249, 245) : Color.White;
                bottomColor = isHovered ? Color.FromArgb(225, 238, 230) : Color.FromArgb(238, 245, 240);
                borderColor = HaaIsTheme.Green;
                ForeColor = HaaIsTheme.Green;
            }

            using (LinearGradientBrush brush = new LinearGradientBrush(btnRect, topColor, bottomColor, LinearGradientMode.Vertical))
            {
                g.FillPath(brush, btnPath);
            }

            using (Pen borderPen = new Pen(borderColor, 1.5f))
            {
                g.DrawPath(borderPen, btnPath);
            }

            TextRenderer.DrawText(g, Text, Font, btnRect, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}