using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string CourseCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Range(1, 10)]
        public int Credits { get; set; }

        public int MaxStudents { get; set; }

        // Faculty assigned to teach the course
        public int? FacultyId { get; set; }

        [StringLength(50)]
        public string Semester { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}