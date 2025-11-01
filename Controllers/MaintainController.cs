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

        // GET: Maintain
        public async Task<ActionResult> Maintain(
            int staffIndex = 0,
            int customerIndex = 0,
            int productIndex = 0,
            int? brandId = null,
            int? categoryId = null)
        {
            // Load Staffs
            ViewBag.Staffs = await db.staffs.Include(s => s.store).ToListAsync();
            ViewBag.StaffIndex = staffIndex;

            // Load Customers
            ViewBag.Customers = await db.customers.ToListAsync();
            ViewBag.CustomerIndex = customerIndex;

            // Load Products with optional filters
            var productsQuery = db.products.Include(p => p.brand).Include(p => p.category).AsQueryable();

            if (brandId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.brand_id == brandId.Value);
            }

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.category_id == categoryId.Value);
            }

            ViewBag.Products = await productsQuery.ToListAsync();
            ViewBag.ProductIndex = productIndex;

            // Load dropdowns for products
            ViewBag.Brands = await db.brands.ToListAsync();
            ViewBag.Categories = await db.categories.ToListAsync();

            return View();
        }

        #region Staff Actions
        public async Task<ActionResult> EditStaff(int id)
        {
            var staff = await db.staffs.FindAsync(id);
            if (staff == null) return HttpNotFound();
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStaff(staff updatedStaff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(updatedStaff).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Maintain");
            }
            return View("EditStaff", updatedStaff);
        }

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
        public async Task<ActionResult> EditCustomer(int id)
        {
            var customer = await db.customers.FindAsync(id);
            if (customer == null) return HttpNotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateCustomer(customer updatedCustomer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(updatedCustomer).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Maintain");
            }
            return View("EditCustomer", updatedCustomer);
        }

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
        public async Task<ActionResult> EditProduct(int id)
        {
            var product = await db.products.FindAsync(id);
            if (product == null) return HttpNotFound();
            ViewBag.Brands = await db.brands.ToListAsync();
            ViewBag.Categories = await db.categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProduct(product updatedProduct)
        {
            if (ModelState.IsValid)
            {
                db.Entry(updatedProduct).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Maintain");
            }
            ViewBag.Brands = await db.brands.ToListAsync();
            ViewBag.Categories = await db.categories.ToListAsync();
            return View("EditProduct", updatedProduct);
        }

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
