using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace TimeOffTracker.Models
{
    public class VacationRequestViewModels
    {   [Required]
        public ApplicationUser User { get; set; }
        [Required]
        public Requests VacationRequest { get; set; }
        [Required]
        public List<VacationTypes> VacationTypes { get; set; }
        [Required]
        public List<ApplicationUser> Approvers { get; set; }
    }

    public class CreateVacationRequestViewModel
    {
        [Required]
        public Requests VacationRequest { get; set; }
        [Required]
        public List<string> ApproversId { get; set; }
    }


    public class HistoryOfVacationViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Type vacation")]
        public string VacationType { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Date start")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Date end")]
        public DateTime DateEnd { get; set; }

        [Display(Name = "Days")]
        public string Days { get; set; }

        public List<RequestChecks> Approvers { get; set; }

        [Display(Name = "Rejection reason")]
        public string Reson { get; set; }

    }

    public class ListHistoryOfVacationViewModel
    {
        public List<UserVacationDays> UserVacationDays { get; set; }
        public List<HistoryOfVacationViewModel> AllVacations { get; set; }
    }
}