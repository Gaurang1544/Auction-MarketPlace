using Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;

namespace Auction.Controllers
{
    public class HomeController : Controller
    {
        private AuctionDataEntities1 db = new AuctionDataEntities1();
        public void UpdateBidCount()
        {
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out int userId))
            {
                int s1 = db.BatDatas
                    .Where(x => x.CustID == userId && x.CustProduct.End_time > DateTime.Now)
                    .GroupBy(x => x.CustProduct.Id)
                    .Count();

                Session["mybid"] = s1;
            }
            else
            {
                Session["mybid"] = 0; // Handle the case where the UserId is invalid or null
            }
        }

        public ActionResult MainHomePage(int? page1,int? id,CustProduct custProduct, string searchdata)
        {
            
            int pageSize = 24; 
            int pageNumber1 = (page1 ?? 1);
            var allProducts = db.CustProducts.ToList(); 
            if (id != null)
            {
                var item = db.Categories.Find(id);
                allProducts = db.CustProducts.Where(x => x.C_id == id).ToList();
            }
            else if (searchdata != null)
            {
                allProducts = db.CustProducts.Where(x => x.Name == searchdata || x.Category.Name == searchdata || x.ProName.Name == searchdata).ToList();
            }
            else
                    {
                allProducts = db.CustProducts.ToList();
            }
            var pagedList1 = allProducts.ToPagedList(pageNumber1, pageSize);
           
            ViewBag.PagedList1 = pagedList1;
            return View();
        }

        [ChildActionOnly]
        public ActionResult ProductsPartial()
        {
            var customers = db.Registrations.ToList();
            var userId = Convert.ToInt32(Session["UserId"]);
            var customer = db.Registrations.FirstOrDefault(c => c.Id == userId);
            return PartialView("ProductsPartial", customer);
        }
        [HttpPost]
        public ActionResult Logout() {
            Session.Remove("UserId");
            Session.RemoveAll();
            return RedirectToAction("MainHomePage","Home");
        }

        [ChildActionOnly]
        public ActionResult CategoryDisplay()
        {
            var Category1 = db.Categories.ToList();
            return PartialView("CategoryDisplay", Category1.ToList());
        }
        public ActionResult Reoprtbuy() 
        { 
            return View(db.Order_01.ToList());
        }
        [HttpPost]
        public ActionResult Reoprtbuy(int? year, int? month)
        {
            if (year != null && month != null)
            {
                var records = db.Order_01
                    .Where(o => o.date.Value.Year == year && o.date.Value.Month == month)
                    .ToList();

                return View(records);
            }
            if(year != null && month == null)
            {
                var records = db.Order_01
                   .Where(o => o.date.Value.Year == year)
                   .ToList();
                return View(records);
            }
            return View(db.Order_01.ToList());
            }
        public ActionResult Home()
        {
            return View(db.CustProducts.ToList());
        }

        public ActionResult PurchaseHistory() { 
            return View(db.Order_01.ToList());
        }

        public ActionResult mybid() 
        {
            UpdateBidCount();
            return View(db.BatDatas.ToList());
        }
    }
}
