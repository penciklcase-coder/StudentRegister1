using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class HomeForm : Form
    {
        private readonly Panel contentArea = new();
        private readonly Dictionary<string, Label> menuLabels = new();
        private string activeTab = "НҮҮР";

        private static readonly Color SabDarkGreen = Color.FromArgb(20, 82, 50);
        private static readonly Color SabMenuGreen = Color.FromArgb(24, 90, 56);

        public HomeForm()
        {
            Text = "ХААИС - Дархан-Уул аймаг дахь салбар сургууль (Коосэн)";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(1020, 640);

            // 1. ДЭЭД УТИЛИТИЙН МӨР
            var topUtilityBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = SabDarkGreen
            };

            topUtilityBar.Controls.Add(new Label
            {
                Text = "📞 70373737   🌐",
                Location = new Point(25, 8),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            });

            topUtilityBar.Controls.Add(new Label
            {
                Text = "MN | EN",
                Location = new Point(810, 9),
                AutoSize = true,
                ForeColor = Color.FromArgb(255, 215, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            });

            var btnTopLogin = new Button
            {
                Text = "Системд нэвтрэх",
                Location = new Point(880, 4),
                Size = new Size(120, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(163, 217, 68),
                ForeColor = Color.FromArgb(15, 60, 35),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTopLogin.FlatAppearance.BorderSize = 0;
            btnTopLogin.Click += (s, e) =>
            {
                using var loginPortal = new LoginForm();
                Hide();
                loginPortal.ShowDialog();
                Show();
            };
            topUtilityBar.Controls.Add(btnTopLogin);

            // 2. ЦАГААН ТОЛГОЙ ХЭСЭГ
            var whiteHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 115,
                BackColor = Color.White
            };

            var logoBox = new Panel
            {
                Location = new Point(155, 15),
                Size = new Size(85, 85),
                BackColor = Color.White
            };
            logoBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var penOuter = new Pen(SabDarkGreen, 3f);
                using var penInner = new Pen(SabDarkGreen, 1.5f);
                e.Graphics.DrawEllipse(penOuter, 4, 4, 76, 76);
                e.Graphics.DrawEllipse(penInner, 12, 12, 60, 60);

                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString("ХААИС\nКООСЭН", new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    new SolidBrush(SabDarkGreen), new RectangleF(12, 12, 60, 60), sf);
            };
            whiteHeader.Controls.Add(logoBox);

            whiteHeader.Controls.Add(new Label
            {
                Text = "ХӨДӨӨ АЖ АХУЙН ИХ СУРГУУЛИЙН ДАРХАН-УУЛ АЙМАГ ДАХЬ",
                Location = new Point(260, 24),
                Size = new Size(650, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(90, 100, 95),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular)
            });

            whiteHeader.Controls.Add(new Label
            {
                Text = "ДАРХАН КООСЭН ТЕХНОЛОГИЙН КОЛЛЕЖ",
                Location = new Point(250, 52),
                Size = new Size(670, 42),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SabDarkGreen,
                Font = new Font("Segoe UI", 19F, FontStyle.Regular)
            });

            // 3. НОГООН ЦЭСНИЙ МӨР (Зайг шахаж бүх дэлгэцэнд багтаав)
            var navBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = SabMenuGreen,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(35, 10, 0, 0)
            };

            string[] menus = {
                "НҮҮР", "БИДНИЙ ТУХАЙ", "СУРГАЛТ", "НЭЭЛТТЭЙ ИХ СУРГУУЛЬ",
                "СУДАЛГАА, ХАМТЫН АЖИЛЛАГАА", "СУРГУУЛИЙН ОРЧИН", "ОЮУТАН", "ЭЛСЭЛТ", "ТӨГСӨГЧ"
            };

            foreach (var item in menus)
            {
                var lbl = new Label
                {
                    Text = item,
                    AutoSize = true,
                    ForeColor = item == "НҮҮР" ? Color.FromArgb(255, 215, 0) : Color.White,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 14, 0) // Хоорондох зайг багасгаж багтаамж сайжруулав
                };
                lbl.Click += (s, e) => SwitchTab(item);
                navBar.Controls.Add(lbl);
                menuLabels[item] = lbl;
            }

            // 4. ГОЛЫН КОНТЕНТ ХЭСЭГ
            contentArea.Dock = DockStyle.Fill;
            contentArea.BackColor = Color.FromArgb(245, 248, 246);

            Controls.Add(contentArea);
            Controls.Add(navBar);
            Controls.Add(whiteHeader);
            Controls.Add(topUtilityBar);

            SwitchTab("НҮҮР");
        }

        private void SwitchTab(string tabName)
        {
            activeTab = tabName;
            foreach (var pair in menuLabels)
            {
                pair.Value.ForeColor = pair.Key == tabName ? Color.FromArgb(255, 215, 0) : Color.White;
            }

            contentArea.SuspendLayout();
            contentArea.Controls.Clear();

            if (tabName == "НҮҮР")
            {
                var heroPanel = new Panel
                {
                    Dock = DockStyle.Fill
                };

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

                    Point[] hills1 = {
                        new Point(0, r.Height),
                        new Point(0, r.Height / 3),
                        new Point(r.Width / 2, r.Height / 2 + 20),
                        new Point(r.Width, r.Height / 4),
                        new Point(r.Width, r.Height)
                    };
                    using (var hillBrush1 = new SolidBrush(Color.FromArgb(80, 130, 75, 30)))
                    {
                        g.FillPolygon(hillBrush1, hills1);
                    }

                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using var shadowFont = new Font("Segoe UI", 22F, FontStyle.Bold);
                    g.DrawString("Ногоон их сургууль", shadowFont, new SolidBrush(Color.FromArgb(140, 0, 0, 0)),
                        new RectangleF(2, r.Height / 2 - 28, r.Width, 60), sf);
                    g.DrawString("Ногоон их сургууль", shadowFont, Brushes.White,
                        new RectangleF(0, r.Height / 2 - 30, r.Width, 60), sf);

                    int dotX = r.Width / 2 - 20;
                    int dotY = r.Height - 28;
                    g.FillEllipse(Brushes.LightGray, dotX, dotY, 7, 7);
                    g.FillEllipse(Brushes.White, dotX + 15, dotY - 1, 9, 9);
                    g.FillEllipse(Brushes.LightGray, dotX + 32, dotY, 7, 7);
                };

                contentArea.Controls.Add(heroPanel);
            }
            else
            {
                string title = tabName;
                string body = "";

                switch (tabName)
                {
                    case "БИДНИЙ ТУХАЙ":
                        body = "• Сургуулийн түүхэн замнал ба эрхэм зорилго\n\n• Захирлын мэндчилгээ\n\n• Удирдлагын зөвлөл болон бүтэц зохион байгуулалт";
                        break;
                    case "СУРГАЛТ":
                        body = "• 5 жилийн инженерийн нэгдсэн хөтөлбөр\n\n• Мэдээллийн технологи, Програм хангамж\n\n• Агроэкологи, Инженерчлэлийн салбар хөтөлбөрүүд";
                        break;
                    case "НЭЭЛТТЭЙ ИХ СУРГУУЛЬ":
                        body = "• Нээлттэй боловсролын эх сурвалжууд\n\n• Цахим номын сан болон лабораторийн орчин";
                        break;
                    case "СУДАЛГАА, ХАМТЫН АЖИЛЛАГАА":
                        body = "• Япон улсын Коосэн сургуулиудтай хэрэгжүүлж буй оюутан солилцоо\n\n• Робокон болон инновацын төслүүд";
                        break;
                    case "СУРГУУЛИЙН ОРЧИН":
                        body = "• Хичээлийн байр, орчин үеийн лабораториуд\n\n• Оюутны дотуур байр, спорт заал, номын сан";
                        break;
                    case "ОЮУТАН":
                        body = "• Оюутны зөвлөл, клуб секцүүд\n\n• Тэтгэлэгт хөтөлбөрүүд\n\n• (Дүн болон хичээл сонголтыг баруун дээд булангийн 'Системд нэвтрэх' товчоор орж харна уу)";
                        break;
                    case "ЭЛСЭЛТ":
                        body = "• Элсэлтийн журам болон бүртгэлийн хугацаа\n\n• Мэргэжил сонголтын зөвлөгөө";
                        break;
                    case "ТӨГСӨГЧ":
                        body = "• Төгсөгчдийн холбоо\n\n• Ажлын байрны зуучлал болон төгсөгчдийн амжилт";
                        break;
                }

                var infoCard = HaaIsTheme.CreateCard(60, 35, 900, 360);
                infoCard.Controls.Add(new Label
                {
                    Text = title,
                    Location = new Point(35, 30),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    ForeColor = SabDarkGreen
                });
                infoCard.Controls.Add(new Label
                {
                    Text = body,
                    Location = new Point(35, 85),
                    Size = new Size(820, 190),
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = Color.FromArgb(40, 50, 45)
                });

                var btnBack = HaaIsTheme.CreateButton("← Нүүр хуудас руу буцах", "btnBack", 35, 290, primary: true);
                btnBack.Size = new Size(230, 42);
                btnBack.Click += (s, e) => SwitchTab("НҮҮР");
                infoCard.Controls.Add(btnBack);

                contentArea.Controls.Add(infoCard);
            }

            contentArea.ResumeLayout();
        }
    }
}