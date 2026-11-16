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
            Text = "ХААИС - Оюутны Нэгдсэн Систем";
            HaaIsTheme.StyleForm(this);
            ClientSize = new Size(840, 560);

            // 1. Дээд ногоон толгой хэсэг
            var topBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = HaaIsTheme.Green
            };
            topBanner.Controls.Add(new Label
            {
                Text = "ХӨДӨӨ АЖ АХУЙН ИХ СУРГУУЛЬ\nMongolian University of Life Sciences",
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = HaaIsTheme.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            });
            Controls.Add(topBanner);

            // 2. Вэб маягийн цэсний мөр (Дарахад доорх контент солигдоно)
            var subMenu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.FromArgb(40, 110, 85)
            };

            string[] menuItems = { "Нүүр", "Удирдах зөвлөл", "Сургалт", "Эрдэм шинжилгээ", "Оюутан", "Холбоо барих" };
            int menuX = 25;
            foreach (var item in menuItems)
            {
                var lblMenu = new Label
                {
                    Text = item,
                    Location = new Point(menuX, 11),
                    AutoSize = true,
                    ForeColor = item == "Нүүр" ? HaaIsTheme.Yellow : Color.White,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                lblMenu.Click += (s, e) => SwitchTab(item);
                subMenu.Controls.Add(lblMenu);
                menuLabels[item] = lblMenu;
                menuX += 130;
            }
            Controls.Add(subMenu);

            // 3. Доод холбоо барих мөр
            var footer = new Label
            {
                Text = "ХОЛБОО БАРИХ: 📞 976-70001077 | ✉️ info@muls.edu.mn | 🌐 muls.edu.mn",
                Dock = DockStyle.Bottom,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.DarkSlateGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            Controls.Add(footer);

            // 4. Голын мэдээлэл солигдох хэсэг
            contentArea.Dock = DockStyle.Fill;
            contentArea.BackColor = HaaIsTheme.White;
            Controls.Add(contentArea);
            contentArea.BringToFront();

            // Анх ачаалахад "Нүүр" цэсийг харуулна
            SwitchTab("Нүүр");
        }

        private void SwitchTab(string tabName)
        {
            // Идэвхтэй цэсний өнгийг шар болгож ялгана
            foreach (var pair in menuLabels)
            {
                pair.Value.ForeColor = pair.Key == tabName ? HaaIsTheme.Yellow : Color.White;
            }

            contentArea.Controls.Clear();

            if (tabName == "Нүүр")
            {
                contentArea.Controls.Add(new Label
                {
                    Text = "ХААИС-ийн Нэгдсэн Мэдээллийн Системд Тавтай Морилно Уу",
                    Location = new Point(140, 45),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = HaaIsTheme.Green
                });

                var btnStudent = HaaIsTheme.CreateButton("👨‍🎓 Оюутны Нэвтрэх / Бүртгэл", "btnStudent", 220, 115, primary: true);
                btnStudent.Size = new Size(400, 50);
                btnStudent.Click += (s, e) =>
                {
                    using var login = new LoginForm();
                    Hide();
                    login.ShowDialog();
                    Show();
                };
                contentArea.Controls.Add(btnStudent);

                var btnTeacher = HaaIsTheme.CreateButton("👨‍🏫 Багшийн Портал (Мөн админ орох)", "btnTeacher", 220, 185, primary: false);
                btnTeacher.Size = new Size(400, 50);
                btnTeacher.Click += (s, e) =>
                {
                    using var tLogin = new TeacherLoginForm();
                    Hide();
                    tLogin.ShowDialog();
                    Show();
                };
                contentArea.Controls.Add(btnTeacher);
            }
            else
            {
                // Бусад цэсүүд дээр дарахад тухайн хэсгийн мэдээллийг харуулна
                string title = "";
                string body = "";

                switch (tabName)
                {
                    case "Удирдах зөвлөл":
                        title = "ХААИС-ийн Удирдах Зөвлөл ба Бүтэц";
                        body = "• Ректорын ажлын алба\n• Сургалт, эрх зүйн газар\n• Эрдэм шинжилгээ, инновацын газар\n• Санхүү, хөрөнгө оруулалтын газар\n• Бүрэлдэхүүн 6 сургууль, эрдэм шинжилгээний 4 хүрээлэн";
                        break;
                    case "Сургалт":
                        title = "Сургалтын Хөтөлбөр ба Мэргэжлүүд";
                        body = "• Бакалаврын өдрийн болон эчнээ хөтөлбөр (40+ мэргэжил)\n• Магистр, докторын сургалт\n• Хамтарсан 2+2 олон улсын хөтөлбөрүүд\n• Улирлын хичээл сонголт, кредит тооцооны систем";
                        break;
                    case "Эрдэм шинжилгээ":
                        title = "Эрдэм Шинжилгээ, Инновац";
                        body = "• Хөдөө аж ахуйн биотехнологийн судалгаа\n• Мал эмнэлгийн хүрээлэнгийн шинэ бүтээлүүд\n• Ургамал газар тариалангийн туршилт судалгааны төв\n• Оюутны эрдэм шинжилгээний бага хурал (Жил бүр)";
                        break;
                    case "Оюутан":
                        title = "Оюутны Үйлчилгээ ба Тэтгэлэг";
                        body = "• Оюутны дотуур байрны бүртгэл\n• Ректорын нэрэмжит тэтгэлэг болон Боловсролын зээлийн сан\n• Оюутны клуб, спортын секцүүд\n• Хичээлийн хуваарь, дүнгийн мэдээллийг 'Нүүр' хэсгээс нэвтэрч харна уу.";
                        break;
                    case "Холбоо барих":
                        title = "Хаяг Байршил ба Холбоо Барих";
                        body = "📍 Хаяг: Улаанбаатар хот, Хан-Уул дүүрэг, 11-р хороо, Зайсан\n📞 Утас: 976-70001077, 976-75107777\n✉️ Цахим шуудан: info@muls.edu.mn\n🕒 Ажлын цаг: Даваа - Баасан 08:30 - 17:30";
                        break;
                }

                contentArea.Controls.Add(new Label
                {
                    Text = title,
                    Location = new Point(50, 35),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    ForeColor = HaaIsTheme.Green
                });

                var infoBox = new Label
                {
                    Text = body,
                    Location = new Point(50, 85),
                    Size = new Size(720, 220),
                    Font = new Font("Segoe UI", 11.5F),
                    ForeColor = Color.FromArgb(45, 55, 50)
                };
                contentArea.Controls.Add(infoBox);

                var btnBackHome = HaaIsTheme.CreateButton("← Нүүр хуудас руу буцах", "btnBackHome", 50, 310, primary: true);
                btnBackHome.Size = new Size(240, 42);
                btnBackHome.Click += (s, e) => SwitchTab("Нүүр");
                contentArea.Controls.Add(btnBackHome);
            }
        }
    }
}