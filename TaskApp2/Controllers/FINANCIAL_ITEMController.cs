using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TaskApp2.Models;

namespace TaskApp2.Controllers
{
    public class FINANCIAL_ITEMController : Controller
    {
        private TaskAppDBEntities db = new TaskAppDBEntities();

       
        public ActionResult Index()
        {
            var lista = db.PARTNER.ToList();
            if (lista.Count==0) { filldata(); }
            var fINANCIAL_ITEM = db.FINANCIAL_ITEM.Include(f => f.PARTNER);
            return View(fINANCIAL_ITEM.ToList());
        }

        private void filldata()
        {
            var random = new Random();

            for (int i = 1; i <= 15; i++)
            {
                var rand = random.Next(1, 15);
                var partner = new PARTNER();
                partner.PARTNER_ID = i;
                partner.PARTNER_NAME = "Partner-" + i.ToString();
                if (rand >= i)
                    partner.PARENT_PARTNER_ID = null;
                else partner.PARENT_PARTNER_ID = rand;
                partner.FEE_PERCENT = random.Next(1, 20);
                db.PARTNER.Add(partner);
                db.SaveChanges();
            }
        }

        public ActionResult Details(decimal? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FINANCIAL_ITEM fINANCIAL_ITEM = db.FINANCIAL_ITEM.Find(id);
            if (fINANCIAL_ITEM == null)
            {
                return HttpNotFound();
            }
            return View(fINANCIAL_ITEM);
        }

     
        public ActionResult Create()
        {
            ViewBag.PARTNER_ID = new SelectList(db.PARTNER, "PARTNER_ID", "PARTNER_NAME");
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FINANCIAL_ITEM_ID,PARTNER_ID,DATE,AMOUNT")] FINANCIAL_ITEM fINANCIAL_ITEM)
        {
            if (ModelState.IsValid)
            {
                db.FINANCIAL_ITEM.Add(fINANCIAL_ITEM);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PARTNER_ID = new SelectList(db.PARTNER, "PARTNER_ID", "PARTNER_NAME", fINANCIAL_ITEM.PARTNER_ID);
            return View(fINANCIAL_ITEM);
        }

        
        public ActionResult Edit(decimal? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FINANCIAL_ITEM fINANCIAL_ITEM = db.FINANCIAL_ITEM.Find(id);
            if (fINANCIAL_ITEM == null)
            {
                return HttpNotFound();
            }
            ViewBag.PARTNER_ID = new SelectList(db.PARTNER, "PARTNER_ID", "PARTNER_NAME", fINANCIAL_ITEM.PARTNER_ID);
            return View(fINANCIAL_ITEM);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FINANCIAL_ITEM_ID,PARTNER_ID,DATE,AMOUNT")] FINANCIAL_ITEM fINANCIAL_ITEM)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fINANCIAL_ITEM).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PARTNER_ID = new SelectList(db.PARTNER, "PARTNER_ID", "PARTNER_NAME", fINANCIAL_ITEM.PARTNER_ID);
            return View(fINANCIAL_ITEM);
        }

       
        public ActionResult Delete(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FINANCIAL_ITEM fINANCIAL_ITEM = db.FINANCIAL_ITEM.Find(id);
            if (fINANCIAL_ITEM == null)
            {
                return HttpNotFound();
            }
            return View(fINANCIAL_ITEM);
        }

 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(decimal id)
        {
            FINANCIAL_ITEM fINANCIAL_ITEM = db.FINANCIAL_ITEM.Find(id);
            db.FINANCIAL_ITEM.Remove(fINANCIAL_ITEM);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Calculation()
        {
            var calculations = new List<Calculation>();
            foreach (var partner in db.PARTNER)
            {
                var calculation = new Calculation();
                calculation.PARTNER_NAME = partner.PARTNER_NAME;
                var partnerSum= partner.FINANCIAL_ITEM.Sum(x => x.AMOUNT.Value);
                var childPartners = db.PARTNER.Where(x => x.PARENT_PARTNER_ID == partner.PARTNER_ID).ToList();
                decimal sum = 0;
                var calculation1 = partnerSum * partner.FEE_PERCENT;
                decimal calculation2 = 0;
                foreach (var childPartner in childPartners)
                {
                    var finItemSum = childPartner.FINANCIAL_ITEM.Sum(x => x.AMOUNT.Value);
                    sum += finItemSum;
                    var childPartnerFee = childPartner.FEE_PERCENT;
                    if (childPartnerFee < partner.FEE_PERCENT)
                    {
                        calculation2 += finItemSum * (partner.FEE_PERCENT - childPartner.FEE_PERCENT);   
                    }

                }                
                calculation.Team_purchase = sum;
                calculation.Total_purchase = partnerSum + calculation.Team_purchase;
                calculation.Total_commission = (calculation1 + calculation2) /100;
                
                calculations.Add(calculation);
                
            }
            return View(calculations.ToList());
        }
        
    }
}
