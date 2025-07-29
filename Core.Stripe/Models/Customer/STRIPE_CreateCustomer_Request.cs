namespace Core.Stripe.Models.Customer
{
    public class STRIPE_CreateCustomer_Request
    {
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string IPAddress { get; set; } = null!;
    }
}