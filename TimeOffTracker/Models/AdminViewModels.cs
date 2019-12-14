using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeOffTracker.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The value {0} must contain at least {2} characters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Employment date")]
        public DateTime EmploymentDate { get; set; }

        [Display(Name = "Roles")]
        public IList<SelectListItem> AvailableRoles { get; set; }
        public IList<string> SelectedRoles { get; set; }
    }

    public class EditUserViewModel
    {
        [Required]
        [Display(Name = "New full name")]
        public string NewFullName { get; set; }
        [Display(Name = "Old full name")]
        public string OldFullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "New Email")]
        public string NewEmail { get; set; }
        [Display(Name = "Old Email")]
        public string OldEmail { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "New employment date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime NewEmploymentDate { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Old employment date")]
        public string OldEmploymentDate { get; set; }

        [Display(Name = "New roles")]
        public IList<SelectListItem> AvailableRoles { get; set; }
        public IList<string> SelectedRoles { get; set; }
        [Display(Name = "Old roles")]
        public string OldRoles { get; set; }

        public string IsChangePassword { get; set; }

        [StringLength(100, ErrorMessage = "The value {0} must contain at least {2} characters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
    }

    public class ShowUserViewModel
    {
        [Display(Name = "Full name")]
        public string FullName { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Employment date")]
        public string EmploymentDate { get; set; }
        [Display(Name = "Roles")]
        public string AllRoles { get; set; }
        public DateTime? LockoutTime { get; set; }
    }
    public class ListShowUserViewModel
    {
        public IList<ShowUserViewModel> MenuItems { get; set; }
    }

    public class EditUserVacationDaysViewModel
    {
        [Display(Name = "Full name")]
        public string FullName { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Employment date")]
        public string EmploymentDate { get; set; }
        [Display(Name = "Roles")]
        public string AllRoles { get; set; }

        public Dictionary<string, int> Vacations { get; set; }
        public List<string> VacationNames { get; set; }
        public List<int> VacationDays { get; set; }
    }

}