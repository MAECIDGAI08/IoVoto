﻿using AppA.Models;
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
            Utils.LogTrace(Request.Headers["X-Forwarded-For"], "VAIALVOTO info: incoming call to Vaialvoto");

            //LOG HEADER DATA
            foreach (var header in Request.Headers)
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "VAIALVOTO header key: " + header.Key.ToString() + " -> " + header.Value.ToString());
            }

            try
            {
                //GET & CHECK IDCS1 DATA
                string codiceElettore = Request.Headers["codice_elettore"];
                string dataNascita = Request.Headers["data_nascita"];

                if (String.IsNullOrEmpty(codiceElettore))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "VAIALVOTO issue: empty codiceElettore");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
                if (String.IsNullOrEmpty(dataNascita))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "VAIALVOTO issue: empty dateOfBirth");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                ////BC USER CHECK
                var queryIn = Service.APIService.InvokeChainCode("voters", "Elettori", "getFullElettori", "", codiceElettore, dataNascita);
                Elettore elettore = Elettore.parseElettoreFromJSON(queryIn.Content);
                if (!string.IsNullOrEmpty(elettore.ErrorDescription) || string.IsNullOrEmpty(elettore.CodiceElettore))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "VAIALVOTO issue: codiceElettore " + codiceElettore + " not found");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                /*QUI L'UTENTE PUO ESSERE AUTENTICATO ANCHE LATO APPLICATIVO*/
                HttpContext.Session.SetString(SessionKeyLogged, "1");

                User user = new User() { UserIP = Request.Headers["X-Forwarded-For"], UserName = CreaGUID() };


                // controllo se l'utente sta nella timezone data dal comites

                var queryInElettoriperRicevuta = Service.APIService.InvokeChainCode("voters", "Elettori", "getFullElettori", "", codiceElettore, dataNascita);
                
                Elettore elettoreRicevuta = Elettore.parseElettoreFromJSON(queryInElettoriperRicevuta.Content);
                if (!string.IsNullOrEmpty(elettore.ErrorDescription))
                {
                    _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + elettore.ErrorDescription);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
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
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
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
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                // Se l'utente ha votato, lo redirigo alla ricevuta di voto
                if (number.Used == true)
                {
                    _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + "USED VALIDATION NUMBER");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/vaialvoto/RicevutaDiVoto");
                }
                              
                // Se il VN non è stato ancora creato viene scritto adesso
                if (number.ValidationNum is null || number.ValidationNum == "")
                {
                    var queryOutVN_2 = APIService.InvokeChainCode("elections", "ValidationNumber", "setValidationnumber", HttpContext.Session.GetString("PseudonimoKey"), HttpContext.Session.GetString("_RandomVN").ToString());
                    ValidationNumber number_2 = ValidationNumber.parseValidationNumberFromJSON(queryOutVN_2.Content);
                    
                    if (!string.IsNullOrEmpty(number_2.ErrorDescription))
                    {
                        _logger.LogInformation(UteLogNam + UtenteGenerico() + UteBloCha + number_2.ErrorDescription);
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                }

                ViewBag.SitoAperto = 1;
                ViewBag.Exit = true;

                /*ALIMENTARE CON ALTRE COSE NECESSARIE************************/

                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "VAIALVOTO action: redirecting to View");
				ViewData["URL"]= Startup.StaticConfig.GetSection("AppB_URL").Value;
                ViewBag.Exit = true;
                ViewData["Title"] = "Vai al voto";
                return View();

            }
            catch (Exception e)
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "VAIALVOTO issue: " + e.Message);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }
        }

        public IActionResult RicevutadiVoto()
        {
            ViewBag.Exit = true;
            ViewData["Title"] = "Ricevuta di voto";
            //_logger.LogInformation("UTENTE: accesso alla pagina RicevutadiVoto");
            _logger.LogInformation(UteLogNam + UtenteGenerico() + UteLogDo + " RICEVUTADIVOTO");

            Utils.LogTrace(Request.Headers["X-Forwarded-For"], "RICEVUTADIVOTO info: incoming call to RICEVUTADIVOTO");

            foreach (var header in Request.Headers)
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "RICEVUTADIVOTO header key: " + header.Key.ToString() + " -> " + header.Value.ToString());
            }

            //LOG HEADER DATA            
            try
                {
                // GET & CHECK IDCS1 DATA
                    string codiceElettore = Request.Headers["codice_elettore"];
                string dataNascita = Request.Headers["data_nascita"];

                if (String.IsNullOrEmpty(codiceElettore))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "RICEVUTADIVOTO issue: empty codiceElettore");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
                if (String.IsNullOrEmpty(dataNascita))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "RICEVUTADIVOTO issue: empty dateOfBirth");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                return View();
            }
            catch (Exception e)
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "RICEVUTADIVOTO issue: " + e.Message);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }
            
        }

        public IActionResult InformativaDatiPersonali()
        {
            ViewBag.Exit = true;
            return View();
        }
        public IActionResult StampaRicevutaDiVoto()
        {
            Utils.LogTrace(Request.Headers["X-Forwarded-For"], "STAMPARICEVUTADIVOTO info: incoming call to STAMPARICEVUTADIVOTO");

            foreach (var header in Request.Headers)
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "STAMPARICEVUTADIVOTO header key: " + header.Key.ToString() + " -> " + header.Value.ToString());
            }

            try
            {
                string codiceElettore = Request.Headers["codice_elettore"];
                string dataNascita = Request.Headers["data_nascita"];

                if (String.IsNullOrEmpty(codiceElettore))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "STAMPARICEVUTADIVOTO issue: empty codiceElettore");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
                if (String.IsNullOrEmpty(dataNascita))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "STAMPARICEVUTADIVOTO issue: empty dateOfBirth");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                // INIZIO STAMPA RICEVUTA DI VOTO

                _logger.LogInformation(UteLogNam + UtenteGenerico() + UteLogDo + " StampaRicevutaDiVoto");

                //passaggio dati elettore in viewdata per visualizzarli nella view
                string qualeGenere = HttpContext.Session.GetString("_SessoUtente");
                string sesso = variousServices.SignoreSignora(qualeGenere);
                ViewData["sesso"] = sesso;

                string soggettoricevuta = HttpContext.Session.GetString(SessionKeyNomeUtente) + " " + HttpContext.Session.GetString(SessionKeyCognomeUtente);
                ViewData["soggetto"] = soggettoricevuta;
                ViewData["luogoNascita"] = HttpContext.Session.GetString(SessionKeyLuogoNascitaUtente);
                ViewData["dataNascita"] = HttpContext.Session.GetString(SessionKeyDataNascitaUtente);
                ViewData["COMITES"] = HttpContext.Session.GetString(SessionKeyComitesUtente);

                string absoluteurl = Startup.StaticConfig.GetSection("AppA_URL").Value;
                string BSUri = "~/lib/bootstrap/css/bootstrap.min.css";

                // Fuso orario locale           
                DateTime dateNow = DateTime.Now;
                string dataItalia = dateNow.ToString("d");
                string oraItalia = dateNow.ToString("T");

                // UTC
                DateTime utcNow = DateTime.UtcNow;
                string dataStampaUtc = utcNow.ToString();
                // fine area dati

                // tentativo di usare lo stream
                var stream = env.WebRootFileProvider.GetFileInfo("img/LogoRepubblica.png").CreateReadStream();
                Image image = Image.FromStream(stream);
                Graphics graphics = Graphics.FromImage(image);

                // Stringa generazione pagina          
                string viewHtml = "<!DOCTYPE html>";
                //viewHtml += "<html lang = \"it\" >";
                //viewHtml += "<meta charset=\"UTF-8\" />";
                viewHtml += "<meta http-equiv=\"Content-Type\" content=\"text/html; charset = utf-8\"/>";
                viewHtml += "<head>";
                viewHtml += "<link rel=\"stylesheet\" href=\"" + absoluteurl + "/css/shared.css\" />";
                viewHtml += "<link rel=\"stylesheet\" href=\"" + absoluteurl + "/css/main.css\" />";
                viewHtml += "</head>";
                viewHtml += "<body>";
                viewHtml += "<div class=\"pdf centro\">";
                viewHtml += "<div id=\"Grid\" class=\"scatola centro\">";
                viewHtml += "<div class=\"centro mt-3\" >";

                viewHtml += "<img class=\"logoRep centro\" src=\"" + absoluteurl + "/img/LogoRepubblica.png \" width=\"100px;\" height=\"100px;\" />";
                viewHtml += "</div>";
                viewHtml += "<div class=\"centro mt-5 mb-5 \">";
                viewHtml += "<h1 class=\"text-uppercase ufficiale\" style=\"text-align: center;\">RICEVUTA ELETTORALE</h1>";
                viewHtml += "<h2 class=\"text-uppercase ufficiale\" style=\"text-align: center;\">ELEZIONI DEI COMITATI DEGLI ITALIANI ALL’ESTERO</h2>";
                viewHtml += "</div>";
                viewHtml += "<div class=\"ufficiale centro mt-4\">";
                viewHtml += "<h3>si attesta che</h4>";
                viewHtml += "</div>";
                viewHtml += "<div class=\"ufficiale centro mt-3\">";
                viewHtml += "<h4>" + " " + @ViewData["sesso"] + " " + @ViewData["soggetto"] + "</h5>";
                viewHtml += "<h4> nato/a a " + @ViewData["luogoNascita"] + " il " + @ViewData["dataNascita"] + "</h5>";
                viewHtml += "</div>";
                viewHtml += "<div class=\"ufficiale centro mt-3 mb-4\">";
                viewHtml += "<h4>ha espresso il suo voto elettronico sulla piattaforma " + _localizer["Sito-Nome"] + " per il COMITES di " + @ViewData["COMITES"] + ".</h5>";
                viewHtml += "</div>";
                viewHtml += "</div>";
                viewHtml += "</div>";
                viewHtml += "</body>";
                viewHtml += "</html>";

                Byte[] res = null;

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (MemoryStream ms = new MemoryStream())
                {
                    string testStylesheet = BSUri;
                    var cssData = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.ParseStyleSheet(testStylesheet, true);
                    var pdf = PdfGenerator.GeneratePdf(viewHtml, PdfSharp.PageSize.A4, 20, cssData);
                    pdf.Save(ms);
                    res = ms.ToArray();
                }
                System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "Portale " + _localizer["Sito-Nome"] + " Ricevuta di voto emessa il " + dataItalia + " alle " + oraItalia + ".pdf",
                    Inline = true
                };
                Response.Headers.Add("Content-Disposition", cd.ToString());
                Response.Headers.Add("X-Content-Type-Options", "nosniff");
                //return new FileContentResult(res, "application/pdf");
                return new FileContentResult(res, "application/octet-stream");
            }
            catch (Exception e)
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "STAMPARICEVUTADIVOTO issue: " + e.Message);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }            
        }
    }
}
