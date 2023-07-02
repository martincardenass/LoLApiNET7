using System.ComponentModel.DataAnnotations;

namespace LoLApiNET7.Models
{
    public class ReviewView
    {
        public int Champion_Id { get; set; }
        public string Name { get; set; }
        public DateTime Release_Date { get; set; }
        public string Region_Name { get; set; }
        public string Role_Name { get; set; }
        public int Review_Id { get; set; }
        public string? Review_Title { get; set; }
        public string? Review { get; set; }
        public DateTime Created { get; set; }
        public byte? Rating { get; set; } // Tinyint sql
        public string? Reviewer { get; set; }
    }
}
