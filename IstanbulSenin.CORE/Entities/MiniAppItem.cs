using IstanbulSenin.CORE.Enums;
using System.ComponentModel.DataAnnotations;

namespace IstanbulSenin.CORE.Entities
{
    public class MiniAppItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen uygulama başlığını giriniz.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Lütfen bir uygulama ikonu yükleyiniz.")]
        public string? Image { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Uygulama URL adresi boş bırakılamaz.")]
        public string? Url { get; set; }

        public string? LogoUrl { get; set; }
        public bool IsNative { get; set; }
        public bool IsTest { get; set; }
        public bool IsHide { get; set; }
        public int DisplayOrder { get; set; }

        public List<PermissionType> Permissions { get; set; } = new();
        public List<PluginType> Plugins { get; set; } = new();

        // Bir uygulama en az bir bölüme ait olmalı kuralını Controller'da yöneteceğiz.
        public List<Section> Sections { get; set; } = new();
    }
}