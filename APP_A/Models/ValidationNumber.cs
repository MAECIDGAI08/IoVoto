using Newtonsoft.Json.Linq;
using System;

namespace AppA.Models
{
    class ValidationNumber
    {
        private String pseudonimo;
        private String validationNumber;
        private String expire;
        private bool used;
        private String comites;

        public string Pseudonimo { get => pseudonimo; set => pseudonimo = value; }
        public string ValidationNum { get => validationNumber; set => validationNumber = value; }
        public string Expire { get => expire; set => expire = value; }
        public bool Used { get => used; set => used = value; }
        public string Comites { get => comites; set => comites = value; }
        public string ErrorDescription { get; set; }
        //////////

        public static ValidationNumber parseValidationNumberFromJSON(String JSON)
        {
            ValidationNumber number = new ValidationNumber();
            ValidationNumber errors = new ValidationNumber();

            JObject root = JObject.Parse(JSON);
            //START MARIO
            JValue resultError = (JValue)root.GetValue("error");
            errors.ErrorDescription = (string)resultError.Value;
            if (!string.IsNullOrEmpty(errors.ToString()))
            {
                JObject resultNode = (JObject)root.GetValue("result");
                JObject payloadArrayNode = (JObject)resultNode.GetValue("payload");

                number.pseudonimo = (String)payloadArrayNode.GetValue("pseudonimo");
                number.validationNumber = (String)payloadArrayNode.GetValue("validationNumber");
                number.used = (Boolean)payloadArrayNode.GetValue("used");
                number.comites = (String)payloadArrayNode.GetValue("comites");
                return number;
            }
            else
            {
                return errors;
            }
            //END MARIO
        }
    }
}
