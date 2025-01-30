using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVideos
{
    public class RuntimeConfigAttribute : Attribute
    {
        public string Category { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strCategory">Optional category path. To split path into individual groups use '.', '\' or '/' char.</param>
        /// <param name="strName">Optional name to override property's name.</param>
        public RuntimeConfigAttribute(string strCategory = null, string strName = null)
        {
            this.Category = strCategory;
            this.Name = strName;
        }
    }
}
