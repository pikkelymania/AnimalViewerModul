using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Web.Mvc;
using AnimalView.Dnn.AnimalView_Modul.Controllers;
using AnimalView.Dnn.AnimalView_Modul.Services;
using Hotcakes.CommerceDTO.v1.Catalog;

namespace AnimalView.Dnn.AnimalView_Modul.Tests
{
    [TestClass]
    public class AnimalControllerTests
    {
        [TestMethod]
        public void Index_SikeresAdatLekeres_VisszaadjaAViewtAzAllatokkal()
        {
            // ARRANGE - A láncolat felépítése
            var mockApi = new Mock<IHotcakesApiService>();
            string tesztKategoriaNev = "Leopárdgekkók";
            string tesztBvin = "teszt-kat-bvin-123";

            // 1. Megmondjuk, mi a kategória azonosítója
            mockApi.Setup(api => api.GetCategoryBvinByName(tesztKategoriaNev)).Returns(tesztBvin);

            // 2. Megmondjuk, milyen állatokat talál ebben a kategóriában
            var mockProducts = new List<ProductDTO>
            {
                new ProductDTO
                {
                    Bvin = "gecko-1",
                    LongDescription = "<p><strong>Név</strong>:<br /> Guszti </p>",
                    SitePrice = 15000m
                }
            };
            mockApi.Setup(api => api.GetProductsByCategory(tesztBvin)).Returns(mockProducts);

            // 3. Készlet szimuláció
            mockApi.Setup(api => api.GetAvailableQuantity("gecko-1")).Returns(2);

            // 4. Injektálás a Controllerbe!
            var service = new AnimalService(mockApi.Object);
            var controller = new AnimalController(service);

            // ACT - Meghívjuk a webes végpontot
            var result = controller.Index() as ViewResult;

            // ASSERT - Ellenőrizzük az MVC válaszát
            Assert.IsNotNull(result, "A visszatérési érték nem ViewResult volt.");

            // Lekérjük a View-nak átadott Modellt (a listát)
            var model = result.Model as List<Models.Animal>;
            Assert.IsNotNull(model, "A View modellje null, vagy nem List<Animal> típusú.");

            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Guszti", model[0].Name);
            Assert.AreEqual(15000m, model[0].Price);
        }
    }
}
