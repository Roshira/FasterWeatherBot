using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FasterWeatherBot.Models
{
    internal class Users
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string LanguageCode { get; set; }
        public bool IsBot { get; set; }

    }
}
