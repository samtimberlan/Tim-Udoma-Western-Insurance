namespace Tim_Udoma_Western_Insurance.DTOs.Responses
{
    public class ProductResponse
    {
        public required string Reference { get; set; }
        public required string SKU { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public bool Active { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
