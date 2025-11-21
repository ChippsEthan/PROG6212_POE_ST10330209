using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PROG6212ST10330209.Models;
using Microsoft.EntityFrameworkCore;

namespace PROG6212ST10330209.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        // Updated: Show home page instead of redirecting
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> LecturerDashboard()
        {
            await EnsureSeedData();
            var claims = _context.Claims.OrderByDescending(c => c.SubmissionDate).ToList();
            return View(claims);
        }

        public async Task<IActionResult> ManagerDashboard()
        {
            await EnsureSeedData();
            var pendingClaims = _context.Claims.Where(c => c.Status == "Pending").ToList();
            var allClaims = _context.Claims.ToList();

            ViewBag.AllClaims = allClaims;
            return View(pendingClaims);
        }

        // Active Lecturers Action
        public async Task<IActionResult> ActiveLecturers()
        {
            await EnsureSeedData();

            // Get distinct lecturer names who have submitted claims
            var activeLecturers = _context.Claims
                .Select(c => c.LecturerName)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            ViewBag.ActiveLecturers = activeLecturers;
            return View();
        }

        // HR Dashboard for Report Generation
        public async Task<IActionResult> HRDashboard()
        {
            await EnsureSeedData();

            var approvedClaims = _context.Claims
                .Where(c => c.Status == "Approved")
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();

            // Calculate totals for reporting
            ViewBag.TotalApprovedAmount = approvedClaims.Sum(c => c.ClaimAmount);
            ViewBag.TotalApprovedClaims = approvedClaims.Count;
            ViewBag.TotalLecturers = approvedClaims.Select(c => c.LecturerName).Distinct().Count();

            return View(approvedClaims);
        }

        // Generate Report Action
        public IActionResult GenerateReport(string reportType)
        {
            var claims = _context.Claims.ToList();

            // Simple report generation - in production, use proper reporting tools
            var reportData = new
            {
                GeneratedDate = DateTime.Now,
                TotalClaims = claims.Count,
                ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                PendingClaims = claims.Count(c => c.Status == "Pending"),
                RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                TotalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.ClaimAmount),
                ClaimsByLecturer = claims.GroupBy(c => c.LecturerName)
                                        .ToDictionary(g => g.Key, g => g.Count())
            };

            ViewBag.ReportData = reportData;
            ViewBag.ReportType = reportType;

            return View("Report");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile supportingDocument)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (supportingDocument != null && supportingDocument.Length > 0)
                    {
                        var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                        var fileExtension = Path.GetExtension(supportingDocument.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("supportingDocument", "Only PDF, DOCX, and XLSX files are allowed.");
                            var claims = _context.Claims.ToList();
                            return View("LecturerDashboard", claims);
                        }

                        if (supportingDocument.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("supportingDocument", "File size must be less than 5MB.");
                            var claims = _context.Claims.ToList();
                            return View("LecturerDashboard", claims);
                        }

                        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await supportingDocument.CopyToAsync(stream);
                        }

                        claim.FileName = supportingDocument.FileName;
                        claim.FilePath = fileName;
                    }

                    claim.Status = "Pending";
                    claim.SubmissionDate = DateTime.Now;

                    _context.Claims.Add(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Claim submitted successfully!";
                    return RedirectToAction("LecturerDashboard");
                }

                var allClaims = _context.Claims.ToList();
                return View("LecturerDashboard", allClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim");
                TempData["ErrorMessage"] = "An error occurred while submitting your claim. Please try again.";
                return RedirectToAction("LecturerDashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim != null)
                {
                    claim.Status = "Approved";
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Claim approved successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving claim");
                TempData["ErrorMessage"] = "An error occurred while approving the claim.";
            }
            return RedirectToAction("ManagerDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> RejectClaim(int id)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim != null)
                {
                    claim.Status = "Rejected";
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Claim rejected successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim");
                TempData["ErrorMessage"] = "An error occurred while rejecting the claim.";
            }
            return RedirectToAction("ManagerDashboard");
        }

        public IActionResult DownloadDocument(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return NotFound();
                }

                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                var claim = _context.Claims.FirstOrDefault(c => c.FilePath == fileName);
                var downloadFileName = claim?.FileName ?? "document";

                return File(memory, "application/octet-stream", downloadFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document");
                TempData["ErrorMessage"] = "An error occurred while downloading the document.";
                return RedirectToAction("ManagerDashboard");
            }
        }

        public IActionResult ViewAllClaims()
        {
            var claims = _context.Claims.OrderByDescending(c => c.SubmissionDate).ToList();
            return View(claims);
        }

        private async Task EnsureSeedData()
        {
            if (!_context.Claims.Any())
            {
                var claims = new List<Claim>
                {
                    new Claim {
                        LecturerName = "Phil Ross",
                        Description = "INSY",
                        HoursWorked = 10,
                        HourlyRate = 350,
                        Status = "Pending",
                        SubmissionDate = DateTime.Now.AddDays(-2)
                    },
                    new Claim {
                        LecturerName = "Karen Tenten",
                        Description = "CLDV",
                        HoursWorked = 8,
                        HourlyRate = 320,
                        Status = "Approved",
                        SubmissionDate = DateTime.Now.AddDays(-5)
                    }
                };

                await _context.Claims.AddRangeAsync(claims);
                await _context.SaveChangesAsync();
            }
        }

        private string FormatAsRands(decimal amount)
        {
            return $"R{amount:N2}";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}