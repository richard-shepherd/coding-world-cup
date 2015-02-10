using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace BootAndShoot
{
    /// <summary>
    /// Helps create and serialize data to JSON. 
    /// </summary><remarks>
    /// This is basically a "typedef" for Dictionary[string, object]
    /// with a helper to serialize to JSON.
    /// </remarks>
    public class JSObject : Dictionary<string, object>
    {
        /// <summary>
        /// Adds a double field, rounding to 4dp.
        /// </summary>
        public void add(string name, double value)
        {
            value = Math.Round(value, 4);
            this.Add(name, value);
        }

        /// <summary>
        /// Adds a string field.
        /// </summary>
        public void add(string name, string value)
        {
            this.Add(name, value);
        }

        /// <summary>
        /// Adds an object field, for example a list.
        /// </summary>
        public void add(string name, object value)
        {
            this.Add(name, value);
        }

        /// <summary>
        /// Converts the object to a JSON string.
        /// </summary>
        public string toJSON()
        {
            return Json.Encode(this);
        }

    }
}
