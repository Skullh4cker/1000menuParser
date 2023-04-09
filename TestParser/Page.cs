using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestParser
{
    internal class Page
    {
        public string DishName { get; set; }
        public string Link { get; set; }
        public int Rating { get; set; }
        public int ServingsNumber { get; set; }
        public int Calories { get; set; }
        public List<ExactIngredient> Ingredients { get; set; }
        public Page(string link, string name, int rating, int servingsNumber, int calories, List<ExactIngredient> ingredients) 
        {
            DishName = name; 
            Link = link;
            Rating = rating;
            ServingsNumber = servingsNumber;
            Calories = calories;
            Ingredients = ingredients; 
        }
    }
}
