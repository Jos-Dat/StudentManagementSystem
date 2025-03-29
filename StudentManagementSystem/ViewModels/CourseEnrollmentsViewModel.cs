using StudentManagementSystem.Models;
using System.Collections.Generic;

namespace StudentManagementSystem.ViewModels
{
    public class CourseEnrollmentsViewModel
    {
        public Course Course { get; set; }
        public List<Student> EnrolledStudents { get; set; }
    }
}