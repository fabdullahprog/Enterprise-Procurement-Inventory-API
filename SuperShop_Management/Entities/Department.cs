using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Department : BaseEntity
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(10, ErrorMessage = "Department Code cannot exceed 10 characters")]
        [Display(Name = "Department Code")]
        public string DepartmentCode { get; set; } = string.Empty;
       

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100, ErrorMessage = "Department Name cannot exceed 100 characters")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        [Display(Name = "Department Email")]
        public string? DepartmentEmail { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        [Display(Name = "Department Phone")]
        public string? DepartmentPhone { get; set; }

        [StringLength(100)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Can Request Item")]
        public bool CanRequestItem { get; set; } = false;   

        [Display(Name = "Can Issue Item")]
        public bool CanIssueItem { get; set; } = false;  

        // Navigation Properties
        //public virtual ICollection<User> Users { get; set; }
        //    = new List<User>();     
    }
}
