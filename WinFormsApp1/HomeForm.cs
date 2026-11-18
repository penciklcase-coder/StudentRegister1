using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class HomeForm : Form
    {
        private readonly Panel contentArea = new();
        private readonly Dictionary<string, Label> menuLabels = new();

        public HomeForm()
        {
            Text = "КООСЭН - Оюутны Нэгдсэн Систем";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(880, 580);

            // 1. Дээд толгой хэсэг
            var topBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = HaaIsTheme.Green
            };

            var badge = new Label
            {
                AutoSize = false,
                Text = "КООСЭН",
                Location = new Point(28, 18),
                Size = new Size(76, 46),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = HaaIsTheme.Yellow,
                ForeColor = HaaIsTheme.Green,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold)
            };
            HaaIsTheme.ApplyRoundedCorners(badge, 10);
            topBanner.Controls.Add(badge);

            topBanner.Controls.Add(new Label
            {
                Text = "ДАРХАН КООСЭН ТЕХНОЛОГИЙН КОЛЛЕЖ",
                Location = new Point(116, 17),
                AutoSize = true,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            });

            topBanner.Controls.Add(new Label
            {
                Text = "Darkhan Kosen College of Technology • Сургалтын Дижитал Орчин",
                Location = new Point(118, 44),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 225, 215),
                Font = new Font("Segoe UI", 9.5F)
            });
            Controls.Add(topBanner);

            // 2. Дугуй Bubble цэсний мөр
            var subMenu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(30, 88, 70)
            };

            string[] menuItems = { "Нүүр", "Удирдах зөвлөл", "Сургалт", "Эрдэм шинжилгээ", "Оюутан", "Холбоо барих" };
            int menuX = 26;
            foreach (var item in menuItems)
            {
                int pillWidth = item.Length > 10 ? 142 : 112;
                var lblMenu = new Label
                {
                    AutoSize = false,
                    Text = item,
                    Location = new Point(menuX, 9),
                    Size = new Size(pillWidth, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                HaaIsTheme.ApplyRoundedCorners(lblMenu, 16);
                lblMenu.Click += (s, e) => SwitchTab(item);
                subMenu.Controls.Add(lblMenu);
                menuLabels[item] = lblMenu;
                menuX += pillWidth + 10;
            }
            Controls.Add(subMenu);

            // 3. Доод мөр
            var footer = new Label
            {
                Text = "📍 Дархан-Уул аймаг, Дархан Коосэн   |   📞 976-7037-XXXX   |   ✉️ academic@kosen.edu.mn",
                Dock = DockStyle.Bottom,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = HaaIsTheme.White,
                ForeColor = HaaIsTheme.MutedText,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            Controls.Add(footer);

            // 4. Голын контент хэсэг
            contentArea.Dock = DockStyle.Fill;
            contentArea.BackColor = HaaIsTheme.BgSoft;
            Controls.Add(contentArea);
            contentArea.BringToFront();

            SwitchTab("Нүүр");
        }

        private void SwitchTab(string tabName)
        {
            foreach (var pair in menuLabels)
            {
                bool active = pair.Key == tabName;
                pair.Value.BackColor = active ? HaaIsTheme.Yellow : Color.Transparent;
                pair.Value.ForeColor = active ? HaaIsTheme.Green : HaaIsTheme.White;
            }

            contentArea.Controls.Clear();

            if (tabName == "Нүүр")
            {
                var heroCard = HaaIsTheme.CreateCard(28, 20, 824, 96, 14);
                heroCard.BackColor = HaaIsTheme.MintBg;
                heroCard.Controls.Add(new Label
                {
                    Text = "Тавтай морилно уу — Дархан Коосэн Сургалтын Дижитал Орчин",
                    Location = new Point(24, 18),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                    ForeColor = HaaIsTheme.Green
                });
                heroCard.Controls.Add(new Label
                {
                    Text = "Цахилгаан, Механик болон Мэдээллийн технологийн инженерийн сургалтын нэгдсэн систем.",
                    Location = new Point(24, 52),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = HaaIsTheme.MutedText
                });
                contentArea.Controls.Add(heroCard);

                // Нэгдсэн Нэвтрэх болон Бүртгэлийн Карт (Оюутан, Багш, Админ бүгд нэг цонхоор орно)
                var portalCard = HaaIsTheme.CreateCard(28, 134, 824, 250, 16);

                var tagStudent = HaaIsTheme.CreateBubbleOption("👨‍🎓 ОЮУТАН", 30, 24, 135, 32, true);
                var tagTeacher = HaaIsTheme.CreateBubbleOption("👨‍🏫 БАГШ", 175, 24, 125, 32, false);
                portalCard.Controls.Add(tagStudent);
                portalCard.Controls.Add(tagTeacher);

                portalCard.Controls.Add(new Label
                {
                    Text = "Сургалтын Удирдлагын Системд Нэвтрэх",
                    Location = new Point(30, 72),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    ForeColor = HaaIsTheme.Green
                });

                portalCard.Controls.Add(new Label
                {
                    Text = "Оюутнууд хичээл сонголт, дүнгийн мэдээллээ харах болон шинээр бүртгүүлэх боломжтой.\nБагш болон системийн удирдлага 'Багш' хэсгийг сонгон өөрийн эрхээр нэвтэрнэ үү.",
                    Location = new Point(30, 110),
                    Size = new Size(740, 48),
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = HaaIsTheme.MutedText
                });

                var btnOpenLogin = HaaIsTheme.CreateButton("Системд Нэвтрэх / Бүртгүүлэх →", "btnOpenLogin", 30, 176, primary: true);
                btnOpenLogin.Size = new Size(340, 46);
                btnOpenLogin.Click += (s, e) =>
                {
                    using var login = new LoginForm();
                    Hide();
                    login.ShowDialog();
                    Show();
                };
                portalCard.Controls.Add(btnOpenLogin);

                contentArea.Controls.Add(portalCard);
            }
            else
            {
                string title = "";
                string body = "";

                switch (tabName)
                {
                    case "Удирдах зөвлөл":
                        title = "Дархан Коосэн - Удирдах Зөвлөл ба Бүтэц";
                        body = "• Захирлын ажлын алба\n• Сургалтын алба\n• Инженерийн тэнхимүүд\n• Оюутны хөгжил, дотуур байрны зөвлөл";
                        break;
                    case "Сургалт":
                        title = "Инженерийн Сургалтын Хөтөлбөрүүд";
                        body = "• Цахилгааны инженер (Electric Engineer)\n• Механик инженер (Mechanic Engineer)\n• Мэдээллийн технологийн инженер (IT Engineer)\n• Япон стандартын Коосэн практик инженерийн хөтөлбөр";
                        break;
                    case "Эрдэм шинжилгээ":
                        title = "Инновац ба Инженерийн Лаборатори";
                        body = "• Роботик болон автоматжуулалтын клуб\n• Програм хангамж, систем хөгжүүлэлт\n• Электроник, тоон хэлхээний туршилтын лаборатори\n• Оюутны техник технологийн олимпиад, төслүүд";
                        break;
                    case "Оюутан":
                        title = "Оюутны Үйлчилгээ ба Тэтгэлэг";
                        body = "• Оюутны дотуур байрны бүртгэл\n• Япон хэл, мэргэжлийн дадлага ба солилцооны хөтөлбөр\n• Роботик, спорт, урлагийн клубууд\n• Хичээлийн хуваарь, дүнгийн мэдээллийг 'Нүүр' хэсгээс нэвтэрч харна уу.";
                        break;
                    case "Холбоо барих":
                        title = "Хаяг Байршил ба Холбоо Барих";
                        body = "📍 Хаяг: Дархан-Уул аймаг, Дархан сум, Дархан Коосэн\n📞 Утас: 976-7037-XXXX\n✉️ Цахим шуудан: academic@kosen.edu.mn\n🕒 Ажлын цаг: Даваа - Баасан 08:30 - 17:30";
                        break;
                }

                var infoCard = HaaIsTheme.CreateCard(28, 24, 824, 360, 16);

                infoCard.Controls.Add(new Label
                {
                    Text = title,
                    Location = new Point(32, 28),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    ForeColor = HaaIsTheme.Green
                });

                infoCard.Controls.Add(new Label
                {
                    Text = body,
                    Location = new Point(32, 75),
                    Size = new Size(740, 190),
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = Color.FromArgb(45, 55, 50)
                });

                var btnBackHome = HaaIsTheme.CreateButton("← Нүүр хуудас руу буцах", "btnBackHome", 32, 285, primary: true);
                btnBackHome.Size = new Size(240, 42);
                btnBackHome.Click += (s, e) => SwitchTab("Нүүр");
                infoCard.Controls.Add(btnBackHome);

                contentArea.Controls.Add(infoCard);
            }
        }
    }
}