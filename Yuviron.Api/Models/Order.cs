namespace Yuviron.Api.Models {
    public class Order {
        public int Id { get; set; }

        public string ProductName { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; } = default!;
    }
}
