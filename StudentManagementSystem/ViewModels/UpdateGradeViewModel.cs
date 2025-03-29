using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.ViewModels
{
    public class UpdateGradeViewModel
    {
        public int CourseId { get; set; }
        public int EnrollmentId { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Grade { get; set; }
    }
}