using System.ComponentModel.DataAnnotations;

namespace APINET_T26.Models.Entities
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public string Description { get; set; }
        public string? Filter { get; set; }
    }
}
