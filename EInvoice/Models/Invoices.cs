using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EInvoice.Models
{
    public class Invoices
    {
        public List<Invoice> Data { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public Invoices()
        {
            //InvoiceList = new List<Invoice>();
        }
    }
}