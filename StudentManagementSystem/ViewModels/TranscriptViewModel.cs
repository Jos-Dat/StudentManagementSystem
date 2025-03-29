using StudentManagementSystem.Models;
using System.Collections.Generic;

namespace StudentManagementSystem.ViewModels
{
    public class TranscriptViewModel
    {
        public Student Student { get; set; }
        public List<Enrollment> Enrollments { get; set; }
    }
}