using AppB.Models;
using AppB.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using static AppB.Service.Utils;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Utility;

namespace AppB.Controllers
{
    public class HomeController : Controller
    {
        #region Parametri
        
        public bool errorPage;

        //  Chiave di sessione per Utente generato random
        const string SessionKeysessionUser = "_sessionUser";

        // Chiave di sessione per lo Pseudonimo
        const string SessionKeyPseudonimo = "_Pseudonimo";

        // chiave di sessione per l'ip Utente
        const string SessionKeyIP = "_IpUser";

        // chiave di sessione per il Validation Number
        //const string SessionKeyRandomVN = "_RandomVN";

        // chiave di sessione per Comites
        const string SessionKeyComites = "_Comites";

        // chiave di sessione per Tracking tempo votazione
        const string SessionKeyOraAttuale = "_Adesso";

        // chiave di sessione per la timezone del comites
        const string SessionKeyTimeZoneComites = "_TimezoneComites";
       
        const string SessionKeyUrlAppA = "_UrlAppA";

        //oggetto random per generare numeri casuali
        Random rnd = new Random();

        // info tempo votazione
        const string TempoSessione = "30:00";
        
        // recupero dati da appsetting JSON
        private readonly Parameters _mySettings;
               
        // mappa statica 

        //public void setupChannelsAssociationMap()
        static void setupChannelsAssociationMap()
        {
            if (chainCodeChannelsAssociationMap.Count == 0)
            { 
            chainCodeChannelsAssociationMap.Add("Lista", "lists");
            chainCodeChannelsAssociationMap.Add("Candidato", "lists");
            chainCodeChannelsAssociationMap.Add("Elettori", "voters");
            chainCodeChannelsAssociationMap.Add("VotoElettore", "elections");
            chainCodeChannelsAssociationMap.Add("ValidationNumber", "elections");
            chainCodeChannelsAssociationMap.Add("Scrutinatore", "internal");
            chainCodeChannelsAssociationMap.Add("Tokens", "internal");
            }

        }

        // dizionario per il recupero dei dati
        static Dictionary<String, String> chainCodeChannelsAssociationMap = new Dictionary<string, string>();



        public string getChainCodeURL(string chainCodeMethodName)
        {
            string channel = chainCodeChannelsAssociationMap[chainCodeMethodName];
            //string blockChainUrl = IDREG + channel + "/transactions";
            string blockChainUrl = GetEndPoint() + channel + "/transactions";
            // GetEndPoint()
            return blockChainUrl;
        }


        

        // Chiave Utility per generazione falsi errori
        int Testing = 0;
        // 0 Default, no testing
        // 1 Errore Pseudonimo
        // 2 Errore VN
        // 3 Errore Comites

        #endregion Parametri

        #region  Costruttore

        // variabile interna per accedere alla localizzazione (SharedResources)
        private readonly IStringLocalizer<SharedResource> _localizer;

        // variabile interna per accedere al logger
        private readonly ILogger<HomeController> _logger;

        // var interna per IHostingEnvironment
        private readonly IHostingEnvironment env;
                
        private IHttpContextAccessor _accessor;

        public HomeController(IStringLocalizer<SharedResource> localizer, ILogger<HomeController> logger, IHostingEnvironment env, IHttpContextAccessor accessor, IOptions<Parameters> mySettingsOptions)
        {
            _localizer = localizer;
            _logger = logger;
            _accessor = accessor;
            this.env = env;
            _mySettings = mySettingsOptions.Value;
        }
        
        public IEnumerable<string> Get()
        {
            var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return new string[] { ip, "value2" };
        }

        #endregion

        #region metodi di servizio

        // registrazione path all'interno di una variabile di sessione
        private void RecordInSession(string action)
        {
            var paths = HttpContext.Session.GetString("actions") ?? string.Empty;
            HttpContext.Session.SetString("actions", paths + ";" + action);
            Get();
        }

        // recupero controller and action
        public string ControllerAndAction()
        {
            string Cont = ControllerContext.ActionDescriptor.ControllerName;
            string Acti = ControllerContext.ActionDescriptor.ActionName;
            string CandA = "Controller: " + Cont + " Azione: " + Acti;
            return CandA;
        }

