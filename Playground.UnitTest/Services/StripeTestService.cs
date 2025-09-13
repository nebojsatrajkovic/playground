using Core.Shared.Models;
using Core.Stripe.Models.Product;
using Core.Stripe.Services;
using log4net;
using Stripe;

namespace Playground.UnitTest.Services
{
    public static class StripeTestService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(StripeTestService));

        public static async Task<ResultOf<Dictionary<string, Product>>> CreateExampleProducts()
        {
            ResultOf<Dictionary<string, Product>> returnValue;

            try
            {
                Dictionary<string, Product> stripeProducts = [];

                string? productId = null;

                var product = await StripeService.CreateOrUpdateProduct(new STRIPE_CreateOrUpdateProduct_Request
                {
                    ProductId = productId,
                    ProductName = "Basic package",
                    ProductDescription = "Basic",
                    PriceData = new STRIPE_COUP_R_PriceInfo
                    {
                        Price = 10,
                        IsRecurring = true,
                        IfRecurring_IsWeekly = true,
                        IfRecurring_IntervalCount = 1
                    }
                });

                if (product.Succeeded && product.OperationResult != null)
                {
                    stripeProducts[product.OperationResult.Id] = product.OperationResult;
                }
                else
                {
                    return new ResultOf<Dictionary<string, Product>>(product);
                }

                product = await StripeService.CreateOrUpdateProduct(new STRIPE_CreateOrUpdateProduct_Request
                {
                    ProductId = productId,
                    ProductName = "Standard package",
                    ProductDescription = "Standard",
                    PriceData = new STRIPE_COUP_R_PriceInfo
                    {
                        Price = 17,
                        IsRecurring = true,
                        IfRecurring_IsMonthly = true,
                        IfRecurring_IntervalCount = 1
                    }
                });

                if (product.Succeeded && product.OperationResult != null)
                {
                    stripeProducts[product.OperationResult.Id] = product.OperationResult;
                }
                else
                {
                    return new ResultOf<Dictionary<string, Product>>(product);
                }

                product = await StripeService.CreateOrUpdateProduct(new STRIPE_CreateOrUpdateProduct_Request
                {
                    ProductId = productId,
                    ProductName = "Premium package",
                    ProductDescription = "Premium",
                    PriceData = new STRIPE_COUP_R_PriceInfo
                    {
                        Price = 30,
                        IsRecurring = true,
                        IfRecurring_IsYearly = true,
                        IfRecurring_IntervalCount = 1
                    }
                });

                if (product.Succeeded && product.OperationResult != null)
                {
                    stripeProducts[product.OperationResult.Id] = product.OperationResult;
                }
                else
                {
                    return new ResultOf<Dictionary<string, Product>>(product);
                }

                returnValue = new ResultOf<Dictionary<string, Product>>(stripeProducts);
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                returnValue = new ResultOf<Dictionary<string, Product>>(ex);
            }

            return returnValue;
        }
    }
}