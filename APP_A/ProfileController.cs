using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppA.Models;
using Microsoft.AspNetCore.Identity;
//using DataLibrary; INTEGRAZIONE SPID
using System.Data;
using Microsoft.AspNetCore.Http;
using Utility;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
//using Fluent.Infrastructure.FluentModel;  INTEGRAZIONE SPID

namespace AppB.Controllers
{
    public class ProfileController : Controller
    {
        //INTEGRAZIONE SPID
    //    private readonly UserManager<ApplicationUser> _userManager;
    //    private readonly SignInManager<ApplicationUser> _signInManager;
    //    private readonly ILogger<ProfileController> _logger;

    //    public ProfileController(ILogger<ProfileController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    //    {
    //        _logger = logger;
    //        _userManager = userManager;
    //        _signInManager = signInManager;
    //    }

    //    [HttpGet]
    //    public IActionResult Index()
    //    {
    //        if (_signInManager.IsSignedIn(User)) { return RedirectToAction("Views", "Profile"); }
    //        else { return RedirectToAction("Account", "Identity", new { id = "Login", path = HttpContext.Request.Path }); }
    //    }

    //    public IActionResult Views()
    //    {
    //        if (_signInManager.IsSignedIn(User)) {
    //            cls_dbo_GE_Anagrafica FindUser = new cls_dbo_GE_Anagrafica();
    //            FindUser.ana_USER_ID = Guid.Parse(_userManager.GetUserId(User));
    //            DataSet FindUserDataSet = FindUser.GetFromGuid();
    //            if (FindUserDataSet.Tables.Count > 0 && FindUserDataSet.Tables[0].Rows.Count > 0) {
    //                ViewData["dataSet"]= FindUserDataSet;
    //                return View(); }
    //            else { return RedirectToAction("", "Error"); }
    //        }
            
    //        else {
    //            return RedirectToAction("Account", "Identity", new { id = "Login", path = HttpContext.Request.Path });
    //        }
    //    }

    //    public IActionResult EditMail()
    //    {
    //        if (_signInManager.IsSignedIn(User))
    //        {
    //            cls_dbo_GE_Anagrafica FindUser = new cls_dbo_GE_Anagrafica();
    //            FindUser.ana_USER_ID = Guid.Parse(_userManager.GetUserId(User));
    //            DataSet FindUserDataSet = FindUser.GetFromGuid();
    //            if (FindUserDataSet.Tables.Count > 0 && FindUserDataSet.Tables[0].Rows.Count > 0)
    //            {
    //                ViewData["dataSet"] = FindUserDataSet;
    //                return View();
    //            }
    //            else { return RedirectToAction("", "Error"); }
    //        }

    //        else
    //        {
    //            return RedirectToAction("Account", "Identity", new { id = "Login", path = HttpContext.Request.Path });
    //        }
    //    }

    //    public async Task<IActionResult> EditPassword()
    //    {
    //        if (_signInManager.IsSignedIn(User))
    //        {
    //            string userID = _userManager.GetUserId(User);
    //            cls_dbo_GE_Anagrafica FindUser = new cls_dbo_GE_Anagrafica();
    //            FindUser.ana_USER_ID = Guid.Parse(userID);
    //            DataSet FindUserDataSet = FindUser.GetFromGuid();
    //            if (FindUserDataSet.Tables.Count > 0 && FindUserDataSet.Tables[0].Rows.Count > 0)
    //            {
    //                var user = await _userManager.FindByIdAsync(userID);
    //                if (user == null)
    //                {
    //                    Utils.LogTrace(HttpContext, "No UserID " + userID);
    //                    return RedirectToAction("", "Error");
    //                }
    //                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
    //                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    //                ViewData["code"] = code;
    //                ViewData["dataSet"] = FindUserDataSet;
    //                return View();
    //            }
    //            else { return RedirectToAction("", "Error"); }
    //        }

