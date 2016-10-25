using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NewUltimusWeb.Models;

namespace NewUltimusWeb.Services
{
    public class UltimusClientService
    {
        private ClientServices.Services _ultService;

        public UltimusClientService()
        {
            _ultService = new ClientServices.Services();
        }

        public UltimusAccount LoginUser(string domain, string userName, string password, out string sessionId, out string errorMsg)
        {
            UltimusAccount account = null;
            bool result = _ultService.LoginUser(domain, userName, password, out sessionId, out errorMsg);
            if (result)
            {
                account = new UltimusAccount
                {
                    Domain = domain,
                    UserName = userName,
                    Password = password,
                    SessionId = sessionId
                };
            }

            return account;
        }

        public bool LogoutUser(string sessionId, out string errorMsg)
        {
            return _ultService.LogoutUser(sessionId, out errorMsg);
        }

        public bool IsValidSessionID(string sessionId, string userId)
        {
            return _ultService.IsValidSessionID(sessionId, userId);
        }
    }
}