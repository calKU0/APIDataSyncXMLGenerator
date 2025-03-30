using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIDataSyncXMLGenerator.Models
{
    public class ProductWrapper
    {
        public Product Product { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
    }
}
