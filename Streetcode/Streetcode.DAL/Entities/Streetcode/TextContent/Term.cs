using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Streetcode.DAL.Entities.Streetcode.TextContent
{
    [Table("terms", Schema = "streetcode")]
    public class Term
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [ForeignKey(nameof(Streetcode))]
        public int StreetcodeId { get; set; }

        public ICollection<RelatedTerm> RelatedTerms { get; set; } = new List<RelatedTerm>();
    }
}
