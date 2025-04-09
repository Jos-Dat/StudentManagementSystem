using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        internal string Name;

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentNumber { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; }

        // Navigation properties
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}