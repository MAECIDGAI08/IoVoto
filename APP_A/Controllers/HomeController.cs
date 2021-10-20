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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Utility;
using System.Globalization;
using static AppA.Models.SharedKeys;
// IP tracking

namespace AppA.Controllers
{
    public class HomeController : Controller
    {
        #region Campi privati e session keys
        public const string sessionUser = "";

        // chiave utility
        public const string CodiceVaiAlVotoClient = "";

        // info tempo votazione
        public const string TempoSessione = "30:00";
       
        // per controllo se votazione è aperta o no
        public bool control;

        // per controllo se sito è aperto  o no
        public int controlloSitoAperto;

        #endregion

        #region Costruttore

        //variabile interna per accedere alla localizzazione (SharedResources)
        private readonly IStringLocalizer<SharedResource> _localizer;

        //var interna per accedere all'interfaccia ILoggerFactory
        private readonly ILogger<HomeController> _logger;

        // var interna per IHostingEnvironment
        private readonly IHostingEnvironment env;

        public HomeController(IStringLocalizer<SharedResource> localizer, ILogger<HomeController> logger, IHostingEnvironment env)
        {
            _localizer = localizer;
            _logger = logger;
            this.env = env;
        }
        #endregion

        #region Gestione Linguaggio 

        // N.B. è stato rimosso dall'interfaccia ma LO LASCIAMO per sicurezza
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                //scadenza dopo 1 anno
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "/";
            }
            return LocalRedirect(returnUrl);
        }

        #endregion

        #region  metodi di servizio
        public static int numero = 1;
        //restituisce un intero incrementale
        public static int Incremento()
        {
            return numero++;
        }
        //verifica il formato del codice pratica inserito dall'utente
        public bool useRegex(string inputCodice)
        {
            Regex regex = new Regex("^[EI]/[0-9][0-9][0-9][0-9][0-9][0-9][0-9]/[0-9][0-9][0-9][0-9][0-9][0-9]+$");
            var ok = regex.IsMatch(inputCodice);
            return ok;
            //return true;
        }
        
             
        // metodo per tracciare l'utente quando loggato dopo SPID
        public string NomeUtente()
        {
            string nomeOnline = HttpContext.Session.GetString(SessionKeyNomeUtente);
            string cognomeOnline = HttpContext.Session.GetString(SessionKeyCognomeUtente);
            string dataNascitaOnline = HttpContext.Session.GetString(SessionKeyDataNascitaUtente);
            string PseudonimoEle = HttpContext.Session.GetString("PseudonimoKey"); 
            string UtenteOnline = UteLogNam + nomeOnline + " " + cognomeOnline + " | " + dataNascitaOnline + " | " + PseudonimoEle + " ";
            return UtenteOnline; 
        }
        #endregion

        // metodo per costruire la stringa dell'utente che arriva sul portale
        public string UtenteGenerico()
        {
            string UtenteOnline = ProgressivoUtente();
            string IpUtenteOnline = IPUtente();
            string UtenteGenerico = UtenteOnline + " | " + IpUtenteOnline;
            return UtenteGenerico;
        }
        
        // metodi recupero URL da config
        public string GetUrlAppA()
        {
            return Startup.StaticConfig.GetSection("AppA_URL").Value.ToString();
        }

        public string GetUrlAppB()
        {
            return Startup.StaticConfig.GetSection("AppB_URL").Value;
        }

        public string UtenteGenericoSessione()
        {
            string UtenteInSessione = HttpContext.Session.GetString(SessionKeysessionUser) + " | " + HttpContext.Session.GetString(SessionKeyIP);
            return UtenteInSessione;
        }
        
        // recupero controller and action
        public string ControllerAndAction()
        {
            string Cont = ControllerContext.ActionDescriptor.ControllerName;
            string Acti = ControllerContext.ActionDescriptor.ActionName;
            string CandA = "Controller: " + Cont + " Azione: " + Acti;
            return CandA;
        }
        
        
        public int VotazioneAperta(DateTime InizioVotazione, DateTime FineVotazione)
        {

            // calcolo ora locale del votante
            DateTime OraLocaleSito = IngressoUtente();

            // result di default è 1, sito aperto
            int result = 1;
            
            int result3 = DateTime.Compare(OraLocaleSito, InizioVotazione);
            // se sono prima della votazione dà -1
            // se sono in votazione dà 1

            int result4 = DateTime.Compare(FineVotazione, OraLocaleSito);
            // prima o nella votazione dà 1
            // dopo la votazione dà -1

            if (result3 == -1)
            {
                result = 0; // riceve e traccia 0, sitoAperto è 0 (schermata votazione da iniziare)
            }
            if (result4 == -1)
            {
                result = -1; // riceve e traccia -1, sitoAperto è 2 (schermata tempo finito)
            }

            return result;
        }

        DateTime OraIngresso = IngressoUtente();

        #region Action Views
        
        public IActionResult Index()
        {

            /*string JWT = "{sub=E/1234567/123456, aud=oracle, nbf=1633183261, iss=oracle, exp=1633183441, iat=1633183321, jti=41b3e89e-697b-46db-8940-7f655b721bff}";
            string publicKeyString = Startup.StaticConfig.GetSection("DSA").Value;
            string encryptedJWT = Utility.Utils.Encrypt(JWT, publicKeyString);
            string decryptedJWT = Utility.Utils.Decrypt(encryptedJWT, publicKeyString);*/
            
            // FASE A:
            // controllo se si è in arco temporale di votazione o no
            var inizioVotazione = Startup.StaticConfig.GetSection("InizioVotazione").Value;
            var fineVotazione = Startup.StaticConfig.GetSection("FineVotazione").Value;

            ViewData["FineVotazione"] = fineVotazione;
            ViewData["TempoSessione"] = TempoSessione;

            // recupero le url degli applicativi
            HttpContext.Session.SetString("_UrlAppA", GetUrlAppA());
            HttpContext.Session.SetString("_UrlAppB", GetUrlAppB());

            Console.WriteLine(HttpContext.Session.GetString("_UrlAppA"));
      
            // invoco il metodo di controllo passandogli data di inizio e fine
            //VotazioneAperta(StartVotazione, EndVotazione);

            // CHECK Fuso orario scaduto
            // 0 prima dell'inizio della votazione
            // 1 in tempo di votazione
            // -1 fuori dal termine di votazione

            //RECUPERO DATI PER PULSANTI SPID
            ViewData["DisableSPID"] = Startup.StaticConfig.GetSection("SPID").GetSection("DisableSPID").Value;
            ViewData["outURL"]      = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("Out").Value;
            ViewData["inURL"]       = HttpUtility.UrlEncode(Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("In").Value);
            ViewData["arubaid"]     = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("arubaid").Value;
            ViewData["infocertid"]  = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("infocertid").Value;
            ViewData["intesaid"]    = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("intesaid").Value;
            ViewData["lepidaid"]    = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("lepidaid").Value;
            ViewData["namirialid"]  = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("namirialid").Value;
            ViewData["posteid"]     = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("posteid").Value;
            ViewData["spiditalia"]  = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("spiditalia").Value;
            ViewData["sielteid"]    = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("sielteid").Value;
            ViewData["timid"]       = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("timid").Value;
            ViewData["Environment"] = Startup.StaticConfig.GetSection("Environment").Value;
            //


            if (inizioVotazione == "" || fineVotazione == "")
            {
                return RedirectToAction("Errore");
            }
                                         
            if (VotazioneAperta(DateTime.Parse(inizioVotazione), DateTime.Parse(fineVotazione)) == 0)
            {
                ViewBag.Exit = false;
                ViewBag.SitoAperto = 0;
                ViewData["InizioVotazione"] = inizioVotazione;
                return View("Index");
            }
            // else finale per caso 2
            else if (VotazioneAperta(DateTime.Parse(inizioVotazione), DateTime.Parse(fineVotazione)) == -1)
            {
                ViewBag.Exit = false;
                ViewBag.SitoAperto = 2;
                ViewData["FineVotazione"] = fineVotazione;
                return View("Index");
            }
                      
            // scrittura logger locale per utente anonimo
            _logger.LogInformation(UteLogNam + UtenteGenerico() + UteLogDo + " Home-Page");          
            ViewBag.SitoAperto = 1;
            ViewBag.Exit = false;            
            return View();
        }
        public IActionResult ZmFrZWhvbWVwYWdl()
        {
            var fineVotazione = Startup.StaticConfig.GetSection("FineVotazione").Value;
            ViewData["FineVotazione"] = fineVotazione;
            ViewData["TempoSessione"] = TempoSessione;


            //RECUPERO DATI PER PULSANTI SPID
            ViewData["DisableSPID"] = 0;
            ViewData["outURL"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("Out").Value;
            ViewData["inURL"] = HttpUtility.UrlEncode(Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("In").Value);
            ViewData["arubaid"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("arubaid").Value;
            ViewData["infocertid"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("infocertid").Value;
            ViewData["intesaid"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("intesaid").Value;
            ViewData["lepidaid"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("lepidaid").Value;
            ViewData["namirialid"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("namirialid").Value;
            ViewData["posteid"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("posteid").Value;
            ViewData["spiditalia"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("spiditalia").Value;
            ViewData["sielteid"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("sielteid").Value;
            ViewData["timid"] = Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("timid").Value;
            ViewData["Environment"] = Startup.StaticConfig.GetSection("Environment").Value;

            ViewBag.SitoAperto = 1;
            ViewBag.Exit = false;
            // check sessione attiva
            return View();
        }
        
        public IActionResult CodiceElettore(String t = null)
        {
            Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE info: GET " + t);

            //INPUT CHECK
            if (String.IsNullOrEmpty(t))
            {
                Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: empty t");
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }

            try
            {
                //DECRYPT & PARSE INPUT
                String decryptedQS = Utility.Utils.Decrypt(t, Startup.StaticConfig.GetSection("DSA").Value);
                string[] QSparts = decryptedQS.Split('|');
                if (QSparts.Length != 3)
                {
                    Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: wrong t" + decryptedQS);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
                String token = QSparts[0];
                String cf = QSparts[1];
                String dt = QSparts[2];

                if (String.IsNullOrEmpty(token))
                {
                    Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: empty token");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                if (String.IsNullOrEmpty(cf) || !Utility.Utils.ValidateField(cf, "req_cf"))
                {
                    Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: wrong cf " + cf);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                if (String.IsNullOrEmpty(dt) || !Utility.Utils.ValidateField(dt, "req_date"))
                {
                    Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: wrong dt " + dt);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                //TOKEN BC CHECK
                var queryInToken = Service.APIService.InvokeChainCode("internal", "Tokens", "getTokensById", token);
                Token bcToken = Token.parseTokenFromJSON(queryInToken.Content);
                if (!string.IsNullOrEmpty(bcToken.ErrorDescription))
                {
                    Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: token not found for " + cf);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
                if (bcToken.CodiceFiscaleVal != cf)
                {
                    Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: no token cf match " + cf + " " + token);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                //CHECK TOKEN EXPIRATION
                var diffInSeconds = (DateTime.Now - DateTime.ParseExact(bcToken.DateVal, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)).TotalSeconds;
                if (diffInSeconds > Int32.Parse(Startup.StaticConfig.GetSection("BlockChain").GetSection("TokenExpire").Value))
                {
                    Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: token expired for " + cf + ": " + bcToken.TokenVal + " " + bcToken.DateVal);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                // SET SESSIONE
                //User user = new User() { UserIP = HttpContext.Connection.RemoteIpAddress.ToString(), UserName = CreaGUID() };

                //SET VIEWDATA
                ViewData["bc-tk"] = token;
                ViewData["bc-cf"] = cf;
                ViewData["bc-dt"] = dt;
                // fine emilio

                // passiamo il comites alla pagina
                ViewData["Comites"] = HttpContext.Session.GetString(SessionKeyComitesUtente);

                ViewData["controlloCodicePratica"] = true;
                // verifica se l'intero restituito dal metodo è maggiore di 1(è uno quando l'utente atterra la prima volta nella vista)
                if (HomeController.Incremento() > 1)
                {
                    // setto la proprietà a false => utilizzata come condizione nella vista
                    ViewData["controlloCodicePratica"] = false;
                }
                
                ViewData["Title"] = "Inserimento Codice Elettore";
                ViewBag.Exit = true;
                
                return View();
            }
            catch (Exception e)
            {
                Utility.Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "COIDCEELETTORE issue: " + e.Message);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }
        }
        
        [HttpPost]  //riceve il codice elettore dal form
        public async Task<IActionResult> GetInpuFromCodiceElettoreClient(IFormCollection form)
        {
            return null;           
        }
        
        public IActionResult RicevutadiVoto()
        {
            ViewBag.Exit = true;
            ViewData["Title"] = "Ricevuta del voto";
            //_logger.LogInformation("UTENTE: accesso alla pagina RicevutadiVoto");
            _logger.LogInformation(UteLogNam + NomeUtente() + UtenteGenerico() + UteLogDo + " VaiAlVoto");
            return SessionOut();
        }
        // per la vista del tempo scaduto
        public IActionResult TimeOver()
        {
            ViewBag.Exit = false;
            ViewData["Title"] = "Tempo per la votazione scaduto";
            ViewData["UrlLogout"] = Startup.StaticConfig.GetSection("UrlLogout");
            //_logger.LogInformation("UTENTE: accesso alla pagina RicevutadiVoto");
            _logger.LogInformation(UteLogNam + NomeUtente() + UtenteGenerico() + UteLogDo + " TimeOver");
            return SessionOut();
        }
        public IActionResult StampaRicevutaDiVoto()
        {
            _logger.LogInformation(UteLogNam + NomeUtente() + UteLogDo + " StampaRicevutaDiVoto");
            
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
            string BSUri = "https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css/";

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
            viewHtml += "<html lang = \"it\" >";
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
            return new FileContentResult(res, "application/pdf");
        }
        public IActionResult Faq()
        {
            ViewBag.Exit = true;
            ViewData["Title"] = "F.A.Q.";
            _logger.LogInformation(UteLogNam + UtenteGenerico() + UteLogDo + " F.A.Q");
            //_logger.LogInformation("UTENTE: accesso alla pagina F.A.Q");
            return SessionOut();
        }
        public IActionResult Logout()
        {

            _logger.LogInformation(UteLogNam + NomeUtente() + UtenteGenerico() +  " | effettua la procedura di Logout");
            //_logger.LogInformation("UTENTE: effettua la procedura di Logout");
            Dispose();
            ViewData["Message"] = "Sessione di voto interrotta.";
            ViewData["UrlLogout"] = Startup.StaticConfig.GetSection("UrlLogout").Value;
            ViewBag.Exit = false;
            return View();
        }
        public IActionResult CookiePolicy()
        {
            ViewBag.Exit = true;
            ViewData["Title"] = "Informativa Cookies";
            //_logger.LogInformation("UTENTE: Accesso alla pagina CookiePolicy");
            _logger.LogInformation(UteLogNam + NomeUtente() + UtenteGenerico() + UteLogDo + " CookiePolicy");
            return SessionOut();
        }
        public IActionResult NoteLegali()
        {
            ViewBag.Exit = true;
            ViewData["Title"] = "Note Legali";

            _logger.LogInformation(UteLogNam + NomeUtente() + UtenteGenerico() + UteLogDo + " NoteLegali");
            //_logger.LogInformation("UTENTE: Accesso alla pagina NoteLegali");
            return SessionOut();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewData["UrlLogout"] = Startup.StaticConfig.GetSection("UrlLogout").Value.ToString();
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Sessione()
        {
            ViewBag.Exit = false;
            Dispose();
            return View();
        }       
        // action per verificare che l'utente sia in sessione
        public IActionResult SessionOut()
        {
            var user = JsonConvert.DeserializeObject<User>(sessionUser);
            _logger.LogInformation(UteLogNam + UtenteGenerico() + " | Check if session UP");
            ViewData["UrlLogout"] = Startup.StaticConfig.GetSection("UrlLogout").Value;
            if (HttpContext.Session.GetString("SessionUser") == null)
            { 
                return LocalRedirect("/Home/Sessione");
            }
            //var user = JsonConvert.DeserializeObject<User>(sessionUser);
            _logger.LogInformation(UteLogNam + UtenteGenerico() + " | session is UP");

            return View();
        }
        public IActionResult Errore()
        {
            ViewData["UrlLogout"] = Startup.StaticConfig.GetSection("UrlLogout").Value;
            ViewBag.Exit = false;
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + ControllerAndAction());            
            return View();           
        }
        #endregion
    }
}
