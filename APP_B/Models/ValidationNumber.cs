using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;

namespace AppB.Models
{
    // questo model è stato creato sui parametri della API CheckValidationNumber

    class ValidationNumber
    {
        private String errorDescription;
        private String pseudonimo;
        private String validationNumber;
        private String expire;
        private bool used;
        private String comites;

        public string ErrorDescription { get => errorDescription; set => errorDescription = value; }
        public string Pseudonimo { get => pseudonimo; set => pseudonimo = value; }
        // Pseudonimo (formato email xxxxxx@xxx.xx) dalla lista elettori
        [Required(ErrorMessage = "{0} è obbligatorio")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        
        // Codice VN (int lenght 8) dalla lista elettori
        public string ValidationNum { get => validationNumber; set => validationNumber = value; }
        [Required(ErrorMessage = "{0} è obbligatorio")]
        [StringLength(8, MinimumLength = 8,
        ErrorMessage = "Il ValidationNumber deve essere di lunghezza 8 caratteri")]
        [DataType(DataType.Text)]
        // Codice VN (int lenght 8) dalla lista elettori     
        
        public string Expire { get => expire; set => expire = value; }       
        
        public bool Used { get => used; set => used = value; }
        // Comites di appartenenza
        [Required(ErrorMessage = "{0} è obbligatorio")]
        [StringLength(50, MinimumLength = 1,
        ErrorMessage = "Il ValidationNumber deve essere di lunghezza almeno 1 carattere")]
        [DataType(DataType.Text)]
        public string Comites { get => comites; set => comites = value; }

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static ValidationNumber parseValidationNumberFromJSON(String JSON)
        {
            ValidationNumber errors = new ValidationNumber();

            JObject root = JObject.Parse(JSON);
            //memorizzo il campo error 
            JValue resultError = (JValue)root.GetValue("error");
            //setto il campo con il valore del parametro ricevuto dal chaincode
            errors.ErrorDescription = (string)resultError.Value;
            //se parametro error nullo o vuoto, fa il parse del json e assegna al campo timeZone il valore ricevuto dal parametro del chaincode
            if (string.IsNullOrEmpty(errors.errorDescription))
            {
                ValidationNumber number = new ValidationNumber();
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
        }
    }
}
