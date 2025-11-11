using Core.DB.Plugin.MySQL.Controllers;
using Core.Shared.Configuration;
using CoreCore.DB.Plugin.Shared.Database;
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

        protected override void Authenticate(CORE_DB_Connection connection)
        {
            AuthenticateAsync(connection).GetAwaiter().GetResult();
        }

        protected override Task AuthenticateAsync(CORE_DB_Connection connection)
        {
            return Task.FromResult(true);
        }

        protected override void Authorize(CORE_DB_Connection connection, List<string> requiredRights)
        {
            AuthorizeAsync(connection, requiredRights).GetAwaiter().GetResult();
        }

        protected override Task AuthorizeAsync(CORE_DB_Connection connection, List<string> requiredRights)
        {
            return Task.FromResult(true);
        }
    }
}