namespace Yuviron.Api.Dtos {
    public class CreateOrderRequest {
        public string ProductName { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = default!;
    }
}
