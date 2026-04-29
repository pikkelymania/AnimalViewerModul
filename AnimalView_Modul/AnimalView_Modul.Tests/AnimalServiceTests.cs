using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hotcakes.CommerceDTO.v1.Catalog;
using AnimalView.Dnn.AnimalView_Modul.Services;
using System;

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
    }
}