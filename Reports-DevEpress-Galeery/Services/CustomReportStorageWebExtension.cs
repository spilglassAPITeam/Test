using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevExpress.XtraReports.UI;
using Galler_Report_Core.PredefinedReports;
using Galler_Report_Core.Data;
using System.Text;
using Galler_Report_Core.Controllers;
using Microsoft.Extensions.Configuration;
using System.ServiceModel;
using System;

namespace Galler_Report_Core.Services
{
    public class CustomReportStorageWebExtension : DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension
    {
        public string name, Param;

        private readonly IConfiguration _config;

        public CustomReportStorageWebExtension(IConfiguration config)
        {
            _config = config;
        }

        string reportDirectory = "E:\\spil\\Study\\Gallery-Report\\GalleryReport\\Reports\\";
        const string FileExtension = ".repx";
        public Dictionary<string, XtraReport> Reports = new Dictionary<string, XtraReport>();

        public CustomReportStorageWebExtension()
        {

            XtraReport report1 = new XtraReport();

        }

        public override bool CanSetData(string url)
        {
            return true;
        }

        public override byte[] GetData(string url)
        {
            // Uses a specified URL to return report layout data stored within a report storage medium.
            // This method is called if the **IsValidUrl** method returns **true**.
            // You can use the **GetData** method to process report parameters sent from the client
            // if the parameters are included in the report URL's query string.
            
            var encodedurl = System.Convert.FromBase64String(url);
            string reporturl = Encoding.UTF8.GetString(encodedurl);
            string[] r_data = reporturl.Split('?');

            name = r_data[0];
            Param = r_data.Length > 1 ? r_data[1] : string.Empty;
            var hashobtained = r_data[2].ToString();

            XtraReport rpt = null;
             allreportstore(name, Param);

           
            try
            {
                var report = Reports[name];
                using (MemoryStream stream = new MemoryStream())
                {
                    report.Parameters["InvDocID"].Value = "7251";
                    report.SaveLayoutToXml(stream);
                    
                    return stream.ToArray();
                }

                /*if (ReportsFactory.Reports.ContainsKey(name))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ReportsFactory.Reports[name]().SaveLayoutToXml(ms);
                        return ms.ToArray();
                    }
                }
                if (Directory.EnumerateFiles(reportDirectory).Select(Path.GetFileNameWithoutExtension).Contains(name))
                {
                    byte[] reportBytes = File.ReadAllBytes(Path.Combine(reportDirectory, name + FileExtension));
                    using (MemoryStream ms = new MemoryStream(reportBytes))
                    {
                        rpt.SaveLayoutToXml(ms);
                        return ms.ToArray();
                    }
                    //return File.ReadAllBytes(Path.Combine(reportDirectory, name + FileExtension));
                }*/
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason("Could not get report data."),
                    new FaultCode("Server"), "GetData");
            }
            throw new FaultException(new FaultReason(string.Format("Could not find report '{0}'.", name)),
                new FaultCode("Server"), "GetData");

        }

        public override Dictionary<string, string> GetUrls()
        {
            return Reports.ToDictionary(x => x.Key, y => y.Key);
        }

        public override void SetData(XtraReport report, string url)
        {
            if (Reports.ContainsKey(url))
            {
                Reports[url] = report;
            }
            else
            {
                Reports.Add(url, report);
            }
        }

        public override string SetNewData(XtraReport report, string defaultUrl)
        {
            SetData(report, defaultUrl);
            return defaultUrl;
        }

        public override bool IsValidUrl(string url)
        {
            return true;
        }


        public XtraReport allreportstore(string reportName, string reportParameter)
        {
            XtraReport report1 = new XtraReport();
            
            CustomerServicesController cs = new CustomerServicesController(_config);

            #region Purchase Order Report
            if (reportName != null || reportParameter != null)
            {
                report1 = cs.report(reportName, reportParameter);

                SetNewData(report1, reportName);

            }
            #endregion
            return report1;

        }
    }
}

/* public class CustomReportStorageWebExtension : DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension
    {
        string reportDirectory = "E:\\spil\\Study\\-Gallery-Report-Core\\New folder\\Galler Report Core\\Galler Report Core\\Reports\\";
        const string FileExtension = ".repx";
        public CustomReportStorageWebExtension(string reportDirectory) {
            if (!Directory.Exists(reportDirectory)) {
                Directory.CreateDirectory(reportDirectory);
            }
            this.reportDirectory = reportDirectory;
        }

        private bool IsWithinReportsFolder(string url, string folder) {
            var rootDirectory = new DirectoryInfo(folder);
            var fileInfo = new FileInfo(Path.Combine(folder, url));
            return fileInfo.Directory.FullName.ToLower().StartsWith(rootDirectory.FullName.ToLower());
        }

        public override bool CanSetData(string url) {
            // Determines whether a report with the specified URL can be saved.
            // Add custom logic that returns **false** for reports that should be read-only.
            // Return **true** if no valdation is required.
            // This method is called only for valid URLs (if the **IsValidUrl** method returns **true**).

            return true;
        }

        public override bool IsValidUrl(string url) {
            // Determines whether the URL passed to the current report storage is valid.
            // Implement your own logic to prohibit URLs that contain spaces or other specific characters.
            // Return **true** if no validation is required.

            return Path.GetFileName(url) == url;
        }

        public override byte[] GetData(string url) {
            // Uses a specified URL to return report layout data stored within a report storage medium.
            // This method is called if the **IsValidUrl** method returns **true**.
            // You can use the **GetData** method to process report parameters sent from the client
            // if the parameters are included in the report URL's query string.
            try {
                if (Directory.EnumerateFiles(reportDirectory).Select(Path.GetFileNameWithoutExtension).Contains(url))
                {
                    return File.ReadAllBytes(Path.Combine(reportDirectory, url + FileExtension));
                }
                if (ReportsFactory.Reports.ContainsKey(url))
                {
                    using (MemoryStream ms = new MemoryStream()) {
                        ReportsFactory.Reports[url]().SaveLayoutToXml(ms);
                        return ms.ToArray();
                    }
                }
            } catch (Exception) {
                throw new FaultException(new FaultReason("Could not get report data."), 
                    new FaultCode("Server"), "GetData");
            }
            throw new FaultException(new FaultReason(string.Format("Could not find report '{0}'.", url)), 
                new FaultCode("Server"), "GetData");
        }

        public override Dictionary<string, string> GetUrls() {
            // Returns a dictionary that contains the report names (URLs) and display names. 
            // The Report Designer uses this method to populate the Open Report and Save Report dialogs.

            return Directory.GetFiles(reportDirectory, "*" + FileExtension)
                                     .Select(Path.GetFileNameWithoutExtension)
                                     .Union(ReportsFactory.Reports.Select(x => x.Key))
                                     .ToDictionary<string, string>(x => x);
        }

        public override void SetData(XtraReport report, string url) {
            // Saves the specified report to the report storage with the specified name
            // (saves existing reports only).

            if(!IsWithinReportsFolder(url, reportDirectory))
                throw new FaultException(new FaultReason("Invalid report name."), new FaultCode("Server"), "GetData");
            report.SaveLayoutToXml(Path.Combine(reportDirectory, url + FileExtension));
        }

        public override string SetNewData(XtraReport report, string defaultUrl) {
            // Allows you to validate and correct the specified name (URL).
            // This method also allows you to return the resulting name (URL),
            // and to save your report to a storage. The method is called only for new reports.

            SetData(report, defaultUrl);
            return defaultUrl;
        }
    }
}*/