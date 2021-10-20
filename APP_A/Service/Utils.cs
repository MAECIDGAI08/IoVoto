using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;


namespace AppA.Service
{
    public static class Utils
    {
        // costanti per snellire il logging
        public const string UteLogNam = " Utente | ";
        public const string UteLogDo = " | accede alla pagina | ";
        public const string UteBloCha = " | Chiamata BlockChain metodo | ";
        public const string UteCant = " | Accesso negato per | ";
        public const string UteComites = " | Comites mancante in BC | ";
        public const string UteLogChkS = "";
        public const string UteLogChkUp = "";
        public const string UteLogChkDown = "";

        // Creazione validation Number (8 numeri)
        public static string CreaValidationNumber()
        {
            Random rnd = new Random();
            int myRandomNr = rnd.Next(10000000, 99999999);          
            return myRandomNr.ToString();
        }

        // randomico per utente che accede al portale
        public static String ProgressivoUtente()
        {            
            Random rnd = new Random();
            int UserRandomNr = rnd.Next(10000000, 99999999);
            string URN = UserRandomNr.ToString();
            string dataAccesso = DateTime.Now.ToLocalTime().ToString();           
            URN = URN + " | " + dataAccesso;           
            return URN;
        }

        // metodo per recuperare l'IP dell'utente
        public static string IPUtente()
        {
            IPHostEntry heserver = Dns.GetHostEntry(Dns.GetHostName());
            var ip = heserver.AddressList[1].ToString();            
            return ip;
        }

        public static DateTime IngressoUtente()
        {
            DateTime quando = DateTime.Now.ToLocalTime();
            return quando;
        }

        // Creazione di una GUID per il processo di login
        public static string CreaGUID()
        {
            System.Guid guid = System.Guid.NewGuid();
            string guid2 = Convert.ToString(guid);    
            return guid2;
        }

    }

    public class variousServices
    {
        
        // metodo di servizio per settare la stringa corrispondente al genere corretto
        public static string SignoreSignora(string qualeGenere)
        {
            string genere = "";
            if (qualeGenere == "M")
            {
                genere = "Il Signor";
            }
            else if (qualeGenere == "F")
            {
                genere = "La Signora";
            }
            return genere;
        }
    
    }

    // estensioni default per handler oggetti di sessione
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) :
                JsonConvert.DeserializeObject<T>(value);
        }
    }

}
