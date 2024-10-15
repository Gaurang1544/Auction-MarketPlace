using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Auction.Models;

namespace Auction.Controllers
{
    public class CustProductsController : Controller
    {
        private AuctionDataEntities1 db = new AuctionDataEntities1();

        // GET: CustProducts
        public ActionResult Index(string sortOrder, string searchQuery, int? selectedCategory)
        {
            var products = db.CustProducts.AsQueryable();

            // Apply search filtering
            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.Name.Contains(searchQuery) || p.ProName.Name.Contains(searchQuery));
            }

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Price); // Default sorting
                    break;
            }

            if (selectedCategory.HasValue)
            {
                products = products.Where(p => p.Category.Id == selectedCategory.Value);
            }
            ViewBag.category = db.Categories.ToList();
            return View(products.ToList());
        }



        // GET: CustProducts/Details/5
        public ActionResult ProductDis(string sortOrder, string searchQuery, int? selectedCategory)
        {
            var products = db.CustProducts.AsQueryable();

            // Apply search filtering
            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.Name.Contains(searchQuery) || p.ProName.Name.Contains(searchQuery));
            }

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Price); // Default sorting
                    break;
            }

            if (selectedCategory.HasValue)
            {
                products = products.Where(p => p.Category.Id == selectedCategory.Value);
            }
            ViewBag.batdata = db.BatDatas.ToList();
            ViewBag.category = db.Categories.ToList();
            return View(products.ToList());
        }

        // GET: CustProducts/Create
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCategories = db.ProNames
                .Where(p => p.C_id == categoryId)
                .Select(p => new { Value = p.Id, Text = p.Name })
                .ToList();
            return Json(subCategories, JsonRequestBehavior.AllowGet);
        }

        // GET: CustProducts/Create
        public ActionResult Create(int? selectedCategoryId, int? selectedProNameId)
        {
            var categories = db.Categories.ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", selectedCategoryId);

            if (selectedCategoryId.HasValue)
            {
                var subCategories = db.ProNames
                    .Where(p => p.C_id == selectedCategoryId.Value)
                    .ToList();
                ViewBag.SubCategoryList = new SelectList(subCategories, "Id", "Name", selectedProNameId);
            }
            else
            {
                ViewBag.SubCategoryList = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustProduct product, int? selectedCategoryId, int? selectedProNameId, HttpPostedFileBase image1,DateTime startdate, DateTime enddate, HttpPostedFileBase image2, HttpPostedFileBase image3, HttpPostedFileBase image4, HttpPostedFileBase video1)
        {
            if (image1 != null)
            {
                string fileExtension = Path.GetExtension(image1.FileName).ToLower();
                string uniquefilename1 = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image1.FileName);
                string pathname1 = Path.Combine(Server.MapPath("~/ProductImage"), uniquefilename1);

                image1.SaveAs(pathname1);
                product.Image1 = uniquefilename1;
            }
            if (image2 != null)
            {
                string uniquefilename2 = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image2.FileName);
                string pathname2 = Path.Combine(Server.MapPath("~/ProductImage"), uniquefilename2);
                image2.SaveAs(pathname2);
                product.Image2 = uniquefilename2;
            }
            if (image3 != null)
            {
                string uniquefilename3 = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image1.FileName);
                string pathname3 = Path.Combine(Server.MapPath("~/ProductImage"), uniquefilename3);
                image3.SaveAs(pathname3);
                product.Image3 = uniquefilename3;
            }
            if (image4 != null)
            {
                string uniquefilename4 = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image4.FileName);
                string pathname4 = Path.Combine(Server.MapPath("~/ProductImage"), uniquefilename4);
                image4.SaveAs(pathname4);
                product.Image4 = uniquefilename4;
            }

            if (video1 != null && video1.ContentLength > 0 && video1.ContentLength < 1048576000)
            {
                string uniqueFileName = Path.GetFileName(video1.FileName);

                string videoDirectory = Server.MapPath("~/ProductVideo/");

                string filePath = Path.Combine(videoDirectory, uniqueFileName);

                video1.SaveAs(filePath);

                product.Video = "~/ProductVideo/" + uniqueFileName;
            }
            if (ModelState.IsValid)
            {
                product.Start_time = startdate;
                product.End_time = enddate; 
                product.C_id= Convert.ToInt32(selectedCategoryId);
                product.Pname_Id = Convert.ToInt32(selectedProNameId);
                product.U_id = Convert.ToInt32(Session["UserId"]);
                db.CustProducts.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        [HttpPost]
        public ActionResult SetDate(DateTime startdate,DateTime enddate,int? id)
        {
            CustProduct prd = db.CustProducts.Find(id);
            prd.Start_time = startdate;
            prd.End_time = enddate;
            db.Entry(prd).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            CustProduct custProduct = db.CustProducts.Find(id);
            db.CustProducts.Remove(custProduct);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult AddCategory()
        {
            ViewBag.sub = db.ProNames.ToList();
            return View(db.Categories.ToList());
        }

        [HttpPost]
        public ActionResult AddCategory(string categoryType, string existingCategory, string newCategory, int? subcategoryCount, FormCollection form)
        {
            if (!subcategoryCount.HasValue || subcategoryCount <= 0)
            {
                ModelState.AddModelError("subcategoryCount", "Please enter a valid number of subcategories.");
                return View(db.Categories.ToList());  // Reload view with categories
            }

            List<string> subcategoryNames = new List<string>();

            // Gather subcategory names from the form
            for (int i = 1; i <= subcategoryCount; i++)
            {
                string subcategoryName = form[$"subcategory_{i}"];
                if (!string.IsNullOrWhiteSpace(subcategoryName))
                {
                    subcategoryNames.Add(subcategoryName);
                }
            }
            try
            {
                int categoryId;

                // If adding a new category
                if (categoryType == "new" && !string.IsNullOrWhiteSpace(newCategory))
                {
                    var newCat = new Category { Name = newCategory };
                    db.Categories.Add(newCat);
                    db.SaveChanges();  // Save new category to get its ID

                    categoryId = newCat.Id; // Use the new category ID
                }
                // If adding subcategories to an existing category
                else if (categoryType == "existing" && !string.IsNullOrWhiteSpace(existingCategory))
                {
                    // Parse the existing category ID
                    if (!int.TryParse(existingCategory, out categoryId))
                    {
                        ModelState.AddModelError("", "Invalid existing category ID.");
                        return View(db.Categories.ToList());
                    }

                    // Check if the category exists in the database
                    var existingCat = db.Categories.Find(categoryId);
                    if (existingCat == null)
                    {
                        ModelState.AddModelError("", "The selected category does not exist.");
                        return View(db.Categories.ToList());
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please select a category type and provide necessary details.");
                    return View(db.Categories.ToList());
                }

                // Add subcategories to the specified category (new or existing)
                AddSubcategories(categoryId, subcategoryNames);
                TempData["SuccessMessage"] = "Category added successfully!";
                // Save all changes
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while adding the category/subcategories: " + ex.Message);
                return View(db.Categories.ToList());
            }

            return RedirectToAction("Addcategory");
        }

        // Helper method to add subcategories
        private void AddSubcategories(int categoryId, List<string> subcategoryNames)
        {
            foreach (var name in subcategoryNames)
            {
                var subcategory = new ProName { Name = name, C_id = categoryId }; // Link subcategory to the selected category
                db.ProNames.Add(subcategory);
            }
            db.SaveChanges();  // Save changes after adding subcategories
        }


        public ActionResult DeleteCategory(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var relatedProducts = db.ProNames.Where(p => p.C_id == id).ToList();
            db.ProNames.RemoveRange(relatedProducts);

            db.Categories.Remove(category);
            db.SaveChanges();

            return RedirectToAction("Addcategory");
        }

        public ActionResult DeleteSubCategory(int? id)
        {
            var category = db.ProNames.Find(id);
            db.ProNames.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Addcategory");
        }

    }
}
