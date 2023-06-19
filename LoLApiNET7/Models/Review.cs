using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoLApiNET7.Models
{
    public class Review
    {
        [Key]
        public int Review_id { get; set; }
        public string? Title { get; set; } //Title its not required. If title not provided the review will take the first word of the text
        public string? Text { get; set; }
        public DateTime Created { get; set; }
        //[Column(TypeName = "tinyint")]
        public byte Rating { get; set; }
        [ForeignKey("User")]
        public int User_Id { get; set; }
        [ForeignKey("Champion")]
        public int Champion_id { get; set; }
    }
}
