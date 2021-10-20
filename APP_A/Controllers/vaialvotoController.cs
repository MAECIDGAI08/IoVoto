using AppA.Models;
using AppA.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using static AppA.Service.Utils;
using Utility;
using Utils = Utility.Utils;
using System.Globalization;
using static AppA.Models.SharedKeys;
using static AppA.Controllers.HomeController;
// IP tracking

namespace AppA.Controllers
{
    public class vaialvotoController : Controller
    {
        //variabile interna per accedere alla localizzazione (SharedResources)
        private readonly IStringLocalizer<SharedResource> _localizer;

        //var interna per accedere all'interfaccia ILoggerFactory
        private readonly ILogger<vaialvotoController> _logger;

        // var interna per IHostingEnvironment
        private readonly IHostingEnvironment env;
       
        // per controllo se votazione è aperta o no
        public bool controlVVC;

        public vaialvotoController(IStringLocalizer<SharedResource> localizer, ILogger<vaialvotoController> logger, IHostingEnvironment env)
        {
            _localizer = localizer;
            _logger = logger;
            this.env = env;
        }

        public string UtenteGenerico()
        {
            string UtenteOnline = CreaGUID();
            string IpUtenteOnline = HttpContext.Connection.RemoteIpAddress.ToString();
            string UtenteGenerico = UtenteOnline + " | " + IpUtenteOnline;
            return UtenteGenerico;
        }

        public object PuoVotare(DateTime OraIngresso, int offsetdaBC, DateTime FineVotazione)
        {
            // setto variabile di check finale           

            // calcolo ora locale del votante
            DateTime OraLocaleVotante = (OraIngresso.AddHours(offsetdaBC));

            // Calcolo l'intervallo tra le due date
            TimeSpan interval = OraLocaleVotante - FineVotazione;

            // check method 2
            TimeSpan interval2 = FineVotazione.Subtract(OraLocaleVotante);

            // assegno a stringa interval
            String IntervalHR = interval.ToString();

            string subIHR = IntervalHR.Substring(0, 1);

            // se subIHR è uguale a "-", ha ancora tempo per votare
            if (subIHR == "-")
            {
                return controlVVC = false;
            }

            return controlVVC = true;
        }

        public IActionResult Index()
        {
            Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "VAIALVOTO info: incoming call to Vaialvoto");

            //LOG HEADER DATA
            foreach (var header in Request.Headers)
            {
                Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "VAIALVOTO header key: " + header.Key.ToString() + " -> " + header.Value.ToString());
            }

            try
            {
                //GET & CHECK IDCS1 DATA
                string codiceElettore = Request.Headers["codice_elettore"];
                string dataNascita = Request.Headers["data_nascita"];

                if (String.IsNullOrEmpty(codiceElettore))
                {
                    Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "VAIALVOTO issue: empty codiceElettore");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
                if (String.IsNullOrEmpty(dataNascita))
                {
                    Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "VAIALVOTO issue: empty dateOfBirth");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                ////BC USER CHECK
                var queryIn = Service.APIService.InvokeChainCode("voters", "Elettori", "getFullElettori", "", codiceElettore, dataNascita);
                Elettore elettore = Elettore.parseElettoreFromJSON(queryIn.Content);
                if (!string.IsNullOrEmpty(elettore.ErrorDescription) || string.IsNullOrEmpty(elettore.CodiceElettore))
                {
                    Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "VAIALVOTO issue: codiceElettore " + codiceElettore + " not found");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                /*QUI L'UTENTE PUO ESSERE AUTENTICATO ANCHE LATO APPLICATIVO*/
                HttpContext.Session.SetString(SessionKeyLogged, "1");

                User user = new User() { UserIP = HttpContext.Connection.RemoteIpAddress.ToString(), UserName = CreaGUID() };


                // controllo se l'utente sta nella timezone data dal comites

                var queryInElettoriperRicevuta = APIService.InvokeChainCode("Elettori", "getFullElettori", codiceElettore, "", dataNascita);

                Elettore elettoreRicevuta = Elettore.parseElettoreFromJSON(queryIn.Content);
                if (!string.IsNullOrEmpty(elettore.ErrorDescription))
                {
                    _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + elettore.ErrorDescription);
                    return RedirectToAction("Errore");
                }

