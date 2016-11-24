using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestaurantBot.Models
{
    public class CategoryObject
    {
        public class Categories
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Category
        {
            public Categories categories { get; set; }
        }

        public class RootObject
        {
            public List<Category> categories { get; set; }
        }

    }
}