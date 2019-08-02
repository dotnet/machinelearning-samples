using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopOnContainers.WebDashboardRazor.ReportsContext;

namespace eShopDashboard.Pages.Reports
{
    public class TimeSeriesModel : PageModel
    {
        public void OnGet()
        {
            ViewData.SetSelectedMenu(SelectedMenu.Reports_TimeSeries);
        }
    }
}