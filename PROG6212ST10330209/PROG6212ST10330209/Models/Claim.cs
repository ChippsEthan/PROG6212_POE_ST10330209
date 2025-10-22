using System.ComponentModel.DataAnnotations;

namespace PROG6212ST10330209.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lecturer name is required")]
        public string LecturerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 100, ErrorMessage = "Hours must be between 1 and 100")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(1, 500, ErrorMessage = "Hourly rate must be between 1 and 500")]
        public decimal HourlyRate { get; set; }

        public decimal ClaimAmount => HoursWorked * HourlyRate;

        public decimal CalculateTotal()
        {
            return HoursWorked * HourlyRate;
        }

        // Add this property for your unit tests
        public string Notes { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string? FilePath { get; set; }
        public string? FileName { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.Now;
    }
}