using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Auction.Models;
using PagedList;

namespace Auction.Controllers
{
    public class BatDatasController : Controller
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
        public void Updatewinner()
        {
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out int userId))
            {
                int s1 = db.Order_01
                    .Where(x => x.U_id == userId && x.CustProduct.End_time < DateTime.Now && x.Approve != "Accepted")
                    .GroupBy(x => x.CustProduct.Id)
                    .Count();

                Session["mywin"] = s1;
            }
            else
            {
                Session["mybid"] = 0; // Handle the case where the UserId is invalid or null
            }
        }
        public ActionResult ContactUs()
        {
            return View();
        }     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BatData batData,int? proid,int bidprice)
        {
            if (ModelState.IsValid)
            {
                batData.CustID = Convert.ToInt32(Session["UserId"]);
                batData.Price = bidprice;
                batData.ProId = proid;
                batData.Date = DateTime.Now.TimeOfDay.ToString();
                db.BatDatas.Add(batData);
                db.SaveChanges();
                UpdateBidCount();
                return RedirectToAction("Details", "AdminProducts", new { id = proid });
            }
            ViewBag.ProId = new SelectList(db.CustProducts, "Id", "P_Name", batData.ProId);
            ViewBag.CustID = new SelectList(db.Registrations, "Id", "First_Name", batData.CustID);
            return View(batData);
        }
        // GET: BatDatas/Delete/5
        public ActionResult Delete(int? id)
        {
            BatData batData = db.BatDatas.Find(id);
            db.BatDatas.Remove(batData);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult BidAdmin(int? page1)
        {
            int pageSize = 12;
            int pageNumber1 = (page1 ?? 1);
            var allProducts = db.BatDatas.ToList();
            var pagedList1 = allProducts.ToPagedList(pageNumber1, pageSize);
            ViewBag.PagedList1 = pagedList1;
            ViewBag.disp = db.Order_01.ToList();
            var acceptedProductIds = db.Order_01
        .Where(o => o.Approve == "Accepted")
        .Select(o => o.CustProduct.Id)
        .ToList();

            // Fetch pending product IDs
            var pendingProductIds = db.Order_01
                .Where(o => o.Approve == "Pending")
                .Select(o => o.CustProduct.Id)
                .ToList();

            // Combine accepted and pending product IDs
            var notAcceptedProductIds = acceptedProductIds.Union(pendingProductIds).ToList();

            ViewBag.NotAcceptedProductIds = notAcceptedProductIds;
            return View(db.BatDatas.ToList());
        }
        public ActionResult BidDetail(int? id)
        {
            ViewBag.Approval = db.Order_01.ToList();
            var bat = db.BatDatas.Where(x => x.ProId == id).ToList();
            return View(bat);
        }
        public ActionResult WinerPage()
        {
            Updatewinner();
            return View(db.Order_01.ToList());
        }
        [HttpPost]
        public ActionResult AddOrder(int price, DateTime date, int uid, int bid, int pid)
        {
            var order_01 = new Order_01
            {
                price = price,
                date = date,
                U_id = uid,
                Bid_id = bid,
                Pro_id = pid,
                Approve = "Panding"
                
            };
            db.Order_01.Add(order_01);
            db.SaveChanges();
            Updatewinner();
            return RedirectToAction("BidDetail", "BatDatas", new { id = pid });
        }

        [HttpPost]
        public ActionResult AcceptOrder(int? id) 
        {
            Order_01 order = db.Order_01.Find(id);
            order.Approve = "Accepted";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            Updatewinner();
            return RedirectToAction("WinerPage");
        }
        [HttpPost]
        public ActionResult Reject(int? id)
        {
            Order_01 order = db.Order_01.Find(id);
            order.Approve = "Reject";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            Updatewinner();
            return RedirectToAction("WinerPage");
        }
        [HttpPost]
        public ActionResult ChangeWinner(int? bid, DateTime date,int price,int uid,int? pid)
        {
            Order_01 order = db.Order_01.Where(x=>x.Pro_id==pid).First();
            order.Approve = "Panding";
            order.date = date;
            order.price = price;
            order.U_id = uid;
            order.Bid_id = bid;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            Updatewinner();
            return RedirectToAction("BidDetail", "BatDatas", new { id = pid });
        }
    }
}
