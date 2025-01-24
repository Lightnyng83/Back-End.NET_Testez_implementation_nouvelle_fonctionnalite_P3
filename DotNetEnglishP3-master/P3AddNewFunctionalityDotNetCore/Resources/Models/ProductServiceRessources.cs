using System.Globalization;
using System.Reflection;
using System.Resources;

namespace P3Core.Resources.Models
{
    public static class ProductService
    {
        private static ResourceManager resourceManager = new ResourceManager("P3.Resources.Models.ProductService", Assembly.GetExecutingAssembly());
        private static CultureInfo resourceCulture;

        public static string MissingName
        {
            get
            {
                return resourceManager.GetString("MissingName", resourceCulture);
            }
        }
        public static string MissingPrice
        {
            get
            {
                return resourceManager.GetString("MissingPrice", resourceCulture);
            }
        }
        public static string MissingStock
        {
            get
            {
                return resourceManager.GetString("MissingStock", resourceCulture);
            }
        }public static string MissingQuantity
        {
            get
            {
                return resourceManager.GetString("MissingQuantity", resourceCulture);
            }
        }
        public static string PriceNotANumber
        {
            get
            {
                return resourceManager.GetString("PriceNotANumber", resourceCulture);
            }
        }
        public static string PriceNotGreaterThanZero
        {
            get
            {
                return resourceManager.GetString("PriceNotGreaterThanZero", resourceCulture);
            }
        }
        public static string StockNotAnInteger
        {
            get
            {
                return resourceManager.GetString("StockNotAnInteger", resourceCulture);
            }
        }
        public static string StockNotGreaterThanZero
        {
            get
            {
                return resourceManager.GetString("StockNotGreaterThanZero", resourceCulture);
            }
        }
    }
}

