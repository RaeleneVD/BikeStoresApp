using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BikeStoresApp.Models;

namespace BikeStoresApp.Controllers
{
    public class HomeController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // GET: Home
        public async Task<ActionResult> Index(int? staffIndex, int? customerIndex, int? productIndex, int? brandId, int? categoryId)
        {
            // Load brands and categories for filtering
            ViewBag.Brands = await db.brands.ToListAsync();
            ViewBag.Categories = await db.categories.ToListAsync();

            // Load stores and staff for CreateStaff modal
            ViewBag.Stores = await db.stores.ToListAsync();
            ViewBag.Managers = await db.staffs.ToListAsync();

            // Load all staff and customers
            var staffs = await db.staffs.Include(s => s.store).ToListAsync();
            var customers = await db.customers.ToListAsync();

            // Load products with optional filtering
            var productsQuery = db.products.Include(p => p.brand).Include(p => p.category).AsQueryable();
            if (brandId.HasValue)
                productsQuery = productsQuery.Where(p => p.brand_id == brandId.Value);
            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.category_id == categoryId.Value);

            var products = await productsQuery.ToListAsync();

            // Initialize indexes safely
            ViewBag.StaffIndex = staffIndex.HasValue && staffIndex.Value >= 0 && staffIndex.Value < staffs.Count ? staffIndex.Value : 0;
            ViewBag.CustomerIndex = customerIndex.HasValue && customerIndex.Value >= 0 && customerIndex.Value < customers.Count ? customerIndex.Value : 0;
            ViewBag.ProductIndex = productIndex.HasValue && productIndex.Value >= 0 && productIndex.Value < products.Count ? productIndex.Value : 0;

            // Put data in ViewBag
            ViewBag.Staffs = staffs;
            ViewBag.Customers = customers;
            ViewBag.Products = products;

            return View();
        }

        // POST: CreateStaff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStaff(staff staff)
        {
            if (ModelState.IsValid)
            {
                db.staffs.Add(staff);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: CreateCustomer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCustomer(customer customer)
        {
            if (ModelState.IsValid)
            {
                db.customers.Add(customer);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}