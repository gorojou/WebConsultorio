using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebMedico.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredEmail")]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Email")]
        [EmailAddress(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "EmailBadFormat")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredPassword")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(ResourcesAccount), Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }

    public class DoctorViewModel
    {

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredName")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Name")]
        public string NAME { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredLastName")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "LastName")]
        public string LASTNAME { get; set; }

        [System.Web.Mvc.Remote("CheckifEmailExists", "Account", HttpMethod = "POST", ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "EmailAlreadyExists")]
        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredEmail")]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Email")]
        [EmailAddress(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "EmailBadFormat")]
        public string EMAIL { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredPassword")]
        [StringLength(100, ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "MinimunLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Password")]
        public string PASSWORD { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "ConfirmPassword")]
        [Compare("Password", ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "ComparePassword")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredGender")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Gender")]
        public string GENDER { get; set; }

        [Display(ResourceType = typeof(ResourcesAccount), Name = "Country")]
        public int ID_COUNTRY { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredSpecialty")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Specialty")]
        public string SPECIALTY { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredOrderNumber")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "OrderNumber")]
        public string ORDER_NUMBER { get; set; }

        [Display(ResourceType = typeof(ResourcesAccount), Name = "InitDateCareer")]
        public System.DateTime INIT_DATE_CAREER { get; set; }

        [Display(ResourceType = typeof(ResourcesAccount), Name = "DateofBirth")]
        public System.DateTime BIRTH_DATE { get; set; }

        [Display(ResourceType = typeof(ResourcesAccount), Name = "Languages")]
        public string[] LANGUAGES { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredTaxPayer")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "TaxPayer")]
        public string TAXPAYER { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredAddress")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Address")]
        public string ADDRESS { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredLocality")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Locality")]
        public string LOCALITY { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredZipCode")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "ZipCode")]
        public string ZIPCODE { get; set; }

        [Required(ErrorMessageResourceType = typeof(ResourcesAccount), ErrorMessageResourceName = "RequiredMobile_Number")]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(ResourcesAccount), Name = "Mobile_Number")]
        public string MOBILE_NUMBER { get; set; }

        [Display(ResourceType = typeof(ResourcesAccount), Name = "Photo")]
        public string PHOTO { get; set; }

        [Display(ResourceType = typeof(ResourcesAccount), Name = "DateCreated")]
        public System.DateTime DATECREATED { get; set; }

        [Display(ResourceType = typeof(ResourcesAccount), Name = "Status")]
        public bool STATUS { get; set; }
    }

    public class ResetPasswordViewModel
    {
        //[Required]
        //[EmailAddress]
        //[Display(Name = "Email")]
        //public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string token { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string Code { get; set; }
    }
}
