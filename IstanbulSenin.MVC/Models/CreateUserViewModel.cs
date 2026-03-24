using System.ComponentModel.DataAnnotations;

namespace IstanbulSenin.MVC.Models
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@ibb\.gov\.tr$",
            ErrorMessage = "E-posta @ibb.gov.tr uzantılı ve yalnızca İngilizce karakter içermelidir.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}
