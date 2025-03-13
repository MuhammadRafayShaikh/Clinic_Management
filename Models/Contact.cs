using System.ComponentModel.DataAnnotations;

namespace Clinic_Management.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public User? User { get; set; }
    }
}
