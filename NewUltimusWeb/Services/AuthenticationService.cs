using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using Microsoft.Owin;
using NewUltimusWeb.Models;
using System.Security.Principal;
using System.Threading.Tasks;

namespace NewUltimusWeb.Services
{
    public interface IBpmAuthenticationManager<IAccount>
    {
        void SignIn(IAccount account);
        void SignOut();
    }

    public class UltimusAuthenticationManager : IBpmAuthenticationManager<IAccount>
    {
        private readonly IOwinContext _context;
        public UltimusAuthenticationManager()
        {
            this._context = HttpContext.Current.GetOwinContext();
        }

        public void SignIn(IAccount account)
        {
            var ultAccount = (UltimusAccount)account;
            var identity = new ClaimsIdentity("BpmAppicationCookie");
            identity.AddClaims(new List<Claim>
            {
                new Claim("SessionID", ultAccount.SessionId),
                new Claim("TaskID", ultAccount.SessionId),
                new Claim(ClaimTypes.NameIdentifier, ultAccount.UserId),
                new Claim("Domain", ultAccount.Domain),
                new Claim(ClaimTypes.Name, ultAccount.UserName),
                new Claim(ClaimTypes.Email, ultAccount.UserName),
                new Claim(ClaimTypes.Sid, ultAccount.Password)
            });
            _context.Authentication.SignIn(identity);

            _context.Response.Cookies.Append("SessionID", ultAccount.SessionId);
            _context.Response.Cookies.Append("UserID", ultAccount.UserId);
            _context.Response.Cookies.Append("Domain", ultAccount.Domain);
            _context.Response.Cookies.Append("Username", ultAccount.UserName);
        }
        public void SignOut()
        {           
            // First we clean the authentication ticket like always
            _context.Authentication.SignOut("BpmAppicationCookie");
            // Second we clear the principal to ensure the user does not retain any authentication
            _context.Authentication.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
            // Third Remove Cookies
            foreach (var cookie in _context.Request.Cookies)
            {                
                _context.Response.Cookies.Delete(cookie.Key);
            }
        }        
    }
}