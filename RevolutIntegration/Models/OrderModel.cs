namespace RevolutIntegration.Models
{
    public class OrderRequest
    {
        public int Price { get; set; }

        public required string Currency {  get; set; }

        public string? Description { get; set; }
    }

    public class OrderResponse
    {
        public string? Token { get; set; }
        public string? CheckoutUrl { get; set; }
        public required string Mode { get; set; }
    }
}
