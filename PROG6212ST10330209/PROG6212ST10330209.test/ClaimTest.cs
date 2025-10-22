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

        [Fact]
        public void AdditionalNotes_Simulation()
        {
            var claim = new Claim();
            claim.Notes = "Additional Notes Submitted";

            var notes = claim.Notes;
            Assert.Equal("Additional Notes Submitted", notes);
        }



        [Fact]
        public void FileProperties_IsStoredCorrectly()
        {
            var claim = new Claim();

            claim.FileName = "invoice.pdf";
            claim.FilePath = "/uploads/invoice.pdf";

            Assert.Equal("invoice.pdf", claim.FileName);
            Assert.Equal("/uploads/invoice.pdf", claim.FilePath);
        }

    }
}
