using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyWeb.Models
{
    public class Category
    {
        //data annotation
        //Key is niet nodig wanneer je prop de naam Id heeft of CategoryID, dan weet EF automatisch dat het de key van dit model is
        //Een andere data annotation = [Required]

        [Key]
        public int Id { get; set; }     
        [Required]
        [DisplayName("Category Name")]
        [MaxLength(30)]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1,100, ErrorMessage ="Value must be between 1 and 100.")]
        public int DisplayOrder { get; set; }

    }
}
