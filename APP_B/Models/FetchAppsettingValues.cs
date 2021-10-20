using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppB.Models
{
       
        public class AppSettings
        {
            public Logging Logging { get; set; }
            public string AllowedHosts { get; set; }
        }

        public class Logging
        {
            public LogLevel LogLevel { get; set; }
        }

        public class LogLevel
        {
            public string Default { get; set; }

            public string Warning { get; set; }

            public string Error { get; set; }
        }

        public class Parameters
        {
        public string URLAppA { get; set; }
        public string URLAppB { get; set; }

        public string InizioVotazione { get; set; }

            public string FineVotazione { get; set; }

            public string TempoSessione { get; set; }

            public string UserAppA { get; set; }

            public string UserAppB { get; set; }

            public string IDREG { get; set; }

            public string EVOTE { get; set; }

            public string EndPointAppB { get; set; }

            public string UrlLogout { get; set; }
    }
}

