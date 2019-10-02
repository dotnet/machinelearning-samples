using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.ReportsContext
{
    public enum SelectedMenu
    {
        Reports_Regression,
        Reports_TimeSeries,
        Reports_Comparison
    }

    public static class ViewDataHelpers
    {
        private static string selectedMenuKey = "selectedMenu";

        public static void SetSelectedMenu(this ViewDataDictionary viewData, SelectedMenu selectedMenu)
        {
            viewData[selectedMenuKey] = selectedMenu;
        }

        public static SelectedMenu GetSelectedMenu(this ViewDataDictionary viewData)
        {
            return (SelectedMenu)viewData[selectedMenuKey];
        }
    }
}
