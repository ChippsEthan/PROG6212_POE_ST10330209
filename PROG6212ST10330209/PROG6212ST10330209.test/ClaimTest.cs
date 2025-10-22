using PROG6212ST10330209.Models;
using Xunit;

namespace PROG6212ST10330209.test
{
    public class ClaimTest
    {
        [Fact]
        public void CalculateTotalAmount()
        {
            var claim = new Claim();

            claim.HourlyRate = 670;
            claim.HoursWorked = 20;

            var getResult = claim.CalculateTotal();

            Assert.Equal(13400, getResult);
        }


    }
}
