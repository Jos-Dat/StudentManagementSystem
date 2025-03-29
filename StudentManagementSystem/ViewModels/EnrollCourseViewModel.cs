using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.ViewModels
{
    public class EnrollCourseViewModel
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Semester")]
        public string Semester { get; set; }

        [Required]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }
    }
}