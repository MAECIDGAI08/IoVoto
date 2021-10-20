using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace AppB.Models
{
    // classe generica per gestire gli errori standard dati dalle API
    [Serializable]
    public class APIErrors
    {
        public int ErrorCode { get; set; }

        public string ErrorDescription { get; set; }
        private readonly ILogger _logger;
        public APIErrors(ILogger logger) => _logger = logger;

        public APIErrors()
        {
        }
        public static APIErrors parseErrorsFromJSON(String JSON)
        {
            APIErrors errors = new APIErrors();
            JObject root = JObject.Parse(JSON);
            JValue resultError = (JValue)root.GetValue("error");
            errors.ErrorDescription = (string)resultError.Value;

            return errors;
        }
    }
}
