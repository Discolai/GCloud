using GCloud.Data;
using GCloud.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Services
{
    public class RsyncdSecretsManager
    {
        private readonly GCloudDbContext _context;
        private readonly RsyncdSettings _rsyncdSettings;

        public RsyncdSecretsManager(GCloudDbContext context, RsyncdSettings rsyncdSettings)
        {
            _context = context;
            _rsyncdSettings = rsyncdSettings;
        }

        public async Task<bool> AddUserAsync(IdentityUser user, RsaKeyPair keyPair)
        {
            if (await AnyUserAsync(user.UserName)) return false;

            var userNameBytes = Encoding.UTF8.GetBytes(user.UserName);
            var password = Convert.ToBase64String(keyPair.ToRsa().SignData(userNameBytes, 0, userNameBytes.Length, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

            using (var writer = File.AppendText(_rsyncdSettings.SecretsPath))
            {
                await writer.WriteLineAsync($"{user.UserName}:{password}");
            }

            return true;
        }

        public async Task<bool> AnyUserAsync(string userName)
        {
            using (var reader = new StreamReader(_rsyncdSettings.SecretsPath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Contains(userName)) return true;
                }
            }
            return false;
        }

        public async Task<bool> RemoveUserAsync(string userName)
        {
            await File.WriteAllLinesAsync(_rsyncdSettings.SecretsPath, (await File.ReadAllLinesAsync(_rsyncdSettings.SecretsPath)).Where(l => !l.Contains(userName)).ToList());
            return true;
        }
    }
}
