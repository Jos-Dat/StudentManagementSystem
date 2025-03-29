using StudentManagementSystem.Models;
using System.Collections.Generic;

namespace StudentManagementSystem.ViewModels
{
    public class CourseRegistrationViewModel
    {
        public Student Student { get; set; }
        public List<Course> AvailableCourses { get; set; }
    }
}