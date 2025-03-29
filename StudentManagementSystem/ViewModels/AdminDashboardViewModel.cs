using StudentManagementSystem.Models;
using System.Collections.Generic;

namespace StudentManagementSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalFaculty { get; set; }
        public int TotalCourses { get; set; }
        public List<Student> RecentStudents { get; set; }
    }
}