using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIDataSyncXMLGenerator.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string CodeGaska { get; set; }
        public string CodeCustomer { get; set; }
        public string Name { get; set; }
        public string Supplier { get; set; }
        public List<ProductPackage>? Packages { get; set; }
        public List<ProductCrossNumber>? CrossNumbers { get; set; }
        public List<ProductApplication>? Applications { get; set; }
        public List<ProductParameter>? Parameters { get; set; }
        public List<ProductImage>? Images { get; set; }
        public List<ProductFile>? Files { get; set; }
    }
}
