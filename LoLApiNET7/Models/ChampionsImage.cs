using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoLApiNET7.Models
{
    public class ChampionsImage
    {
        [Key]
        public int R_Id { get; set; }
        // creates a relation between the images and images table to store multiple images
        [ForeignKey("Champion")]
        public int Champion_Id { get; set; }
        [ForeignKey("Images")]
        public int Image_Id { get; set; }
    }
}
