using Playground.UnitTest.Services;

namespace Playground.UnitTest
{
    public class StripeUnitTest
    {
        [Fact]
        public async Task TestProductCreation()
        {
            var exampleProducts = await StripeTestService.CreateExampleProducts();

            Assert.True(exampleProducts.Succeeded, exampleProducts.Message);
            Assert.NotNull(exampleProducts.OperationResult);
            Assert.NotEmpty(exampleProducts.OperationResult);
            Assert.All(exampleProducts.OperationResult, x => Assert.False(string.IsNullOrEmpty(x.Key), "Product key should not be null or empty"));
        }
    }
}