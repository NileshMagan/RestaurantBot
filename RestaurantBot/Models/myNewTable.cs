using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace RestaurantBot.Models
{
    public class myNewTable
    {
        [JsonProperty(PropertyName = "Id")]
        public string id { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public string createdAt { get; set; }

        [JsonProperty(PropertyName = "updatedAt")]
        public string updatedAt { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string version { get; set; }

        [JsonProperty(PropertyName = "deleted")]
        public bool deleted { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string username { get; set; }

        [JsonProperty(PropertyName = "restaurantName")]
        public string restaurantName { get; set; }

    }
}