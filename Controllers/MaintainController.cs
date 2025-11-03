using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BikeStoresApp.Models;

namespace BikeStoresApp.Controllers
{
    public class MaintainController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // GET: Maintain - Main page
        public async Task<ActionResult> Maintain(
            int? staffIndex,
            int? customerIndex,
            int? productIndex,
            int? brandId,
            int? categoryId)
        {
            // Load brands and categories for filtering
            ViewBag.Brands = await db.brands.ToListAsync();
            ViewBag.Categories = await db.categories.ToListAsync();

            // Load stores and managers for staff editing
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

            // Initialize indexes 
            ViewBag.StaffIndex = staffIndex.HasValue && staffIndex.Value >= 0 && staffIndex.Value < staffs.Count ? staffIndex.Value : 0;
            ViewBag.CustomerIndex = customerIndex.HasValue && customerIndex.Value >= 0 && customerIndex.Value < customers.Count ? customerIndex.Value : 0;
            ViewBag.ProductIndex = productIndex.HasValue && productIndex.Value >= 0 && productIndex.Value < products.Count ? productIndex.Value : 0;

            // Put data in ViewBag
            ViewBag.Staffs = staffs;
            ViewBag.Customers = customers;
            ViewBag.Products = products;

            return View();
        }

        #region Staff Actions
        // POST: Update staff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditStaff(staff updatedStaff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(updatedStaff).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Maintain");
        }

        // GET: Delete staff
        public async Task<ActionResult> DeleteStaff(int id)
        {
            var staff = await db.staffs.FindAsync(id);
            if (staff != null)
            {
                db.staffs.Remove(staff);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Maintain");
        }
        #endregion

        #region Customer Actions
        // POST: Update customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCustomer(customer updatedCustomer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(updatedCustomer).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Maintain");
        }

        // GET: Delete customer
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            var customer = await db.customers.FindAsync(id);
            if (customer != null)
            {
                db.customers.Remove(customer);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Maintain");
        }
        #endregion

        #region Product Actions
        // POST: Update product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProduct(product updatedProduct)
        {
            if (ModelState.IsValid)
            {
                db.Entry(updatedProduct).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Maintain");
        }

        // GET: Delete product
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await db.products.FindAsync(id);
            if (product != null)
            {
                db.products.Remove(product);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Maintain");
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}