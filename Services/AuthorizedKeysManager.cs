using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GCloud.Services
{
    public class AuthorizedKeysManager
    {
        private readonly RsyncdSettings _rsyncdSettings;

        public AuthorizedKeysManager(RsyncdSettings rsyncdSettings)
        {
            _rsyncdSettings = rsyncdSettings;
        }

        public async Task<bool> AddAsync(string publicKey)
        {
            if (await AnyAsync(publicKey)) return false;

            string rsyncCommand = $"\"{_rsyncdSettings.RsyncPath} --config={_rsyncdSettings.ConfPath} --server --daemon .\",no-agent-forwarding,no-port-forwarding,no-pty,no-user-rc,no-x11-forwarding";
            string authorizedKeysEntry = $"command={rsyncCommand} {publicKey}";

            using (var writer = File.AppendText(_rsyncdSettings.AuthorizedKeysPath))
            {
                await writer.WriteLineAsync(authorizedKeysEntry);
            }
            return true;
        }

        public async Task<bool> AnyAsync(string publicKey)
        {
            using (var reader = new StreamReader(_rsyncdSettings.AuthorizedKeysPath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Contains(publicKey)) return true;
                }
            }
            return false;
        }

        public async Task<bool> RemoveAsync(string publicKey)
        {
            await File.WriteAllLinesAsync(_rsyncdSettings.AuthorizedKeysPath, (await File.ReadAllLinesAsync(_rsyncdSettings.AuthorizedKeysPath)).Where(l => !l.Contains(publicKey)).ToList());
            return true;
        }
    }
}
