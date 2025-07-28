using Core.Shared.Models;
using log4net;
using Stripe;
using Product = Stripe.Product;
using ProductService = Stripe.ProductService;

namespace Core.Stripe.Services
{
    public static class StripeService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(StripeService));

        public static async Task<ResultOf<Product>> CreateProduct(ProductCreateOptions parameter)
        {
            /*
             var optionsProduct = new ProductCreateOptions
                {
                  Name = "Starter Subscription",
                  Description = "$12/Month subscription",
                };
             */

            ResultOf<Product> returnValue;

            try
            {
                var serviceProduct = new ProductService();
                Product product = await serviceProduct.CreateAsync(parameter);

                returnValue = new ResultOf<Product>(product);
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                returnValue = new ResultOf<Product>(ex);
            }

            return returnValue;
        }

        public static async Task<ResultOf<Price>> CreatePrice(PriceCreateOptions parameter)
        {
            /*
             var optionsPrice = new PriceCreateOptions
                {
                  UnitAmount = 1200,
                  Currency = "usd",
                  Recurring = new PriceRecurringOptions
                  {
                      Interval = "month",
                  },
                  Product = product.Id
                };
             */

            ResultOf<Price> returnValue;

            try
            {
                var servicePrice = new PriceService();
                Price price = await servicePrice.CreateAsync(parameter);

                returnValue = new ResultOf<Price>(price);
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                returnValue = new ResultOf<Price>(ex);
            }

            return returnValue;
        }
    }
}