using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reports_DevEpress_Galeery.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Reports_DevEpress_Galeery.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        private readonly IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public XtraReport report(string Name, string paramValueFromUser)
        {
            XtraReport report = GetReport("PurchaseOrderGlassMarket");

            //get data source
            SqlDataSource ds = new SqlDataSource("Report_Connection_String");

            //get stored procedure
            StoredProcQuery sp = new StoredProcQuery("RptGetPODetails", "RptGetPODetails");
            sp.Parameters.Add(new QueryParameter(
                 name: "@InvDocID",
                 type: typeof(string),
                 value: "7251"));


            ds.Queries.Add(sp);
            ds.ConnectionOptions.DbCommandTimeout = 0;
            ds.Fill();

            //assign datasource to report
            report.DataSource = ds;

            return report;
        }

        public XtraReport GetReport(string Name)
        {
            string path = _config.GetValue<string>("ReportPath:ReportPath");
            XtraReport report = XtraReport.FromFile(@path + Name + ".repx", true);

            return report;
        }
    }
}
