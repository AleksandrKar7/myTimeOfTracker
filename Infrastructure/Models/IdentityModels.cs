using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeOffTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("ФИО")]
        public string FullName { get; set; }

        [Required]
        [DisplayName("Дата приема на работу")]
        [Column(TypeName = "date")]
        public DateTime EmploymentDate { get; set; }

        public Notification Profile { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public class Notification
    {
        [Key]
        [ForeignKey("User")]
        public string Id { get; set; }
        //[Key]
        //virtual public ApplicationUser Id { get; set; }

        [DisplayName("Телеграм чат Id")]
        public int TelegramChatID { get; set; }

        [DisplayName("Телеграм чат Id")]
        public bool NotifyByTelegram { get; set; }

        public ApplicationUser User { get; set; }
    }

    public class Requests
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [Required]
        [DisplayName("From")]
        public DateTime DateStart { get; set; }

        [Required]
        [DisplayName("To")]
        public DateTime DateEnd { get; set; }

        [DisplayName("Employee")]
        virtual public ApplicationUser Employee { get; set; }

        [ForeignKey("Employee")]
        public string EmployeeId { get; set; }

        [DisplayName("Vacation type")]
        virtual public VacationTypes VacationTypes { get; set; }

        [Required]
        [ForeignKey("VacationTypes")]
        public int VacationTypesId { get; set; }

        [Required(ErrorMessage = "Please enter description.")]
        [DisplayName("Description")]
        public string Description { get; set; }
    }

    public class VacationTypes
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Название отпуска")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Макс. кол-во дней")]
        public int MaxDays { get; set; }
    }

    public class UserVacationDays
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Сотрудник")]
        virtual public ApplicationUser User { get; set; }

        [Required]
        [DisplayName("Тип отпуска")]
        virtual public VacationTypes VacationType { get; set; }

        [Required]
        [DisplayName("Текущее кол-во дней")]
        public int VacationDays { get; set; }

        [Required]
        [DisplayName("Дата последнего обновления")]
        [DataType(DataType.Date)]
        public DateTime LastUpdate { get; set; }
    }

    public class RequestStatuses
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Статус")]
        public string Name { get; set; }
    }

    public class RequestChecks
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [DisplayName("Запрос")]
        virtual public Requests Request { get; set; }

        [ForeignKey("Request")]
        public int RequestId { get; set; }

        [DisplayName("Проверяющий")]
        virtual public ApplicationUser Approver { get; set; }

        [ForeignKey("Approver")]
        public string ApproverId { get; set; }

        [DisplayName("Статус")]
        virtual public RequestStatuses Status { get; set; }

        [ForeignKey("Status")]
        public int StatusId { get; set; }

        [Required]
        [DisplayName("Приоритет проверяющего")]
        public int Priority { get; set; }
        
        [DisplayName("Причина отказа")]
        public string Reason { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        public DbSet<Requests> Requests { get; set; }
        public DbSet<VacationTypes> VacationTypes { get; set; }
        public DbSet<UserVacationDays> UserVacationDays { get; set; }
        public DbSet<RequestStatuses> RequestStatuses { get; set; }
        public DbSet<RequestChecks> RequestChecks { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}