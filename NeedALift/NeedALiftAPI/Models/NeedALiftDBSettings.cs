using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeedALiftAPI.Models
{
    public class NeedALiftDBSettings : INeedALiftDBSettings
    {
        public string CollectionName1 { get; set; }
        public string CollectionName2 { get; set; }
        public string CollectionName3 { get; set; }
        public string ConnectionString { get; set; }
        public string DBName { get; set; }
        public string Secret { get; set; }
    }

    public interface INeedALiftDBSettings
    {
        string CollectionName1 { get; set; }
        string CollectionName2 { get; set; }
        string CollectionName3 { get; set; }
        string ConnectionString { get; set; }
        string DBName { get; set; }
        string Secret { get; set; }
    }
}
