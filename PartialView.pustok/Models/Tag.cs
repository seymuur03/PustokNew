﻿using System.ComponentModel.DataAnnotations;

namespace PartialView.pustok.Models
{
    public class Tag : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public List<BookTag> BookTags { get; set; }
    }
}
