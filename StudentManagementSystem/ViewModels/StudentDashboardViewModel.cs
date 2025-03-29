using StudentManagementSystem.Models;
using System.Collections.Generic;

namespace StudentManagementSystem.ViewModels
{
    public class StudentDashboardViewModel
    {
        public Student Student { get; set; }
        public List<Enrollment> CurrentEnrollments { get; set; }
        public List<Enrollment> CompletedCourses { get; set; }
    }
}