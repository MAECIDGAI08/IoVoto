using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppB.Models
{
    public class Candidato
    {
        #region Campi comuni per tutte le API
        // nome della API
        //[Required(ErrorMessage = "{0} è obbligatorio")]
        //[StringLength(50, MinimumLength = 1,
        //        ErrorMessage = "")]
        //[DataType(DataType.Text)]
        //public string API { get; set; }
        // nome della API

        // error code dalla API
        //[Required(ErrorMessage = "{0} è obbligatorio")]
        //[StringLength(2, MinimumLength = 1,
        //    ErrorMessage = "Il campo ErrorCode deve essere di lunghezza minima 1 carattere")]
        //public int ErrorCode { get; set; }
        // error code dalla API

        //attributi descrizione errore della API
        public string ErrorDescription { get; set; }
        [StringLength(50, MinimumLength = 1,
            ErrorMessage = "Il campo ErrorDescription parte da un minimo di 1 carattere ad un massimo di 50 caratteri")]
        [DataType(DataType.Text)]
        //attributi descrizione errore della API

        #endregion fine campi comuni per tutte le API

        private String uniqueID;
        private String NomeCandidato;
        private String cognomeCandidato;
        private String dataNascitaCandidato;
        private String luogoNascitaCandidato;
        private int ordine;
        private String lista;

        public string UniqueID { get => uniqueID; set => uniqueID = value; }
        public string nomeCandidato { get => NomeCandidato; set => NomeCandidato = value; }
        public string CognomeCandidato { get => cognomeCandidato; set => cognomeCandidato = value; }
        public string DataNascitaCandidato { get => dataNascitaCandidato; set => dataNascitaCandidato = value; }
        public string LuogoNascitaCandidato { get => luogoNascitaCandidato; set => luogoNascitaCandidato = value; }
        public int Ordine { get => ordine; set => ordine = value; }
        public string Lista { get => lista; set => lista = value; }

        //////////////////////////////////////////////////////////////////////////////
        public static List<Candidato> parseCandidatiFromJSON(String JSON)
        {
            List<Candidato> resultList = new List<Candidato>();

            JObject root = JObject.Parse(JSON);
            JObject resultNode = (JObject)root.GetValue("result");
            JArray payloadArrayNode = (JArray)resultNode.GetValue("payload");

            if (payloadArrayNode.Count > 0)
            {
                foreach (JObject item in payloadArrayNode)
                {

                    JObject candidatoNode = (JObject)item.GetValue("candidati");

                    Candidato candidato = new Candidato();
                    candidato.uniqueID = (String)candidatoNode.GetValue("uniqueID");
                    candidato.NomeCandidato = (String)candidatoNode.GetValue("nomeCandidato");
                    candidato.CognomeCandidato = (String)candidatoNode.GetValue("cognomeCandidato");
                    candidato.dataNascitaCandidato = (String)candidatoNode.GetValue("dataNascitaCandidato");
                    candidato.luogoNascitaCandidato = (String)candidatoNode.GetValue("luogoNascitaCandidato");
                    candidato.ordine = (int)candidatoNode.GetValue("ordine");
                    candidato.lista = (String)candidatoNode.GetValue("lista");

                    resultList.Add(candidato);
                }
                return resultList;
            }
            else
            {
                List<Candidato> erroreCandidato = new List<Candidato>()
                { 
                   new Candidato{ErrorDescription = "Attenzione, Dato mancante!"}
                };
                return erroreCandidato;              
            }
        }
    }
}
