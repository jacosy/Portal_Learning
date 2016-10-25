using Microsoft.Owin.Security;
using NewUltimusWeb.Services;
using NewUltimusWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using Microsoft.Ajax.Utilities;
using System.Threading.Tasks;

namespace NewUltimusWeb.Controllers
{
    public class AuthController : Controller
    {
        private IBpmAuthenticationManager<IAccount> _authManager;
        public AuthController(IBpmAuthenticationManager<IAccount> authManager)
        {
            this._authManager = authManager;
        }

        // GET: /account/login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // We do not want to use any existing identity information
            EnsureLoggedOut();
            //_authManager.SignIn(HttpContext.GetOwinContext(), new UltimusAccount());
            // Store the originating URL so we can attach it to a form field
            var viewModel = new AccountLoginModel { ReturnUrl = returnUrl };

            return View(viewModel);
        }

        // POST: /account/login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountLoginModel model)
        {
            // Ensure we have a valid viewModel to work with
            if (!ModelState.IsValid)
                return View(model);

            // Verify if a user exists with the provided identity information
            UltimusClientService ultService = new UltimusClientService();
            string sesssionId, errMsg;
            UltimusAccount account = ultService.LoginUser(model.Domain, model.UserName, model.Password, out sesssionId, out errMsg);
            // If a user was found
            if (account != null)
            {
                // Then create an identity for it and sign it in                
                SignIn(account);

                // If the user came from a specific page, redirect back to it
                return RedirectToLocal(model.ReturnUrl);
            }

            // No existing user was found that matched the given criteria
            ModelState.AddModelError("", "Invalid username or password.");

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void SignIn(IAccount account)
        {
            // Clear any lingering authencation data
            _authManager.SignOut();

            // Create a claims based identity for the current user
            _authManager.SignIn(account);
        }

        private ActionResult RedirectToLocal(string returnUrl = "")
        {
            // If the return url starts with a slash "/" we assume it belongs to our site
            // so we will redirect to this "action"
            if (!returnUrl.IsNullOrWhiteSpace() && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // If we cannot verify if the url is local to our host we redirect to a default location
            return RedirectToAction("index", "home");
        }

        private string GetClaimValueByType(string claimType)
        {
            return HttpContext.GetOwinContext().Authentication.User.Claims.FirstOrDefault(c => c.Type == claimType).Value;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            string errorMsg;
            string sessionId = GetClaimValueByType("SessionID");
            if (!string.IsNullOrEmpty(sessionId))
            {
                UltimusClientService ultService = new UltimusClientService();
                ultService.LogoutUser(sessionId, out errorMsg);
            }

            _authManager.SignOut();            

            // we redirect to a controller/action that requires authentication to ensure a redirect takes place
            // this clears the Request.IsAuthenticated flag since this triggers a new request
            return RedirectToLocal();
        }

        private void EnsureLoggedOut()
        {
            // If the request is (still) marked as authenticated we send the user to the logout action
            if (Request.IsAuthenticated)
                Logout();
        }
    }
}