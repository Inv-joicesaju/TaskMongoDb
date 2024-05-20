using System.ComponentModel.DataAnnotations;

namespace TaskMongoDb.Models
{
    public class UpdatingItem
    {
        [MinLength(2, ErrorMessage = "Item name must be at least 2 characters long.")]
        public string? ItemName { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Item price must be a positive value.")]
        public decimal? ItemPrice { get; set; }
    }
}
