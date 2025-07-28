using Core.Shared.Models;
using Core.Stripe.Models.Product;
using log4net;
using Stripe;
using Product = Stripe.Product;
using ProductService = Stripe.ProductService;

namespace Core.Stripe.Services
{
    public static class StripeService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(StripeService));

        public static async Task<ResultOf<Product>> CreateOrUpdateProduct(STRIPE_CreateOrUpdateProduct_Request parameter)
        {
            ResultOf<Product> returnValue;

            try
            {
                var serviceProduct = new ProductService();
                var servicePrice = new PriceService();

                Product product;

                if (string.IsNullOrEmpty(parameter.ProductId))
                {
                    var productOptions = new ProductCreateOptions
                    {
                        Id = string.IsNullOrEmpty(parameter.ProductId) ? null : parameter.ProductId,
                        Name = parameter.ProductName,
                        Description = parameter.ProductDescription
                    };

                    product = await serviceProduct.CreateAsync(productOptions);

                    if (parameter.PriceData != null)
                    {
                        var newPriceOptions = InitializeNewPriceCreateOptions(product.Id, parameter.PriceData);

                        var newPrice = await servicePrice.CreateAsync(newPriceOptions);

                        product = await serviceProduct.UpdateAsync(product.Id, new ProductUpdateOptions { DefaultPrice = newPrice.Id });
                    }
                }
                else
                {
                    var productOptions = new ProductUpdateOptions
                    {
                        Name = parameter.ProductName,
                        Description = parameter.ProductDescription
                    };

                    product = await serviceProduct.UpdateAsync(parameter.ProductId, productOptions);

                    if (parameter.PriceData != null) // update price data, otherwise ignore
                    {
                        var oldPrice = (await servicePrice.ListAsync(new PriceListOptions { Product = product.Id, Active = true })).Data.FirstOrDefault();

                        if (parameter.PriceData.DiffersFrom(oldPrice))
                        {
                            // create new price

                            var newPriceOptions = InitializeNewPriceCreateOptions(product.Id, parameter.PriceData);

                            var newPrice = await servicePrice.CreateAsync(newPriceOptions);

                            product = await serviceProduct.UpdateAsync(product.Id, new ProductUpdateOptions { DefaultPrice = newPrice.Id });

                            // archive old price
                            if (oldPrice != null)
                            {
                                oldPrice = servicePrice.Update(oldPrice.Id, new PriceUpdateOptions { Active = false });
                            }
                        }
                    }
                }

                returnValue = new ResultOf<Product>(product);
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                returnValue = new ResultOf<Product>(ex);
            }

            return returnValue;
        }

        static bool DiffersFrom(this STRIPE_COUP_R_PriceInfo priceData, Price? price)
        {
            if (price == null) return true;

            if (priceData.Price != price.UnitAmountDecimal * 100)
            {
                return true;
            }

            if ((priceData.IsRecurring && price.Recurring == null) || (!priceData.IsRecurring && price.Recurring != null))
            {
                return true;
            }

            if (priceData.IsRecurring && price.Recurring != null)
            {
                if (priceData.IfRecurring_IsDaily && price.Recurring.Interval != "day") return true;
                if (priceData.IfRecurring_IsWeekly && price.Recurring.Interval != "week") return true;
                if (priceData.IfRecurring_IsMonthly && price.Recurring.Interval != "month") return true;
                if (priceData.IfRecurring_IsYearly && price.Recurring.Interval != "year") return true;
                if (priceData.IfRecurring_IntervalCount != price.Recurring.IntervalCount) return true;
            }

            return false;
        }

        static PriceCreateOptions InitializeNewPriceCreateOptions(string productId, STRIPE_COUP_R_PriceInfo priceData)
        {
            var newPriceOptions = new PriceCreateOptions
            {
                Product = productId,
                Currency = "eur",
                UnitAmountDecimal = priceData.Price * 100
            };

            if (priceData.IsRecurring)
            {
                var interval = "day";

                if (priceData.IfRecurring_IsMonthly)
                {
                    interval = "month";
                }
                else if (priceData.IfRecurring_IsWeekly)
                {
                    interval = "week";
                }
                else
                {
                    interval = "year";
                }

                newPriceOptions.Recurring = new PriceRecurringOptions
                {
                    Interval = interval,
                    IntervalCount = priceData.IfRecurring_IntervalCount
                };
            }

            return newPriceOptions;
        }
    }
}