using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnimalView.Dnn.AnimalView_Modul.Components;
using AnimalView.Dnn.AnimalView_Modul.Models;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using System.Net.Http;
using System.Net.Http.Headers;
using DotNetNuke.Entities.Modules;


namespace AnimalView.Dnn.AnimalView_Modul.Controllers
{
    [DnnHandleError]
    public class AnimalController : DnnController
    {
        private List<Models.Animal> Animals { get; set; }
        
        // GET: Animal
        public ActionResult Index()
        {
            
            return View(Animals);
        }

        // GET: Animal/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Animal/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Animal/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Animal/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Animal/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Animal/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Animal/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
