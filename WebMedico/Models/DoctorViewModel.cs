using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebMedico.Models
{
    public class SendDoctorViewModel
    {
        public int ID_DOCTOR { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NAME { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [StringLength(100, ErrorMessage = "The {0} must be valid.", MinimumLength = 3)]
        [Display(Name = "Last Name")]
        public string LASTNAME { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email")]
        public string EMAIL { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Display(Name = "Password")]
        public string PASSWORD { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Gender")]
        public string GENDER { get; set; }

        public int ID_COUNTRY { get; set; }


        [Required]
        [DataType(DataType.Text)]
        [StringLength(100, ErrorMessage = "The {0} must be valid")]
        [Display(Name = "Specialty")]
        public string SPECIALTY { get; set; }
        public string ORDER_NUMBER { get; set; }
        public System.DateTime INIT_DATE_CAREER { get; set; }
        public System.DateTime BIRTH_DATE { get; set; }
        public string LANGUAGES { get; set; }
        public string TAXPAYER { get; set; }
        public string LOCALITY { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Mobile")]
        public string MOBILE_NUMBER { get; set; }

        public string PHOTO { get; set; }

        public System.DateTime DATECREATED { get; set; }
        public bool STATUS { get; set; }

        //public virtual TABLE_COUNTRY TABLE_COUNTRY { get; set; }
    }
}