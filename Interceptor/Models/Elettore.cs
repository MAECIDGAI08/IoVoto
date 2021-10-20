using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Model per la API GetFullListeElettori
// reperisce i dati degli elettori dalla BC (a valle del processo di autenticazione SPID
// N.B. Probabilmente RangoSedeCompetenza e CittaSedeCompetenza non verranno usati neanche nel BO, farsi dare conferma da Zamponi / Enrico
// N.B. I campi verranno restituiti ma non usati a meno di metodo custom

namespace Interceptor.Models
{
    public class Elettore
    {
        private String nome;
        private String cognome;
        private String sesso;
        private String dataNascita;
        private String luogodiNascita;
        private string codiceFiscale;
        private String comites;
        private String pseudonimo;
        private String codiceElettore;

        public string Nome { get => nome; set => nome = value; }
        public string Cognome { get => cognome; set => cognome = value; }
        public string Sesso { get => sesso; set => sesso = value; }
        public string DataNascita { get => dataNascita; set => dataNascita = value; }
        public string LuogodiNascita { get => luogodiNascita; set => luogodiNascita = value; }
        public string CodiceFiscale { get => codiceFiscale; set => codiceFiscale = value; }
        public string Comites { get => comites; set => comites = value; }
        public string Pseudonimo { get => pseudonimo; set => pseudonimo = value; }
        public string CodiceElettore { get => codiceElettore; set => codiceElettore = value; }
        public string ErrorDescription { get; set; }
        // parserizza la risposta del chaincode e setta i dati ricevuti con i campi della classe
        public static Elettore parseElettoreFromJSON(String JSON)
        {
            Elettore errors = new Elettore();
            JObject root = JObject.Parse(JSON);
            JValue resultError = (JValue)root.GetValue("error");
            errors.ErrorDescription = (string)resultError.Value;
            //START MARIO
            if (resultError.Value != null)
            {
                Elettore elettore = new Elettore();
                JObject result = (JObject)root.GetValue("result");
                JArray payload = (JArray)result.GetValue("payload");
                if (payload.Count() == 0) {
                    errors.ErrorDescription = "No record found";
                    return errors;
                }
                JObject payloadArrayNode = (JObject)payload[0];
                JObject elettori = (JObject)payloadArrayNode.GetValue("elettori");

                String nome = (string)elettori.GetValue("nome");
                String cognome = (String)elettori.GetValue("cognome");
                String sesso = (String)elettori.GetValue("sesso");
                String dataNascita = (String)elettori.GetValue("datadiNascita");
                String luogodiNascita = (String)elettori.GetValue("luogodiNascita");
                String codiceFiscale = (String)elettori.GetValue("codiceFiscale");
                String comites = (String)elettori.GetValue("comites");
                String pseudonimo = (String)elettori.GetValue("pseudonimo");
                String codiceElettore = (String)elettori.GetValue("codiceElettore");

                elettore.nome = nome;
                elettore.cognome = cognome;
                elettore.sesso = sesso;
                elettore.dataNascita = dataNascita;
                elettore.luogodiNascita = luogodiNascita;
                elettore.codiceFiscale = codiceFiscale;
                elettore.comites = comites;
                elettore.pseudonimo = pseudonimo;
                elettore.codiceElettore = codiceElettore;

                return elettore;
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
