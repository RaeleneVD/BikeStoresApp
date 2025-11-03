using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BikeStoresApp.Models;

namespace BikeStoresApp.Controllers
{
    public class ReportController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // GET: Report
        public async Task<ActionResult> Report()
        {
            // Generate Popular Products Report using explicit joins
            var popularProducts = await (from oi in db.order_items
                                         join p in db.products on oi.product_id equals p.product_id
                                         join b in db.brands on p.brand_id equals b.brand_id
                                         join c in db.categories on p.category_id equals c.category_id
                                         group oi by new
                                         {
                                             p.product_id,
                                             p.product_name,
                                             b.brand_name,
                                             c.category_name
                                         } into g
                                         select new
                                         {
                                             ProductName = g.Key.product_name,
                                             BrandName = g.Key.brand_name,
                                             CategoryName = g.Key.category_name,
                                             TotalOrders = g.Sum(x => x.quantity),
                                             TotalRevenue = g.Sum(x => x.quantity * x.list_price)
                                         })
                                         .OrderByDescending(p => p.TotalOrders)
                                         .Take(10)
                                         .ToListAsync();

            ViewBag.PopularProducts = popularProducts;

            // Get list of saved reports from Reports folder
            var reportsFolder = Server.MapPath("~/Reports");
            if (!Directory.Exists(reportsFolder))
            {
                Directory.CreateDirectory(reportsFolder);
            }

            var fileInfoList = new DirectoryInfo(reportsFolder).GetFiles();
            var savedReports = fileInfoList
                .Select(f => new
                {
                    FileName = f.Name,
                    FilePath = f.FullName,
                    FileSize = f.Length,
                    DateCreated = f.CreationTime
                })
                .OrderByDescending(f => f.DateCreated)
                .ToList();

            ViewBag.SavedReports = savedReports;

            return View();
        }

        // POST: Save Report
        [HttpPost]
        public async Task<ActionResult> SaveReport(string fileName, string fileType)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                TempData["Error"] = "Please enter a filename.";
                return RedirectToAction("Report");
            }

            // Generate report content using explicit joins
            var popularProducts = await (from oi in db.order_items
                                         join p in db.products on oi.product_id equals p.product_id
                                         join b in db.brands on p.brand_id equals b.brand_id
                                         join c in db.categories on p.category_id equals c.category_id
                                         group oi by new
                                         {
                                             p.product_id,
                                             p.product_name,
                                             b.brand_name,
                                             c.category_name
                                         } into g
                                         select new
                                         {
                                             ProductName = g.Key.product_name,
                                             BrandName = g.Key.brand_name,
                                             CategoryName = g.Key.category_name,
                                             TotalOrders = g.Sum(x => x.quantity),
                                             TotalRevenue = g.Sum(x => x.quantity * x.list_price)
                                         })
                                         .OrderByDescending(p => p.TotalOrders)
                                         .Take(10)
                                         .ToListAsync();
            ViewBag.PopularProducts = popularProducts.Cast<dynamic>().ToList();

            // Create report content
            var reportContent = "BIKE STORES - POPULAR PRODUCTS REPORT\n";
            reportContent += "Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n";
            reportContent += "=".PadRight(80, '=') + "\n\n";
            reportContent += string.Format("{0,-30} {1,-15} {2,-15} {3,10} {4,12}\n",
                "Product", "Brand", "Category", "Orders", "Revenue");
            reportContent += "-".PadRight(80, '-') + "\n";

            foreach (var product in popularProducts)
            {
                var productName = product.ProductName ?? "Unknown";
                if (productName.Length > 30)
                    productName = productName.Substring(0, 27) + "...";

                reportContent += string.Format("{0,-30} {1,-15} {2,-15} {3,10} ${4,11:N2}\n",
                    productName,
                    product.BrandName ?? "Unknown",
                    product.CategoryName ?? "Unknown",
                    product.TotalOrders,
                    product.TotalRevenue);
            }

            // Save to file
            var reportsFolder = Server.MapPath("~/Reports");
            if (!Directory.Exists(reportsFolder))
            {
                Directory.CreateDirectory(reportsFolder);
            }

            var extension = fileType == "csv" ? ".csv" : ".txt";
            var filePath = Path.Combine(reportsFolder, fileName + extension);

            if (fileType == "csv")
            {
                // Save as CSV
                var csvContent = "Product,Brand,Category,Total Orders,Total Revenue\n";
                foreach (var product in popularProducts)
                {
                    csvContent += $"\"{product.ProductName ?? "Unknown"}\",\"{product.BrandName ?? "Unknown"}\",\"{product.CategoryName ?? "Unknown"}\",{product.TotalOrders},{product.TotalRevenue:F2}\n";
                }
                System.IO.File.WriteAllText(filePath, csvContent);
            }
            else
            {
                // Save as TXT
                System.IO.File.WriteAllText(filePath, reportContent);
            }

            TempData["Success"] = "Report saved successfully!";
            return RedirectToAction("Report");
        }

        // GET: Download Report
        public ActionResult DownloadReport(string fileName)
        {
            var filePath = Server.MapPath("~/Reports/" + fileName);

            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "File not found.";
                return RedirectToAction("Report");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var extension = Path.GetExtension(fileName).ToLower();
            var contentType = extension == ".csv" ? "text/csv" : "text/plain";

            return File(fileBytes, contentType, fileName);
        }

        // GET: Delete Report
        public ActionResult DeleteReport(string fileName)
        {
            var filePath = Server.MapPath("~/Reports/" + fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["Success"] = "Report deleted successfully!";
            }
            else
            {
                TempData["Error"] = "File not found.";
            }

            return RedirectToAction("Report");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}