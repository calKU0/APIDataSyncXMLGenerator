using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIDataSyncXMLGenerator.Models
{
    public class ProductPackage
    {
        public string PackUnit { get; set; }
        public decimal PackQty { get; set; }
        public decimal PackNettWeight { get; set; }
        public decimal PackGrossWeight { get; set; }
        public string PackEan { get; set; }
        public int PackRequired { get; set; }
    }
}
