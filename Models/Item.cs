using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMongoDb.Models
{
    public class Item
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ItemName { get; set; }

        [Required]
        public decimal ItemPrice { get; set; }

        public string? OwnerId { get; set; }
    }
}