        // recupero valore registrato del path
        public string DoveStaUtente()
        {
            string where = HttpContext.Session.GetString("actions");
            return where;
        }

        // action che recupera il pdf da una url locale in base al comites della vista 
        public IActionResult GetPdfCOMITES([FromQuery] string filename)
        {
            String filePath = filename;
            Response.Headers.Add("content-disposition", string.Format("inline;FileName=\"{0}\"", filePath));
            return File(filePath, "application/pdf");
        }
     
        // metodo che gestisce la lingua (non usato)
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

        // metodo per costruire la stringa dell'utente che arriva sul portale
        public string UtenteGenerico()
        {
            DateTime IngUte = IngressoUtente();
            HttpContext.Session.SetString("_Adesso", IngUte.ToString());

            string UtenteOnline = ProgressivoUtente();
            HttpContext.Session.SetString("_sessionUser", UtenteOnline);

            string IpUtenteOnline = IPUtente();
            HttpContext.Session.SetString("_IpUser", IpUtenteOnline);

            string UtenteGenerico = UtenteOnline + " | " + IpUtenteOnline;
            return UtenteGenerico;
        }

        // metodo per controllo utente messo in sessione da UtenteGenerico()
        public string UtenteGenericoSessione()
        {
            string UtenteInSessione = HttpContext.Session.GetString(SessionKeysessionUser) + " | " + HttpContext.Session.GetString(SessionKeyIP);
            return UtenteInSessione;
        }

        // passo data e ora di ingresso per il metodo di calcolo se l'utente può ancora votare
        DateTime OraIngresso = IngressoUtente();

        public bool control;

        public object PuoVotare(DateTime OraIngresso, int offsetdaBC, DateTime FineVotazione)
        {
            // setto variabile di check finale           

            // calcolo ora locale del votante
            DateTime OraLocaleVotante = (OraIngresso.AddHours(offsetdaBC));

            // Calcolo l'intervallo tra le due date
            TimeSpan interval = OraLocaleVotante - FineVotazione;

            // assegno a stringa interval
            String IntervalHR = interval.ToString();

            string subIHR = IntervalHR.Substring(0, 1);

            // se subIHR è uguale a "-", ha ancora tempo per votare
            if (subIHR == "-")
            {
                return control = false;
            }
            return control = true;
        }
        #endregion

        #region Actions che restituiscono le viste
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]

        [HttpGet]
        public string GetEndPoint()
        {
            return _mySettings.EVOTE;
        }

        [HttpGet]
        public string GetUrlAppA()
        {
            return _mySettings.URLAppA;
        }

        [HttpGet]
        public string GetUsername()
        {
            return _mySettings.UserAppB;
        }

        [HttpGet]
        public string GetDataInizioVotazione()
        {
            return _mySettings.InizioVotazione;
        }

        [HttpGet]
        public string GetDataFineVotazione()
        {
            return _mySettings.FineVotazione;
        }

        [HttpGet]
        public string GetUrlLogout()
        {
            return _mySettings.UrlLogout;
        }


        public IActionResult Index()
        {
		Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "CABINAELETTORALE info: incoming call to Index");
            
