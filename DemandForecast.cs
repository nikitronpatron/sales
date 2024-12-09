using System.Text;
using Newtonsoft.Json;

namespace SalesService
{
    public class DemandForecast
    {
        public int Id { get; set; } 
        public string ProductName { get; set; }
        public DateTime ForecastDate { get; set; }
        public int PredictedDemand { get; set; }
        public string Recommendation { get; set; }
    }

}
