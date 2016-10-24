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
        void SignIn(IOwinContext context, IAccount account);
        void SignOut(IOwinContext context, IAccount account);
    }

    public class UltimusAuthenticationManager : IBpmAuthenticationManager
    {
        public void SignIn(IOwinContext context, IAccount account)
        {
            var identity = new ClaimsIdentity("BpmAppicationCookie");
            //identity.AddClaims(new List<Claim>
            //    {
            //        new Claim("SessionId", SessionId),
            //        new Claim(ClaimTypes.NameIdentifier, model.UserName),
            //        new Claim(ClaimTypes.Name, model.UserName),
            //        new Claim("Password", model.Password)
            //    });
            context.Authentication.SignIn(identity);
        }

        public void SignOut(IOwinContext context, IAccount account)
        {
            throw new NotImplementedException();
        }
    }
}