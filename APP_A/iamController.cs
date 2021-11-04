using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppA.Models;
//using DataLibrary;  // Da utilizare 
using System.Data;
using System.Web;
using System.Text;
using System.Xml.Linq;
using Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
//using Fluent.Infrastructure.FluentModel; INTEGRAZIONE SPID

namespace AppB.Controllers
{
    public class iamController : Controller
    {
        //private readonly Microsoft.AspNet.Identity.UserManager<ApplicationUser> _userManager;
        //private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly ILogger<iamController> _logger;

        //public iamController(Microsoft.AspNet.Identity.UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<iamController> logger)
        //{
        //    _userManager = userManager;
        //    _signInManager = signInManager;
        //    _logger = logger;
        //}

        //[HttpGet]
        //public async Task<IActionResult> Interceptor()
        //{
        //    /*foreach (var header in Request.Headers)
        //    {
        //        Utils.LogTrace(HttpContext, "SPID Header Key: " + header.Key.ToString() + " --> " + header.Value.ToString());
        //    }*/

        //    //Referer check
        //    if (Request.Headers["Referer"] != Startup.StaticConfig.GetSection("SPID").GetSection("URL").GetSection("Allowed").Value)
        //    {
        //        Utils.LogTrace(HttpContext, "SPID issue: bad Referer " + Request.Headers["Referer"]);
        //        return RedirectToAction("", "Error");
        //    }

        //    //recupero e decoding response
        //    try
        //    {
        //        string name = Request.Headers["NAME"];
        //        string familyName = Request.Headers["FAMILYNAME"];
        //        string placeOfBirth = Request.Headers["PLACEOFBIRTH"];
        //        string countyOfBirth = Request.Headers["COUNTYOFBIRTH"];
        //        string dateOfBirth = Request.Headers["DATEOFBIRTH"];
        //        string gender = Request.Headers["GENDER"];
        //        string fiscalNumber = Request.Headers["FISCALNUMBER"];
        //        string mobilePhone = Request.Headers["MOBILEPHONE"];
        //        string email = Request.Headers["EMAIL"];
        //        fiscalNumber = fiscalNumber.Substring(fiscalNumber.Length - 16);
        //        if (!Utils.ValidateField(fiscalNumber, "req_cf"))
        //        {
        //            Utils.LogTrace(HttpContext, "SPID issue: no req_cf validation for " + fiscalNumber);
        //            return RedirectToAction("", "Error");
        //        }
        //        //utente già registrato?

                
        //        cls_dbo_GE_Anagrafica FindCF = new cls_dbo_GE_Anagrafica();
        //        FindCF.ana_CODICE_FISCALE = fiscalNumber.ToUpper();
        //        DataSet FindCFDataSet = FindCF.GetDs();


        //        if (FindCFDataSet.Tables.Count > 0 && FindCFDataSet.Tables[0].Rows.Count > 0)
        //        {
        //            var user = await _userManager.FindByEmailAsync(FindCFDataSet.Tables[0].Rows[0]["UserName"].ToString());
        //            if (user == null)
        //            {
        //                Utils.LogTrace(HttpContext, "SPID issue: no FindByEmailAsync for " + fiscalNumber);
        //                return RedirectToAction("", "Error");
        //            }
        //            await _signInManager.SignInAsync(user, isPersistent: false);
        //            _logger.LogInformation("User logged in.");
                    

                 
        //        HttpContext.Session.SetBoolean("logged", true);
                  
        //        HttpContext.Session.SetString("id", user.Id);
        //            return RedirectToAction("", "Home");
        //        }
        //        else
        //        {
        //            //Aggiungo i campi Email e Telefono e popolo il DataSet anagrafico derivante dal SAML
        //            Dictionary<string, string> DCTanag = new Dictionary<string, string>();
        //            DCTanag.Add("ana_NOME", name);
        //            DCTanag.Add("ana_COGNOME", familyName);
        //            DCTanag.Add("ana_PROV_NASCITA", placeOfBirth);
        //            DCTanag.Add("ana_COD_COMUNE_NASCITA", countyOfBirth);
        //            DCTanag.Add("ana_DT_NASCITA", dateOfBirth);
        //            DCTanag.Add("ana_SESSO", gender);
        //            DCTanag.Add("ana_CODICE_FISCALE", fiscalNumber);
        //            DCTanag.Add("ana_EMAIL", email);
        //            DCTanag.Add("ana_TELEFONO", mobilePhone);
        //            //Converto in stringa il dictionary con l'anagrafica per poterla passare al controller
        //            string STRanag = "";
        //            foreach (KeyValuePair<string, string> keyValues in DCTanag)
        //            {
        //                STRanag += keyValues.Key + " : " + keyValues.Value + ", ";
        //            }
        //            STRanag = STRanag.TrimEnd(',', ' ');

        //            return RedirectToAction("Account", "Identity", new { id = "Register", anagrafica = STRanag });
        //        }
        //        }
        //    catch (Exception e)
        //    {
        //        Utils.LogTrace(HttpContext, "SPID issue: " + e.Message);
        //        return RedirectToAction("", "Error");
        //    }
        //}
    }
}
