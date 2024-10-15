using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Auction.Models;
using CaptchaMvc.Infrastructure;
using CaptchaMvc;
using CaptchaMvc.HtmlHelpers;

namespace Auction.Controllers
{
    public class BatRegsController : Controller
    {
        private AuctionDataEntities1 db = new AuctionDataEntities1();

        // GET: BatRegs
        public ActionResult Index()
        {
            var batRegs = db.BatRegs.Include(b => b.AdminProduct).Include(b => b.Registration);
            return View(batRegs.ToList());
        }
    
      [HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create(BatReg batReg, int? proid)
{
    if (ModelState.IsValid)
    {
            batReg.proid = proid;
            batReg.CustId = Convert.ToInt32(Session["UserId"]);
            db.BatRegs.Add(batReg);
            db.SaveChanges();
            return RedirectToAction("Details", "AdminProducts", new {id = proid });
    }
    return View();
}

        public ActionResult Delete(int? id)
        {
            BatReg batReg = db.BatRegs.Find(id);
            db.BatRegs.Remove(batReg);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
