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
using AnimalView.Dnn.AnimalView_Modul.Services;


namespace AnimalView.Dnn.AnimalView_Modul.Controllers
{
    [DnnHandleError]
    public class AnimalController : DnnController
    {
        private readonly AnimalService _animalService = new AnimalService();
        //private List<Models.Animal> Animals { get; set; }

        // GET: Animal
        [ModuleAction]
        public ActionResult Index()
        {
            List<Models.Animal> Animals = _animalService.GetAnimals(_animalService.GetSpeciesBvin()); //Setting! majd
            return View(Animals);
        }

        // POST: Animal/Create
        [HttpPost]
        public ActionResult Create(string collection)
        {
            try
            {
                // TODO: Add insert logic here
                _animalService.AddOrder();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

    }
}
