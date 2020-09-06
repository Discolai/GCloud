using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCloud.Services
{
    public class RsyncdSettings
    {
        public string RsyncPath { get; set; }
        public string ConfPath { get; set; }
        public string GCloudRoot { get; set; }
        public string SshUser { get; set; }
        public string AuthorizedKeysPath { get; set; }
        public string SecretsPath { get; set; }
    }
}