    //        else
    //        {
    //            return RedirectToAction("Account", "Identity", new { id = "Login", path = HttpContext.Request.Path });
    //        }
    //    }

    //    [HttpPost]
    //    public IActionResult EditMail(IFormCollection form)
    //    {
    //        if (_signInManager.IsSignedIn(User))
    //        {
    //            //**check di routine in input
    //            if (form == null)
    //            {
    //                Utils.LogTrace(HttpContext, "Empty form");
    //                return RedirectToAction("", "Error");
    //            }
    //            //

    //            //***validazione dei campi in ingresso
    //            string[] validationFields = { "Email" };
    //            string[] validationTypes = { "req_email" };
    //            if (!Utils.validationLoop(HttpContext, validationFields, validationTypes, form))
    //            {
    //                return RedirectToAction("", "Error");
    //            }
    //            //

    //            //***check email (unicità)
    //            cls_dbo_AspNetUsers AspNetUsers = new cls_dbo_AspNetUsers();
    //            AspNetUsers.Email = form["Email"].ToString().ToLower();
    //            DataSet AspNetUsersDataSet = AspNetUsers.GetDs();
    //            if (AspNetUsersDataSet.Tables.Count > 0 && AspNetUsersDataSet.Tables[0].Rows.Count > 0)
    //            {
    //                return RedirectToAction("", "Error");
    //            }
    //            //

    //            string message = "";
    //            AspNetUsers.Id = Guid.Parse(_userManager.GetUserId(User));
    //            AspNetUsers.Email = form["Email"].ToString().ToLower();
    //            AspNetUsers.UserName = form["Email"].ToString().ToLower();
    //            AspNetUsers.asp_USR_ULT_AGG = _userManager.GetUserId(User);
    //            int result = AspNetUsers.Update(out message);
    //            if (result < 0)
    //            {
    //                Utils.LogTrace(HttpContext, message);
    //                return RedirectToAction("", "Error");
    //            }
    //            else {
    //                HttpContext.Session.SetString("Message", "1"); 
    //                return RedirectToAction("", "Profile"); }
    //        }

    //        else
    //        {
    //            return RedirectToAction("Account", "Identity", new { id = "Login", path = HttpContext.Request.Path });
    //        }
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> EditPassword(IFormCollection form)
    //    {
    //        if (_signInManager.IsSignedIn(User))
    //        {
    //            //**check di routine in input
    //            if (form == null)
    //            {
    //                Utils.LogTrace(HttpContext, "Empty form");
    //                return RedirectToAction("", "Error");
    //            }
    //            //

    //            //***validazione dei campi in ingresso
    //            string[] validationFields = { "Password", "PasswordMatch", "code" };
    //            string[] validationTypes = { "password", "required", "required" };
    //            if (!Utils.validationLoop(HttpContext, validationFields, validationTypes, form))
    //            {
    //                return RedirectToAction("", "Error");
    //            }
    //            //
    //            string userID = _userManager.GetUserId(User);
    //            var user = await _userManager.FindByIdAsync(userID);
    //            if (user == null)
    //            {
    //                Utils.LogTrace(HttpContext, "No UserID " + userID);
    //                return RedirectToAction("", "Error");
    //            }
    //            var result = await _userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(form["code"])), form["Password"]);
    //            if (result.Succeeded)
    //            {
    //                HttpContext.Session.SetString("Message", "2");
    //                return RedirectToAction("", "Profile");
    //            }
    //            foreach (var error in result.Errors)
    //            {
    //                ModelState.AddModelError(string.Empty, error.Description);
    //                Utils.LogTrace(HttpContext, error.Description);
    //                return RedirectToAction("", "Error");
    //            }
    //            return RedirectToAction("", "Error");
    //        }

    //        else
    //        {
    //            return RedirectToAction("Account", "Identity", new { id = "Login", path = HttpContext.Request.Path });
    //        }
    //    }

    }
}
