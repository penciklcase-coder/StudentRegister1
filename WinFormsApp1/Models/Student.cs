using System;

namespace WinFormsApp1.Models
{
    /// <summary>
    /// Оюутны мэдээллийг төлөөлөх Model класс (students хүснэгт)
    /// </summary>
    public class Student
    {
        public string StudentId { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Major { get; set; } = "";
        public string Password { get; set; } = "1234";
    }

    /// <summary>
    /// Оюутны дүн, төлбөрийн үлдэгдэл ба ирцийн анхааруулгын нэгтгэл
    /// </summary>
    public class StudentAlertSummary
    {
        public string StudentId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Major { get; set; } = "";
        public double AvgScore { get; set; }
        public int UnpaidFee { get; set; }
        public int AbsentCount { get; set; }
        public string AlertReason { get; set; } = "Хэвийн";
        public bool HasAlert { get; set; }
    }
}