using Core.DB.Plugin.MySQL.Controllers;
using Core.Shared.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Core.Stripe.Controllers
{
    public class StripeController(ILogger<StripeController> logger) : MySQL_ResultOf_AbstractController(logger, CORE_Configuration.Database.ConnectionString)
    {
        [HttpPost("create-checkout-session")]
        public ActionResult CreateCheckoutSession()
        {
            var options = new SessionCreateOptions
            {
                LineItems =
                [
                  new SessionLineItemOptions
                  {
                      PriceData = new SessionLineItemPriceDataOptions
                      {
                          UnitAmountDecimal = 2000,
                          Currency = "eur",
                          Product = string.Empty, // TODO
                          Recurring = new SessionLineItemPriceDataRecurringOptions
                          {
                              Interval = "month",
                              IntervalCount = 1
                          }
                      },
                      Quantity = 1
                  },
                ],
                Mode = "subscription",
                SuccessUrl = "http://localhost:4242/success",
                CancelUrl = "http://localhost:4242/cancel",
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>(),
                    TrialPeriodDays = 7,
                    Description = "",
                    ApplicationFeePercent = 12.5m
                },
                Metadata = new Dictionary<string, string>(),
                Customer = string.Empty, // TODO
                AllowPromotionCodes = true
            };

            var service = new SessionService();
            var session = service.Create(options);

            HttpContext.Response.Headers.Location = session.Url;

            return new StatusCodeResult(303);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            const string endpointSecret = "whsec_...";

            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                var signatureHeader = Request.Headers["Stripe-Signature"];

                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);

                var stripeEventType = stripeEvent.Type;

                switch (stripeEventType)
                {
                    case EventTypes.CustomerSubscriptionCreated:
                    case EventTypes.CustomerSubscriptionUpdated:
                    case EventTypes.CustomerSubscriptionDeleted:
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Failed to execute {method}", nameof(StripeWebhook));

                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to execute {method}", nameof(StripeWebhook));

                return StatusCode(500);
            }
        }
    }
}