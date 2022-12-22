using System.ComponentModel.DataAnnotations;

namespace xDC_Web.Models
{
    public class LoginViewModel
    {
        private string _Username;

        [Required]
        [Display(Name = "Username")]
        public string Username { 
            get { return _Username; }
            set { _Username = value.Trim(); }
        }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string IpAddress { get; set; }
        public string ClientBrowser { get; set; }

    }
}