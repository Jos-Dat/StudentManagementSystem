using StudentManagementSystem.Models;
using System.Collections.Generic;
using System;
namespace StudentManagementSystem.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        public bool IsAdminOrFaculty { get; set; }
        public List<Student> EnrolledStudents { get; set; }
    }
}