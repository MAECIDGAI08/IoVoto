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
// IP tracking

namespace AppA.Controllers
{
    public class LoginController : Controller
    {
        //variabile interna per accedere alla localizzazione (SharedResources)
        private readonly IStringLocalizer<SharedResource> _localizer;

        //var interna per accedere all'interfaccia ILoggerFactory
        private readonly ILogger<LoginController> _logger;

        // var interna per IHostingEnvironment
        private readonly IHostingEnvironment env;

        public LoginController(IStringLocalizer<SharedResource> localizer, ILogger<LoginController> logger, IHostingEnvironment env)
        {
            _localizer = localizer;
            _logger = logger;
            this.env = env;
        }

        public IActionResult LoginFromCF(String t = null)
        {
            Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF info: GET " + t);

            //INPUT CHECK
            if (String.IsNullOrEmpty(t))
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: empty t");
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }

            try
            {
                //DECRYPT & PARSE INPUT
                String decryptedQS = Utils.Decrypt(t, Startup.StaticConfig.GetSection("DSA").Value);
                string[] QSparts = decryptedQS.Split('|');
                if (QSparts.Length != 3)
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: wrong t" + decryptedQS);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                String token = QSparts[0];
                String cf = QSparts[1];
                String ce = QSparts[2];

                if (String.IsNullOrEmpty(token))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: empty token");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                if (String.IsNullOrEmpty(cf) || !Utils.ValidateField(cf, "req_cf"))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: wrong cf " + cf);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                if (String.IsNullOrEmpty(ce))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: empty ce");
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                //TOKEN BC CHECK
                var queryInToken = Service.APIService.InvokeChainCode("internal", "Tokens", "getTokensById", token);
                Token bcToken = Token.parseTokenFromJSON(queryInToken.Content);
                if (!string.IsNullOrEmpty(bcToken.ErrorDescription))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: token not found for " + cf);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
                if (bcToken.CodiceFiscaleVal != cf)
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: no token cf match " + cf + " " + token);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                //CHECK TOKEN EXPIRATION
                var diffInSeconds = (DateTime.Now - DateTime.ParseExact(bcToken.DateVal, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)).TotalSeconds;
                if (diffInSeconds > Int32.Parse(Startup.StaticConfig.GetSection("BlockChain").GetSection("TokenExpire").Value))
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: token expired for " + cf + ": " + bcToken.TokenVal + " " + bcToken.DateVal);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }

                //CREATE JWT
                var newToken = Guid.NewGuid();
                DateTime now = DateTime.UtcNow;
                Int32 iat = (Int32)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                Int32 nbf = (Int32)(now.AddSeconds(-60).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                Int32 exp = (Int32)(now.AddSeconds(120).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                String JWT = "{sub=" + ce.Replace('/', '_') + ", aud=oracle, nbf=" + nbf + ", iss=oracle, exp=" + exp + ", iat=" + iat + ", jti=" + newToken.ToString() + "}";

                //ENCRYPT JWT
                string publicKeyString = Startup.StaticConfig.GetSection("DSA").Value;
                string encryptedJWT = Utils.Encrypt(JWT, publicKeyString);

                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF action: JWT sent for cf " + cf + " ce " + ce + " " + JWT + " " + encryptedJWT);

                //POST JWT TO IDPAdapter
                ViewData["URL"] = Startup.StaticConfig.GetSection("IDPAdapter_uRL").Value;
                ViewData["JWT"] = encryptedJWT;
                return View("~/Views/Login/Index.cshtml");

            }
            catch (Exception e)
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCF issue: " + e.Message);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }
        }

        [HttpPost]
        public IActionResult LoginFromCE(IFormCollection form)
        {
            //INPUT CHECK
            if (form == null)
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: empty form");
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }

            if (!form.ContainsKey("CodiceElettoreclient") || !form.ContainsKey("bc-cf") || !form.ContainsKey("bc-tk") || !form.ContainsKey("bc-dt") || String.IsNullOrEmpty(form["CodiceElettoreclient"]) || String.IsNullOrEmpty(form["bc-cf"]) || String.IsNullOrEmpty(form["bc-dt"]) || String.IsNullOrEmpty(form["bc-tk"]))
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: bad form");
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }

            if (!Utility.Utils.ValidateField(form["bc-cf"], "req_cf"))
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: wrong cf " + form["bc-cf"]);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }

            if (!Utility.Utils.ValidateField(form["bc-dt"], "req_date"))
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: wrong dt " + form["bc-dt"]);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }

            //CE<->DT BLOCKCHAIN CHECK
            var queryInCFDT = Service.APIService.InvokeChainCode("voters", "Elettori", "getFullElettori", "", form["CodiceElettoreclient"], form["bc-dt"]);
            Elettore elettoreCFDT = Elettore.parseElettoreFromJSON(queryInCFDT.Content);
            if (!string.IsNullOrEmpty(elettoreCFDT.ErrorDescription))
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: error for CE: " + form["CodiceElettoreclient"] + " DT: " + form["bc-dt"] + " ERR: " + elettoreCFDT.ErrorDescription);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/ErroreCodiceElettore");
            }
            if (string.IsNullOrEmpty(elettoreCFDT.CodiceElettore))
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: no codice elettore " + form["CodiceElettoreclient"] + " for data di nascita " + form["bc-dt"]);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/ErroreCodiceElettore");
            }

            //TOKEN BC CHECK
            var queryInToken = Service.APIService.InvokeChainCode("internal", "Tokens", "getTokensById", form["bc-tk"]);
            Token bcToken = Token.parseTokenFromJSON(queryInToken.Content);
            if (!string.IsNullOrEmpty(bcToken.ErrorDescription))
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: token not found for " + form["bc-cf"]);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }
            if (bcToken.CodiceFiscaleVal != form["bc-cf"])
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: no token cf match " + form["bc-cf"] + " " + form["bc-tk"]);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }

            //CHECK TOKEN EXPIRATION
            var diffInSeconds = (DateTime.Now - DateTime.ParseExact(bcToken.DateVal, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)).TotalSeconds;
            if (diffInSeconds > Int32.Parse(Startup.StaticConfig.GetSection("BlockChain").GetSection("TokenExpire").Value))
            {
                Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE issue: token expired for " + form["bc-cf"] + ": " + bcToken.TokenVal + " " + bcToken.DateVal);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }

            //CREATE JWT
            var newToken = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            Int32 iat = (Int32)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Int32 nbf = (Int32)(now.AddSeconds(-60).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Int32 exp = (Int32)(now.AddSeconds(120).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            String JWT = "{sub=" + form["CodiceElettoreclient"].ToString().Replace('/', '_') + ", aud=oracle, nbf=" + nbf + ", iss=oracle, exp=" + exp + ", iat=" + iat + ", jti=" + newToken.ToString() + "}";

            //ENCRYPT JWT
            string publicKeyString = Startup.StaticConfig.GetSection("DSA").Value;
            string encryptedJWT = Utility.Utils.Encrypt(JWT, publicKeyString);

            Utility.Utils.LogTrace(Request.Headers["X-Forwarded-For"], "LOGINFROMCE action: JWT sent for cf " + form["bc-cf"] + " ce " + form["CodiceElettoreclient"] + " " + JWT + " " + encryptedJWT);

            //POST JWT TO IDPAdapter
            ViewData["URL"] = Startup.StaticConfig.GetSection("IDPAdapter_uRL").Value;
            ViewData["JWT"] = encryptedJWT;
            return View("~/Views/Login/Index.cshtml");
        }

    }
}
