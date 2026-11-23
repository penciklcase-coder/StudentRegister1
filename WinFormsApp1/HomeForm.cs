using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class HomeForm : Form
    {
        private readonly Panel topUtilityBar = new();
        private readonly Label lblPhone = new();
        private readonly Label lblLang = new();
        private readonly Button btnTopLogin = new();
        private readonly Panel contentArea = new();
        private readonly Panel whiteHeader = new();
        private readonly Panel headerCenterGroup = new();
        private readonly Panel navBar = new();
        private readonly FlowLayoutPanel navFlow = new();
        private readonly Dictionary<string, Label> menuLabels = new();
        private string activeTab = "";

        private static readonly Color SabDarkGreen = Color.FromArgb(20, 82, 50);
        private static readonly Color SabMenuGreen = Color.FromArgb(24, 90, 56);
        private static readonly Color Gold = Color.FromArgb(255, 215, 0);

        // Фонтуудыг нэг удаа үүсгэнэ (Paint болон SwitchTab дотор дахин дахин үүсгэхгүй)
        private static readonly Font FontPhone = new("Segoe UI", 9.5F, FontStyle.Bold);
        private static readonly Font FontLang = new("Segoe UI", 9F, FontStyle.Bold);
        private static readonly Font FontLogin = new("Segoe UI", 8.5F, FontStyle.Bold);
        private static readonly Font FontLogo = new("Segoe UI", 8.5F, FontStyle.Bold);
        private static readonly Font FontSub = new("Segoe UI", 11F, FontStyle.Regular);
        private static readonly Font FontTitle = new("Segoe UI", 20F, FontStyle.Regular);
        private static readonly Font FontMenu = new("Segoe UI", 8.5F, FontStyle.Bold);
        private static readonly Font FontHero = new("Segoe UI", 24F, FontStyle.Bold);
        private static readonly Font FontCardTitle = new("Segoe UI", 18F, FontStyle.Bold);
        private static readonly Font FontCardBody = new("Segoe UI", 12F);

        private static readonly string[] Menus =
        {
            "НҮҮР", "БИДНИЙ ТУХАЙ", "СУРГАЛТ", "НЭЭЛТТЭЙ ИХ СУРГУУЛЬ",
            "СУДАЛГАА, ХАМТЫН АЖИЛЛАГАА", "СУРГУУЛИЙН ОРЧИН", "ОЮУТАН", "ЭЛСЭЛТ", "ТӨГСӨГЧ"
        };

        private static readonly Dictionary<string, string> TabBodies = new()
        {
            ["БИДНИЙ ТУХАЙ"] = "• Сургуулийн түүхэн замнал ба эрхэм зорилго\n\n• Захирлын мэндчилгээ\n\n• Удирдлагын зөвлөл болон бүтэц зохион байгуулалт",
            ["СУРГАЛТ"] = "• 5 жилийн инженерийн нэгдсэн хөтөлбөр\n\n• Мэдээллийн технологи, Програм хангамж\n\n• Агроэкологи, Инженерчлэлийн салбар хөтөлбөрүүд",
            ["НЭЭЛТТЭЙ ИХ СУРГУУЛЬ"] = "• Нээлттэй боловсролын эх сурвалжууд\n\n• Цахим номын сан болон лабораторийн орчин",
            ["СУДАЛГАА, ХАМТЫН АЖИЛЛАГАА"] = "• Япон улсын Коосэн сургуулиудтай хэрэгжүүлж буй оюутан солилцоо\n\n• Робокон болон инновацын төслүүд",
            ["СУРГУУЛИЙН ОРЧИН"] = "• Хичээлийн байр, орчин үеийн лабораториуд\n\n• Оюутны дотуур байр, спорт заал, номын сан",
            ["ОЮУТАН"] = "• Оюутны зөвлөл, клуб секцүүд\n\n• Тэтгэлэгт хөтөлбөрүүд\n\n• (Дүн болон хичээл сонголтыг баруун дээд булангийн 'Системд нэвтрэх' товчоор орж харна уу)",
            ["ЭЛСЭЛТ"] = "• Элсэлтийн журам болон бүртгэлийн хугацаа\n\n• Мэргэжил сонголтын зөвлөгөө",
            ["ТӨГСӨГЧ"] = "• Төгсөгчдийн холбоо\n\n• Ажлын байрны зуучлал болон төгсөгчдийн амжилт"
        };

        public HomeForm()
        {
            Text = "ХААИС - Дархан-Уул аймаг дахь салбар сургууль (Коосэн)";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(1060, 660);
            MinimumSize = new Size(960, 600);

            BuildTopUtilityBar();
            BuildWhiteHeader();
            BuildNavBar();

            contentArea.Dock = DockStyle.Fill;
            contentArea.BackColor = Color.FromArgb(245, 248, 246);

            // Dock дараалал: эхэнд нэмсэн нь хамгийн сүүлд байрлана (Fill), сүүлд нэмсэн нь хамгийн дээд
            Controls.Add(contentArea);
            Controls.Add(navBar);
            Controls.Add(whiteHeader);
            Controls.Add(topUtilityBar);

            // Анхны байрлалыг форм харагдахад тооцоолно (Resize event заавал гарна гэсэн баталгаа байхгүй)
            Load += (s, e) => LayoutAll();
            Resize += (s, e) => LayoutAll();

            SwitchTab("НҮҮР");
        }

        // ---------- 1. Дээд ногоон мөр ----------
        private void BuildTopUtilityBar()
        {
            topUtilityBar.Dock = DockStyle.Top;
            topUtilityBar.Height = 38;
            topUtilityBar.BackColor = SabDarkGreen;

            lblPhone.Text = "📞 70373737   🌐";
            lblPhone.Location = new Point(25, 9);
            lblPhone.AutoSize = true;
            lblPhone.ForeColor = Color.White;
            lblPhone.Font = FontPhone;
            topUtilityBar.Controls.Add(lblPhone);

            lblLang.Text = "MN | EN";
            lblLang.AutoSize = true;
            lblLang.ForeColor = Gold;
            lblLang.Font = FontLang;
            topUtilityBar.Controls.Add(lblLang);

            btnTopLogin.Text = "Системд нэвтрэх";
            btnTopLogin.Size = new Size(130, 28);
            btnTopLogin.FlatStyle = FlatStyle.Flat;
            btnTopLogin.BackColor = Color.FromArgb(163, 217, 68);
            btnTopLogin.ForeColor = Color.FromArgb(15, 60, 35);
            btnTopLogin.Font = FontLogin;
            btnTopLogin.Cursor = Cursors.Hand;
            btnTopLogin.FlatAppearance.BorderSize = 0;
            btnTopLogin.Click += (s, e) =>
            {
                using var loginPortal = new LoginForm();
                Hide();
                try { loginPortal.ShowDialog(); }
                finally { Show(); Activate(); }
            };
            topUtilityBar.Controls.Add(btnTopLogin);

            topUtilityBar.Resize += (s, e) => LayoutTopBar();
        }

        private void LayoutTopBar()
        {
            btnTopLogin.Location = new Point(topUtilityBar.Width - btnTopLogin.Width - 20,
                (topUtilityBar.Height - btnTopLogin.Height) / 2);
            lblLang.Location = new Point(btnTopLogin.Left - lblLang.Width - 12,
                (topUtilityBar.Height - lblLang.Height) / 2);
        }

        // ---------- 2. Цагаан толгой ----------
        private void BuildWhiteHeader()
        {
            whiteHeader.Dock = DockStyle.Top;
            whiteHeader.Height = 115;
            whiteHeader.BackColor = Color.White;

            headerCenterGroup.Size = new Size(820, 100);
            headerCenterGroup.BackColor = Color.White;

            var logoBox = new Panel
            {
                Location = new Point(10, 8),
                Size = new Size(85, 85),
                BackColor = Color.White
            };
            logoBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var penOuter = new Pen(SabDarkGreen, 3f);
                using var penInner = new Pen(SabDarkGreen, 1.5f);
                g.DrawEllipse(penOuter, 4, 4, 76, 76);
                g.DrawEllipse(penInner, 12, 12, 60, 60);

                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var brush = new SolidBrush(SabDarkGreen);
                g.DrawString("ХААИС\nКООСЭН", FontLogo, brush, new RectangleF(12, 12, 60, 60), sf);
            };
            headerCenterGroup.Controls.Add(logoBox);

            headerCenterGroup.Controls.Add(new Label
            {
                Text = "ХӨДӨӨ АЖ АХУЙН ИХ СУРГУУЛИЙН ДАРХАН-УУЛ АЙМАГ ДАХЬ",
                Location = new Point(115, 18),
                Size = new Size(680, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(90, 100, 95),
                Font = FontSub
            });

            headerCenterGroup.Controls.Add(new Label
            {
                Text = "ДАРХАН КООСЭН ТЕХНОЛОГИЙН КОЛЛЕЖ",
                Location = new Point(115, 46),
                Size = new Size(680, 42),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SabDarkGreen,
                Font = FontTitle
            });

            whiteHeader.Controls.Add(headerCenterGroup);
            whiteHeader.Resize += (s, e) => LayoutHeader();
        }

        private void LayoutHeader()
        {
            headerCenterGroup.Location = new Point(
                Math.Max(0, (whiteHeader.Width - headerCenterGroup.Width) / 2),
                Math.Max(0, (whiteHeader.Height - headerCenterGroup.Height) / 2));
        }

        // ---------- 3. Цэсний мөр ----------
        private void BuildNavBar()
        {
            navBar.Dock = DockStyle.Top;
            navBar.Height = 40;
            navBar.BackColor = SabMenuGreen;

            navFlow.AutoSize = true;
            navFlow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            navFlow.BackColor = Color.Transparent;
            navFlow.FlowDirection = FlowDirection.LeftToRight;
            navFlow.WrapContents = false;

            foreach (var item in Menus)
            {
                var lbl = new Label
                {
                    Text = item,
                    AutoSize = true,
                    ForeColor = Color.White,
                    Font = FontMenu,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(10, 0, 10, 0)
                };
                lbl.Click += (s, e) => SwitchTab(item);
                navFlow.Controls.Add(lbl);
                menuLabels[item] = lbl;
            }

            navBar.Controls.Add(navFlow);
            navBar.Resize += (s, e) => LayoutNav();
        }

        // Цэсний нийт өргөн нарийн цонхонд багтахгүй бол зайг (margin) автоматаар багасгана.
        private void LayoutNav()
        {
            int n = menuLabels.Count;
            if (n == 0 || navBar.Width <= 0) return;

            int textTotal = 0;
            foreach (var lbl in menuLabels.Values) textTotal += lbl.PreferredWidth;

            int available = navBar.Width - 20;
            int margin = Math.Clamp((available - textTotal) / (2 * n), 3, 12);

            navFlow.SuspendLayout();
            foreach (var lbl in menuLabels.Values)
                lbl.Margin = new Padding(margin, 0, margin, 0);
            navFlow.ResumeLayout(true);

            int totalWidth = textTotal + 2 * margin * n;
            navFlow.Location = new Point(
                Math.Max(0, (navBar.Width - totalWidth) / 2),
                Math.Max(0, (navBar.Height - navFlow.Height) / 2));
        }

        private void LayoutAll()
        {
            LayoutTopBar();
            LayoutHeader();
            LayoutNav();
        }

        // ---------- 4. Контент ----------
        private void ClearContent()
        {
            // Clear() нь контролыг dispose хийдэггүй тул гараар устгана
            while (contentArea.Controls.Count > 0)
            {
                var c = contentArea.Controls[0];
                contentArea.Controls.RemoveAt(0);
                c.Dispose();
            }
        }

        private void SwitchTab(string tabName)
        {
            if (tabName == activeTab) return;
            activeTab = tabName;

            foreach (var pair in menuLabels)
                pair.Value.ForeColor = pair.Key == tabName ? Gold : Color.White;

            contentArea.SuspendLayout();
            ClearContent();

            if (tabName == "НҮҮР")
            {
                contentArea.Padding = Padding.Empty;
                contentArea.Controls.Add(CreateHeroPanel());
            }
            else
            {
                // Дотор талын зайг Padding-ээр өгч, картыг Dock.Fill болгосноор
                // жижиг цонхонд ч тасрахгүй, томруулахад ч дагаж сунана.
                contentArea.Padding = new Padding(80, 35, 80, 35);
                TabBodies.TryGetValue(tabName, out var body);
                contentArea.Controls.Add(CreateInfoCard(tabName, body ?? ""));
            }

            contentArea.ResumeLayout(true);
        }

        private Panel CreateHeroPanel()
        {
            var heroPanel = new Panel { Dock = DockStyle.Fill };
            HaaIsTheme.SetDoubleBuffered(heroPanel); // анивчилтыг арилгана
            heroPanel.Resize += (s, e) => heroPanel.Invalidate();

            heroPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = heroPanel.ClientRectangle;
                if (r.Width == 0 || r.Height == 0) return;

                using (var bgBrush = new LinearGradientBrush(r,
                    Color.FromArgb(65, 42, 22), Color.FromArgb(22, 55, 32), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(bgBrush, r);
                }

                Point[] hills =
                {
                    new Point(0, r.Height),
                    new Point(0, r.Height / 3),
                    new Point(r.Width / 2, r.Height / 2 + 20),
                    new Point(r.Width, r.Height / 4),
                    new Point(r.Width, r.Height)
                };
                using (var hillBrush = new SolidBrush(Color.FromArgb(80, 130, 75, 30)))
                {
                    g.FillPolygon(hillBrush, hills);
                }

                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var shadowBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                {
                    g.DrawString("Ногоон их сургууль", FontHero, shadowBrush,
                        new RectangleF(2, r.Height / 2 - 28, r.Width, 60), sf);
                }
                g.DrawString("Ногоон их сургууль", FontHero, Brushes.White,
                    new RectangleF(0, r.Height / 2 - 30, r.Width, 60), sf);

                int dotX = r.Width / 2 - 20;
                int dotY = r.Height - 28;
                g.FillEllipse(Brushes.LightGray, dotX, dotY, 7, 7);
                g.FillEllipse(Brushes.White, dotX + 15, dotY - 1, 9, 9);
                g.FillEllipse(Brushes.LightGray, dotX + 32, dotY, 7, 7);
            };

            return heroPanel;
        }

        private Panel CreateInfoCard(string title, string body)
        {
            var card = HaaIsTheme.CreateCard(0, 0, 700, 340);
            card.Dock = DockStyle.Fill;

            var lblTitle = new Label
            {
                Text = title,
                Location = new Point(40, 35),
                AutoSize = true,
                Font = FontCardTitle,
                ForeColor = SabDarkGreen,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTitle);

            var lblBody = new Label
            {
                Text = body,
                Font = FontCardBody,
                ForeColor = Color.FromArgb(40, 50, 45),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblBody);

            var btnBack = HaaIsTheme.CreateButton("← Нүүр хуудас руу буцах", "btnBack", 40, 0, primary: true);
            btnBack.Size = new Size(230, 44);
            btnBack.Click += (s, e) => SwitchTab("НҮҮР");
            card.Controls.Add(btnBack);

            // Anchor-ын оронд Resize дээр гараар байрлуулна: Dock.Fill карт дээр Anchor
            // нь анхны хэмжээнээс тооцогддог тул буруу сунаж болдог.
            void LayoutCard()
            {
                lblBody.SetBounds(40, 95, Math.Max(100, card.Width - 80), Math.Max(40, card.Height - 190));
                btnBack.Location = new Point(40, Math.Max(100, card.Height - 70));
            }
            card.Resize += (s, e) => LayoutCard();
            LayoutCard();

            return card;
        }
    }
}