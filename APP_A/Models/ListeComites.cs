using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppA.Models
{
    public class Lista
    {
        #region Campi comuni per tutte le API
        // nome della API
        [Required(ErrorMessage = "{0} è obbligatorio")]
        [StringLength(50, MinimumLength = 1,
                ErrorMessage = "")]
        [DataType(DataType.Text)]
        public string API { get; set; }
        // nome della API

        // error code dalla API
        [Required(ErrorMessage = "{0} è obbligatorio")]
        [StringLength(2, MinimumLength = 1,
            ErrorMessage = "Il campo ErrorCode deve essere di lunghezza minima 1 carattere")]
        public int ErrorCode { get; set; }
        // error code dalla API

        //attributi descrizione errore della API
        [StringLength(50, MinimumLength = 1,
            ErrorMessage = "Il campo ErrorDescription parte da un minimo di 1 carattere ad un massimo di 50 caratteri")]
        [DataType(DataType.Text)]
        public int ErrorDescription { get; set; }
        //attributi descrizione errore della API

        #endregion fine campi comuni per tutte le API

        public String name;
        public String imageUrl;
        public String comites;
        public String rangoSedeCompetenza;
        public String cittaSedeCompetenza;
        public int timeZone;
        public int maxSel;
        public int ordine;

        public string Name { get => name; set => name = value; }
        public string ImageUrl { get => imageUrl; set => imageUrl = value; }
        public string Comites { get => comites; set => comites = value; }
        public string RangoSedeCompetenza { get => rangoSedeCompetenza; set => rangoSedeCompetenza = value; }
        public string CittaSedeCompetenza { get => cittaSedeCompetenza; set => cittaSedeCompetenza = value; }
        public int TimeZone { get => timeZone; set => timeZone = value; }
        public int MaxSel { get => maxSel; set => maxSel = value; }
        public int Ordine { get => ordine; set => ordine = value; }

        ///////////////////////////////////////////////////////////////////////////////
      
        // recupero timezone dal ledger Liste
        public static Lista parseTimeZoneFromJSON(String JSON)
        {            
            JObject root = JObject.Parse(JSON);
            JObject resultNode = (JObject)root.GetValue("result");
            JObject payloadArrayNode = (JObject)resultNode.GetValue("payload");
            Lista timezone = new Lista();
            timezone.timeZone = (int)payloadArrayNode.GetValue("timeZone");
            return timezone;
        }
    }
}
