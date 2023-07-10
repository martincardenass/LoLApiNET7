using System.ComponentModel.DataAnnotations;

namespace LoLApiNET7.Models
{
    public class ChampionInfo
    {
        [Key]
        public int Champion_Id { get; set; }
        public string Name { get; set; }
        public DateTime Release_Date { get; set; }
        public string Image { get; set; }
        public string Region_Name { get; set; }
        public string Region_Emblem { get; set; }
        public string Role_Name { get; set; }
        public string Role_Icon { get; set; }
        public string Champ_Icons { get; set; }
        public string Catchphrase { get; set; }
        public string Description { get; set; }
    }
}