            //LOG HEADER DATA
            foreach (var header in Request.Headers)
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "CABINAELETTORALE header key: " + header.Key.ToString() + " -> " + header.Value.ToString());
            }
            string IDSC2_pseudonimo = "";
            try
            {
                //GET & CHECK IDCS2 DATA
                IDSC2_pseudonimo = Request.Headers["pseudonimo"];

                if (String.IsNullOrEmpty(IDSC2_pseudonimo))
                {
                    Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "CABINAELETTORALE issue: empty pseudonimo");
                    return RedirectToAction("Errore");
                }



            }
            catch (Exception e)
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "CABINAELETTORALE issue: " + e.Message);
                return RedirectToAction("Errore");
            }
            Parameters parameter = new Parameters();
            HttpContext.Session.SetString(SessionKeyUrlAppA, GetUrlAppA());
            ViewData["Title"] = "Cabina Elettorale";
            ViewBag.Exit = true;

            User user = new User() { UserIP = Request.Headers["X-Forwarded-For"], UserName = CreaGUID() };

            //HttpContext.Session.SetString(SessionKeyControlloCandidatiNull, "errore");
            //string TermineVotazione = DateTime.Now.ToString("03/12/2021 00:00");


            DateTime nuova = Convert.ToDateTime(GetDataFineVotazione());
            ViewData["FineVotazione"] = GetDataFineVotazione();
            ViewData["TempoSessione"] = TempoSessione;
            // registro dove sono in sessione
            // [
            // inizializzo UtenteGenerico();
            UtenteGenerico();
            RecordInSession(ControllerAndAction());
            // ]
            // set parametri in sessione
            HttpContext.Session.SetString("SessionUser", JsonConvert.SerializeObject(user.UserName));
            HttpContext.Session.SetString("SessionKeyIp", JsonConvert.SerializeObject(user.UserIP));
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            const string SessionKeyRandomVN = "_VN";
            
            setupChannelsAssociationMap();
            // chiamata al chaincode VN che controlla lo pseudonimo e restituisce vn e comites associati ad esso

            
            // query nuova
            var queryValidationNumber = APIService.InvokeChaincodeParameters("ValidationNumber", "getValidationnumberById", getChainCodeURL("ValidationNumber"), GetUsername(), IDSC2_pseudonimo);

            // logging chiamata BC
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + queryValidationNumber.StatusCode);
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + queryValidationNumber.Content);

            // contiene la risposta del chaincode parserizzata => pseudonimo, vn, comites, used(di default a false fin tanto che l'elettore non ha votato)
            ValidationNumber number = ValidationNumber.parseValidationNumberFromJSON(queryValidationNumber.Content);
            //verifica errori da chaincode
            if (!string.IsNullOrEmpty(number.ErrorDescription) || string.IsNullOrEmpty(number.ValidationNum))
            {
                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + number.ErrorDescription);
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "CABINAELETTORALE issue: no VN for " + UteBloCha);
                return RedirectToAction("Errore");
            }

            // start check timezone

            // query nuova
            var queryFusoOrario = APIService.InvokeChaincodeParameters("Lista", "getTimeZone", getChainCodeURL("Lista"), GetUsername(), number.Comites);

            // logging chiamata BC
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + queryFusoOrario.StatusCode);
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + queryFusoOrario.Content);

            // nuovo oggetto
            Lista timezone = Lista.parseTimeZoneFromJSON(queryFusoOrario.Content);

            if (!string.IsNullOrEmpty(timezone.ErrorDescription))
            {
                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + timezone.ErrorDescription);
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "CABINAELETTORALE issue: no TimeZone for " + UteBloCha);
                return RedirectToAction("Errore");
            }
            // assegno valore a int per test sul metodo
            int offset = timezone.TimeZone;
            string inizioVotazione = GetDataInizioVotazione();
            string fineVotazione = GetDataFineVotazione();

            // check tempo votazione metodo per il calcolo della differenza di orario
            PuoVotare(OraIngresso, offset, DateTime.Parse(fineVotazione));

            // fine check timezone

            // CHECK Fuso orario scaduto
            if (control == true)
            {
                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteCant + UteComites);
                ViewData["Title"] = "Tempo per la votazione scaduto";
                ViewBag.Exit = false;
                Dispose();
                return View("TimeOver");
            }

            // imposto la sessione con il vn, lo pseudonimo e il comites ricevuti dal chaincode ValidationNumber

            // Pseudonimo & check
            HttpContext.Session.SetString(SessionKeyPseudonimo, IDSC2_pseudonimo);

            // TEST pseudonimo non in BC
            if (Testing == 1)
            {
                HttpContext.Session.SetString(SessionKeyPseudonimo, "");
            }
            // verifica pseudonimo in sessione
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyPseudonimo)))
            {
                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteCant + UtePseudonimo);
                ViewData["Title"] = "Accesso Negato";
                //Dispose();
                return View("NonAutorizzato");
            }

            // VN & check
            HttpContext.Session.SetString(SessionKeyRandomVN, number.ValidationNum);

            // HttpContext.Session.SetString(SessionKeyRandomVN, "12345");

            // TEST VN non in BC
            if (Testing == 2)
            {
                HttpContext.Session.SetString(SessionKeyRandomVN, "");
            }

            // verifica vn in sessione
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyRandomVN)))
            {
                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteCant + UteVN);
                ViewData["Title"] = "Accesso Negato";
                Dispose();
                return View("NonAutorizzato");
            }

            // Comites & check
            HttpContext.Session.SetString(SessionKeyComites, number.Comites);

            // TEST VN non in BC
            if (Testing == 3)
            {
                HttpContext.Session.SetString(SessionKeyComites, "");
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyComites)))
            {
                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteCant + UteComites);
                ViewData["Title"] = "Accesso Negato";
                Dispose();
                return View("NonAutorizzato");
            }

            // chiamata al chaincode Lista che restituisce le liste associate al comites
            var queryInListe = APIService.InvokeChaincodeParameters("Lista", "getListe", getChainCodeURL("Lista"), GetUsername(), number.Comites);

            //var queryInListe = APIService.InvokeChaincode("Lista", "getListe", number.Comites);

            // logging chiamata BC
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + queryInListe.StatusCode);
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + queryInListe.Content);

            //contiene la risposta del chaincode parserizzata con i dati della lista ricevuta
            List<Lista> resultList = Lista.parseListaFromJSON(queryInListe.Content);
            var listaComites = timezone.ErrorDescription;
            if (!string.IsNullOrEmpty(listaComites))
            {
                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + "NO LISTE");
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "CABINAELETTORALE issue: no Liste");
                return RedirectToAction("Errore");
            }
            //memorizza le liste passate alla vista
            ViewData["lista"] = resultList;
            ViewData["comitesName"] = number.Comites;
            //ViewData["dimensione"] = dimensione;
            Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "CABINAELETTORALE action: redirecting to View");
            return View();
        }
        
        public IActionResult GetListaCandidati(string lista)//riceve la lista dalla chiamata ajax
        {

            //chiamata al chaincode Candidato che restituisce i candidati associati alla lista scelta
            //var queryInListe = APIService.InvokeChaincode("Candidato", "getCandidati", lista);
            // test
            // chiamata al chaincode Lista che restituisce le liste associate al comites
            //setupChannelsAssociationMap();
            var queryInCandidati = APIService.InvokeChaincodeParameters("Candidato", "getCandidati", getChainCodeURL("Candidato"), GetUsername(), lista);

            // logging chiamata BC
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + queryInCandidati.StatusCode);
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + queryInCandidati.Content);

            //memorizza il json parserizzato contenente i candidati
            List<Candidato> resultList = Candidato.parseCandidatiFromJSON(queryInCandidati.Content);
            if (resultList.Count == 1)
            {
                string htmlResult2 = "<script>window.location = 'https://localhost:44321/Home/Errore'</script>";

                return new ContentResult
                {
                    Content = htmlResult2
                };
            }
            else
            {
                string htmlResult = "";
                //cicla l'oggetto resultList e genera l'html con i dati dei candidati
                foreach (var item in resultList)
                {
                    htmlResult += "<tr><td class='toremove text-center'><div class='custom-control custom-checkbox text-center'>" +
                    "<input type='checkbox' name='candidato_" + item.UniqueID + "' value='" + item.UniqueID + "' id='inpCandidato" + item.UniqueID + "' onclick='ControlloValoreCandidati()' /></div></td>" +
                    "<td><label id='lblcandidato" + item.UniqueID + "''>" + item.nomeCandidato + " " + item.CognomeCandidato + "</label></td> <td>" + item.DataNascitaCandidato
                    + "</td><td>" + item.LuogoNascitaCandidato + "</td></tr>";

                }

                // restituisce l'html alla vista
                return new ContentResult
                {
                    Content = htmlResult
                };
            }
        }
        public IActionResult ConfermaVoto()
        {
            // TODO mandare i dati del voto ricevuti dal submit al metodo API che scrive i dati in OUTPUT in questo caso.
            //      
            //ViewData["comitesName"];
            ViewData["Title"] = "Conferma del Voto";
            // gestione pulsante esci in header
            ViewBag.Exit = true;
            RecordInSession(ControllerAndAction());
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            return SessionOut();
        }
        // per la vista del tempo scaduto
        public IActionResult TimeOver()
        {
            ViewData["Title"] = "Tempo per la votazione scaduto";
            ViewData["UrlLogOff"] = GetUrlLogout();
            // gestione pulsante esci in header
            ViewBag.Exit = true;
            RecordInSession(ControllerAndAction());
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            return SessionOut();
        }
        
        // solo per routing della vista parziale
        public IActionResult SchedaBianca()
        {
            return SessionOut();
        }
        public IActionResult LogoutVotato()
        {
            ViewData["UrlLogOff"] = GetUrlLogout();
            
            ViewData["Title"] = "Fine votazione";
            // gestione pulsante esci in header
            ViewBag.Exit = false;
            RecordInSession(ControllerAndAction());
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            return SessionOut();
        }
        public IActionResult Logout()
        {
            ViewData["Title"] = "Uscita";
            ViewData["UrlLogOff"] = GetUrlLogout();
            // gestione pulsante esci in header
            ViewBag.Exit = false;
            RecordInSession(ControllerAndAction());
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            ViewData["Message"] = "Sessione di voto interrotta.";
            return SessionOut();
        }

        public IActionResult Errore()
        {
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + ControllerAndAction());
            ViewBag.Exit = false;
            //prende il nome dell'action
            //ViewBag.VistaErrore = Url.ActionContext.ActionDescriptor.DisplayName;
            //Dispose();
            return View();
        }

        #region Action links footer         
        public IActionResult CookiePolicy()
        {
            ViewData["Title"] = "Informativa Cookies";
            // gestione pulsante esci in header
            ViewBag.Exit = false;
            RecordInSession(ControllerAndAction());
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            return SessionOut();
        }

        public IActionResult NoteLegali()
        {
            ViewData["Title"] = "Note Legali";
            // gestione pulsante esci in header
            ViewBag.Exit = false;
            RecordInSession(ControllerAndAction());
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            return SessionOut();
        }

        public IActionResult Faq()
        {
            ViewData["Title"] = "F.A.Q.";
            // gestione pulsante esci in header
            ViewBag.Exit = false;
            RecordInSession(ControllerAndAction());
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            return SessionOut();
        }

        #endregion


        // metodo che gestisce la sessione utente
        //[HttpPost]
        public IActionResult Sessione()
        {
            string urlAppA = GetUrlAppA();
            ViewData["Title"] = "Sessione Scaduta";
            ViewBag.Exit = false;
            //ViewData["UrlAppA"] = urlAppA;
            RecordInSession(ControllerAndAction());
            _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + DoveStaUtente());
            return View();
        }

        public IActionResult SessionOut()
        {
            //var user = JsonConvert.DeserializeObject<User>(_sessionUser);
            _logger.LogInformation(UteLogNam + UtenteGenerico() + " | Check if session UP");
            //if ( (HttpContext.Session.GetString("SessionUser") == null) | (HttpContext.Session.GetString("_IpUser") == null) )
            //    return LocalRedirect("/Home/Sessione");

            if (HttpContext.Session.GetString("SessionUser") == null)
            {
                    ViewBag.Exit = false;
                _logger.LogInformation(UteLogNam + UtenteGenerico() + " | session is DOWN");
                
                return RedirectToAction ("Sessione");
            }
            else
                // var user = JsonConvert.DeserializeObject<User>(sessionUser);
                _logger.LogInformation(UteLogNam + UtenteGenerico() + " | session is UP");

            return View();
        }
        #endregion
        #region  metodo che gestisce i cookie

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        #region metodi POST

        [HttpPost]
        public IActionResult SetVoto2(IFormCollection form) //riceve i dati dal form contenente il flusso di voto

        {
		Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "SETVOTO info: incoming call to Setvoto");

            // LOG HEADER DATA
            foreach (var header in Request.Headers)
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "SETVOTO header key: " + header.Key.ToString() + " -> " + header.Value.ToString());
            }
            string IDSC2_pseudonimo = "";
            try
            {
                //GET & CHECK IDCS2 DATA
                IDSC2_pseudonimo = Request.Headers["pseudonimo"];

                if (String.IsNullOrEmpty(IDSC2_pseudonimo))
                {
                    Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "SETVOTO issue: empty pseudonimo");
                    return RedirectToAction("Errore");
                }

            }
            catch (Exception e)
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "SETVOTO issue: " + e.Message);
                return RedirectToAction("Errore");
            }

            // parametro Comites per le query BC
            var comites = HttpContext.Session.GetString(SessionKeyComites);
            var pseudonimo = HttpContext.Session.GetString(SessionKeyPseudonimo);

            // controllo se oggetto form non è null
            if (form != null)
            {
                int choices = form.Count();
                // switch per la logica principale
                // se è uno è scheda bianca
                // se è due è voto di preferenza sulla lista
                // se è diverso è la votazione normale

                // logica principale
                switch (choices)
                {
                    //caso scheda bianca
                    case 1:

                        // creo la lista per caricare i candidati selezionati coi valori dalla BC
                        List<CandidatoVoto> candidatiSchedaBianca = new List<CandidatoVoto>();

                        // creo la lista per caricare gli id relativi ai candidati salvati
                        List<String> GuidcandidatiSchedaBianca = new List<String>();

                        List<Voto> ListaVotoSchedaBianca = new List<Voto>();

                        _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + "Invio Scheda bianca");
                        CandidatoVoto candSchedaBianca = new CandidatoVoto();
                        Voto InvioListaVoto = new Voto();
                        InvioListaVoto.Lista = "Scheda Bianca";
                        //candSchedaBianaca.lista = "Scheda Bianca";
                        //candSchedaBianaca.Lista = form["Partito"];
                        candSchedaBianca.nomeCandidato = "Scheda Bianca";
                        candSchedaBianca.cognomeCandidato = "Scheda Bianca";
                        candSchedaBianca.dataNascitaCandidato = "00/00/0000";
                        candSchedaBianca.luogoNascitaCandidato = "Scheda Bianca";
                        candSchedaBianca.ordine = 1;
                        candSchedaBianca.comites = comites;
                        candidatiSchedaBianca.Add(candSchedaBianca);
                        GuidcandidatiSchedaBianca.Add(CreaGUID());

                        string[] jsonParamsSchedBianca = {
                                 JsonConvert.SerializeObject(GuidcandidatiSchedaBianca),
                                 InvioListaVoto.Lista,
                                 JsonConvert.SerializeObject(candidatiSchedaBianca),
                                 pseudonimo
                                };

                        var querySetVotoSB = APIService.InvokeChaincodeVote("VotoElettore", "vote", getChainCodeURL("VotoElettore"), GetUsername(), jsonParamsSchedBianca);

                        _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + "vote per scheda bianca");
                        _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + querySetVotoSB.Content);

                        // contiene la risposta del chaincode parserizzata => pseudonimo, vn, comites, used(di default a false fin tanto che l'elettore non ha votato)
                        JObject logVoto = Voto.parseVoteresponseFromJSON(querySetVotoSB.Content);

                        _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + " | risultato payload voto" + logVoto);


                        Console.WriteLine("Ingresso in check scheda bianca");

                        return RedirectToAction("ConfermaVoto");
                        break;

                    //caso lista senza candidati
                    case 2:
                        // chiamo le liste relative al comites

                        var queryInListeVuoto = APIService.InvokeChaincodeParameters("Lista", "getListe", getChainCodeURL("Lista"), GetUsername(), comites);


                        // var queryInListeVuoto = APIService.InvokeChaincode("Lista", "getListe", comites);
                        List<Lista> resultListVuoto = Lista.parseListaFromJSON(queryInListeVuoto.Content);

                        // ciclo per ogni lista appartenente al Comites
                        foreach (Lista lista in resultListVuoto)
                        {
                            // quando trovo la lista che corrisponde a quella inviata nell'oggetto form
                            if (lista.Name == form["Partito"].ToString())
                            {

                                // creo la lista per caricare i candidati selezionati coi valori dalla BC
                                List<CandidatoVoto> candidativotoVuoto = new List<CandidatoVoto>();

                                // creo la lista per caricare gli id relativi ai candidati salvati
                                List<String> GuidcandidativotoVuoto = new List<String>();

                                int a = 6;

                                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + "Invio Voto di preferenza Lista");
                                CandidatoVoto candVuoto = new CandidatoVoto();
                                //votoSchedaBianca.Lista = "Scheda Bianca";                              
                                candVuoto.nomeCandidato = "Nessuno";
                                candVuoto.cognomeCandidato = "Nessuno";
                                candVuoto.dataNascitaCandidato = "00/00/0000";
                                candVuoto.luogoNascitaCandidato = "Nessuno";
                                candVuoto.ordine = 1;
                                candVuoto.comites = comites;
                                candidativotoVuoto.Add(candVuoto);
                                GuidcandidativotoVuoto.Add(CreaGUID());

                                string[] jsonParamsVuoto = {
                                 JsonConvert.SerializeObject(GuidcandidativotoVuoto),
                                 lista.Name,
                                 JsonConvert.SerializeObject(candidativotoVuoto),
                                 pseudonimo
                                };

                                var querySetVotoVuoto = APIService.InvokeChaincodeVote("VotoElettore", "vote", getChainCodeURL("VotoElettore"), GetUsername(), jsonParamsVuoto);

                                //var querySetVotoVuoto = APIService.InvokeChaincodeVote("VotoElettore", "vote", jsonParamsVuoto);

                                // logging chiamata BC
                                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + "vote per preferenza lista");
                                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + querySetVotoVuoto.Content);

                                // contiene la risposta del chaincode parserizzata => pseudonimo, vn, comites, used(di default a false fin tanto che l'elettore non ha votato)
                                JObject logVotoVuoto = Voto.parseVoteresponseFromJSON(querySetVotoVuoto.Content);

                                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + " | risultato payload voto per preferenza lista" + logVotoVuoto);
                            }
                        }



                        return RedirectToAction("ConfermaVoto");
                        break;

                    //caso lista con candidati
                    default:

                        // chiamo le liste relative al comites

                        var queryInListe = APIService.InvokeChaincodeParameters("Lista", "getListe", getChainCodeURL("Lista"), GetUsername(), comites);

                        //var queryInListe = APIService.InvokeChaincode("Lista", "getListe", comites);
                        List<Lista> resultList = Lista.parseListaFromJSON(queryInListe.Content);

                        // ciclo per ogni lista appartenente al Comites
                        foreach (Lista lista in resultList)
                        {
                            // quando trovo la lista che corrisponde a quella inviata nell'oggetto form
                            if (lista.Name == form["Partito"].ToString())
                            {

                                // carico i candidati relativi alla lista inviata nell'oggetto form

                                var queryInCandidati = APIService.InvokeChaincodeParameters("Candidato", "getCandidati", getChainCodeURL("Candidato"), GetUsername(), lista.Name);

                                //var queryInCandidati = APIService.InvokeChaincode("Candidato", "getCandidati", lista.Name);
                                List<Candidato> resultCandidati = Candidato.parseCandidatiFromJSON(queryInCandidati.Content);

                                // Creo una lista di tipo string con i candidati ricevuti dal form
                                List<String> listaOrderIdCandidatoSelForm = new List<String>();

                                // ciclo ogni candidato ricevuto dall'oggetto form
                                foreach (String orderIdForm in form.Keys.ToList())
                                {
                                    // se il tipo della key è -1 è relativo alla chiave "candidato_"
                                    if (orderIdForm.IndexOf("candidato_") != -1)
                                    {

                                        //ricavo l'orderId dalla Key del candidato eliminando il prefisso canditato_ 
                                        string orderNumber = orderIdForm.Remove(0, 10);

                                        // aggiungo alla lista l'ordine dei candidati ricevuti
                                        listaOrderIdCandidatoSelForm.Add(orderNumber);
                                    }
                                }

                                // creo la lista per caricare i candidati selezionati coi valori dalla BC
                                List<CandidatoVoto> candidativoto = new List<CandidatoVoto>();

                                // creo la lista per caricare gli id relativi ai candidati salvati
                                List<String> Guidcandidativoto = new List<String>();

                                // per ogni candidato trovato nella lista BC
                                foreach (Candidato candidatoChaincode in resultCandidati)
                                {
                                    // ciclo sugli id della lista ricavata prima
                                    foreach (String orderIdForm in listaOrderIdCandidatoSelForm)
                                    {
                                        // quando corrispondono, li metto dentro l'oggetto
                                        if (candidatoChaincode.UniqueID.ToString() == orderIdForm)
                                        {
                                            // costruzione dell'array dei parametri candidati per la chaincode metodo SetVoto
                                            // qui puoi ricavare tutti i dati del candidato dalla chaincode
                                            CandidatoVoto candvoto = new CandidatoVoto();
                                            candvoto.nomeCandidato = candidatoChaincode.nomeCandidato;
                                            candvoto.cognomeCandidato = candidatoChaincode.CognomeCandidato;
                                            candvoto.dataNascitaCandidato = candidatoChaincode.DataNascitaCandidato;
                                            candvoto.luogoNascitaCandidato = candidatoChaincode.LuogoNascitaCandidato;
                                            candvoto.comites = comites;
                                            candvoto.ordine = candidatoChaincode.Ordine;
                                            candidativoto.Add(candvoto);
                                            Guidcandidativoto.Add(CreaGUID());
                                        }

                                    }


                                }

                                // vecchio metodo per invio array
                                //string[] jsonParams = {
                                // "{\"ids\":" + JsonConvert.SerializeObject(Guidcandidativoto) + "}",
                                // lista.Name,
                                //  "{\"candidato\":" + JsonConvert.SerializeObject(candidativoto) + "}",                                  
                                //    pseudonimo
                                //};

                                string[] jsonParams = {
                                 JsonConvert.SerializeObject(Guidcandidativoto),
                                 lista.Name,
                                 JsonConvert.SerializeObject(candidativoto),
                                 pseudonimo
                                };

                                //string[] jsonParams = {
                                // JsonConvert.SerializeObject(Guidcandidativoto).Replace("[","").Replace("]","").ToString(),
                                // lista.Name,
                                // JsonConvert.SerializeObject(candidativoto).Replace("[","").Replace("]","").ToString(),
                                // pseudonimo
                                //};

                                //string[] GuiArray = { JsonConvert.SerializeObject(Guidcandidativoto) };
                                //string[] GuiArray = Guidcandidativoto.ToArray();

                                //// string[] CandidatiArray = { JsonConvert.SerializeObject(candidativoto) };
                                //string[] CandidatiArray = candidativoto.ToArray();

                                //jsonParams[0] = "{\"ids\":" + JsonConvert.SerializeObject(Guidcandidativoto) + "}"; 
                                //jsonParams[1] = lista.Name;
                                //jsonParams[2] = "{\"candidato\":" + JsonConvert.SerializeObject(candidativoto) + "}";
                                //jsonParams[3] = pseudonimo;

                                // chiamante precedente
                                //var querySetVoto = APIService.InvokeChaincodeVote("VotoElettore", "vote", jsonParams);

                                // chiamante di nuovo test
                                // InvokeChaincodeVote

                                var querySetVoto = APIService.InvokeChaincodeVote("VotoElettore", "vote", getChainCodeURL("VotoElettore"), GetUsername(), jsonParams);



                                //var querySetVoto = APIService.InvokeChaincodeVote("VotoElettore", "vote", jsonParams);

                                // logging chiamata BC
                                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + "vote di Default");
                                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteBloCha + querySetVoto.Content);

                                // contiene la risposta del chaincode parserizzata => pseudonimo, vn, comites, used(di default a false fin tanto che l'elettore non ha votato)
                                JObject logVotoDefault = Voto.parseVoteresponseFromJSON(querySetVoto.Content);

                                _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + " | risultato payload voto Default" + logVotoDefault);
                            }



                        }


                        _logger.LogInformation(UteLogNam + UtenteGenericoSessione() + UteLogDo + "Invio Voto con candidati");
                        return RedirectToAction("ConfermaVoto");
                        break;
                }
            }
            else
            {  // gestione pulsante esci in header
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "SETVOTO issue: empty form");
                ViewBag.Exit = false;
                // errore di qualche tipo
                Dispose();
                return View("Errore");
            }

            return null;
        }
        #endregion
    
    }
}

