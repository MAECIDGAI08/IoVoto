using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AppB.Models
{
    public class Voto
    {
    //    "args": [
    //    "vote",
    //    "{\"ids\":[\"444\",\"333\"]}",
    //    "Lista01",
    //    "{\"candidato\":
    //    [
    //    {\"nomeCandidato\":\"candidato1\",\"cognomeCandidato\":\"cognome1\",\"dataNascitaCandidato\":\"21-11-1978\",\"luogoNascitaCandidato\":\"roma\",\"ordine\":1,\"comites\":\"Berlino\"},
    //    {\"nomeCandidato\":\"candidato2\",\"cognomeCandidato\":\"cognome1\",\"dataNascitaCandidato\":\"21-11-1978\",\"luogoNascitaCandidato\":\"roma\",\"ordine\":1,\"comites\":\"Berlino\"}]}",
    //    "TestDaniele@test.it"
    //],
        
        
        
        
        // stringa per il recupero del voto
        private string  errorDescription;
        private string  UniqueId;
        private string  lista;
        private string  nomeCandidato;
        private string  cognomeCandidato;
        private string  dataNascitaCandidato;
        private string  luogoNascitaCandidato;
        private int     ordine;
        private string  comites;

        public string ErrorDescription { get => errorDescription; set => errorDescription = value; }
        //attributi descrizione errore della API

        public string Lista { get => lista; set => lista = value; }
        //attributi descrizione errore della API

        public string ids { get => UniqueId; set => UniqueId = value; }

        public string NomeCandidato { get => nomeCandidato; set => nomeCandidato = value; }

        public string CognomeCandidato { get => cognomeCandidato; set => cognomeCandidato = value; }

        public string DataNascitaCandidato { get => dataNascitaCandidato; set => dataNascitaCandidato = value; }

        public string LuogoNascitaCandidato { get => luogoNascitaCandidato; set => luogoNascitaCandidato = value; }

        public int Ordine { get => ordine; set => ordine = value; }

        public string Comites { get => comites; set => comites = value; }

        public static JObject parseVoteresponseFromJSON(String JSON)
        {
            JObject root = JObject.Parse(JSON);
            JObject resultNode = (JObject)root.GetValue("result");
            JObject payloadArrayNode = (JObject)resultNode.GetValue("payload");

            return payloadArrayNode;
        }
           
        //public static Voto sendVotoToJSON(String JSON)
        //{
        //    Voto votazione = new Voto();

        //    JObject root = JObject.Parse(JSON);
        //    JObject resultNode = (JObject)root.GetValue("result");
        //    JObject payloadArrayNode = (JObject)resultNode.GetValue("payload");

        //    //number.pseudonimo = (String)payloadArrayNode.GetValue("pseudonimo");
        //    //number.validationNumber = (String)payloadArrayNode.GetValue("validationNumber");
        //    //number.used = (Boolean)payloadArrayNode.GetValue("used");
        //    //number.comites = (String)payloadArrayNode.GetValue("comites");
            
        //    return votazione;

        //}

    
        


    }
}
