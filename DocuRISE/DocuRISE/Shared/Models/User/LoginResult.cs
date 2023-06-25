using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuRISE.Shared.Models.User
{
    public class LoginResult
    {
        public bool Successful { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public string Token { get; set; }
    }
}
