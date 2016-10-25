using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using Microsoft.Owin;
using NewUltimusWeb.Models;

namespace NewUltimusWeb.Services
{
    public interface IBpmAuthenticationManager
    {
        void SignIn(IAccount account);
        void SignOut(IAccount account);
    }

    public class UltimusAuthenticationManager : IBpmAuthenticationManager
    {       
        private readonly IOwinContext _context;
        public UltimusAuthenticationManager(IOwinContext context)
        {
            this._context = context;
        }

        public void SignIn(IAccount account)
        {
            UltimusAccount ultAccount = account as UltimusAccount;
            var identity = new ClaimsIdentity("BpmAppicationCookie");

            _context.Response.Cookies.Append("SessionId", "aaaa");
            _context.Response.Cookies.Append("Username", "LongoriaYou");
            _context.Response.Cookies.Append("Password", "aaabbbccc");
            //identity.AddClaims(new List<Claim>
            //    {
            //        new Claim("SessionId", SessionId),
            //        new Claim(ClaimTypes.NameIdentifier, model.UserName),
            //        new Claim(ClaimTypes.Name, model.UserName),
            //        new Claim("Password", model.Password)
            //    });
            _context.Authentication.SignIn(identity);
        }

        public void SignOut(IAccount account)
        {
            throw new NotImplementedException();
        }
    }
}