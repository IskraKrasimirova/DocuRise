using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuRISE.Shared.Models.User
{
    public class UploadResult
    {
        public bool Successful { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public IEnumerable<string> Success { get; set; }
    }
}
