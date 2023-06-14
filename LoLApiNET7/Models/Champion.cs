using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace LoLApiNET7.Models
{
    public class Champion
    {
        [Key]
        public int Champion_id { get; set; }
        public string Name { get; set; }
        public DateTime Release_date { get; set; }
        public string? Image { get; set; } // ? means that is nullable
        [ForeignKey("Region")]
        public int Region_id { get; set; }
        public ICollection<Region>? Regions { get; set; }
        [ForeignKey("Role")]
        public int Role_id { get; set; }
        public ICollection<Role>? Roles { get; set; }
    }
}
