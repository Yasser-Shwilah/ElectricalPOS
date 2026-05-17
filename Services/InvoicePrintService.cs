using ElectricalPOS.Models;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ElectricalPOS.Services
{
    /// <summary>
    /// Service for generating and printing Arabic invoices
    /// </summary>
    public class InvoicePrintService
    {
        private readonly string _templatePath;

        public InvoicePrintService()
        {
            // Get the template path relative to the application directory
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            _templatePath = Path.Combine(appDir, "Templates", "ArabicInvoiceTemplate.html");
        }

        /// <summary>
        /// Generates an HTML invoice from an Invoice object (جدول الفواتير)
        /// </summary>
        public string GenerateInvoiceHtml(Invoice invoice)
        {
            // Try to load from file first
            string templateHtml = string.Empty;
            if (File.Exists(_templatePath))
            {
                templateHtml = File.ReadAllText(_templatePath, Encoding.UTF8);
            }
            else
            {
                // Use embedded template if file not found
                templateHtml = GetEmbeddedTemplate();
            }

            // Replace placeholders with actual data
            templateHtml = ReplacePlaceholders(templateHtml, invoice);

            return templateHtml;
        }

        /// <summary>
        /// Generates an HTML invoice from a Sale object (for backward compatibility)
        /// </summary>
        public string GenerateInvoiceHtml(Sale sale)
        {
            // Try to load from file first
            string templateHtml = string.Empty;
            if (File.Exists(_templatePath))
            {
                templateHtml = File.ReadAllText(_templatePath, Encoding.UTF8);
            }
            else
            {
                // Use embedded template if file not found
                templateHtml = GetEmbeddedTemplate();
            }

            // Replace placeholders with actual data
            templateHtml = ReplacePlaceholders(templateHtml, sale);

            return templateHtml;
        }

        /// <summary>
<<<<<<< HEAD
        /// Opens the invoice HTML file in the default external browser for printing.
        /// </summary>
        public async Task PrintInvoiceAsync(Invoice invoice)
        {
            var html = PrepareHtmlForThermalPrint(GenerateInvoiceHtml(invoice), autoPrint: false);
            await OpenInvoiceInExternalBrowserAsync(html, invoice.InvoiceNumber);
        }

        /// <summary>
        /// Opens the invoice HTML file in the default external browser for printing.
        /// </summary>
        public async Task PrintInvoiceAsync(Sale sale)
        {
            if (sale.SaleItems == null || sale.SaleItems.Count == 0)
                throw new InvalidOperationException("لا توجد أصناف في الفاتورة للطباعة.");

            var html = PrepareHtmlForThermalPrint(GenerateInvoiceHtml(sale), autoPrint: false);
            await OpenInvoiceInExternalBrowserAsync(html, sale.InvoiceNumber ?? "sale");
        }

        private static Task OpenInvoiceInExternalBrowserAsync(string html, string invoiceNumber)
        {
            var safeId = string.Join("_", (invoiceNumber ?? "invoice").Split(Path.GetInvalidFileNameChars()));
            return PrintHtmlViaBrowserAsync(html, $"Invoice_{safeId}");
        }

        private static async Task PrintHtmlViaBrowserAsync(string html, string fileNamePrefix)
        {
            var safePrefix = string.Join("_", fileNamePrefix.Split(Path.GetInvalidFileNameChars()));
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                $"{safePrefix}_{DateTime.Now:yyyyMMddHHmmss}.html");

            await File.WriteAllTextAsync(tempPath, html, new UTF8Encoding(true));
=======
        /// Prints an invoice by generating HTML and opening in browser for printing
        /// </summary>
        public async Task PrintInvoiceAsync(Invoice invoice)
        {
            var html = GenerateInvoiceHtml(invoice);
            
            // Create a temporary HTML file
            var tempPath = Path.Combine(Path.GetTempPath(), $"Invoice_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmss}.html");
            
            // Write HTML with UTF-8 BOM for proper Arabic encoding
            var utf8Bom = new UTF8Encoding(true);
            await File.WriteAllTextAsync(tempPath, html, utf8Bom);

            // Open in default browser for printing
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Verb = "print"
                };
                Process.Start(psi);
            }
            catch
            {
                // Fallback: just open in browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// Prints an invoice from Sale object
        /// </summary>
        public async Task PrintInvoiceAsync(Sale sale)
        {
            var html = GenerateInvoiceHtml(sale);
            
            // Create a temporary HTML file
            var tempPath = Path.Combine(Path.GetTempPath(), $"Invoice_{sale.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmss}.html");
            
            // Write HTML with UTF-8 BOM for proper Arabic encoding
            var utf8Bom = new UTF8Encoding(true);
            await File.WriteAllTextAsync(tempPath, html, utf8Bom);

            // Open in default browser for printing
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Verb = "print"
                };
                Process.Start(psi);
            }
            catch
            {
                // Fallback: just open in browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// Shows invoice preview in default browser
        /// </summary>
        public async Task ShowInvoicePreviewAsync(Invoice invoice)
        {
            var html = GenerateInvoiceHtml(invoice);
            
            var tempPath = Path.Combine(Path.GetTempPath(), $"Invoice_{invoice.InvoiceNumber.Replace("-", "_")}_{DateTime.Now:yyyyMMddHHmmss}.html");
            
            var utf8Bom = new UTF8Encoding(true);
            await File.WriteAllTextAsync(tempPath, html, utf8Bom);
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e

            Process.Start(new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            });
        }

<<<<<<< HEAD
        private static string PrepareHtmlForThermalPrint(string html, bool autoPrint)
        {
            if (!html.Contains("class=\"thermal-receipt\"", StringComparison.OrdinalIgnoreCase)
                && !html.Contains("class='thermal-receipt'", StringComparison.OrdinalIgnoreCase))
            {
                html = html.Replace("<body>", "<body class=\"thermal-receipt\">", StringComparison.OrdinalIgnoreCase);
            }

            if (autoPrint && !html.Contains("thermal-auto-print", StringComparison.Ordinal))
            {
                const string autoPrintScript = """
                    <script class="thermal-auto-print">
                    window.addEventListener('load', function () {
                        setTimeout(function () { window.print(); }, 700);
                    });
                    </script>
                    """;
                html = html.Replace("</body>", autoPrintScript + "</body>", StringComparison.OrdinalIgnoreCase);
            }

            return html;
        }

        /// <summary>
        /// Shows invoice preview in the default external browser.
        /// </summary>
        public async Task ShowInvoicePreviewAsync(Invoice invoice)
        {
            var html = PrepareHtmlForThermalPrint(GenerateInvoiceHtml(invoice), autoPrint: false);
            await OpenInvoiceInExternalBrowserAsync(html, invoice.InvoiceNumber);
        }

        /// <summary>
        /// Shows invoice preview in the default external browser.
        /// </summary>
        public async Task ShowInvoicePreviewAsync(Sale sale)
        {
            var html = PrepareHtmlForThermalPrint(GenerateInvoiceHtml(sale), autoPrint: false);
            await OpenInvoiceInExternalBrowserAsync(html, sale.InvoiceNumber ?? "sale");
=======
        /// <summary>
        /// Shows invoice preview in default browser (for Sale object)
        /// </summary>
        public async Task ShowInvoicePreviewAsync(Sale sale)
        {
            var html = GenerateInvoiceHtml(sale);
            
            var tempPath = Path.Combine(Path.GetTempPath(), $"Invoice_{sale.InvoiceNumber.Replace("-", "_")}_{DateTime.Now:yyyyMMddHHmmss}.html");
            
            var utf8Bom = new UTF8Encoding(true);
            await File.WriteAllTextAsync(tempPath, html, utf8Bom);

            Process.Start(new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            });
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        /// <summary>
        /// Replace placeholders for Invoice object (جدول الفواتير)
        /// </summary>
        private string ReplacePlaceholders(string html, Invoice invoice)
        {
            var sb = new StringBuilder(html);

            // Invoice details - use invoice number as-is
            var invoiceNum = invoice.InvoiceNumber ?? "";
            sb.Replace("0001", invoiceNum);
            
            // Date and Time - format Gregorian
            var invoiceDate = invoice.InvoiceDate;
            var dateStr = invoiceDate.ToString("yyyy/MM/dd");
            var timeStr = invoiceDate.ToString("HH:mm:ss");
            sb.Replace("id=\"invoiceDate\">----/--/--", "id=\"invoiceDate\">" + dateStr);
            sb.Replace("id=\"invoiceTime\">--:--:--", "id=\"invoiceTime\">" + timeStr);
            
            // Customer info
            sb.Replace("زبون عام", string.IsNullOrEmpty(invoice.CustomerName) ? "زبون عام" : invoice.CustomerName);
            sb.Replace(@"id=""customerPhone"">-</span>", 
                string.IsNullOrEmpty(invoice.CustomerPhone) ? @"id=""customerPhone"">-</span>" : $"id=\"customerPhone\">{invoice.CustomerPhone}</span>");

            // Generate items HTML
            var itemsHtml = new StringBuilder();
            int index = 1;

            foreach (var item in invoice.InvoiceItems)
            {
                var productName = item.ProductName ?? "منتج";
<<<<<<< HEAD
                itemsHtml.Append($"<tr>\n                    <td class=\"left-align value-bold\">{item.UnitPrice:N2}</td>\n                    <td class=\"left-align value-bold\">{item.Quantity}</td>\n                    <td class=\"product-name value-bold\">{productName}</td>\n                    <td class=\"left-align value-bold\">{item.Subtotal:N2}</td>\n                    <td class=\"number value-bold\">{index}</td>\n                </tr>");
=======
                itemsHtml.Append($"<tr>\n                    <td class=\"left-align\">{item.UnitPrice:N2}</td>\n                    <td class=\"left-align\">{item.Quantity}</td>\n                    <td class=\"product-name\">{productName}</td>\n                    <td class=\"left-align\">{item.Subtotal:N2}</td>\n                    <td>{index}</td>\n                </tr>");
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
                index++;
            }

            // Replace items in template
            var itemsPattern = new Regex(@"<tbody id=""itemsBody"">[\s\S]*?</tbody>", RegexOptions.IgnoreCase);
            sb = new StringBuilder(itemsPattern.Replace(sb.ToString(), 
                $"<tbody id=\"itemsBody\">{itemsHtml}</tbody>"));

            // Totals - no discount for Invoice table
            sb.Replace("575.00", $"${invoice.Total:N2}");
            sb.Replace("0.00", "$0.00");
            sb.Replace(@"id=""totalAmount"">$575.00", $"id=\"totalAmount\">${invoice.Total:N2}");

            // Amount in words
            var amountInWords = NumberToArabicWords(invoice.Total);
            sb.Replace("خمسمائة وخمسة وسبعون دولار فقط", amountInWords + " دولار فقط");

            // Balance section - as per requirements
            sb.Replace(@"id=""previousBalance"">$0.00", $"id=\"previousBalance\">${invoice.PreviousBalance:N2}");
            sb.Replace(@"id=""currentInvoiceValue"">$575.00", $"id=\"currentInvoiceValue\">${invoice.Total:N2}");
            sb.Replace(@"id=""currentBalance"">$575.00", $"id=\"currentBalance\">${invoice.CurrentBalance:N2}");

            return sb.ToString();
        }

        private string ReplacePlaceholders(string html, Sale sale)
        {
            var sb = new StringBuilder(html);

            // Invoice details - use invoice number as-is
            var invoiceNum = sale.InvoiceNumber ?? "";
            sb.Replace("0001", invoiceNum);
            
            // Date and Time - format Gregorian
            var saleDate = sale.Date;
            var dateStr = saleDate.ToString("yyyy/MM/dd");
            var timeStr = saleDate.ToString("HH:mm:ss");
            sb.Replace("id=\"invoiceDate\">----/--/--", "id=\"invoiceDate\">" + dateStr);
            sb.Replace("id=\"invoiceTime\">--:--:--", "id=\"invoiceTime\">" + timeStr);
            
            // Customer info
            sb.Replace("زبون عام", string.IsNullOrEmpty(sale.CustomerName) ? "زبون عام" : sale.CustomerName);
            sb.Replace("id=\"customerPhone\">-</span>", 
                string.IsNullOrEmpty(sale.CustomerPhone) ? "id=\"customerPhone\">-</span>" : $"id=\"customerPhone\">{sale.CustomerPhone}</span>");

            // Generate items HTML
            var itemsHtml = new StringBuilder();
            int index = 1;
            decimal subtotal = 0;

            foreach (var item in sale.SaleItems)
            {
                var productName = item.Product?.Name ?? "منتج";
                itemsHtml.Append($@"
                <tr>
<<<<<<< HEAD
                    <td class=""left-align value-bold"">{item.UnitPrice:N2}</td>
                    <td class=""left-align value-bold"">{item.Quantity}</td>
                    <td class=""product-name value-bold"">{productName}</td>
                    <td class=""left-align value-bold"">{item.TotalPrice:N2}</td>
                    <td class=""number value-bold"">{index}</td>
=======
                    <td class=""left-align"">{item.UnitPrice:N2}</td>
                    <td class=""left-align"">{item.Quantity}</td>
                    <td class=""product-name"">{productName}</td>
                    <td class=""left-align"">{item.TotalPrice:N2}</td>
                    <td class=""number"">{index}</td>
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
                </tr>");
                subtotal += item.TotalPrice;
                index++;
            }

            // Replace items in template
            var itemsPattern = new Regex(@"<tbody id=""itemsBody"">[\s\S]*?</tbody>", RegexOptions.IgnoreCase);
            sb = new StringBuilder(itemsPattern.Replace(sb.ToString(), 
                $"<tbody id=\"itemsBody\">{itemsHtml}</tbody>"));

            // Calculate totals with discount
            var discount = sale.Discount;
            var total = subtotal - discount;

            // Totals - use specific IDs to avoid replacement conflicts
            sb.Replace("id=\"subtotal\">$575.00", "id=\"subtotal\">" + subtotal.ToString("N2"));
            sb.Replace("id=\"discount\">$0.00", "id=\"discount\">" + discount.ToString("N2"));
            sb.Replace("id=\"totalAmount\">$575.00", "id=\"totalAmount\">" + total.ToString("N2"));

            // Amount in words
            var amountInWords = NumberToArabicWords(total);
            sb.Replace("خمسمائة وخمسة وسبعون دولار فقط", amountInWords + " دولار فقط");

            // Balance section (values set but section is hidden via CSS)
            sb.Replace("id=\"previousBalance\">$0.00", "id=\"previousBalance\">" + sale.PreviousBalance.ToString("N2"));
            sb.Replace("id=\"currentInvoiceValue\">$575.00", "id=\"currentInvoiceValue\">" + total.ToString("N2"));
            sb.Replace("id=\"currentBalance\">$575.00", "id=\"currentBalance\">" + sale.CurrentBalance.ToString("N2"));

            return sb.ToString();
        }

        /// <summary>
        /// Converts number to Arabic words for currency
        /// </summary>
        private static string NumberToArabicWords(decimal number)
        {
            if (number == 0)
                return "صفر";

            var units = new[] { "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة" };
            var teens = new[] { "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر" };
            var tens = new[] { "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };
            var hundreds = new[] { "", "مائة", "مئتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة" };

            int intPart = (int)number;

            if (intPart >= 1000)
            {
                int thousandsCount = intPart / 1000;
                int remainder = intPart % 1000;

                if (thousandsCount == 1)
                    return "ألف" + (remainder > 0 ? " و " + FormatNumberBelow1000(remainder) : "");
                else if (thousandsCount == 2)
                    return "ألفان" + (remainder > 0 ? " و " + FormatNumberBelow1000(remainder) : "");
                else
                    return FormatNumberBelow1000(thousandsCount) + " آلاف" + (remainder > 0 ? " و " + FormatNumberBelow1000(remainder) : "");
            }
            else
            {
                return FormatNumberBelow1000(intPart);
            }
        }

        private static string FormatNumberBelow1000(int number)
        {
            if (number == 0)
                return "";

            var units = new[] { "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة" };
            var teens = new[] { "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر" };
            var tens = new[] { "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };
            var hundreds = new[] { "", "مائة", "مئتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة" };

            if (number >= 100)
            {
                int hundred = number / 100;
                int remainder = number % 100;
                if (remainder > 0)
                    return hundreds[hundred] + " و " + FormatNumberBelow100(remainder);
                return hundreds[hundred];
            }
            else
            {
                return FormatNumberBelow100(number);
            }
        }

        private static string FormatNumberBelow100(int number)
        {
            var units = new[] { "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة" };
            var teens = new[] { "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر" };
            var tens = new[] { "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };

            if (number < 10)
                return units[number];
            else if (number < 20)
                return teens[number - 10];
            else
            {
                int unit = number % 10;
                int ten = number / 10;
                if (unit == 0)
                    return tens[ten];
                return units[unit] + " و " + tens[ten];
            }
        }

        private static string GetEmbeddedTemplate()
        {
<<<<<<< HEAD
            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "ArabicInvoiceTemplate.html");
            if (File.Exists(templatePath))
                return File.ReadAllText(templatePath, Encoding.UTF8);

=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            return @"<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
<<<<<<< HEAD
    <meta name=""viewport"" content=""width=272, initial-scale=1.0"">
    <title>فاتورة - حاج حساني</title>
    <style>
        @page {
            size: 80mm 1200mm;
=======
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>فاتورة - حاج حساني</title>
    <style>
        @page {
            size: 80mm auto;
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            margin: 0;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

<<<<<<< HEAD
        html {
            width: 72mm;
            max-width: 72mm;
            height: auto !important;
            overflow: visible !important;
        }

        body {
            width: 72mm;
            max-width: 72mm;
            height: auto !important;
            overflow: visible !important;
=======
        html, body {
            width: 80mm;
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            font-family: 'Segoe UI', 'Tahoma', sans-serif;
            font-size: 10px;
            line-height: 1.3;
            color: #000;
            background: #fff;
<<<<<<< HEAD
            margin: 0 auto;
        }

        .invoice-container {
            width: 72mm;
            max-width: 72mm;
            height: auto !important;
            overflow: visible !important;
            padding: 1mm 2mm;
=======
        }

        .invoice-container {
            width: 80mm;
            padding: 2mm;
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            background: #fff;
        }

        .header {
            text-align: center;
            border-bottom: 1px dashed #000;
            padding-bottom: 3mm;
            margin-bottom: 3mm;
        }

        .store-name-main {
            font-size: 14px;
            font-weight: bold;
            color: #000;
            margin-bottom: 1mm;
        }

        .store-name-sub {
            font-size: 11px;
<<<<<<< HEAD
            font-weight: bold;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            color: #000;
            margin-bottom: 2mm;
        }

        .store-address {
            font-size: 9px;
<<<<<<< HEAD
            font-weight: bold;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            margin-bottom: 1mm;
        }

        .store-phone {
            font-size: 9px;
<<<<<<< HEAD
            font-weight: bold;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        .invoice-info {
            margin-bottom: 2mm;
        }

        .invoice-title {
            font-size: 12px;
            font-weight: bold;
            text-align: center;
            margin-bottom: 2mm;
        }

        .invoice-details {
            font-size: 9px;
<<<<<<< HEAD
            display: block;
=======
            display: flex;
            justify-content: space-between;
            flex-wrap: wrap;
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        .invoice-details p {
            margin-bottom: 1mm;
<<<<<<< HEAD
            display: block;
            width: 100%;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        .label {
            font-weight: bold;
        }

<<<<<<< HEAD
        .value-bold,
        #invoiceNumber,
        #invoiceDate,
        #invoiceTime,
        #customerPhone {
            font-weight: bold;
        }

=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        .customer-info {
            margin-bottom: 2mm;
            font-size: 9px;
        }

        .customer-info p {
            margin-bottom: 1mm;
        }

        .items-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 2mm;
            font-size: 9px;
<<<<<<< HEAD
            table-layout: fixed;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        .items-table th {
            background-color: #fff;
            color: #000;
            padding: 2mm 1mm;
            text-align: center;
            font-weight: bold;
            font-size: 9px;
            border-bottom: 1px dashed #000;
        }

        .items-table td {
            padding: 1.5mm 1mm;
            text-align: center;
            font-size: 9px;
            border-bottom: 1px dotted #ccc;
            vertical-align: top;
        }

        .items-table td.product-name {
            text-align: right;
            direction: rtl;
            max-width: 30mm;
            word-wrap: break-word;
<<<<<<< HEAD
            font-weight: bold;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        .items-table .left-align {
            text-align: left;
            direction: ltr;
            unicode-bidi: bidi-override;
<<<<<<< HEAD
            font-weight: bold;
        }

        .items-table td.number {
            font-weight: bold;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        .totals-section {
            margin-top: 2mm;
            border-top: 1px dashed #000;
            padding-top: 2mm;
        }

        .totals-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 10px;
        }

        .totals-table td {
            padding: 1mm 0;
        }

        .totals-table .label-cell {
            text-align: right;
            font-weight: bold;
        }

        .totals-table .value-cell {
            text-align: left;
            direction: ltr;
            unicode-bidi: bidi-override;
<<<<<<< HEAD
            font-weight: bold;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        .total-row td {
            font-weight: bold;
        }

        .grand-total {
            font-weight: bold;
            border: none;
        }

        .amount-in-words {
            margin-top: 2mm;
            font-size: 9px;
            font-style: italic;
            border-top: 1px dotted #ccc;
            padding-top: 1mm;
        }

        .note-section {
            margin-top: 2mm;
            font-size: 9px;
            min-height: 3mm;
        }

        .note-section p {
            margin-bottom: 1mm;
        }

        .note-line {
            border-bottom: 1px dotted #ccc;
            min-height: 3mm;
        }

        /* Balance Section - Hidden by default */
        .balance-section {
            display: none;
            margin-top: 2mm;
            padding-top: 2mm;
            border-top: 1px dashed #000;
            font-size: 9px;
        }

        .balance-section p {
            margin-bottom: 1mm;
            display: flex;
            justify-content: space-between;
        }

        .balance-section .label {
            font-weight: bold;
        }

        .balance-section .value {
            direction: ltr;
            unicode-bidi: bidi-override;
<<<<<<< HEAD
            font-weight: bold;
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        .footer {
            margin-top: 3mm;
            text-align: center;
            padding-top: 2mm;
            border-top: 1px dashed #000;
            font-size: 9px;
            color: #000;
        }

        .footer p {
            margin-bottom: 1mm;
        }

        @media print {
            html, body {
<<<<<<< HEAD
                width: 72mm !important;
                max-width: 72mm !important;
                height: auto !important;
                margin: 0 auto !important;
                padding: 0 !important;
                overflow: visible !important;
                zoom: 1 !important;
            }

            .invoice-container,
            .items-table,
            .totals-section,
            .footer {
                width: 100% !important;
                height: auto !important;
                overflow: visible !important;
                position: static !important;
            }

            .items-table { display: table !important; }
            .items-table tr { display: table-row !important; }
            .items-table th,
            .items-table td { display: table-cell !important; }

            .store-name-sub,
            .store-address,
            .store-phone,
            #invoiceNumber,
            #invoiceDate,
            #invoiceTime,
            #customerPhone,
            .items-table td.left-align,
            .items-table td.number,
            .items-table td.product-name,
            .totals-table .value-cell,
            .balance-section .value {
                font-weight: 700 !important;
=======
                width: 80mm;
                margin: 0;
                padding: 0;
            }

            .invoice-container {
                width: 80mm;
                padding: 0;
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            }

            .no-print {
                display: none !important;
            }

            @page {
<<<<<<< HEAD
                size: 80mm 1200mm;
=======
                size: 80mm auto;
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
                margin: 0;
            }
        }

        .print-button {
            position: fixed;
            top: 10px;
            right: 10px;
            padding: 8px 16px;
            background-color: #000;
            color: white;
            border: none;
            border-radius: 3px;
            cursor: pointer;
            font-size: 12px;
            font-family: 'Segoe UI', Tahoma, sans-serif;
            z-index: 1000;
        }

        .print-button:hover {
            background-color: #333;
        }

        .preview-container {
            display: flex;
            justify-content: center;
            padding: 20px;
            background: #f0f0f0;
        }
    </style>
</head>
<<<<<<< HEAD
<body class=""thermal-receipt"">
=======
<body>
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
    <button class=""print-button no-print"" onclick=""window.print()"">🖨️ طباعة</button>

    <div class=""invoice-container"">
        <div class=""header"">
            <div class=""store-name-main"">الحاج حساني</div>
            <div class=""store-name-sub"">لتجارة الأدوات المنزلية الكهربائية</div>
<<<<<<< HEAD
            <div class=""store-address"">الشدادي - الشارع العام مقابل البلدية</div>
=======
            <div class=""store-address"">الشدادي - الشارع العام</div>
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            <div class=""store-phone"">خدمة الزبائن: 0996120955</div>
        </div>

        <div class=""invoice-info"">
            <div class=""invoice-title"">فاتورة</div>
            <div class=""invoice-details"">
                <p><span class=""label"">رقم:</span> <span id=""invoiceNumber"">0001</span></p>
            </div>
        </div>

        <div class=""customer-info"">
            <p><span class=""label"">الزبون:</span> <span id=""customerName"">زبون عام</span></p>
            <p><span class=""label"">الهاتف:</span> <span id=""customerPhone"">-</span></p>
        </div>

        <table class=""items-table"">
            <thead>
                <tr>
                    <th style=""width: 18%;"">سعر</th>
                    <th style=""width: 12%;"">الكمية</th>
                    <th style=""width: 45%;"">المادة</th>
                    <th style=""width: 18%;"">الإجمالي</th>
                    <th style=""width: 7%;"">#</th>
                </tr>
            </thead>
            <tbody id=""itemsBody"">
            </tbody>
        </table>

        <div class=""totals-section"">
            <table class=""totals-table"">
                <tr>
                    <td class=""label-cell"">المجموع:</td>
                    <td class=""value-cell"" id=""subtotal"">$575.00</td>
                </tr>
                <tr>
                    <td class=""label-cell"">الخصم:</td>
                    <td class=""value-cell"" id=""discount"">$0.00</td>
                </tr>
                <tr class=""total-row"">
                    <td class=""label-cell"">الإجمالي:</td>
                    <td class=""value-cell grand-total"" id=""totalAmount"">$575.00</td>
                </tr>
            </table>

            <div class=""amount-in-words"">
                <span class=""label"">فقط: </span>
                <span id=""amountInWords"">خمسمائة وخمسة وسبعون دولار فقط</span>
            </div>

            <div class=""note-section"">
                <p><span class=""label"">ملاحظة:</span></p>
                <div class=""note-line""></div>
            </div>
        </div>

        <div class=""balance-section"">
            <p><span class=""label"">الرصيد السابق:</span> <span class=""value"" id=""previousBalance"">$0.00</span></p>
            <p><span class=""label"">قيمة الفاتورة:</span> <span class=""value"" id=""currentInvoiceValue"">$575.00</span></p>
            <p><span class=""label"">الرصيد الحالي:</span> <span class=""value"" id=""currentBalance"">$575.00</span></p>
        </div>

        <div class=""footer"">
            <p>شكرا لتعاملكم معنا</p>
        </div>
    </div>

    <script>
        function generateInvoiceHTML(sale) {
            const date = new Date(sale.Date);
            const dateStr = date.toLocaleDateString('ar-SA');
            const timeStr = date.toLocaleTimeString('ar-SA');

            let invoiceNum = sale.InvoiceNumber;
            if (invoiceNum) {
                invoiceNum = invoiceNum.replace(/[^0-9]/g, '');
            }

            document.getElementById('invoiceNumber').textContent = invoiceNum || '000000000000';
            document.getElementById('customerName').textContent = sale.CustomerName || 'زبون عام';
            document.getElementById('customerPhone').textContent = sale.CustomerPhone || '-';

            let itemsHTML = '';
            sale.SaleItems.forEach((item, index) => {
                itemsHTML += `
                    <tr>
                        <td class=""left-align"">${item.UnitPrice.toFixed(2)}</td>
                        <td class=""left-align"">${item.Quantity}</td>
<<<<<<< HEAD
                        <td class=""product-name value-bold"">${item.Product?.Name || 'منتج'}</td>
=======
                        <td class=""product-name"">${item.Product?.Name || 'منتج'}</td>
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
                        <td class=""left-align"">${item.TotalPrice.toFixed(2)}</td>
                        <td class=""number"">${index + 1}</td>
                    </tr>
                `;
            });
            document.getElementById('itemsBody').innerHTML = itemsHTML;

            const subtotal = sale.SaleItems.reduce((sum, item) => sum + item.TotalPrice, 0);
            const discount = sale.Discount || 0;
            const total = subtotal - discount;

            document.getElementById('subtotal').textContent = subtotal.toFixed(2);
            document.getElementById('discount').textContent = discount.toFixed(2);
            document.getElementById('totalAmount').textContent = total.toFixed(2);

            document.getElementById('amountInWords').textContent = numberToArabicWords(total);

            document.getElementById('previousBalance').textContent = sale.PreviousBalance?.toFixed(2) || '0.00';
            document.getElementById('currentInvoiceValue').textContent = total.toFixed(2);
            document.getElementById('currentBalance').textContent = sale.CurrentBalance?.toFixed(2) || total.toFixed(2);
        }

        function numberToArabicWords(number) {
            if (number === 0) return ""صفر"";

            const units = ["""", ""واحد"", ""اثنان"", ""ثلاثة"", ""أربعة"", ""خمسة"", ""ستة"", ""سبعة"", ""ثمانية"", ""تسعة""];
            const teens = [""عشرة"", ""أحد عشر"", ""اثنا عشر"", ""ثلاثة عشر"", ""أربعة عشر"", ""خمسة عشر"", ""ستة عشر"", ""سبعة عشر"", ""ثمانية عشر"", ""تسعة عشر""];
            const tens = ["""", ""عشرة"", ""عشرون"", ""ثلاثون"", ""أربعون"", ""خمسون"", ""ستون"", ""سبعون"", ""ثمانون"", ""تسعون""];
            const hundreds = ["""", ""مائة"", ""مئتان"", ""ثلاثمائة"", ""أربعمائة"", ""خمسمائة"", ""ستمائة"", ""سبعمائة"", ""ثمانمائة"", ""تسعمائة""];

            let result = """";
            const intPart = Math.floor(number);

            if (intPart >= 1000) {
                const thousands = Math.floor(intPart / 1000);
                const remainder = intPart % 1000;

                if (thousands === 1) result += ""ألف"";
                else if (thousands === 2) result += ""ألفان"";
                else result += formatNumberBelow1000(thousands) + "" آلاف"";

                if (remainder > 0) result += "" و "" + formatNumberBelow1000(remainder);
            } else {
                result = formatNumberBelow1000(intPart);
            }

            return result + "" ريال فقط"";
        }

        function formatNumberBelow1000(number) {
            if (number === 0) return """";

            const units = ["""", ""واحد"", ""اثنان"", ""ثلاثة"", ""أربعة"", ""خمسة"", ""ستة"", ""سبعة"", ""ثمانية"", ""تسعة""];
            const teens = [""عشرة"", ""أحد عشر"", ""اثنا عشر"", ""ثلاثة عشر"", ""أربعة عشر"", ""خمسة عشر"", ""ستة عشر"", ""سبعة عشر"", ""ثمانية عشر"", ""تسعة عشر""];
            const tens = ["""", ""عشرة"", ""عشرون"", ""ثلاثون"", ""أربعون"", ""خمسون"", ""ستون"", ""سبعون"", ""ثمانون"", ""تسعون""];
            const hundreds = ["""", ""مائة"", ""مئتان"", ""ثلاثمائة"", ""أربعمائة"", ""خمسمائة"", ""ستمائة"", ""سبعمائة"", ""ثمانمائة"", ""تسعمائة""];

            let result = """";

            if (number >= 100) {
                const hundred = Math.floor(number / 100);
                const remainder = number % 100;
                result += hundreds[hundred];
                if (remainder > 0) result += "" و "" + formatNumberBelow100(remainder);
            } else {
                result = formatNumberBelow100(number);
            }

            return result;
        }

        function formatNumberBelow100(number) {
            const units = ["""", ""واحد"", ""اثنان"", ""ثلاثة"", ""أربعة"", ""خمسة"", ""ستة"", ""سبعة"", ""ثمانية"", ""تسعة""];
            const teens = [""عشرة"", ""أحد عشر"", ""اثنا عشر"", ""ثلاثة عشر"", ""أربعة عشر"", ""خمسة عشر"", ""ستة عشر"", ""سبعة عشر"", ""ثمانية عشر"", ""تسعة عشر""];
            const tens = ["""", ""عشرة"", ""عشرون"", ""ثلاثون"", ""أربعون"", ""خمسون"", ""ستون"", ""سبعون"", ""ثمانون"", ""تسعون""];

            if (number < 10) {
                return units[number];
            } else if (number < 20) {
                return teens[number - 10];
            } else {
                const unit = number % 10;
                const ten = Math.floor(number / 10);
                if (unit === 0) {
                    return tens[ten];
                }
                return units[unit] + "" و "" + tens[ten];
            }
        }
    </script>
</body>
</html>";
        }
    }
}
