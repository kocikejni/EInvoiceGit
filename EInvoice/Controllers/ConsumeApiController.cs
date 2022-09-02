using EInvoice.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace EInvoice.Controllers
{
    public class ConsumeApiController : Controller
    {
        // GET: ConsumeApi
        public async System.Threading.Tasks.Task<System.Web.Mvc.ActionResult> ConsumeExternalAPI([FromRoute] int month, int year)
        {
            var path = "http://localhost:61013/api/values?month=" + month + "&year=" + year + " ";
            var client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                dynamic a = await response.Content.ReadAsStringAsync();
                var invoices = (Invoices)JsonConvert.DeserializeObject<Invoices>(a);
                if (invoices.Status == "200")
                {
                    var filteredInvoices = invoices.Data.Where(x => x.type == "BUYER" && x.status == "DELIVERED").ToList();
                    await GetPdfPaths(filteredInvoices, client);
                }
            }
            return null;
        }
        private async Task<List<string>> GetPdfPaths(List<Models.Invoice> invoices, HttpClient client)
        {
            List<string> PdfPaths = new List<string>();
            List<fatura.Invoice> InvoiceDatas = new List<fatura.Invoice>();
            foreach (var invoice in invoices)
            {
                var endpoint = "http://localhost:61013/api/values/InvoicePdfResponse/" + invoice.eic;

                HttpResponseMessage response = await client.GetAsync(endpoint);
                dynamic a = await response.Content.ReadAsStringAsync();
                var deserializeResponse = (GetPdfModel)JsonConvert.DeserializeObject<GetPdfModel>(a);
                PdfPaths.Add(deserializeResponse.path);
            }
            foreach (var path in PdfPaths)
            {
                var invoice = MapXmlToObject(path);
                InvoiceDatas.Add(invoice);
            }
            return null;
        }
        //Extracting XML from PDF File section: Start
        private fatura.Invoice MapXmlToObject(string path)
        {
            byte[] bts = GetEmbeddedFileData(path, "");
            string result = System.Text.Encoding.UTF8.GetString(bts);
            //var parsedResult = XDocument.Parse(result).Element("CC5Response").Descendants().ToList();

            StringReader reader = new StringReader(result);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(fatura.Invoice));
            var invoice = (fatura.Invoice)xmlSerializer.Deserialize(reader);

            return invoice;
        }
        public static Stream GetEmbeddedFileStream(string pdfFileName, string embeddedFileName)
        {
            byte[] data = GetEmbeddedFileData(pdfFileName, embeddedFileName);
            if (data == null)
                return null;
            else
                return new MemoryStream(data);
        }
        private static byte[] GetEmbeddedFileData(string pdfFileName, string embeddedFileName)
        {
            byte[] attachedFileBytes = null;
            var reader = new iTextSharp.text.pdf.PdfReader(pdfFileName);
            if (reader != null)
            {
                var root = reader.Catalog;
                if (root != null)
                {
                    var names = root.GetAsDict(iTextSharp.text.pdf.PdfName.NAMES);
                    if (names != null)
                    {
                        var embeddedFiles = names.GetAsDict(iTextSharp.text.pdf.PdfName.EMBEDDEDFILES);
                        if (embeddedFiles != null)
                        {
                            var namesArray = embeddedFiles.GetAsArray(iTextSharp.text.pdf.PdfName.NAMES);
                            if (namesArray != null)
                            {
                                int n = namesArray.Size;
                                for (int i = 0; i < n; i++)
                                {
                                    i++;
                                    var fileArray = namesArray.GetAsDict(i);
                                    var file = fileArray.GetAsDict(iTextSharp.text.pdf.PdfName.EF);
                                    foreach (iTextSharp.text.pdf.PdfName key in file.Keys)
                                    {
                                        string attachedFileName = fileArray.GetAsString(key).ToString();

                                        var stream = (iTextSharp.text.pdf.PRStream)iTextSharp.text.pdf.PdfReader.GetPdfObject(file.GetAsIndirectObject(key));
                                        attachedFileBytes = iTextSharp.text.pdf.PdfReader.GetStreamBytes(stream);
                                        break;

                                    }
                                    if (attachedFileBytes != null) break;
                                }
                            }
                        }
                    }
                }
                reader.Close();
            }
            return attachedFileBytes;
        }
        //Extracting XML from PDF File section: End
    }
}