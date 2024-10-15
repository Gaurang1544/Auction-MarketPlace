using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Auction.Models;
using Microsoft.Ajax.Utilities;
using PagedList;

namespace Auction.Controllers
{
    public class AdminProductsController : Controller
    {
        private AuctionDataEntities1 db = new AuctionDataEntities1();

        //Admin dashboard
        public ActionResult DashBoard()
        {
            var groupedItems = db.Order_01.Count();
            ViewBag.winer1 = groupedItems;
            int count_u = db.Registrations.Count();
            ViewBag.Count = count_u;
            int count_p = db.CustProducts.Count();
            ViewBag.Product = count_p;
            int count_a = db.BatDatas.Count();
            ViewBag.BatDatas = count_a;
            return View();
        }

        public ActionResult ProductList(string sortOrder, string searchQuery,int? selectedCategory)
        {
            // Fetch the data from your data source
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
            // Pass the sorted and filtered data to the view
            return View(products.ToList());
        }


        //this Actionmethod is use to display the product details


        public ActionResult Details(int? id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Registrations");
            }
            
            ViewBag.totalbet = db.BatDatas.Where(x => x.ProId == id).OrderByDescending(x => x.Price).ToList();
            CustProduct custProduct = db.CustProducts.Find(id);
            if (custProduct != null)
            {
                // Filter products by comparing the CategoryId instead of the whole object
                ViewBag.product = db.CustProducts
                    .Where(x => x.Category.Id == custProduct.C_id)
                    .ToList().Take(5);
            }
            else
            {
                ViewBag.product = new List<CustProduct>();
            }
            int userid = Convert.ToInt32(Session["UserId"]);
            
            if (Session["UserID"] != null)
            {
                //id of max bet user
                var maxPrice = db.BatDatas.Max(x => x.Price);
                ViewBag.MaxId = db.BatDatas.Where(x => x.Price == maxPrice).Select(x => x.Registration.Id).FirstOrDefault();
               
                ViewBag.User = db.Registrations.ToList();

                return View(custProduct);
            }
            return View(custProduct);
        }
        public ActionResult ProductDisplay() { 
            return View(db.Order_01.ToList());
        }
        public ActionResult Vendor(int? page1)
        {
            int pageSize = 12;
            int pageNumber1 = (page1 ?? 1);
            var allProducts = db.Registrations.ToList();
            var pagedList1 = allProducts.ToPagedList(pageNumber1, pageSize);
            ViewBag.PagedList1 = pagedList1;
            return View();
        }
        public ActionResult VendorDetail(int? id,int? page) {
            Registration registration = db.Registrations.Find(id);
            int pageSize = 8;
            int pageNumber1 = (page ?? 1);
            var allProducts = db.CustProducts.Where(x => x.U_id == id).ToList();
            var pagedList1 = allProducts.ToPagedList(pageNumber1, pageSize);
            ViewBag.PagedList = pagedList1;
            return View(registration);
        }
        public ActionResult Admineditprofile(int? id,Registration registration) { 
            registration = db.Registrations.Find(id);
            return View(registration);
        }
        [HttpPost]
        public ActionResult Admineditprofile(Registration registration, HttpPostedFileBase file,string newpass)
        {
            if (file != null && file.ContentLength > 0)
            {
                string uniquefilename = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string pathname = Path.Combine(Server.MapPath("~/Images"), uniquefilename);
                file.SaveAs(pathname);
                registration.Image = uniquefilename;
            }
            if (!string.IsNullOrEmpty(newpass))
            {
                registration.Password = newpass;
            }
            db.Entry(registration).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Admineditprofile", new { id = registration.Id });
        }

        public ActionResult LiveAuction(int? page1)
        {
            int pageSize = 12;
            int pageNumber1 = (page1 ?? 1);
            var allProducts = db.BatDatas.ToList();
            var pagedList1 = allProducts.ToPagedList(pageNumber1, pageSize);
            ViewBag.PagedList1 = pagedList1;
            return View(db.BatDatas.ToList());
        }
    }
}
