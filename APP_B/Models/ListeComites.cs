using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppB.Models;
using Microsoft.Extensions.Logging;

namespace AppB.Models
{
    public class Lista
    {
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
        //attributi descrizione errore della API
        [StringLength(50, MinimumLength = 1,
            ErrorMessage = "Il campo ErrorDescription parte da un minimo di 1 carattere ad un massimo di 50 caratteri")]
        [DataType(DataType.Text)]
        public string ErrorDescription { get; set; }
        ///////////////////////////////////////////////////////////////////////////////

        public Lista(int idLista, String nomePartito, string imgPartito)
        {
            ordine = idLista;
            name = nomePartito;
            imageUrl = imgPartito;
        }
        public Lista() { }

        ///////////////////////////////////////////////////////////////////////////////
        public static List<Lista> parseListaFromJSON(String JSON)
        {
            List<Lista> resultList = new List<Lista>();

            JObject root = JObject.Parse(JSON);
            JObject resultNode = (JObject)root.GetValue("result");
            JArray payloadArrayNode = (JArray)resultNode.GetValue("payload");

            foreach (JObject item in payloadArrayNode)
            {
                JObject listaNode = (JObject)item.GetValue("liste");

                Lista lista = new Lista();
                lista.name = (String)listaNode.GetValue("name");
                lista.imageUrl = (String)listaNode.GetValue("imageUrl");
                lista.comites = (String)listaNode.GetValue("comites");
                lista.rangoSedeCompetenza = (String)listaNode.GetValue("rangoSedeCompetenza");
                lista.cittaSedeCompetenza = (String)listaNode.GetValue("cittaSedeCompetenza");
                lista.timeZone = (int)listaNode.GetValue("timeZone");
                lista.maxSel = (int)listaNode.GetValue("maxSel");
                lista.ordine = (int)listaNode.GetValue("ordine");

                resultList.Add(lista);
            }

            return resultList;

        }
        
        // recupero timezone dal ledger Liste
        public static Lista parseTimeZoneFromJSON(String JSON)
        {
            Lista errors = new Lista();
            //memorizzo l'oggetto ricevuto dalla chiamata al chaincode
            JObject root = JObject.Parse(JSON);
            //memorizzo il campo error 
            JValue resultError = (JValue)root.GetValue("error");
            //setto il campo con il valore del parametro ricevuto dal chaincode
            errors.ErrorDescription = (string)resultError.Value;
            //se parametro error nullo o vuoto, fa il parse del json e assegna al campo timeZone il valore ricevuto dal parametro del chaincode
            if (string.IsNullOrEmpty(errors.ToString()))
            {
                Lista timezone = new Lista();
                JObject resultNode = (JObject)root.GetValue("result");
                JObject payloadArrayNode = (JObject)resultNode.GetValue("payload");
                timezone.timeZone = (int)payloadArrayNode.GetValue("timeZone");
                return timezone;
            }
            else
            {
             return errors;
            }
        }
    }
}
