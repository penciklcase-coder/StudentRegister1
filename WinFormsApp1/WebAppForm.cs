using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public class WebAppForm : Form
    {
        private readonly WebView2 webView = new();
        private const string ConnectionString = "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public WebAppForm()
        {
            Text = "Дархан Коосэн - Сургалтын Нэгдсэн Систем (LMS)";
            ClientSize = new Size(1260, 780);
            StartPosition = FormStartPosition.CenterScreen;

            webView.Dock = DockStyle.Fill;
            Controls.Add(webView);

            Load += WebAppForm_Load;
        }

        private async void WebAppForm_Load(object? sender, EventArgs e)
        {
            await webView.EnsureCoreWebView2Async();

            string webFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.kosen", webFolder, CoreWebView2HostResourceAccessKind.Allow);

            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webView.CoreWebView2.Navigate("https://app.kosen/index.html");
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            var root = doc.RootElement;
            string action = root.GetProperty("action").GetString() ?? "";

            switch (action)
            {
                case "login":
                    HandleLogin(
                        root.GetProperty("role").GetString() ?? "",
                        root.GetProperty("id").GetString() ?? "",
                        root.GetProperty("password").GetString() ?? ""
                    );
                    break;

                // Нүүр хуудас ба Элсэлт (Public)
                case "getPublicData":
                    SendPublicData();
                    break;

                case "submitAdmission":
                    SubmitAdmission(
                        root.GetProperty("fullName").GetString() ?? "",
                        root.GetProperty("registerNo").GetString() ?? "",
                        root.GetProperty("phone").GetString() ?? "",
                        root.GetProperty("programName").GetString() ?? "",
                        root.GetProperty("eyeshScore").GetInt32(),
                        root.GetProperty("discount").GetInt32(),
                        root.GetProperty("finalTuition").GetInt32()
                    );
                    break;

                // Оюутны үйлдлүүд
                case "getStudentData":
                    SendStudentData(root.GetProperty("studentId").GetString() ?? "");
                    break;

                case "enrollCourse":
                    EnrollCourse(
                        root.GetProperty("studentId").GetString() ?? "",
                        root.GetProperty("courseName").GetString() ?? "",
                        root.GetProperty("credit").GetInt32()
                    );
                    break;

                case "unenrollCourse":
                    UnenrollCourse(
                        root.GetProperty("studentId").GetString() ?? "",
                        root.GetProperty("courseName").GetString() ?? ""
                    );
                    break;

                case "getCourseMaterials":
                    SendCourseMaterials(root.GetProperty("courseName").GetString() ?? "");
                    break;

                // Багшийн үйлдлүүд
                case "getTeacherData":
                    SendTeacherData();
                    break;

                case "teacherAddCourse":
                    TeacherAddCourse(
                        root.GetProperty("courseName").GetString() ?? "",
                        root.GetProperty("credit").GetInt32(),
                        root.GetProperty("department").GetString() ?? ""
                    );
                    break;

                case "teacherAddMaterial":
                    TeacherAddMaterial(
                        root.GetProperty("courseName").GetString() ?? "",
                        root.GetProperty("moduleTitle").GetString() ?? "",
                        root.GetProperty("itemTitle").GetString() ?? "",
                        root.GetProperty("itemType").GetString() ?? "lecture",
                        root.GetProperty("description").GetString() ?? "",
                        root.GetProperty("fileName").GetString() ?? ""
                    );
                    break;

                case "teacherDeleteMaterial":
                    TeacherDeleteMaterial(root.GetProperty("id").GetInt32());
                    break;

                case "teacherSaveGrade":
                    TeacherSaveGrade(
                        root.GetProperty("studentId").GetString() ?? "",
                        root.GetProperty("courseName").GetString() ?? "",
                        root.GetProperty("credit").GetInt32(),
                        root.GetProperty("score").GetInt32()
                    );
                    break;

                case "teacherDeleteGrade":
                    TeacherDeleteGrade(
                        root.GetProperty("studentId").GetString() ?? "",
                        root.GetProperty("courseName").GetString() ?? ""
                    );
                    break;

                case "teacherAddClassEvent":
                    TeacherAddClassEvent(
                        root.GetProperty("courseName").GetString() ?? "",
                        root.GetProperty("title").GetString() ?? "",
                        root.GetProperty("date").GetString() ?? "",
                        root.GetProperty("time").GetString() ?? "09:00"
                    );
                    break;

                // Админы үйлдлүүд
                case "getAdminData":
                    SendAdminData();
                    break;

                case "adminAddStudent":
                    AdminAddStudent(
                        root.GetProperty("lastName").GetString() ?? "",
                        root.GetProperty("firstName").GetString() ?? "",
                        root.GetProperty("studentId").GetString() ?? "",
                        root.GetProperty("email").GetString() ?? "",
                        root.GetProperty("major").GetString() ?? "",
                        root.GetProperty("password").GetString() ?? ""
                    );
                    break;

                case "adminDeleteStudent":
                    AdminDeleteStudent(root.GetProperty("studentId").GetString() ?? "");
                    break;

                case "adminAddTeacher":
                    AdminAddTeacher(
                        root.GetProperty("teacherId").GetString() ?? "",
                        root.GetProperty("fullName").GetString() ?? "",
                        root.GetProperty("department").GetString() ?? "",
                        root.GetProperty("password").GetString() ?? ""
                    );
                    break;

                case "adminDeleteTeacher":
                    AdminDeleteTeacher(root.GetProperty("teacherId").GetString() ?? "");
                    break;

                case "adminAddCourse":
                    AdminAddCourse(
                        root.GetProperty("courseName").GetString() ?? "",
                        root.GetProperty("credit").GetInt32(),
                        root.GetProperty("department").GetString() ?? ""
                    );
                    break;

                case "adminDeleteCourse":
                    AdminDeleteCourse(root.GetProperty("courseName").GetString() ?? "");
                    break;

                case "adminBroadcastNotif":
                    AdminBroadcastNotif(root.GetProperty("message").GetString() ?? "");
                    break;

                case "adminAddNews":
                    AdminAddNews(
                        root.GetProperty("title").GetString() ?? "",
                        root.GetProperty("category").GetString() ?? "Мэдээ",
                        root.GetProperty("summary").GetString() ?? "",
                        root.GetProperty("content").GetString() ?? "",
                        root.GetProperty("imageColor").GetString() ?? "#0e4d2d",
                        root.GetProperty("imagePosition").GetString() ?? "top",
                        root.GetProperty("imageBase64").GetString() ?? "",
                        root.GetProperty("imageFileName").GetString() ?? ""
                    );
                    break;

                case "incrementNewsView":
                    IncrementNewsView(root.GetProperty("id").GetInt32());
                    break;

                case "uploadAvatar":
                    UploadAvatar(
                        root.GetProperty("targetRole").GetString() ?? "",
                        root.GetProperty("targetId").GetString() ?? "",
                        root.GetProperty("imageBase64").GetString() ?? "",
                        root.GetProperty("imageFileName").GetString() ?? ""
                    );
                    break;

                case "changePassword":
                    ChangePassword(
                        root.GetProperty("role").GetString() ?? "",
                        root.GetProperty("userId").GetString() ?? "",
                        root.GetProperty("oldPass").GetString() ?? "",
                        root.GetProperty("newPass").GetString() ?? ""
                    );
                    break;

                case "adminDeleteNews":
                    AdminDeleteNews(root.GetProperty("id").GetInt32());
                    break;

                case "adminToggleAdmission":
                    AdminToggleAdmission(root.GetProperty("isOpen").GetBoolean());
                    break;

                case "adminUpdateAdmissionStatus":
                    AdminUpdateAdmissionStatus(
                        root.GetProperty("id").GetInt32(),
                        root.GetProperty("status").GetString() ?? "Хүлээгдэж буй"
                    );
                    break;

                case "adminConvertApplicant":
                    AdminConvertApplicant(root.GetProperty("id").GetInt32());
                    break;

                case "getPrivateFiles":
                    SendPrivateFiles(root.GetProperty("userId").GetString() ?? "");
                    break;

                case "uploadPrivateFile":
                    UploadPrivateFile(
                        root.GetProperty("userId").GetString() ?? "",
                        root.GetProperty("fileName").GetString() ?? "file",
                        root.GetProperty("fileSize").GetString() ?? "0 KB",
                        root.GetProperty("fileBase64").GetString() ?? ""
                    );
                    break;

                case "deletePrivateFile":
                    DeletePrivateFile(
                        root.GetProperty("id").GetInt32(),
                        root.GetProperty("userId").GetString() ?? ""
                    );
                    break;

                case "getCourseParticipants":
                    SendCourseParticipants(root.GetProperty("courseName").GetString() ?? "");
                    break;

                case "getGroupChats":
                    SendGroupChats(root.GetProperty("userId").GetString() ?? "");
                    break;

                case "createChatGroup":
                    var members = new List<string>();
                    if (root.TryGetProperty("members", out var memArr) && memArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var m in memArr.EnumerateArray())
                        {
                            string? uid = m.GetString();
                            if (!string.IsNullOrWhiteSpace(uid)) members.Add(uid);
                        }
                    }
                    CreateChatGroup(
                        root.GetProperty("groupName").GetString() ?? "Шинэ групп",
                        root.GetProperty("createdBy").GetString() ?? "",
                        members
                    );
                    break;

                case "getGroupMessages":
                    SendGroupMessages(root.GetProperty("groupId").GetInt32());
                    break;

                case "sendGroupMessage":
                    SendGroupMessage(
                        root.GetProperty("groupId").GetInt32(),
                        root.GetProperty("senderId").GetString() ?? "",
                        root.GetProperty("senderName").GetString() ?? "",
                        root.GetProperty("messageText").GetString() ?? ""
                    );
                    break;

                case "adminAddPayment":
                    AdminAddPayment(
                        root.GetProperty("studentId").GetString() ?? "",
                        root.GetProperty("totalFee").GetInt32(),
                        root.GetProperty("discountAmount").GetInt32(),
                        root.GetProperty("paidAmount").GetInt32()
                    );
                    break;

                case "adminMarkPaymentPaid":
                    AdminMarkPaymentPaid(root.GetProperty("id").GetInt32());
                    break;

                case "adminAddStaff":
                    AdminAddStaff(
                        root.GetProperty("username").GetString() ?? "",
                        root.GetProperty("password").GetString() ?? "",
                        root.GetProperty("fullName").GetString() ?? "",
                        root.GetProperty("role").GetString() ?? "editor",
                        root.GetProperty("department").GetString() ?? ""
                    );
                    break;

                case "adminDeleteStaff":
                    AdminDeleteStaff(root.GetProperty("username").GetString() ?? "");
                    break;

                // Календарь, Мэдэгдэл, Чат
                case "getCalendarEvents":
                    SendCalendarEvents(root.GetProperty("userId").GetString() ?? "");
                    break;

                case "addCalendarEvent":
                    AddCalendarEvent(
                        root.GetProperty("userId").GetString() ?? "",
                        root.GetProperty("courseName").GetString() ?? "",
                        root.GetProperty("title").GetString() ?? "",
                        root.GetProperty("date").GetString() ?? "",
                        root.GetProperty("time").GetString() ?? "09:00"
                    );
                    break;

                case "getNotifications":
                    SendNotifications(root.GetProperty("userId").GetString() ?? "");
                    break;

                case "markNotifRead":
                    MarkNotifRead(
                        root.GetProperty("userId").GetString() ?? "",
                        root.GetProperty("notifId").GetInt32()
                    );
                    break;

                case "markAllNotifsRead":
                    MarkAllNotifsRead(root.GetProperty("userId").GetString() ?? "");
                    break;

                case "getContacts":
                    SendContacts(root.GetProperty("userId").GetString() ?? "");
                    break;

                case "getMessages":
                    SendMessages(
                        root.GetProperty("userId").GetString() ?? "",
                        root.GetProperty("partnerId").GetString() ?? ""
                    );
                    break;

                case "sendMessage":
                    SaveChatMessage(
                        root.GetProperty("senderId").GetString() ?? "",
                        root.GetProperty("senderName").GetString() ?? "",
                        root.GetProperty("receiverId").GetString() ?? "",
                        root.GetProperty("text").GetString() ?? ""
                    );
                    break;
            }
        }

        // ================= 1. НЭВТРЭХ (AUTH) =================
        private void HandleLogin(string role, string id, string password)
        {
            if (role == "admin")
            {
                try
                {
                    using var conn = new MySqlConnection(ConnectionString);
                    conn.Open();
                    string query = "SELECT full_name, role, department, avatar_url FROM staff WHERE username = @u AND password = @p";
                    using var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", id);
                    cmd.Parameters.AddWithValue("@p", password);

                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        SendJson(new
                        {
                            action = "loginResult",
                            success = true,
                            role = "admin",
                            staffRole = r.GetString("role"),
                            id,
                            firstName = r.GetString("full_name"),
                            lastName = "",
                            email = $"{id}@sab.edu.mn",
                            major = r.IsDBNull(r.GetOrdinal("department")) ? "Удирдлага" : r.GetString("department"),
                            avatarUrl = r.IsDBNull(r.GetOrdinal("avatar_url")) ? "" : r.GetString("avatar_url")
                        });
                    }
                    else
                    {
                        SendJson(new { action = "loginResult", success = false, message = "Ажилтны нэвтрэх нэр эсвэл нууц үг буруу байна." });
                    }
                }
                catch (Exception ex)
                {
                    SendJson(new { action = "loginResult", success = false, message = "MySQL алдаа: " + ex.Message });
                }
                return;
            }

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                if (role == "student")
                {
                    string query = "SELECT first_name, last_name, email, major, avatar_url FROM students WHERE student_id = @id AND password = @pwd";
                    using var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@pwd", password);

                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        string fName = r.GetString("first_name");
                        string lName = r.GetString("last_name");
                        string email = r.GetString("email");
                        string major = r.GetString("major");
                        string avatarUrl = r.IsDBNull(r.GetOrdinal("avatar_url")) ? "" : r.GetString("avatar_url");
                        r.Close();

                        AddNotificationInternal(conn, id, "New sign in to your KOSEN LMS account");

                        SendJson(new
                        {
                            action = "loginResult",
                            success = true,
                            role = "student",
                            id,
                            firstName = fName,
                            lastName = lName,
                            email,
                            major,
                            avatarUrl
                        });
                    }
                    else
                    {
                        SendJson(new { action = "loginResult", success = false, message = "Оюутны код эсвэл нууц үг буруу байна." });
                    }
                }
                else if (role == "teacher")
                {
                    string query = "SELECT full_name, department, avatar_url FROM teachers WHERE teacher_id = @id AND password = @pwd";
                    using var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@pwd", password);

                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        string fullName = r.GetString("full_name");
                        string dept = r.GetString("department");
                        string avatarUrl = r.IsDBNull(r.GetOrdinal("avatar_url")) ? "" : r.GetString("avatar_url");
                        r.Close();

                        AddNotificationInternal(conn, id, "New sign in to your KOSEN LMS account");

                        SendJson(new
                        {
                            action = "loginResult",
                            success = true,
                            role = "teacher",
                            id,
                            firstName = fullName,
                            lastName = "",
                            email = $"{id}@sab.edu.mn",
                            major = dept,
                            avatarUrl
                        });
                    }
                    else
                    {
                        SendJson(new { action = "loginResult", success = false, message = "Багшийн ID эсвэл нууц үг буруу байна." });
                    }
                }
            }
            catch (Exception ex)
            {
                SendJson(new { action = "loginResult", success = false, message = "MySQL холболтын алдаа (XAMPP асаасан уу?): " + ex.Message });
            }
        }

        // ================= 2. ОЮУТНЫ ХИЧЭЭЛ & ДҮН =================
        private void SendStudentData(string studentId)
        {
            var available = new List<object>();
            var enrolled = new List<object>();
            var grades = new List<object>();
            var payments = new List<object>();

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                using (var cmd = new MySqlCommand("SELECT course_name, credit, department FROM available_courses", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        available.Add(new { name = r.GetString("course_name"), credit = r.GetInt32("credit"), department = r.GetString("department") });
                }

                using (var cmd = new MySqlCommand("SELECT course_name, credit FROM student_enrolled_courses WHERE student_id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", studentId);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                        enrolled.Add(new { name = r.GetString("course_name"), credit = r.GetInt32("credit") });
                }

                using (var cmd = new MySqlCommand("SELECT course_name, credit, score, letter_grade FROM student_grades WHERE student_id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", studentId);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                        grades.Add(new { name = r.GetString("course_name"), credit = r.GetInt32("credit"), score = r.GetInt32("score"), letter = r.GetString("letter_grade") });
                }

                using (var cmd = new MySqlCommand("SELECT id, semester, total_fee, discount_amount, paid_amount, status FROM tuition_payments WHERE student_id = @id ORDER BY id DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@id", studentId);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        payments.Add(new
                        {
                            id = r.GetInt32("id"),
                            semester = r.GetString("semester"),
                            totalFee = r.GetInt32("total_fee"),
                            discountAmount = r.GetInt32("discount_amount"),
                            paidAmount = r.GetInt32("paid_amount"),
                            status = r.GetString("status")
                        });
                    }
                }
            }
            catch { }

            SendJson(new { action = "studentData", available, enrolled, grades, payments });
        }

        private void EnrollCourse(string studentId, string courseName, int credit)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM student_enrolled_courses WHERE student_id = @id AND course_name = @course", conn))
                {
                    checkCmd.Parameters.AddWithValue("@id", studentId);
                    checkCmd.Parameters.AddWithValue("@course", courseName);
                    if (Convert.ToInt64(checkCmd.ExecuteScalar()) > 0)
                    {
                        SendJson(new { action = "toast", message = "Та энэ хичээлийг аль хэдийн сонгосон байна." });
                        return;
                    }
                }

                using (var cmd = new MySqlCommand("INSERT INTO student_enrolled_courses (student_id, course_name, credit) VALUES (@id, @course, @credit)", conn))
                {
                    cmd.Parameters.AddWithValue("@id", studentId);
                    cmd.Parameters.AddWithValue("@course", courseName);
                    cmd.Parameters.AddWithValue("@credit", credit);
                    cmd.ExecuteNonQuery();
                }

                AddNotificationInternal(conn, studentId, $"Welcome to {courseName}");
                SendJson(new { action = "toast", message = $"\"{courseName}\" хичээлийг амжилттай сонголоо!" });
                SendStudentData(studentId);
                SendNotifications(studentId);
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Алдаа: " + ex.Message });
            }
        }

        private void UnenrollCourse(string studentId, string courseName)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM student_enrolled_courses WHERE student_id = @id AND course_name = @course LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@course", courseName);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{courseName}\" хичээлийг хаслаа." });
                SendStudentData(studentId);
            }
            catch { }
        }

        // ================= 3. БАГШИЙН ҮЙЛДЛҮҮД (TEACHER) =================
        private void SendTeacherData()
        {
            var courses = new List<object>();
            var students = new List<object>();
            var grades = new List<object>();
            var materials = new List<object>();

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                using (var cmd = new MySqlCommand("SELECT course_name, credit, department FROM available_courses ORDER BY course_name ASC", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        courses.Add(new { name = r.GetString("course_name"), credit = r.GetInt32("credit"), department = r.GetString("department") });
                }

                using (var cmd = new MySqlCommand("SELECT student_id, last_name, first_name, email, major FROM students ORDER BY student_id ASC", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        students.Add(new
                        {
                            id = r.GetString("student_id"),
                            lastName = r.GetString("last_name"),
                            firstName = r.GetString("first_name"),
                            email = r.GetString("email"),
                            major = r.GetString("major")
                        });
                }

                using (var cmd = new MySqlCommand("SELECT student_id, course_name, credit, score, letter_grade FROM student_grades ORDER BY student_id ASC", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        grades.Add(new
                        {
                            studentId = r.GetString("student_id"),
                            courseName = r.GetString("course_name"),
                            credit = r.GetInt32("credit"),
                            score = r.GetInt32("score"),
                            letter = r.GetString("letter_grade")
                        });
                }

                using (var cmd = new MySqlCommand("SELECT id, course_name, module_title, item_title, item_type, description, file_name FROM course_materials ORDER BY id DESC", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        materials.Add(new
                        {
                            id = r.GetInt32("id"),
                            courseName = r.GetString("course_name"),
                            moduleTitle = r.GetString("module_title"),
                            itemTitle = r.GetString("item_title"),
                            itemType = r.IsDBNull(r.GetOrdinal("item_type")) ? "lecture" : r.GetString("item_type"),
                            description = r.IsDBNull(r.GetOrdinal("description")) ? "" : r.GetString("description"),
                            fileName = r.IsDBNull(r.GetOrdinal("file_name")) ? "" : r.GetString("file_name")
                        });
                }
            }
            catch { }

            SendJson(new { action = "teacherData", courses, students, grades, materials });
        }

        private void TeacherAddCourse(string courseName, int credit, string department)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("INSERT INTO available_courses (course_name, credit, department) VALUES (@n, @c, @d)", conn);
                cmd.Parameters.AddWithValue("@n", courseName);
                cmd.Parameters.AddWithValue("@c", credit);
                cmd.Parameters.AddWithValue("@d", department);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{courseName}\" хичээл амжилттай нэмэгдлээ!" });
                SendTeacherData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Хичээл нэмэхэд алдаа гарлаа: " + ex.Message });
            }
        }

        private void TeacherAddMaterial(string courseName, string moduleTitle, string itemTitle, string itemType, string description, string fileName)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = @"INSERT INTO course_materials (course_name, module_title, item_title, item_type, description, file_name)
                                 VALUES (@c, @m, @t, @tp, @d, @f)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@c", courseName);
                cmd.Parameters.AddWithValue("@m", moduleTitle);
                cmd.Parameters.AddWithValue("@t", itemTitle);
                cmd.Parameters.AddWithValue("@tp", itemType);
                cmd.Parameters.AddWithValue("@d", description);
                cmd.Parameters.AddWithValue("@f", fileName);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Хичээлийн материал амжилттай байршлаа!" });
                SendTeacherData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Материал нэмэхэд алдаа гарлаа: " + ex.Message });
            }
        }

        private void TeacherDeleteMaterial(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM course_materials WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Материал устгагдлаа." });
                SendTeacherData();
            }
            catch { }
        }

        private void TeacherSaveGrade(string studentId, string courseName, int credit, int score)
        {
            string letter = score >= 90 ? "A" : score >= 80 ? "B" : score >= 70 ? "C" : score >= 60 ? "D" : "F";
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO student_grades (student_id, course_name, credit, score, letter_grade) VALUES (@id, @c, @cr, @sc, @lt)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@c", courseName);
                cmd.Parameters.AddWithValue("@cr", credit);
                cmd.Parameters.AddWithValue("@sc", score);
                cmd.Parameters.AddWithValue("@lt", letter);
                cmd.ExecuteNonQuery();

                AddNotificationInternal(conn, studentId, $"Шинэ дүн орлоо: {courseName} ({score} оноо - {letter})");

                SendJson(new { action = "toast", message = $"Дүн хадгалагдлаа! ({score} - {letter})" });
                SendTeacherData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Дүн хадгалахад алдаа гарлаа: " + ex.Message });
            }
        }

        private void TeacherDeleteGrade(string studentId, string courseName)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM student_grades WHERE student_id = @id AND course_name = @c LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@c", courseName);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Дүн устгагдлаа." });
                SendTeacherData();
            }
            catch { }
        }

        private void TeacherAddClassEvent(string courseName, string title, string date, string time)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO calendar_events (user_id, course_name, title, event_date, event_time, event_type) VALUES ('ALL', @c, @t, @d, @tm, 'Course event')";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@c", courseName);
                cmd.Parameters.AddWithValue("@t", title);
                cmd.Parameters.AddWithValue("@d", date);
                cmd.Parameters.AddWithValue("@tm", time);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Нийт оюутны календарьт амжилттай нийтлэгдлээ!" });
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Алдаа: " + ex.Message });
            }
        }

        // ================= 4. АДМИНЫ ҮЙЛДЛҮҮД (ADMIN) =================
        private void SendAdminData()
        {
            var students = new List<object>();
            var teachers = new List<object>();
            var courses = new List<object>();
            var staff = new List<object>();
            var news = new List<object>();
            var admissions = new List<object>();
            var payments = new List<object>();
            long totalGrades = 0;
            bool admissionOpen = true;

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                using (var cmd = new MySqlCommand("SELECT student_id, last_name, first_name, email, major FROM students ORDER BY id DESC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        students.Add(new { id = r.GetString("student_id"), lastName = r.GetString("last_name"), firstName = r.GetString("first_name"), email = r.GetString("email"), major = r.GetString("major") });

                using (var cmd = new MySqlCommand("SELECT teacher_id, full_name, department FROM teachers ORDER BY id DESC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        teachers.Add(new { id = r.GetString("teacher_id"), name = r.GetString("full_name"), department = r.GetString("department") });

                using (var cmd = new MySqlCommand("SELECT course_name, credit, department FROM available_courses ORDER BY course_name ASC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        courses.Add(new { name = r.GetString("course_name"), credit = r.GetInt32("credit"), department = r.GetString("department") });

                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM student_grades", conn))
                    totalGrades = Convert.ToInt64(cmd.ExecuteScalar());

                using (var cmd = new MySqlCommand("SELECT setting_value FROM system_settings WHERE setting_key = 'admission_open'", conn))
                    admissionOpen = (cmd.ExecuteScalar()?.ToString() != "0");

                using (var cmd = new MySqlCommand("SELECT username, full_name, role, department FROM staff ORDER BY id ASC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        staff.Add(new { username = r.GetString("username"), fullName = r.GetString("full_name"), role = r.GetString("role"), department = r.IsDBNull(r.GetOrdinal("department")) ? "" : r.GetString("department") });

                using (var cmd = new MySqlCommand("SELECT id, title, category, summary, DATE_FORMAT(created_at, '%Y-%m-%d') AS ndate FROM news_articles ORDER BY id DESC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        news.Add(new { id = r.GetInt32("id"), title = r.GetString("title"), category = r.GetString("category"), summary = r.IsDBNull(r.GetOrdinal("summary")) ? "" : r.GetString("summary"), date = r.GetString("ndate") });

                using (var cmd = new MySqlCommand("SELECT id, full_name, register_no, phone, program_name, eyesh_score, discount_percent, final_tuition, status FROM admissions ORDER BY id DESC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        admissions.Add(new
                        {
                            id = r.GetInt32("id"),
                            fullName = r.GetString("full_name"),
                            registerNo = r.GetString("register_no"),
                            phone = r.GetString("phone"),
                            programName = r.GetString("program_name"),
                            eyeshScore = r.GetInt32("eyesh_score"),
                            discount = r.GetInt32("discount_percent"),
                            finalTuition = r.GetInt32("final_tuition"),
                            status = r.GetString("status")
                        });

                using (var cmd = new MySqlCommand("SELECT id, student_id, semester, total_fee, discount_amount, paid_amount, status FROM tuition_payments ORDER BY id DESC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        payments.Add(new
                        {
                            id = r.GetInt32("id"),
                            studentId = r.GetString("student_id"),
                            semester = r.GetString("semester"),
                            totalFee = r.GetInt32("total_fee"),
                            discountAmount = r.GetInt32("discount_amount"),
                            paidAmount = r.GetInt32("paid_amount"),
                            status = r.GetString("status")
                        });
            }
            catch { }

            SendJson(new { action = "adminData", students, teachers, courses, totalGrades, staff, news, admissions, payments, admissionOpen });
        }

        // Зураг файлыг wwwroot/uploads/ фолдерт хадгалаад URL буцаана
        private string SaveBase64Image(string base64Data, string originalFileName, string prefix)
        {
            if (string.IsNullOrWhiteSpace(base64Data) || !base64Data.Contains(",")) return "";
            try
            {
                string uploadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);

                string ext = Path.GetExtension(originalFileName);
                if (string.IsNullOrEmpty(ext)) ext = ".png";

                string safeName = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..6]}{ext}";
                string fullPath = Path.Combine(uploadsDir, safeName);

                byte[] bytes = Convert.FromBase64String(base64Data.Split(',')[1]);
                File.WriteAllBytes(fullPath, bytes);

                return $"https://app.kosen/uploads/{safeName}";
            }
            catch
            {
                return "";
            }
        }

        private void AdminAddNews(string title, string category, string summary, string content, string imageColor, string imagePosition, string imageBase64, string imageFileName)
        {
            try
            {
                string imageUrl = SaveBase64Image(imageBase64, imageFileName, "news");

                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = @"INSERT INTO news_articles (title, category, summary, content, image_color, image_url, image_position, views)
                                 VALUES (@t, @c, @s, @cnt, @col, @img, @pos, 1)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@t", title);
                cmd.Parameters.AddWithValue("@c", category);
                cmd.Parameters.AddWithValue("@s", summary);
                cmd.Parameters.AddWithValue("@cnt", content);
                cmd.Parameters.AddWithValue("@col", imageColor);
                cmd.Parameters.AddWithValue("@img", imageUrl);
                cmd.Parameters.AddWithValue("@pos", imagePosition);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Зурагтай мэдээ нүүр хуудсанд амжилттай нийтлэгдлээ!" });
                SendAdminData();
                SendPublicData();
            }
            catch (Exception ex) { SendJson(new { action = "toast", message = "Алдаа: " + ex.Message }); }
        }

        private void IncrementNewsView(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("UPDATE news_articles SET views = views + 1 WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private void UploadAvatar(string targetRole, string targetId, string imageBase64, string imageFileName)
        {
            string avatarUrl = SaveBase64Image(imageBase64, imageFileName, $"avatar_{targetId}");
            if (string.IsNullOrEmpty(avatarUrl))
            {
                SendJson(new { action = "toast", message = "Зураг хадгалахад алдаа гарлаа." });
                return;
            }

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                string query = targetRole switch
                {
                    "student" => "UPDATE students SET avatar_url = @url WHERE student_id = @id",
                    "teacher" => "UPDATE teachers SET avatar_url = @url WHERE teacher_id = @id",
                    _ => "UPDATE staff SET avatar_url = @url WHERE username = @id"
                };

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@url", avatarUrl);
                cmd.Parameters.AddWithValue("@id", targetId);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "avatarUpdated", targetId, avatarUrl });
            }
            catch (Exception ex) { SendJson(new { action = "toast", message = "Алдаа: " + ex.Message }); }
        }

        private void ChangePassword(string role, string userId, string oldPass, string newPass)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                string query = role switch
                {
                    "student" => "UPDATE students SET password = @new WHERE student_id = @id AND password = @old",
                    "teacher" => "UPDATE teachers SET password = @new WHERE teacher_id = @id AND password = @old",
                    _ => "UPDATE staff SET password = @new WHERE username = @id AND password = @old"
                };

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@new", newPass);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@old", oldPass);
                int affected = cmd.ExecuteNonQuery();

                if (affected > 0)
                    SendJson(new { action = "toast", message = "Нууц үг амжилттай шинэчлэгдлээ!" });
                else
                    SendJson(new { action = "toast", message = "Одоогийн нууц үг буруу байна." });
            }
            catch (Exception ex) { SendJson(new { action = "toast", message = "Алдаа: " + ex.Message }); }
        }

        private void AdminDeleteNews(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM news_articles WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Мэдээ устгагдлаа." });
                SendAdminData();
                SendPublicData();
            }
            catch { }
        }

        private void AdminToggleAdmission(bool isOpen)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("UPDATE system_settings SET setting_value = @v WHERE setting_key = 'admission_open'", conn);
                cmd.Parameters.AddWithValue("@v", isOpen ? "1" : "0");
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = isOpen ? "Элсэлтийн бүртгэл нээгдлээ!" : "Элсэлтийн бүртгэл хаагдлаа!" });
                SendAdminData();
                SendPublicData();
            }
            catch { }
        }

        private void AdminUpdateAdmissionStatus(int id, string status)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("UPDATE admissions SET status = @s WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@s", status);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"Элсэгчийн төлөв \"{status}\" боллоо." });
                SendAdminData();
            }
            catch { }
        }

        private void AdminConvertApplicant(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                string fullName = "", programName = "";
                int discountPercent = 0, finalTuition = 0;

                using (var cmd = new MySqlCommand("SELECT full_name, program_name, discount_percent, final_tuition FROM admissions WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var r = cmd.ExecuteReader();
                    if (!r.Read())
                    {
                        SendJson(new { action = "toast", message = "Элсэгчийн мэдээлэл олдсонгүй." });
                        return;
                    }
                    fullName = r.GetString("full_name").Trim();
                    programName = r.GetString("program_name");
                    discountPercent = r.GetInt32("discount_percent");
                    finalTuition = r.GetInt32("final_tuition");
                }

                // Овог, Нэрийг салгах
                string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string lastName = parts.Length > 1 ? parts[0] : "Элсэгч";
                string firstName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : fullName;

                // Оюутны код үүсгэх (Жнь: K26001)
                string newStudentId = $"K26{id:D3}";
                string email = $"{newStudentId.ToLower()}@kosen.edu.mn";

                // 1. students хүснэгтэд бүртгэх
                string insStu = @"INSERT IGNORE INTO students (student_id, last_name, first_name, email, major, password)
                                  VALUES (@sid, @ln, @fn, @em, @maj, '1234')";
                using (var cmd = new MySqlCommand(insStu, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", newStudentId);
                    cmd.Parameters.AddWithValue("@ln", lastName);
                    cmd.Parameters.AddWithValue("@fn", firstName);
                    cmd.Parameters.AddWithValue("@em", email);
                    cmd.Parameters.AddWithValue("@maj", programName);
                    cmd.ExecuteNonQuery();
                }

                // 2. Үндсэн төлбөр болон хөнгөлөлтийг тооцоод tuition_payments-д бүртгэх
                int baseTuition = finalTuition;
                if (discountPercent > 0 && discountPercent < 100)
                    baseTuition = (int)Math.Round(finalTuition * 100.0 / (100 - discountPercent));
                else if (discountPercent == 100)
                    baseTuition = 3200000;

                int discountAmt = Math.Max(0, baseTuition - finalTuition);
                string payStatus = finalTuition == 0 ? "Төлсөн" : "Төлөөгүй";

                string insPay = @"INSERT INTO tuition_payments (student_id, semester, total_fee, discount_amount, paid_amount, status)
                                  VALUES (@sid, '2026 Намар', @tot, @disc, 0, @st)";
                using (var cmd = new MySqlCommand(insPay, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", newStudentId);
                    cmd.Parameters.AddWithValue("@tot", baseTuition);
                    cmd.Parameters.AddWithValue("@disc", discountAmt);
                    cmd.Parameters.AddWithValue("@st", payStatus);
                    cmd.ExecuteNonQuery();
                }

                // 3. Элсэгчийн төлөвийг 'Бүртгэгдсэн' болгох
                using (var cmd = new MySqlCommand("UPDATE admissions SET status = 'Бүртгэгдсэн' WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                SendJson(new { action = "toast", message = $"Амжилттай! Оюутны код: {newStudentId}, Нууц үг: 1234" });
                SendAdminData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Оюутан болгоход алдаа гарлаа: " + ex.Message });
            }
        }

        // ================= ХУВИЙН ФАЙЛУУД (PRIVATE FILES) =================
        private void SendPrivateFiles(string userId)
        {
            var files = new List<object>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = @"SELECT id, file_name, file_url, file_size, DATE_FORMAT(uploaded_at, '%Y-%m-%d %H:%i') AS udate
                                 FROM private_files WHERE user_id = @uid ORDER BY id DESC";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    files.Add(new
                    {
                        id = r.GetInt32("id"),
                        fileName = r.GetString("file_name"),
                        fileUrl = r.GetString("file_url"),
                        fileSize = r.GetString("file_size"),
                        uploadedAt = r.GetString("udate")
                    });
                }
            }
            catch { }

            SendJson(new { action = "privateFilesData", files });
        }

        private void UploadPrivateFile(string userId, string fileName, string fileSize, string fileBase64)
        {
            if (string.IsNullOrWhiteSpace(fileBase64) || !fileBase64.Contains(","))
            {
                SendJson(new { action = "toast", message = "Файл уншихад алдаа гарлаа." });
                return;
            }

            try
            {
                string uploadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);

                string ext = Path.GetExtension(fileName);
                string safeName = $"pf_{userId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..6]}{ext}";
                string fullPath = Path.Combine(uploadsDir, safeName);

                byte[] bytes = Convert.FromBase64String(fileBase64.Split(',')[1]);
                File.WriteAllBytes(fullPath, bytes);

                string fileUrl = $"https://app.kosen/uploads/{safeName}";

                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO private_files (user_id, file_name, file_url, file_size) VALUES (@uid, @fn, @url, @sz)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@fn", fileName);
                cmd.Parameters.AddWithValue("@url", fileUrl);
                cmd.Parameters.AddWithValue("@sz", fileSize);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{fileName}\" амжилттай хадгалагдлаа!" });
                SendPrivateFiles(userId);
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Файл хадгалах алдаа: " + ex.Message });
            }
        }

        private void DeletePrivateFile(int id, string userId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM private_files WHERE id = @id AND user_id = @uid", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Файл устгагдлаа." });
                SendPrivateFiles(userId);
            }
            catch { }
        }

        // ================= ХИЧЭЭЛИЙН ОРОЛЦОГЧИД БА ДҮН =================
        private void SendCourseParticipants(string courseName)
        {
            var participants = new List<object>();
            var courseGrades = new List<object>();

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // 1. Энэ хичээлийг сонгосон оюутнууд (эсвэл нийт бүртгэлтэй оюутнууд)
                string partQuery = @"SELECT DISTINCT s.student_id, s.last_name, s.first_name
                                     FROM students s
                                     INNER JOIN student_courses sc ON s.student_id = sc.student_id
                                     WHERE sc.course_name = @c
                                     ORDER BY s.first_name ASC";
                try
                {
                    using var cmd = new MySqlCommand(partQuery, conn);
                    cmd.Parameters.AddWithValue("@c", courseName);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        participants.Add(new
                        {
                            id = r.GetString("student_id"),
                            lastName = r.GetString("last_name"),
                            firstName = r.GetString("first_name")
                        });
                    }
                }
                catch
                {
                    // Хэрэв хүснэгтийн нэр өөр байвал бүх оюутныг харуулна
                    using var cmd = new MySqlCommand("SELECT student_id, last_name, first_name FROM students ORDER BY first_name ASC", conn);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        participants.Add(new
                        {
                            id = r.GetString("student_id"),
                            lastName = r.GetString("last_name"),
                            firstName = r.GetString("first_name")
                        });
                    }
                }

                // 2. Энэ хичээлийн дүнгийн мэдээлэл
                string gradeQuery = @"SELECT sg.student_id, CONCAT(s.last_name, ' ', s.first_name) AS sname, sg.score
                                      FROM student_grades sg
                                      LEFT JOIN students s ON sg.student_id = s.student_id
                                      WHERE sg.course_name = @c";
                using (var cmd = new MySqlCommand(gradeQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@c", courseName);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        int sc = r.GetInt32("score");
                        string lt = sc >= 90 ? "A" : sc >= 80 ? "B" : sc >= 70 ? "C" : sc >= 60 ? "D" : "F";
                        courseGrades.Add(new
                        {
                            studentId = r.GetString("student_id"),
                            studentName = r.IsDBNull(r.GetOrdinal("sname")) ? r.GetString("student_id") : r.GetString("sname"),
                            score = sc,
                            letter = lt
                        });
                    }
                }
            }
            catch { }

            SendJson(new { action = "courseParticipantsData", participants, courseGrades });
        }

        // ================= ГРУПП ЧАТ (GROUP CHAT) =================
        private void SendGroupChats(string userId)
        {
            var groups = new List<object>();
            var users = new List<object>();

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // 1. Энэ хэрэглэгчийн харьяалагдах группүүд
                string gQuery = @"SELECT g.id, g.group_name,
                                         (SELECT COUNT(*) FROM chat_group_members m2 WHERE m2.group_id = g.id) AS member_count
                                  FROM chat_groups g
                                  INNER JOIN chat_group_members m ON g.id = m.group_id
                                  WHERE m.user_id = @uid
                                  ORDER BY g.id DESC";
                using (var cmd = new MySqlCommand(gQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        groups.Add(new
                        {
                            id = r.GetInt32("id"),
                            groupName = r.GetString("group_name"),
                            memberCount = r.GetInt32("member_count")
                        });
                    }
                }

                // 2. Шинэ группт нэмж болох бүх хэрэглэгчид (Оюутан, Багш, Ажилтан)
                using (var cmd = new MySqlCommand("SELECT student_id, CONCAT(last_name, ' ', first_name) AS name FROM students ORDER BY first_name ASC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        users.Add(new { id = r.GetString("student_id"), name = r.GetString("name"), roleLabel = "Оюутан" });

                using (var cmd = new MySqlCommand("SELECT teacher_id, full_name FROM teachers ORDER BY full_name ASC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        users.Add(new { id = r.GetString("teacher_id"), name = r.GetString("full_name"), roleLabel = "Багш" });

                using (var cmd = new MySqlCommand("SELECT username, full_name FROM staff ORDER BY full_name ASC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        users.Add(new { id = r.GetString("username"), name = r.GetString("full_name"), roleLabel = "Ажилтан" });
            }
            catch { }

            SendJson(new { action = "groupChatList", groups, users });
        }

        private void CreateChatGroup(string groupName, string createdBy, List<string> members)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                long newGroupId = 0;
                using (var cmd = new MySqlCommand("INSERT INTO chat_groups (group_name, created_by) VALUES (@name, @by); SELECT LAST_INSERT_ID();", conn))
                {
                    cmd.Parameters.AddWithValue("@name", groupName);
                    cmd.Parameters.AddWithValue("@by", createdBy);
                    newGroupId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                foreach (var uid in members.Distinct())
                {
                    using var cmd = new MySqlCommand("INSERT INTO chat_group_members (group_id, user_id) VALUES (@gid, @uid)", conn);
                    cmd.Parameters.AddWithValue("@gid", newGroupId);
                    cmd.Parameters.AddWithValue("@uid", uid);
                    cmd.ExecuteNonQuery();
                }

                SendJson(new { action = "toast", message = $"\"{groupName}\" групп амжилттай үүслээ!" });
                SendGroupChats(createdBy);
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Групп үүсгэх алдаа: " + ex.Message });
            }
        }

        private void SendGroupMessages(int groupId)
        {
            var messages = new List<object>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                string query = @"SELECT id, sender_id, sender_name, message_text, DATE_FORMAT(created_at, '%m-%d %H:%i') AS cdate
                                 FROM chat_group_messages
                                 WHERE group_id = @gid
                                 ORDER BY id ASC";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@gid", groupId);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    messages.Add(new
                    {
                        id = r.GetInt32("id"),
                        senderId = r.GetString("sender_id"),
                        senderName = r.GetString("sender_name"),
                        messageText = r.GetString("message_text"),
                        createdAt = r.GetString("cdate")
                    });
                }
            }
            catch { }

            SendJson(new { action = "groupMessagesData", groupId, messages });
        }

        private void SendGroupMessage(int groupId, string senderId, string senderName, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText)) return;

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                string query = "INSERT INTO chat_group_messages (group_id, sender_id, sender_name, message_text) VALUES (@gid, @sid, @sname, @msg)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@gid", groupId);
                cmd.Parameters.AddWithValue("@sid", senderId);
                cmd.Parameters.AddWithValue("@sname", senderName);
                cmd.Parameters.AddWithValue("@msg", messageText);
                cmd.ExecuteNonQuery();

                SendGroupMessages(groupId);
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Зурвас илгээх алдаа: " + ex.Message });
            }
        }

        private void AdminAddPayment(string studentId, int totalFee, int discountAmount, int paidAmount)
        {
            int netFee = Math.Max(0, totalFee - discountAmount);
            string status = paidAmount >= netFee ? "Төлсөн" : paidAmount > 0 ? "Дутуу" : "Төлөөгүй";
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("INSERT INTO tuition_payments (student_id, total_fee, discount_amount, paid_amount, status) VALUES (@id, @tot, @disc, @paid, @st)", conn);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@tot", totalFee);
                cmd.Parameters.AddWithValue("@disc", discountAmount);
                cmd.Parameters.AddWithValue("@paid", paidAmount);
                cmd.Parameters.AddWithValue("@st", status);
                cmd.ExecuteNonQuery();

                AddNotificationInternal(conn, studentId, $"Сургалтын төлбөрийн нэхэмжлэх: {netFee:N0}₮ ({status})");
                SendJson(new { action = "toast", message = "Төлбөрийн бүртгэл нэмэгдлээ!" });
                SendAdminData();
            }
            catch (Exception ex) { SendJson(new { action = "toast", message = "Алдаа: " + ex.Message }); }
        }

        private void AdminMarkPaymentPaid(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("UPDATE tuition_payments SET paid_amount = GREATEST(0, total_fee - discount_amount), status = 'Төлсөн' WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Төлбөр төлөгдсөн төлөвт шилжлээ!" });
                SendAdminData();
            }
            catch { }
        }

        private void AdminAddStaff(string username, string password, string fullName, string role, string department)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("INSERT INTO staff (username, password, full_name, role, department) VALUES (@u, @p, @n, @r, @d)", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                cmd.Parameters.AddWithValue("@n", fullName);
                cmd.Parameters.AddWithValue("@r", role);
                cmd.Parameters.AddWithValue("@d", department);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{fullName}\" ажилтан нэмэгдлээ!" });
                SendAdminData();
            }
            catch (Exception ex) { SendJson(new { action = "toast", message = "Алдаа (Нэвтрэх нэр давхардсан байж магадгүй): " + ex.Message }); }
        }

        private void AdminDeleteStaff(string username)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM staff WHERE username = @u AND username != 'admin'", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{username}\" ажилтан устгагдлаа." });
                SendAdminData();
            }
            catch { }
        }

        private void AdminAddStudent(string lastName, string firstName, string studentId, string email, string major, string password)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = @"INSERT INTO students (last_name, first_name, student_id, email, major, password)
                                 VALUES (@last, @first, @id, @email, @major, @pwd)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@last", lastName);
                cmd.Parameters.AddWithValue("@first", firstName);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@major", major);
                cmd.Parameters.AddWithValue("@pwd", password);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{studentId}\" оюутны хаяг амжилттай үүслээ!" });
                SendAdminData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Оюутан нэмэхэд алдаа гарлаа (Код давхардсан байж магадгүй): " + ex.Message });
            }
        }

        private void AdminDeleteStudent(string studentId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM students WHERE student_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{studentId}\" оюутны бүртгэл устгагдлаа." });
                SendAdminData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Устгахад алдаа гарлаа: " + ex.Message });
            }
        }

        private void AdminAddTeacher(string teacherId, string fullName, string department, string password)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO teachers (teacher_id, full_name, department, password) VALUES (@id, @name, @dept, @pwd)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", teacherId);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@dept", department);
                cmd.Parameters.AddWithValue("@pwd", password);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{fullName}\" багшийн хаяг амжилттай үүслээ!" });
                SendAdminData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Багш нэмэхэд алдаа гарлаа (ID давхардсан байж магадгүй): " + ex.Message });
            }
        }

        private void AdminDeleteTeacher(string teacherId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM teachers WHERE teacher_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", teacherId);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{teacherId}\" багшийн бүртгэл устгагдлаа." });
                SendAdminData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Устгахад алдаа гарлаа: " + ex.Message });
            }
        }

        private void AdminAddCourse(string courseName, int credit, string department)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("INSERT INTO available_courses (course_name, credit, department) VALUES (@n, @c, @d)", conn);
                cmd.Parameters.AddWithValue("@n", courseName);
                cmd.Parameters.AddWithValue("@c", credit);
                cmd.Parameters.AddWithValue("@d", department);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{courseName}\" хичээл нэмэгдлээ!" });
                SendAdminData();
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Хичээл нэмэхэд алдаа гарлаа: " + ex.Message });
            }
        }

        private void AdminDeleteCourse(string courseName)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DELETE FROM available_courses WHERE course_name = @n LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@n", courseName);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = $"\"{courseName}\" хичээл устгагдлаа." });
                SendAdminData();
            }
            catch { }
        }

        private void AdminBroadcastNotif(string message)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var userIds = new List<string>();
                using (var cmd = new MySqlCommand("SELECT student_id FROM students", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) userIds.Add(r.GetString("student_id"));
                }

                using (var cmd = new MySqlCommand("SELECT teacher_id FROM teachers", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) userIds.Add(r.GetString("teacher_id"));
                }

                foreach (var uid in userIds)
                {
                    AddNotificationInternal(conn, uid, $"📢 [Админ зарлал]: {message}");
                }

                SendJson(new { action = "toast", message = $"Нийт {userIds.Count} хэрэглэгчид зарлал амжилттай илгээгдлээ!" });
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Зарлал илгээхэд алдаа гарлаа: " + ex.Message });
            }
        }

        // ================= 5. ХИЧЭЭЛИЙН МАТЕРИАЛ (COURSE) =================
        private void SendCourseMaterials(string courseName)
        {
            var materials = new List<object>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT id, module_title, item_title, item_type, description, file_name FROM course_materials WHERE course_name = @c ORDER BY id ASC";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@c", courseName);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    materials.Add(new
                    {
                        id = r.GetInt32("id"),
                        moduleTitle = r.GetString("module_title"),
                        itemTitle = r.GetString("item_title"),
                        itemType = r.IsDBNull(r.GetOrdinal("item_type")) ? "lecture" : r.GetString("item_type"),
                        description = r.IsDBNull(r.GetOrdinal("description")) ? "" : r.GetString("description"),
                        fileName = r.IsDBNull(r.GetOrdinal("file_name")) ? "" : r.GetString("file_name")
                    });
                }
            }
            catch { }

            SendJson(new { action = "courseMaterials", materials });
        }

        // ================= 6. КАЛЕНДАРЬ (CALENDAR) =================
        private void SendCalendarEvents(string userId)
        {
            var events = new List<object>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT id, course_name, title, DATE_FORMAT(event_date, '%Y-%m-%d') AS edate, event_time, event_type FROM calendar_events WHERE user_id = @u OR user_id = 'ALL' ORDER BY event_date ASC";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", userId);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    events.Add(new
                    {
                        id = r.GetInt32("id"),
                        courseName = r.IsDBNull(r.GetOrdinal("course_name")) ? "" : r.GetString("course_name"),
                        title = r.GetString("title"),
                        date = r.GetString("edate"),
                        time = r.IsDBNull(r.GetOrdinal("event_time")) ? "09:00" : r.GetString("event_time"),
                        eventType = r.IsDBNull(r.GetOrdinal("event_type")) ? "Course event" : r.GetString("event_type")
                    });
                }
            }
            catch { }

            SendJson(new { action = "calendarEvents", events });
        }

        private void AddCalendarEvent(string userId, string courseName, string title, string date, string time)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO calendar_events (user_id, course_name, title, event_date, event_time, event_type) VALUES (@u, @c, @t, @d, @tm, 'Course event')";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@c", courseName);
                cmd.Parameters.AddWithValue("@t", title);
                cmd.Parameters.AddWithValue("@d", date);
                cmd.Parameters.AddWithValue("@tm", time);
                cmd.ExecuteNonQuery();

                SendJson(new { action = "toast", message = "Календарь дээр арга хэмжээ нэмэгдлээ!" });
                SendCalendarEvents(userId);
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Алдаа: " + ex.Message });
            }
        }

        // ================= 7. МЭДЭГДЭЛ (NOTIFICATIONS) =================
        private void AddNotificationInternal(MySqlConnection conn, string userId, string title)
        {
            try
            {
                using var cmd = new MySqlCommand("INSERT INTO notifications (user_id, title, is_read) VALUES (@u, @t, 0)", conn);
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@t", title);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private void SendNotifications(string userId)
        {
            var items = new List<object>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "SELECT id, title, is_read, DATE_FORMAT(created_at, '%Y-%m-%d %H:%i') AS ctime FROM notifications WHERE user_id = @u ORDER BY id DESC LIMIT 15";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", userId);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    items.Add(new
                    {
                        id = r.GetInt32("id"),
                        title = r.GetString("title"),
                        isRead = r.GetInt32("is_read") == 1,
                        createdAt = r.GetString("ctime")
                    });
                }
            }
            catch { }

            SendJson(new { action = "notifications", items });
        }

        private void MarkNotifRead(string userId, int notifId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("UPDATE notifications SET is_read = 1 WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", notifId);
                cmd.ExecuteNonQuery();
                SendNotifications(userId);
            }
            catch { }
        }

        private void MarkAllNotifsRead(string userId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("UPDATE notifications SET is_read = 1 WHERE user_id = @u", conn);
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.ExecuteNonQuery();
                SendNotifications(userId);
            }
            catch { }
        }

        // ================= 8. ЧАТ (MESSAGES & CONTACTS) =================
        private void SendContacts(string currentUserId)
        {
            var contacts = new List<object>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                if (currentUserId != "admin")
                    contacts.Add(new { id = "admin", name = "Систем Админ", role = "Администратор" });

                using (var cmd = new MySqlCommand("SELECT teacher_id, full_name, department FROM teachers", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string tid = r.GetString("teacher_id");
                        if (tid != currentUserId)
                            contacts.Add(new { id = tid, name = r.GetString("full_name"), role = "Багш • " + r.GetString("department") });
                    }
                }

                using (var cmd = new MySqlCommand("SELECT student_id, last_name, first_name FROM students", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string sid = r.GetString("student_id");
                        if (sid != currentUserId)
                            contacts.Add(new { id = sid, name = $"{r.GetString("last_name")} {r.GetString("first_name")}", role = "Оюутан" });
                    }
                }
            }
            catch { }

            SendJson(new { action = "contacts", contacts });
        }

        private void SendMessages(string userId, string partnerId)
        {
            var messages = new List<object>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = @"SELECT sender_id, message_text, DATE_FORMAT(created_at, '%H:%i') AS mtime 
                                 FROM messages 
                                 WHERE (sender_id = @u AND receiver_id = @p) OR (sender_id = @p AND receiver_id = @u) 
                                 ORDER BY id ASC";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@p", partnerId);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    messages.Add(new
                    {
                        senderId = r.GetString("sender_id"),
                        text = r.GetString("message_text"),
                        time = r.GetString("mtime")
                    });
                }
            }
            catch { }

            SendJson(new { action = "messages", messages });
        }

        private void SaveChatMessage(string senderId, string senderName, string receiverId, string text)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string query = "INSERT INTO messages (sender_id, sender_name, receiver_id, message_text) VALUES (@sid, @sname, @rid, @txt)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", senderId);
                cmd.Parameters.AddWithValue("@sname", senderName);
                cmd.Parameters.AddWithValue("@rid", receiverId);
                cmd.Parameters.AddWithValue("@txt", text);
                cmd.ExecuteNonQuery();

                AddNotificationInternal(conn, receiverId, $"Шинэ зурвас ({senderName}): {text}");
                SendMessages(senderId, receiverId);
            }
            catch { }
        }

        // ================= 9. НҮҮР ХУУДАС & ЭЛСЭЛТ (PUBLIC) =================
        private void SendPublicData()
        {
            bool admissionOpen = true;
            var programs = new List<object>();
            var news = new List<object>();

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                using (var cmd = new MySqlCommand("SELECT setting_value FROM system_settings WHERE setting_key = 'admission_open'", conn))
                {
                    var val = cmd.ExecuteScalar()?.ToString();
                    admissionOpen = (val != "0");
                }

                using (var cmd = new MySqlCommand("SELECT id, program_name, eng_name, quota, enrolled_count, tuition_fee FROM admission_programs ORDER BY id ASC", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        programs.Add(new
                        {
                            id = r.GetInt32("id"),
                            name = r.GetString("program_name"),
                            engName = r.IsDBNull(r.GetOrdinal("eng_name")) ? "ENGINEER" : r.GetString("eng_name"),
                            quota = r.GetInt32("quota"),
                            enrolled = r.GetInt32("enrolled_count"),
                            tuition = r.GetInt32("tuition_fee")
                        });
                    }
                }

                using (var cmd = new MySqlCommand("SELECT id, title, category, summary, content, image_color, image_url, image_position, views, DATE_FORMAT(created_at, '%Y-%m-%d') AS ndate FROM news_articles ORDER BY id DESC LIMIT 15", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        news.Add(new
                        {
                            id = r.GetInt32("id"),
                            title = r.GetString("title"),
                            category = r.GetString("category"),
                            summary = r.IsDBNull(r.GetOrdinal("summary")) ? "" : r.GetString("summary"),
                            content = r.IsDBNull(r.GetOrdinal("content")) ? "" : r.GetString("content"),
                            imageColor = r.IsDBNull(r.GetOrdinal("image_color")) ? "#0e4d2d" : r.GetString("image_color"),
                            imageUrl = r.IsDBNull(r.GetOrdinal("image_url")) ? "" : r.GetString("image_url"),
                            imagePosition = r.IsDBNull(r.GetOrdinal("image_position")) ? "top" : r.GetString("image_position"),
                            views = r.IsDBNull(r.GetOrdinal("views")) ? 0 : r.GetInt32("views"),
                            date = r.GetString("ndate")
                        });
                    }
                }
            }
            catch { }

            SendJson(new { action = "publicData", admissionOpen, programs, news });
        }

        private void SubmitAdmission(string fullName, string registerNo, string phone, string programName, int eyeshScore, int discount, int finalTuition)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // Суудал дүүрсэн эсэхийг шалгах
                using (var chk = new MySqlCommand("SELECT quota, enrolled_count FROM admission_programs WHERE program_name = @p", conn))
                {
                    chk.Parameters.AddWithValue("@p", programName);
                    using var r = chk.ExecuteReader();
                    if (r.Read() && r.GetInt32("enrolled_count") >= r.GetInt32("quota"))
                    {
                        SendJson(new { action = "toast", message = "Уучлаарай, энэ ангийн суудал дүүрсэн байна!" });
                        return;
                    }
                }

                string query = @"INSERT INTO admissions (full_name, register_no, phone, program_name, eyesh_score, discount_percent, final_tuition, status)
                                 VALUES (@n, @reg, @ph, @prog, @score, @disc, @fee, 'Хүлээгдэж буй')";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@n", fullName);
                    cmd.Parameters.AddWithValue("@reg", registerNo);
                    cmd.Parameters.AddWithValue("@ph", phone);
                    cmd.Parameters.AddWithValue("@prog", programName);
                    cmd.Parameters.AddWithValue("@score", eyeshScore);
                    cmd.Parameters.AddWithValue("@disc", discount);
                    cmd.Parameters.AddWithValue("@fee", finalTuition);
                    cmd.ExecuteNonQuery();
                }

                // Суудлын тоог +1 нэмэх
                using (var upd = new MySqlCommand("UPDATE admission_programs SET enrolled_count = enrolled_count + 1 WHERE program_name = @p", conn))
                {
                    upd.Parameters.AddWithValue("@p", programName);
                    upd.ExecuteNonQuery();
                }

                SendJson(new { action = "admissionSuccess", message = $"Амжилттай бүртгэгдлээ! ({programName} — {discount}% хөнгөлөлт)" });
            }
            catch (Exception ex)
            {
                SendJson(new { action = "toast", message = "Бүртгэхэд алдаа гарлаа: " + ex.Message });
            }
        }

        private void SendJson(object data)
        {
            webView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(data));
        }
    }
}