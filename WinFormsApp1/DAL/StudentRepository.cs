using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using WinFormsApp1.Models;

namespace WinFormsApp1.DAL
{
    /// <summary>
    /// Оюутны CRUD үйлдэл, хайлт, тайлан ба статистикийн Data Access Layer
    /// </summary>
    public class StudentRepository
    {
        /// <summary>
        /// Бүх оюутныг нэр/код болон мэргэжлээр шүүж авах (READ & FILTER)
        /// </summary>
        public List<Student> GetAllStudents(string searchText = "", string majorFilter = "Бүгд")
        {
            var list = new List<Student>();
            using var conn = MySqlDbHelper.GetConnection();
            conn.Open();

            string query = @"SELECT student_id, last_name, first_name, email, major, password
                             FROM students
                             WHERE (@maj = 'Бүгд' OR major = @maj)
                               AND (@q = '' OR student_id LIKE @ql OR first_name LIKE @ql OR last_name LIKE @ql)
                             ORDER BY student_id ASC";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@maj", majorFilter);
            cmd.Parameters.AddWithValue("@q", searchText.Trim());
            cmd.Parameters.AddWithValue("@ql", $"%{searchText.Trim()}%");

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Student
                {
                    StudentId = r.GetString("student_id"),
                    LastName = r.GetString("last_name"),
                    FirstName = r.GetString("first_name"),
                    Email = r.GetString("email"),
                    Major = r.GetString("major"),
                    Password = r.GetString("password")
                });
            }
            return list;
        }

        /// <summary>
        /// Шинэ оюутан нэмэх (CREATE)
        /// </summary>
        public void AddStudent(Student s)
        {
            using var conn = MySqlDbHelper.GetConnection();
            conn.Open();

            string query = @"INSERT INTO students (student_id, last_name, first_name, email, major, password)
                             VALUES (@id, @ln, @fn, @em, @maj, @pwd)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", s.StudentId);
            cmd.Parameters.AddWithValue("@ln", s.LastName);
            cmd.Parameters.AddWithValue("@fn", s.FirstName);
            cmd.Parameters.AddWithValue("@em", s.Email);
            cmd.Parameters.AddWithValue("@maj", s.Major);
            cmd.Parameters.AddWithValue("@pwd", string.IsNullOrWhiteSpace(s.Password) ? "1234" : s.Password);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Оюутны мэдээлэл засах (UPDATE)
        /// </summary>
        public void UpdateStudent(Student s)
        {
            using var conn = MySqlDbHelper.GetConnection();
            conn.Open();

            string query = @"UPDATE students
                             SET last_name = @ln, first_name = @fn, email = @em, major = @maj
                             WHERE student_id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", s.StudentId);
            cmd.Parameters.AddWithValue("@ln", s.LastName);
            cmd.Parameters.AddWithValue("@fn", s.FirstName);
            cmd.Parameters.AddWithValue("@em", s.Email);
            cmd.Parameters.AddWithValue("@maj", s.Major);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Оюутан устгах (DELETE)
        /// </summary>
        public void DeleteStudent(string studentId)
        {
            using var conn = MySqlDbHelper.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("DELETE FROM students WHERE student_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", studentId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Анхааруулгын тайлан: Төлбөрийн үлдэгдэл, муу дүн (<60), таслалт (>=3) шалгах
        /// </summary>
        public List<StudentAlertSummary> GetStudentAlertSummaries()
        {
            var list = new List<StudentAlertSummary>();
            using var conn = MySqlDbHelper.GetConnection();
            conn.Open();

            string query = @"
                SELECT s.student_id, s.last_name, s.first_name, s.major,
                       IFNULL((SELECT ROUND(AVG(g.score), 1) FROM student_grades g WHERE g.student_id = s.student_id), 0) AS avg_score,
                       IFNULL((SELECT SUM(GREATEST(0, (p.total_fee - p.discount_amount) - p.paid_amount)) FROM tuition_payments p WHERE p.student_id = s.student_id), 0) AS unpaid_fee,
                       IFNULL((SELECT COUNT(*) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'Тасалсан'), 0) AS absent_count
                FROM students s
                ORDER BY s.student_id ASC";

            using var cmd = new MySqlCommand(query, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                double avg = Convert.ToDouble(r["avg_score"]);
                int unpaid = Convert.ToInt32(r["unpaid_fee"]);
                int absent = Convert.ToInt32(r["absent_count"]);

                var reasons = new List<string>();
                if (unpaid > 0) reasons.Add($"Төлбөр дутуу ({unpaid:N0}₮)");
                if (avg > 0 && avg < 60) reasons.Add($"Дундаж бага ({avg})");
                if (absent >= 3) reasons.Add($"Таслалт их ({absent})");

                list.Add(new StudentAlertSummary
                {
                    StudentId = r.GetString("student_id"),
                    FullName = $"{r.GetString("last_name")} {r.GetString("first_name")}",
                    Major = r.GetString("major"),
                    AvgScore = avg,
                    UnpaidFee = unpaid,
                    AbsentCount = absent,
                    HasAlert = reasons.Count > 0,
                    AlertReason = reasons.Count > 0 ? string.Join(" | ", reasons) : "Хэвийн"
                });
            }
            return list;
        }

        /// <summary>
        /// График 1: Мэргэжил бүрийн оюутны тоо
        /// </summary>
        public Dictionary<string, int> GetMajorCounts()
        {
            var map = new Dictionary<string, int>();
            using var conn = MySqlDbHelper.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("SELECT major, COUNT(*) AS cnt FROM students GROUP BY major ORDER BY cnt DESC", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                map[r.GetString("major")] = Convert.ToInt32(r["cnt"]);

            return map;
        }

        /// <summary>
        /// График 2: Хичээл бүрийн дундаж оноо
        /// </summary>
        public Dictionary<string, double> GetCourseAverageScores()
        {
            var map = new Dictionary<string, double>();
            using var conn = MySqlDbHelper.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("SELECT course_name, ROUND(AVG(score), 1) AS avg_sc FROM student_grades GROUP BY course_name ORDER BY avg_sc DESC", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                map[r.GetString("course_name")] = Convert.ToDouble(r["avg_sc"]);

            return map;
        }
    }
}