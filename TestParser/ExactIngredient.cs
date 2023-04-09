using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TestParser
{
    class Ingredient
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Ingredient(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
    class ExactIngredient : Ingredient
    {
        public float Measurement { get; set; }
        public ExactIngredient(string name, string type, float quantity) : base(name, type)
        {
            Measurement = quantity;
        }
    }
}
