namespace Core.Stripe.Models.Product
{
    public class STRIPE_CreateOrUpdateProduct_Request
    {
        public string? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public STRIPE_COUP_R_PriceInfo? PriceData { get; set; }
    }

    public class STRIPE_COUP_R_PriceInfo
    {
        public decimal? Price { get; set; }
        public bool IsRecurring { get; set; }
        public bool IfRecurring_IsDaily { get; set; }
        public bool IfRecurring_IsWeekly { get; set; }
        public bool IfRecurring_IsMonthly { get; set; }
        public bool IfRecurring_IsYearly { get; set; }
        public int IfRecurring_IntervalCount { get; set; }
    }
}