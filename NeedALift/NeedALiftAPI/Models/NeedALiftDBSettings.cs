using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeedALiftAPI.Models
{
    public class NeedALiftDBSettings : INeedALiftDBSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DBName { get; set; }
    }

    public interface INeedALiftDBSettings
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DBName { get; set; }
    }
}
