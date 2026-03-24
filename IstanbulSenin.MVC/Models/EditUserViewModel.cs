using System.ComponentModel.DataAnnotations;

namespace IstanbulSenin.MVC.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@ibb\.gov\.tr$",
            ErrorMessage = "E-posta @ibb.gov.tr uzantılı ve yalnızca İngilizce karakter içermelidir.")]
        public string Email { get; set; } = string.Empty;

        /// Boş bırakılırsa şifre değiştirilmez
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
        [DataType(DataType.Password)]
        public string? NewPasswordConfirm { get; set; }

        public List<string> Roles { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
