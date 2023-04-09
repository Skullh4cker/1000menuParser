using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestParser
{
    internal class Page
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public string ImageLink { get; set; }
        public int Rating { get; set; }
        public int ServingsNumber { get; set; }
        public int Calories { get; set; }
        public Time CookingTime { get; set; }
        public List<ExactIngredient> Ingredients { get; set; }
        public Page(string link, string name, string imagelink, int rating, int servingsNumber, int calories, Time cookingTime, List<ExactIngredient> ingredients) 
        {
            Name = name; 
            Link = link;
            ImageLink = imagelink;
            Rating = rating;
            ServingsNumber = servingsNumber;
            Calories = calories;
            CookingTime = cookingTime;
            Ingredients = ingredients; 
        }
    }
}
