using System.ComponentModel.DataAnnotations;

namespace LoLApiNET7.Models
{
    public class Role
    {
        [Key]
        public int Role_id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
