using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppA.Models
{
    public class Token
    {
        private String token;
        private String date;
        private String codiceFiscale;

        public string TokenVal { get => token; set => token = value; }
        public string DateVal { get => date; set => date = value; }
        public string CodiceFiscaleVal { get => codiceFiscale; set => codiceFiscale = value; }
        public string ErrorDescription { get; set; }

        public static Token parseTokenFromJSON(String JSON)
        {
            Token errors = new Token();
            JObject root = JObject.Parse(JSON);
            JValue resultError = (JValue)root.GetValue("error");
            errors.ErrorDescription = (string)resultError.Value;
            //START MARIO
            if (resultError.Value != null)
            {
                Token token = new Token();
                JObject result = (JObject)root.GetValue("result");
                JObject payload = (JObject)result.GetValue("payload");
                token.token = (string)payload.GetValue("token");
                token.date = (string)payload.GetValue("date");
                token.codiceFiscale = (string)payload.GetValue("codiceFiscale");

                return token;
            }
            else
            {
                errors.ErrorDescription = "Attenzione, Dato mancante!";
                return errors;
            }
            //END MARIO
        }
    }
}
