using Microsoft.Owin.Security;
using NewUltimusWeb.Services;
using NewUltimusWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewUltimusWeb.Controllers
{
    public class AuthController : Controller
    {
        private IBpmAuthenticationManager _authManager;
        public AuthController(IBpmAuthenticationManager authManager)
        {
            this._authManager = authManager;
        }

        // GET: /account/login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // We do not want to use any existing identity information
            //EnsureLoggedOut();
            _authManager.SignIn(HttpContext.GetOwinContext(), new UltimusAccount());
            // Store the originating URL so we can attach it to a form field
            var viewModel = new AccountLoginModel { ReturnUrl = returnUrl };

            return View(viewModel);
        }
    }
}