using System.ComponentModel.DataAnnotations;

namespace IstanbulSenin.MVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@ibb\.gov\.tr$",
            ErrorMessage = "E-posta @ibb.gov.tr uzantılı olmalıdır.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
