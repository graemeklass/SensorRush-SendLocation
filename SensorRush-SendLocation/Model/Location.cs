using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace SensorRush_SendLocation.Model
{
    public class MyLocation
    {
        public class LocationData
        {
            [JsonProperty(PropertyName = "ts")] //this maps the property to "ts" when we serialise to a JSON format.
            public DateTime timestampUTC { get; set; }
            public double Lattitude { get; set; }
            public double Longitude { get; set; }
            public double Altitude { get; set; }
            public float Accuracy { get; set; }
        }
    }
}