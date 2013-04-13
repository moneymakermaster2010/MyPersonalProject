using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using T4EF.Models.Default;

namespace T4EF.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var ctx = new MovieReviewEntities();
            var model = ctx.Movies
                           .OrderByDescending(m => m.ReleaseDate)
                           .Take(25);
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var ctx = new MovieReviewEntities();
            var model = ctx.Movies.Single(m => m.ID == id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var ctx = new MovieReviewEntities();
            var model = ctx.Movies.Single(m => m.ID == id);
            if (TryUpdateModel(model))
            {
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
