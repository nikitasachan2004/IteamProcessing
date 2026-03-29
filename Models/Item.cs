using System;
using System.ComponentModel.DataAnnotations;

namespace ItemProcessingSystemCore.Models
{
    public class Item
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(0.1, 10000, ErrorMessage = "Weight must be between 0.1 and 10000")]
        public double Weight { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}