                //dati elettore in sessione
                HttpContext.Session.SetString(SessionKeyPseudonimo, elettore.Pseudonimo);
                HttpContext.Session.SetString(SessionKeyNomeUtente, elettore.Nome);
                HttpContext.Session.SetString(SessionKeyCognomeUtente, elettore.Cognome);
                HttpContext.Session.SetString(SessionKeySessoUtente, elettore.Sesso);
                HttpContext.Session.SetString(SessionKeyDataNascitaUtente, elettore.DataNascita);
                HttpContext.Session.SetString(SessionKeyLuogoNascitaUtente, elettore.LuogodiNascita);
                HttpContext.Session.SetString(SessionKeyCodiceFiscaleUtente, elettore.CodiceFiscale);
                HttpContext.Session.SetString(SessionKeyComitesUtente, elettore.Comites);
                string RandomVN = CreaValidationNumber();
                HttpContext.Session.SetString("_RandomVN", RandomVN.ToString());

                // passiamo il comites alla pagina
                ViewData["Comites"] = HttpContext.Session.GetString(SessionKeyComitesUtente);

                // start check timezone
                
                var queryFusoOrario = APIService.InvokeChainCode("lists", "Lista", "getTimeZone", elettore.Comites);

                // logging chiamata BC
                _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + queryFusoOrario.StatusCode);
                _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + queryFusoOrario.Content);

                // nuovo oggetto
                Lista timezone = Lista.parseTimeZoneFromJSON(queryFusoOrario.Content);               
                if (string.IsNullOrEmpty(timezone.ErrorDescription.ToString()))
                {
                    _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + timezone.ErrorDescription);
                    return RedirectToAction("Errore");
                }

                // test
                //FineVotazione = "26/09/2021 00:00";          

                var inizioVotazione = Startup.StaticConfig.GetSection("InizioVotazione").Value;
                var fineVotazione   = Startup.StaticConfig.GetSection("FineVotazione").Value;

                DateTime OraIngressoVaiAlVoto = DateTime.Now.ToLocalTime();
                PuoVotare(OraIngressoVaiAlVoto, timezone.TimeZone, DateTime.Parse(fineVotazione));

                // CHECK orario scaduto
                if (controlVVC == true)
                {
                    _logger.LogInformation(UteLogNam + UtenteGenerico() + UteCant + UteComites);
                    ViewData["Title"] = "Tempo per la votazione scaduto";
                    ViewData["FineVotazione"] = fineVotazione;
                    ViewBag.Exit = false;
                    Dispose();
                    return View("TimeOver");
                }
                // controllo se l'utente rientra nella timezone data dal comites

                // gestione VN

                //chiamata al chaincode VN che controlla lo pseudonimo e restituisce vn e comites associati ad esso
                var queryInVN = APIService.InvokeChainCode("elections", "ValidationNumber", "getValidationnumberById", elettore.Pseudonimo);

                // logging chiamata BC
                _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + queryInVN.StatusCode);
                _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + queryInVN.Content);

                // contiene la risposta del chaincode parserizzata => pseudonimo, vn, comites, used(di default a false finchè elettore non ha votato)
                ValidationNumber number = ValidationNumber.parseValidationNumberFromJSON(queryInVN.Content);

                if (!string.IsNullOrEmpty(number.ErrorDescription))
                {
                    _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + number.ErrorDescription);
                    return RedirectToAction("Errore");
                }

                // Se l'utente ha votato, lo redirigo alla ricevuta di voto
                if (number.Used == true)
                {
                    _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + "USED VALIDATION NUMBER");
                    return RedirectToAction("RicevutadiVoto");
                }
                              
                // Se il VN non è stato ancora creato viene scritto adesso
                if (number.ValidationNum is null || number.ValidationNum == "")
                {
                    var queryOutVN_2 = APIService.InvokeChainCode("elections", "ValidationNumber", "setValidationnumber", HttpContext.Session.GetString("PseudonimoKey"), HttpContext.Session.GetString("_RandomVN").ToString());
                    ValidationNumber number_2 = ValidationNumber.parseValidationNumberFromJSON(queryOutVN_2.Content);
                    
                    if (!string.IsNullOrEmpty(number_2.ErrorDescription))
                    {
                        _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + number_2.ErrorDescription);
                        return RedirectToAction("Errore");
                    }

                }

                ViewBag.SitoAperto = 1;
                ViewBag.Exit = true;

                /*ALIMENTARE CON ALTRE COSE NECESSARIE************************/

                Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "VAIALVOTO action: redirecting to View");
				ViewData["URL"]= Startup.StaticConfig.GetSection("AppB_URL").Value;
                ViewBag.Exit = true;
                return View();

            }
            catch (Exception e)
            {
                Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "VAIALVOTO issue: " + e.Message);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }
        }

        


    }
}
