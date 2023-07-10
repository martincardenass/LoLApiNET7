namespace LoLApiNET7.Dto
{
    public class ChampionDto
    {
        public int Champion_id { get; set; }
        public string? Name { get; set; }
        public DateTime? Release_date { get; set; }
        public string? Image { get; set; }
        public ICollection<string>? AdditionalImages { get; set; }
        public string Region_Name { get; set; }
        public string Region_Emblem { get; set; }
        public string Role_Name { get; set; }
        public string Role_Icon { get; set; }
        public string Champ_Icons { get; set; }
        public string Catchphrase { get; set; }
        public string Description { get; set; }
    }
}
