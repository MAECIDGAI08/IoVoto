using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;

namespace AppB.Service
{
    public static class Utils
    {
        // costanti per snellire il logging
        public const string UteLogNam = " Utente | ";
        public const string UteLogDo = " | accede alla pagina | ";
        public const string UteBloCha = " | Chiamata BlockChain metodo | ";
        public const string UteCant = " | Accesso negato per | ";
        public const string UtePseudonimo = " | Pseudonimo mancante in BC | ";
        public const string UteVN = " | Validation Number mancante in BC | ";
        public const string UteComites = " | Comites mancante in BC | ";
        public const string UteLogChkS = "";
        public const string UteLogChkUp = "";
        public const string UteLogChkDown = "";

        // Creazione validation Number
        public static int CreaValidationNumber()
        {
            Random rnd = new Random();
            int myRandomNr = rnd.Next(10000000, 99999999); // creates a 8 digit random no.
            return myRandomNr;
        }

        // randomico per utente che accede al portale
        public static string ProgressivoUtente()
        {
            Random rnd = new Random();
            int UserRandomNr = rnd.Next(10000000, 99999999);
            string URN = UserRandomNr.ToString();
            string dataAccesso = DateTime.Now.ToString();
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
        
        public static string CreatePseudonimo(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }           
            return res.ToString();
        }

        public static int LettereCasuali()
        {
            Random rnd = new Random();
            int randomBetween5And15 = rnd.Next(5, 15);
            return randomBetween5And15;
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
