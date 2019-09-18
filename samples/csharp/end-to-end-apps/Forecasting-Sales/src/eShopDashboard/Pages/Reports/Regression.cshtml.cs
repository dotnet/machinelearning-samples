using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopOnContainers.WebDashboardRazor.ReportsContext;

namespace eShopDashboard.Pages.Reports
{
    public class RegressionModel : PageModel
    {
        public void OnGet()
        {
            ViewData.SetSelectedMenu(SelectedMenu.Reports_Regression);
        }
    }
}