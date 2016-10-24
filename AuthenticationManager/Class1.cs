using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationManager
{
    public interface IAuthenticationManager
    {
        void SignIn();
        void SignOut();
    }
}
