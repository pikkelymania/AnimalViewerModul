using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hotcakes.CommerceDTO.v1.Catalog;
using AnimalView.Dnn.AnimalView_Modul.Services;
using System;

namespace AnimalView.Dnn.AnimalView_Modul.Tests
{
    [TestClass]
    public class AnimalServiceTests
    {
        [TestMethod]
        public void GetAnimalData_SikeresenKinyeriAzAdatokatAHTMLbol_HappyPath()
        {
            // ARRANGE
            var service = new AnimalService();
            var tesztDatum = new DateTime(2025, 10, 18);

            var rawAnimal = new ProductDTO
            {
                Bvin = "TEST-BVIN-123",
                ImageFileMedium = "kigyo.jpg",
                SitePrice = 25000m,
                LongDescription = @"
                    <p>Valami bevezető szöveg...</p>
                    <p><strong>Név</strong>:<br /> Sziszike </p>
                    <p><strong>Nem</strong>:<br /> Hím </p>
                    <p><strong>Genetika</strong>:<br /> Albínó </p>
                    <p><strong>Személyiség</strong>:<br /> Békés </p>
                    <p><strong>Született</strong>:<br /> 2025. 10. 18. </p>
                    <p>Záró gondolatok...</p>"
            };

            // ACT
            var result = service.GetAnimalData(rawAnimal);

            // ASSERT
            Assert.AreEqual("TEST-BVIN-123", result.AnimalId);
            Assert.AreEqual("Sziszike", result.Name);
            Assert.AreEqual("Hím", result.Gender);
            Assert.AreEqual("Albínó", result.Genetics);
            Assert.AreEqual("Békés", result.Personality);
            Assert.AreEqual(tesztDatum.Date, result.BirthDate.Date);
            Assert.AreEqual(25000m, result.Price);
            Assert.AreEqual("kigyo.jpg", result.Image);
        }

        [TestMethod]
        public void GetAnimalData_HianyozoAdatokEsetenAlapertekeketAd_SadPath()
        {
            // ARRANGE
            var service = new AnimalService();
            var rawAnimal = new ProductDTO
            {
                Bvin = "TEST-BVIN-456",
                LongDescription = "<p>Ebben a leírásban nincsenek benne a kötelező HTML tagek.</p>"
            };

            // ACT
            var result = service.GetAnimalData(rawAnimal);

            // ASSERT
            Assert.AreEqual("Hiányzik a név", result.Name);
            Assert.AreEqual("Hiányzik a nem", result.Gender);
            Assert.AreEqual("Hiányzik a genetika", result.Genetics);
            Assert.AreEqual("Hiányzik a személyiség", result.Personality);
        }

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

        [TestMethod]
        public void GetAnimalData_NullLeirasEseten_NemFagyLeEsAlapertekeketAd()
        {
            // ARRANGE
            var service = new AnimalService();
            var rawAnimal = new ProductDTO
            {
                Bvin = "TEST-BVIN-789",
                LongDescription = null
            };

            // ACT
            var result = service.GetAnimalData(rawAnimal);

            // ASSERT
            Assert.AreEqual("Hiányzik a név", result.Name);
            Assert.AreEqual("Hiányzik a személyiség", result.Personality);
        }

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