using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FasterWeatherBot.Models
{
    /// <summary>
    /// Weather query history model.
    /// </summary>
    public class WeatherHistoryDate
    {
        public long UserId { get; set; }
        public string City { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// Date of request.
        /// </summary>
        public string TimeString { get; set; }
    }
}
