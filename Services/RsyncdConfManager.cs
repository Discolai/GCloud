using GCloud.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GCloud.Services
{
    public class RsyncdConfManager
    {
        private readonly GCloudDbContext _context;
        private readonly RsyncdSettings _rsyncdSettings;

        public RsyncdConfManager(GCloudDbContext context, RsyncdSettings rsyncdSettings)
        {
            _context = context;
            _rsyncdSettings = rsyncdSettings;
        }

        public async Task<bool> AddModuleAsync(string moduleName)
        {
            if (await AnyModuleAsync(moduleName)) return false;

            string module = $"[{moduleName}]\t#{moduleName}\n  path = {_rsyncdSettings.GCloudRoot}/{moduleName}\t#{moduleName}\n  read only = false\t#{moduleName}\n";

            await File.AppendAllTextAsync(_rsyncdSettings.ConfPath, module);
            return true;
        }

        public async Task<bool> RemoveModuleAsync(string moduleName)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(_rsyncdSettings.ConfPath))
            {
                string line;
                bool moduleFound = false;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // Remove module header and all the following indented lines
                    if ((moduleFound && line.StartsWith("  ")) || line.Contains($"[{moduleName}]")) moduleFound = true;
                    else
                    {
                        moduleFound = false;
                        lines.Add(line);
                    }
                }
            }
            await File.WriteAllLinesAsync(_rsyncdSettings.ConfPath, lines);
            return true;
        }


        public async Task<bool> AnyModuleAsync(string moduleName)
        {
            using (var reader = new StreamReader(_rsyncdSettings.ConfPath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Contains($"[{moduleName}]")) return true;
                }
            }
            return false;
        }
    }
}
