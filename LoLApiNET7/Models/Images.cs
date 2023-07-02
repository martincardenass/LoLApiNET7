using System.ComponentModel.DataAnnotations;

namespace LoLApiNET7.Models
{
    public class Images
    {
        [Key]
        public int Image_Id { get; set; }
        public string Image { get; set; }
    }
}
