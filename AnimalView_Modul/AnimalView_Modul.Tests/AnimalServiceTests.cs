using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hotcakes.CommerceDTO.v1.Catalog;
using AnimalView.Dnn.AnimalView_Modul.Services;
using System;
using Moq;
using System.Collections.Generic;

namespace AnimalView.Dnn.AnimalView_Modul.Tests
{
    [TestClass]
    public class AnimalServiceTests
    {
        // 1. Az átfogó (Data-Driven) teszt a különböző HTML leírások és hiányosságok ellenőrzésére
        [DataTestMethod]
        [DataRow(
            "Minden adat tökéletes",
            "<p><strong>Név</strong>:<br /> Béla </p><p><strong>Nem</strong>:<br /> Hím </p><p><strong>Genetika</strong>:<br /> Albínó </p>",
            "Béla", "Hím", "Albínó"
        )]
        [DataRow(
            "Csak a név van meg, a többi hiányzik",
            "<p><strong>Név</strong>:<br /> Sanyi </p>",
            "Sanyi", "Hiányzik a nem", "Hiányzik a genetika"
        )]
        [DataRow(
            "Kicsit más a HTML formázás (nincs szóköz a br tagben)",
            "<p><strong>Név</strong>:<br/>Zsuzsi</p><p><strong>Nem</strong>:<br/>Nőstény</p>",
            "Zsuzsi", "Nőstény", "Hiányzik a genetika"
        )]
        [DataRow(
            "Teljesen üres leírás",
            "",
            "Hiányzik a név", "Hiányzik a nem", "Hiányzik a genetika"
        )]
        [DataRow(
            "Null leírás (Edge Case)",
            null,
            "Hiányzik a név", "Hiányzik a nem", "Hiányzik a genetika"
        )]
        public void GetAnimalData_KulonbozoHtmlLeirasok_MegfeleloenKinyeriAzAdatokat(
            string tesztNeve, string htmlLeiras, string elvartNev, string elvartNem, string elvartGenetika)
        {
            // ARRANGE
            var service = new AnimalService();
            var rawAnimal = new ProductDTO
            {
                Bvin = "TEST-BVIN",
                LongDescription = htmlLeiras
            };

            // ACT
            var result = service.GetAnimalData(rawAnimal);

            // ASSERT - A hibaüzenetbe beletesszük a 'tesztNeve' változót, hogy bukás esetén tudjuk, melyik sor a ludas
            Assert.AreEqual(elvartNev, result.Name, $"Hiba a '{tesztNeve}' esetnél: Név eltérés.");
            Assert.AreEqual(elvartNem, result.Gender, $"Hiba a '{tesztNeve}' esetnél: Nem eltérés.");
            Assert.AreEqual(elvartGenetika, result.Genetics, $"Hiba a '{tesztNeve}' esetnél: Genetika eltérés.");
        }

        // 2. Extrém Edge Case: Amikor az egész Hotcakes termék objektum hiányzik (null)
        [TestMethod]
        public void GetAnimalData_NullParameterEseten_NemFagyLeEsAlapertekeketAd()
        {
            // ARRANGE
            var service = new AnimalService();
            ProductDTO nullAnimal = null;

            // ACT
            var result = service.GetAnimalData(nullAnimal);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual("N/A", result.AnimalId);
            Assert.AreEqual("Hiányzik a név", result.Name);
            Assert.AreEqual(DateTime.UtcNow.Date, result.BirthDate.Date);
        }

        // 3. Extrém Edge Case: Amikor a dátum formátuma olvashatatlan
        [TestMethod]
        public void GetAnimalData_ErvenytelenDatumSzovegEseten_NemFagyLeEsMaiNapotAd()
        {
            // ARRANGE
            var service = new AnimalService();
            var rawAnimal = new ProductDTO
            {
                Bvin = "TEST-BVIN-DATE",
                LongDescription = "<p><strong>Született</strong>:<br /> Valamikor tavaly nyáron </p>"
            };

            // ACT
            var result = service.GetAnimalData(rawAnimal);

            // ASSERT 
            Assert.AreEqual(DateTime.UtcNow.Date, result.BirthDate.Date);
        }

        // 4. Moq Teszt: Amikor a szerver ad vissza állatokat, és van is belőlük raktáron
        [TestMethod]
        public void GetAnimals_VanTalalatEsVanKeszleten_VisszaadjaAzAllatokat()
        {
            // ARRANGE - A Moq "bábu" beállítása
            var mockApi = new Mock<IHotcakesApiService>();
            string testCatId = "teszt-kategoria-id";
            string testBvin = "teszt-bvin-1";

            // Szimuláljuk, hogy a Hotcakes mit ad vissza a kategória lekérdezésre
            var mockProducts = new List<ProductDTO>
            {
                new ProductDTO
                {
                    Bvin = testBvin,
                    LongDescription = "<p><strong>Név</strong>:<br /> Béla </p>",
                    SitePrice = 10000m
                }
            };
            mockApi.Setup(api => api.GetProductsByCategory(testCatId)).Returns(mockProducts);

            // Szimuláljuk a fejlesztő új logikáját: van belőle készleten
            mockApi.Setup(api => api.GetAvailableQuantity(testBvin)).Returns(5);

            // Injektáljuk a hamisított API-t a szervizünkbe
            var service = new AnimalService(mockApi.Object);

            // ACT
            var result = service.GetAnimals(testCatId);

            // ASSERT
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Béla", result[0].Name);
            Assert.AreEqual(10000m, result[0].Price);
        }

        // 5. Moq Teszt: Amikor a szerver üres listát ad vissza
        [TestMethod]
        public void GetAnimals_UresKategoriaEseten_AlapertelmezettAllatotAd()
        {
            // ARRANGE
            var mockApi = new Mock<IHotcakesApiService>();
            string testCatId = "ures-kategoria-id";

            // Szimuláljuk, hogy nincs találat (üres listát ad a Hotcakes szerver)
            mockApi.Setup(api => api.GetProductsByCategory(testCatId)).Returns(new List<ProductDTO>());

            var service = new AnimalService(mockApi.Object);

            // ACT
            var result = service.GetAnimals(testCatId);

            // ASSERT - A kódnak be kellett raknia a "biztonsági" alapállatot
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("név", result[0].Name);
            Assert.AreEqual("id", result[0].AnimalId);
            Assert.AreEqual("genetika", result[0].Genetics);
        }

    }
}