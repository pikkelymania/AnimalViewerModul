using AnimalView.Dnn.AnimalView_Modul.Components;
using AnimalView.Dnn.AnimalView_Modul.Models;
using AnimalView.Dnn.AnimalView_Modul.Services;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace AnimalView.Dnn.AnimalView_Modul.Controllers
{
    [DnnHandleError]
    public class AnimalController : DnnController
    {
        private readonly AnimalService _animalService;

        // 1. Eredeti konstruktor (ezt hívja a DotNetNuke élesben)
        public AnimalController()
        {
            _animalService = new AnimalService();
        }

        // 2. Új konstruktor a teszteléshez (ide tudjuk beinjektálni a Moq-ot)
        public AnimalController(AnimalService animalService)
        {
            _animalService = animalService;
        }

        // 3. Biztonságos függvény a DNN kontextus lekérésére (tesztben nem fagy le)
        protected virtual string GetAnimalSetting()
        {
            if (ModuleContext == null || ModuleContext.Configuration == null)
            {
                return "Leopárdgekkók"; // Alapértelmezett érték tesztkörnyezetben
            }
            return ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("AnimalView_Modul_Setting1", "Leopárdgekkók");
        }

        // GET: Animal
        [ModuleAction]
        public ActionResult Index()
        {
            string species = GetAnimalSetting();
            List<Models.Animal> Animals = _animalService.GetAnimals(_animalService.GetSpeciesBvin(species));
            return View(Animals);
        }

        // POST: Animal/Create
        public ActionResult Create(string AnimalBvin)
        {
            try
            {
                _animalService.AddOrder(AnimalBvin);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}