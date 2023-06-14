using System.ComponentModel.DataAnnotations;

namespace LoLApiNET7.Models
{
    public class Region
    {
        [Key]
        public int Region_id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
