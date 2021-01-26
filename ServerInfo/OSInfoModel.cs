using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInfo
{
    public class OSInfoModel
    {
        public string Caption { get; set; }
        public string CSName { get; set; }
        public ulong TotalVisibleMemorySize { get; set; }
    }
}
