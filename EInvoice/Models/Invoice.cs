using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EInvoice.Models
{
    public class Invoice
    {
        public string eic { get; set; }
        public string invoiceNumber { get; set; }
        public string importedinvoiceNumber { get; set; }
        public string receivedDate { get; set; }
        public string dueDate { get; set; }
        public string amount { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string NIPT { get; set; }
    }
}