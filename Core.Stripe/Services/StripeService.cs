using Core.Shared.Models;
using Core.Stripe.Models.Customer;
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

        public static async Task<ResultOf<Customer>> CreateCustomer(STRIPE_CreateCustomer_Request parameter)
        {
            ResultOf<Customer> returnValue;

            try
            {
                var optionsCustomer = new CustomerCreateOptions
                {
                    Address = new AddressOptions(),
                    Balance = 0,
                    CashBalance = new CustomerCashBalanceOptions { Settings = new CustomerCashBalanceSettingsOptions { ReconciliationMode = "automatic" } },
                    Description = string.Empty,
                    Email = parameter.Email,
                    InvoicePrefix = $"000{DateTime.Now.Millisecond}",
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = null,
                        Footer = $"Invoice footer for {parameter.FirstName} {parameter.LastName}",
                        RenderingOptions = new CustomerInvoiceSettingsRenderingOptionsOptions
                        {
                            AmountTaxDisplay = "include_inclusive_tax"
                        },
                        CustomFields = []
                    },
                    Name = $"{parameter.FirstName} {parameter.LastName}",
                    NextInvoiceSequence = 1,
                    Phone = string.Empty,
                    PreferredLocales = ["de"],
                    Shipping = new ShippingOptions
                    {
                        Address = new AddressOptions
                        {
                            City = string.Empty,
                            Country = string.Empty,
                            Line1 = string.Empty,
                            Line2 = string.Empty,
                            PostalCode = string.Empty,
                            State = string.Empty
                        },
                        Name = $"{parameter.FirstName} {parameter.LastName}",
                        Phone = string.Empty
                    },
                    Source = null,
                    Tax = new CustomerTaxOptions
                    {
                        IpAddress = parameter.IPAddress
                    },
                    TaxExempt = "none",
                    Metadata = []
                };

#if DEBUG
                optionsCustomer.Tax.IpAddress = "175.216.11.165";
#endif

                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(optionsCustomer);

                returnValue = new ResultOf<Customer>(customer);
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                returnValue = new ResultOf<Customer>(ex);
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