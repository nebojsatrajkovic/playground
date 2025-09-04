using log4net;

namespace Core.Stripe.Services
{
    public static class TestService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(TestService));

        public static async void CreateExampleProducts()
        {
            try
            {
                string? productId = null;

                var product = await StripeService.CreateOrUpdateProduct(new Models.Product.STRIPE_CreateOrUpdateProduct_Request
                {
                    ProductId = productId,
                    ProductName = "Basic package",
                    ProductDescription = "Basic",
                    PriceData = new Models.Product.STRIPE_COUP_R_PriceInfo
                    {
                        Price = 10,
                        IsRecurring = true,
                        IfRecurring_IsWeekly = true,
                        IfRecurring_IntervalCount = 1
                    }
                });

                product = await StripeService.CreateOrUpdateProduct(new Models.Product.STRIPE_CreateOrUpdateProduct_Request
                {
                    ProductId = productId,
                    ProductName = "Standard package",
                    ProductDescription = "Standard",
                    PriceData = new Models.Product.STRIPE_COUP_R_PriceInfo
                    {
                        Price = 17,
                        IsRecurring = true,
                        IfRecurring_IsMonthly = true,
                        IfRecurring_IntervalCount = 1
                    }
                });

                product = await StripeService.CreateOrUpdateProduct(new Models.Product.STRIPE_CreateOrUpdateProduct_Request
                {
                    ProductId = productId,
                    ProductName = "Premium package",
                    ProductDescription = "Premium",
                    PriceData = new Models.Product.STRIPE_COUP_R_PriceInfo
                    {
                        Price = 30,
                        IsRecurring = true,
                        IfRecurring_IsYearly = true,
                        IfRecurring_IntervalCount = 1
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}