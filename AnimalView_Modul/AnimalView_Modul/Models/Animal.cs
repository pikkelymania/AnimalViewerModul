using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnimalView.Dnn.AnimalView_Modul.Models
{
    public class Animal
    {
        public bool IsFish { get; set; }

        public string AnimalId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int Price { get; set; }
        public DateTime BirthDate { get; set; }

        public string Gender { get; set; }
        public string Genetics { get; set; }
        public string Personality { get; set; }

        public string Traits { get; set; }
        public string Care { get; set; }
        public string Water { get; set; }
        public string Feeding { get; set; }
        public string Breeding { get; set; }
        public int Quantity { get; set; }
    }
}