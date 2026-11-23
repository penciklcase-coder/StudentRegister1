#nullable enable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        // Фонтуудыг нэг удаа үүсгэж дахин ашиглана (GDI handle алдагдахгүй)
        public static readonly Font FontBase = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font FontLabel = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontInput = new Font("Segoe UI", 11F);
        public static readonly Font FontButton = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        public static readonly Font FontGridHeader = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontGridCell = new Font("Segoe UI", 9.5F);

        // Handler давхар бүртгэгдэхээс сэргийлэх
        private static readonly ConditionalWeakTable<Control, object> _hooked = new();

        public static void SetDoubleBuffered(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        public static void StyleForm(Form form)
        {
            form.BackColor = White;
            form.Font = FontBase;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MaximizeBox = true;
            form.MinimizeBox = true;

            SetDoubleBuffered(form);

            // Аль хэдийн нэмэгдсэн контролуудыг стильлэх
            foreach (Control c in form.Controls) StyleNestedControls(c);

            // Дараа нэмэгдэх контролуудыг стильлэх
            HookControlAdded(form);
        }

        private static void HookControlAdded(Control parent)
        {
            if (_hooked.TryGetValue(parent, out _)) return;
            _hooked.Add(parent, new object());
            parent.ControlAdded += (s, e) =>
            {
                if (e.Control != null) StyleNestedControls(e.Control);
            };
        }

        private static void StyleNestedControls(Control? parent)
        {
            if (parent == null) return;
            if (parent is DataGridView dgv) StyleGrid(dgv);
            foreach (Control child in parent.Controls) StyleNestedControls(child);
            HookControlAdded(parent);
        }

        /// <summary>
        /// Томруулахад контентыг (картууд, товчнууд бүхий Panel) цонхны төвд байрлуулна.
        /// Форм дээр: HaaIsTheme.CenterContent(this, contentPanel);
        /// </summary>
        public static void CenterContent(Control container, Control content)
        {
            content.Anchor = AnchorStyles.None;
            void Center()
            {
                int x = Math.Max(0, (container.ClientSize.Width - content.Width) / 2);
                int y = Math.Max(0, (container.ClientSize.Height - content.Height) / 2);
                content.Location = new Point(x, y);
            }
            container.Resize += (s, e) => Center();
            Center();
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
                Font = FontLabel,
                BackColor = Color.Transparent
            };
        }

        public static void StyleTextBox(TextBox txt, int x, int y, int width)
        {
            txt.Location = new Point(x, y);
            txt.Width = width;
            txt.Font = FontInput;
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
            grid.ColumnHeadersDefaultCellStyle.Font = FontGridHeader;
            grid.ColumnHeadersHeight = 38;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = TextDark;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 240, 225);
            grid.DefaultCellStyle.SelectionForeColor = Green;
            grid.DefaultCellStyle.Font = FontGridCell;
            grid.RowTemplate.Height = 32;

            // Томруулахад мөрүүд ашиглагдаагүй зайг дүүргэнэ
            if (grid.Columns.Count > 0 && grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.None)
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        public static Panel CreateCard(int x, int y, int w, int h)
        {
            return new CardPanel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.White
            };
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

    /// <summary>Булантай хүрээтэй карт. Double buffer + resize үед дахин зурагдана.</summary>
    public class CardPanel : Panel
    {
        public CardPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Булангийн гаднах хэсгийг эцэг контролын өнгөөр цэвэрлэнэ
            Color outer = (Parent != null && Parent.BackColor != Color.Transparent) ? Parent.BackColor : Color.White;
            g.Clear(outer);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = HaaIsTheme.GetRoundPath(rect, 12);
            using var fill = new SolidBrush(BackColor);
            using var pen = new Pen(Color.FromArgb(210, 225, 215), 1.5f);
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }
    }

    public class ModernButton : Button
    {
        private readonly bool isPrimary;
        private bool isHovered = false;
        private bool isPressed = false;
        private readonly int radius = 10;

        private bool ctorDone = false;
        private bool hasCustomBack = false; // Хэрэглэгч BackColor-ыг өөрөө онооход л true болно

        public ModernButton(bool primary)
        {
            isPrimary = primary;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.White;
            ForeColor = primary ? Color.White : HaaIsTheme.Green;
            Font = HaaIsTheme.FontButton;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { isHovered = false; isPressed = false; Invalidate(); };
            MouseDown += (s, e) => { isPressed = true; Invalidate(); };
            MouseUp += (s, e) => { isPressed = false; Invalidate(); };

            ctorDone = true;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (ctorDone) hasCustomBack = BackColor != Color.Transparent;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled) { isHovered = false; isPressed = false; }
            Invalidate();
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
            Color topColor, bottomColor, borderColor, textColor;

            if (!Enabled)
            {
                topColor = Color.FromArgb(236, 238, 237);
                bottomColor = Color.FromArgb(226, 229, 227);
                borderColor = Color.FromArgb(200, 205, 202);
                textColor = Color.FromArgb(150, 158, 153);
            }
            else if (hasCustomBack)
            {
                topColor = isHovered ? ControlPaint.Light(BackColor, 0.15f) : BackColor;
                bottomColor = ControlPaint.Dark(BackColor, 0.1f);
                borderColor = bottomColor;
                textColor = Color.White;
            }
            else if (isPrimary)
            {
                topColor = isHovered ? Color.FromArgb(46, 138, 88) : Color.FromArgb(35, 115, 73);
                bottomColor = isHovered ? HaaIsTheme.Green : Color.FromArgb(22, 78, 49);
                borderColor = Color.FromArgb(18, 65, 40);
                textColor = Color.White;
            }
            else
            {
                topColor = isHovered ? Color.FromArgb(242, 249, 245) : Color.White;
                bottomColor = isHovered ? Color.FromArgb(225, 238, 230) : Color.FromArgb(238, 245, 240);
                borderColor = HaaIsTheme.Green;
                textColor = HaaIsTheme.Green;
            }

            using (var brush = new LinearGradientBrush(btnRect, topColor, bottomColor, LinearGradientMode.Vertical))
            {
                g.FillPath(brush, btnPath);
            }

            using (var borderPen = new Pen(borderColor, 1.5f))
            {
                g.DrawPath(borderPen, btnPath);
            }

            TextRenderer.DrawText(g, Text, Font, btnRect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
