using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppA.Models
{
    public class SharedKeys
    {
        
        // chiave di sessione per l'ip Utente
        public const string SessionKeyIP = "_IpUser";

        //  Chiave di sessione per Utente generato random
        public const string SessionKeysessionUser = "_sessionUser";

        // chiave di sessione per il Validation Number
        public const string SessionKeyRandomVN = "_RandomVN";

        // Chiave di sessione per il Nome Utente 
        public const string SessionKeyNomeUtente = "_NomeUtente";

        // Chiave di sessione per il Cognome Utente 
        public const string SessionKeyCognomeUtente = "_CognomeUtente";

        // Chiave di sessione per il Luogo Nascita Utente 
        public const string SessionKeySessoUtente = "_SessoUtente";

        // Chiave di sessione per la DataNascita dell'utente SPID (usato anche per la generazione della ricevuta)
        public const string SessionKeyDataNascitaUtente = "_DataNascita";

        // Chiave di sessione per il Codice Fiscale 
        public const string SessionKeyCodiceFiscaleUtente = "_CodiceFiscale";

        // Chiave di sessione per il Luogo Nascita Utente 
        public const string SessionKeyLuogoNascitaUtente = "_LuogoNascitaUtente";

        // Chiave di sessione per il Comites Utente 
        public const string SessionKeyComitesUtente = "_ComitesUtente";

        // Chiave di sessione per il TimeZone Utente 
        public const string SessionKeyTimeZoneComites = "_TimezoneComites";

        // check di accesso terminato
        public const string SessionKeyLogged = "LOGGED";

        // Chiave di sessione per lo Pseudonimo
        public const string SessionKeyPseudonimo = "PseudonimoKey";

        // url portale APP A
        public const string SessionKeyUrlAppA = "_UrlAppA";

        // url portale APP B
        public const string SessionKeyUrlAppB = "_UrlAppB";

        // per controllo se votazione è aperta o no
        public bool control;
    }
}
