using System.ComponentModel.DataAnnotations;

namespace IstanbulSenin.CORE.Entities
{
    public class Section
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Bölüm başlığı zorunludur.")]
        public string? Title { get; set; }
        public string Role { get; set; } = "all";
        public string Size { get; set; } = "slider";
        public int DisplayOrder { get; set; }

        // Bölüm, kendisine ait uygulamaları bir liste olarak tutar
        public List<MiniAppItem> Items { get; set; } = new();
    }
}