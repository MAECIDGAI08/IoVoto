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
using AppA.Models;
using Newtonsoft.Json.Linq;

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
            Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR info: Incoming call to Interceptor");
            try
            {
                //MUTUAL AUTHENTICATION

                /***MISSING***/


                //LOG HEADER DATA
                /*foreach (var header in Request.Headers)
                {
                    Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR Header Key: " + header.Key.ToString() + " -> " + header.Value.ToString());
                }*/

                //HEADER DATA RETRIEVE
                try
                {
                    string fiscalNumber = Request.Headers["FISCALNUMBER"];
                    string dateOfBirth = Request.Headers["DATEOFBIRTH"];

                    //NON VALID DATEOFBIRTH CHECK
                    if (String.IsNullOrEmpty(dateOfBirth))
                    {
                        Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR issue: Empty dateOfBirth");
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                    //NON VALID CF CHECK
                    if (String.IsNullOrEmpty(fiscalNumber))
                    {
                        Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR issue: Empty fiscalNumber");
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                    //CF PARSE
                    fiscalNumber = fiscalNumber.Substring(fiscalNumber.Length - 16);

                    //CF REGEX CHECK
                    if (!Utils.ValidateField(fiscalNumber, "req_cf"))
                    {
                        Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR issue: No req_cf validation for " + fiscalNumber);
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                    //DATEOFBIRTH REFORMAT
                    dateOfBirth = Utils.Eng2ItDate(dateOfBirth);

                    //DATEOFBIRTH REGEX CHECK
                    if (!Utils.ValidateField(dateOfBirth, "req_date"))
                    {
                        Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR issue: No req_date validation for " + dateOfBirth);
                        return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                    }

                    //SET TOKEN IN BLOCKCHAIN
                    var bcToken = Guid.NewGuid();
                    var dateNow = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    var queryInToken = Service.APIService.InvokeChainCode("internal", "Tokens", "createTokens", "{\\\"token\\\":\\\"" + bcToken + "\\\",\\\"date\\\":\\\"" + dateNow + "\\\",\\\"codiceFiscale\\\":\\\"" + fiscalNumber + "\\\"}");
                    Token token = Token.parseTokenFromJSON(queryInToken.Content);
                    if (!string.IsNullOrEmpty(token.ErrorDescription))
                    {
                        Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR issue: unable to set token for " + fiscalNumber + ": " + token.ErrorDescription);
                        return RedirectToAction("Errore");
                    }
                    Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR action: token set for " + fiscalNumber + ": " + bcToken + " at " + dateNow);

                    //CF BLOCKCHAIN CHECK
                    var queryIn = Service.APIService.InvokeChainCode("voters", "Elettori", "getFullElettori", fiscalNumber, "", dateOfBirth);
                    Elettore elettore = Elettore.parseElettoreFromJSON(queryIn.Content);
                    if (!string.IsNullOrEmpty(elettore.ErrorDescription) || string.IsNullOrEmpty(elettore.CodiceFiscale))
                    {
                        Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR action: " + fiscalNumber + " not found, redirecting to CodiceElettore");
                        ViewData["URL"]= Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/CodiceElettore?&cf="+ fiscalNumber + "&token=" + bcToken;
                    }

                    else {
                        Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR action: " + fiscalNumber + " found, redirecting to LoginFromCF");
                        ViewData["URL"] = Startup.StaticConfig.GetSection("AppA_URL").Value + "/Login/LoginFromCF/?ce=" + elettore.CodiceElettore + "&cf=" + fiscalNumber + "&token=" + bcToken;
                    }

                    return View();
                }
                catch (Exception e)
                {
                    Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR issue: " + e.Message);
                    return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
                }
            }
            catch (Exception e)
            {
                Utils.LogTrace(HttpContext.Connection.RemoteIpAddress.ToString(), "INTERCEPTOR issue: " + e.Message);
                return Redirect(Startup.StaticConfig.GetSection("AppA_URL").Value + "/Home/Errore");
            }
        }
    }
}
