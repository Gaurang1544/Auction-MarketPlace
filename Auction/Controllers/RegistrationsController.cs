using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Auction.Models;
using Microsoft.Ajax.Utilities;

namespace Auction.Controllers
{
    public class RegistrationsController : Controller
    {
        private AuctionDataEntities1 db = new AuctionDataEntities1();

        //Login method
        public ActionResult Login()
        {
            return View();
        }
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

        [HttpPost]
        public ActionResult Login(string Email, string Password, string select)
        {
            var login = db.Registrations.Where(x => x.Email == Email && x.Password == Password).ToList();
            if (login.Any())
            {
                if (login.First().Type == "Customer")
                {
                    Session["UserId"] = login.First().Id;
                    UpdateBidCount();
                    Updatewinner();
                    TempData["SuccessMessage"] = "Login successful! Welcome back "+login.First().First_Name+" "+login.First().LastName+".";
                    return RedirectToAction("Home", "Home");
                }
                if (login.First().Type == "Admin")
                {
                    Session["UserId"] = login.First().Id;
                    TempData["SuccessMessage"] = "Login successful! Welcome to the admin dashboard.";
                    return RedirectToAction("DashBoard", "AdminProducts");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid email or password.";
                return RedirectToAction("Login"); // Redirect to avoid form resubmission
            }

            return View();
        }

        // GET: Registrations/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Registration registration, HttpPostedFileBase image1, string ConfirmPassword)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Registrations.FirstOrDefault(x => x.Email == registration.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(registration);
                }

                if (registration.Password != ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    return View(registration);
                }

                if (image1 != null && image1.ContentLength > 0)
                {
                    string uniquefilename = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image1.FileName);
                    string pathname = Path.Combine(Server.MapPath("~/Images"), uniquefilename);
                    image1.SaveAs(pathname);
                    registration.Image = uniquefilename;
                }

                registration.Type = "Customer";
                db.Registrations.Add(registration);
                db.SaveChanges();
                Session["UserId"] = registration.Id;
                TempData["SuccessMessage"] = "Registration successful! Welcome " + registration.First_Name + " " + registration.LastName + " in Auction Market Place.";
                return RedirectToAction("Home", "Home");
            }
            return View(registration);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registration registration = db.Registrations.Find(id);
            if (registration == null)
            {
                return HttpNotFound();
            }
            return View(registration);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Registration registration, HttpPostedFileBase image1)
        {
            if (ModelState.IsValid)
            {
                if (image1 != null && image1.ContentLength > 0)
                {
                    string uniquefilename = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image1.FileName);
                    string pathname = Path.Combine(Server.MapPath("~/Images"), uniquefilename);
                    image1.SaveAs(pathname);
                    registration.Image = uniquefilename;
                }
                registration.Type = "Customer";
                db.Entry(registration).State = EntityState.Modified;
                db.SaveChanges();

                // Set a TempData message
                TempData["SuccessMessage"] = "Your profile has been successfully updated!";
                return RedirectToAction("Edit", new { id = registration.Id });
            }
            return View(registration);
        }
        // GET: Registrations/Delete/5
        public ActionResult Delete(int? id)
        {
            Registration registration = db.Registrations.Find(id);
            db.Registrations.Remove(registration);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult AboutUs()
        {
            return View();
        }
    }
}
