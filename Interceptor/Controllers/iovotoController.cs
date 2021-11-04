using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Web;
using System.Text;
using System.Xml.Linq;
using Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Interceptor;
using Interceptor.Models;

namespace AppA.Controllers
{
    public class iovotoController : Controller
    {
        private readonly ILogger<iovotoController> _logger;

        public iovotoController(ILogger<iovotoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //LOG INCOMING CALL TO INTERCEPTOR
            Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR info: incoming call to Interceptor");
            try
            {
                //LOG HEADER DATA
                foreach (var header in Request.Headers)
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR header key: " + header.Key.ToString() + " -> " + header.Value.ToString());
                }

                //HEADER DATA RETRIEVE
                try
                {
                    string fiscalNumber = Request.Headers["FISCALNUMBER"];
                    string dateOfBirth = Request.Headers["DATEOFBIRTH"];

                    //NON VALID DATEOFBIRTH CHECK
                    if (String.IsNullOrEmpty(dateOfBirth))
                    {
                        Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR issue: empty dateOfBirth");
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                    //NON VALID CF CHECK
                    if (String.IsNullOrEmpty(fiscalNumber))
                    {
                        Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR issue: empty fiscalNumber");
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                    //CF PARSE
                    fiscalNumber = fiscalNumber.Substring(fiscalNumber.Length - 16);

                    //CF REGEX CHECK
                    if (!Utils.ValidateField(fiscalNumber, "req_cf"))
                    {
                        Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR issue: no req_cf validation for " + fiscalNumber);
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                    //DATEOFBIRTH REFORMAT
                    dateOfBirth = Utils.Eng2ItDate(dateOfBirth);

                    //DATEOFBIRTH REGEX CHECK
                    if (!Utils.ValidateField(dateOfBirth, "req_date"))
                    {
                        Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR issue: no req_date validation for " + dateOfBirth);
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                    //SET TOKEN IN BLOCKCHAIN
                    var bcToken = Guid.NewGuid();
                    var dateNow = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    var queryInToken = Interceptor.Service.APIService.InvokeChainCode("internal", "Tokens", "createTokens", "{\\\"token\\\":\\\"" + bcToken + "\\\",\\\"date\\\":\\\"" + dateNow + "\\\",\\\"codiceFiscale\\\":\\\"" + fiscalNumber + "\\\"}");
                    Token token = Token.parseTokenFromJSON(queryInToken.Content);
                    if (!string.IsNullOrEmpty(token.ErrorDescription))
                    {
                        Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR issue: unable to set token for " + fiscalNumber + ": " + token.ErrorDescription);
                        return RedirectToAction("Errore");
                    }
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR action: token set for " + fiscalNumber + ": " + bcToken + " at " + dateNow);

                    //CF BLOCKCHAIN CHECK
                    var queryIn = Interceptor.Service.APIService.InvokeChainCode("voters", "Elettori", "getFullElettori", fiscalNumber, "", dateOfBirth);
                    Elettore elettore = Elettore.parseElettoreFromJSON(queryIn.Content);
                    if (!string.IsNullOrEmpty(elettore.ErrorDescription) || string.IsNullOrEmpty(elettore.CodiceFiscale))
                    {
                        String queryString = bcToken + "|" + fiscalNumber + "|" + dateOfBirth;
                        string encryptedQueryString = HttpUtility.UrlEncode(Utils.Encrypt(queryString, Startup.StaticConfig.GetSection("DSA").Value));
                        ViewData["URL"]= Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/CodiceElettore?t=" + encryptedQueryString;
                        Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR action: " + fiscalNumber + " not found, redirecting to " + Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/CodiceElettore?t=" + encryptedQueryString);
                    }

                    else {
                        String queryString = bcToken + "|" + fiscalNumber + "|" + elettore.CodiceElettore;
                        string encryptedQueryString = HttpUtility.UrlEncode(Utils.Encrypt(queryString, Startup.StaticConfig.GetSection("DSA").Value));
                        ViewData["URL"] = Startup.StaticConfig.GetSection("AppA_URL").Value + "/Login/LoginFromCF?t=" + encryptedQueryString;
                        Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR action: " + fiscalNumber + " found, redirecting to " + Startup.StaticConfig.GetSection("AppA_URL").Value + "/Login/LoginFromCF?t=" + encryptedQueryString);
                    }

                    return View();
                }
                catch (Exception e)
                {
                    Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR issue: " + e.Message);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
            }
            catch (Exception e)
            {
                Utils.LogTrace(Request.Headers["X-Forwarded-For"], "INTERCEPTOR issue: " + e.Message);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }
        }
    }
}
