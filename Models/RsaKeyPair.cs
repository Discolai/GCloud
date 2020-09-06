using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Models
{

    public class RsaKeyPair
    {
        private enum KeyType
        {
            Private,
            Public
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [MaxLength(2048)]
        public byte[] PrivateKey { get; set; }
        [MaxLength(2048)]
        public byte[] PublicKey { get; set; }


        public RsaKeyPair() { }

        public RsaKeyPair(RSA rsa)
        {
            PrivateKey = rsa.ExportRSAPrivateKey();
            PublicKey = rsa.ExportRSAPublicKey();
        }

        public RSA ToRsa()
        {
            var rsa = RSA.Create();

            rsa.ImportRSAPrivateKey(PrivateKey, out int bytesRead);
            if (bytesRead == 0) throw new Exception("Failed to read private key");

            //rsa.ImportRSAPublicKey(PublicKey, out bytesRead);
            //if (bytesRead == 0) throw new Exception("Failed to read private key");

            return rsa;
        }

        public string ExportPemPrivateKey() => ExportKey(KeyType.Private);
        public string ExportPemPublicKey() => ExportKey(KeyType.Public);

        public string ExportOpenSSHPublicKey(string comment = "GCloud")
        {
            return $"ssh-rsa {Convert.ToBase64String(PublicKey)} {comment}";
        }

        private string ExportKey(KeyType type)
        {
            var builder = new StringBuilder();
            string base64String;

            if (type == KeyType.Private)
            {
                builder.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
                base64String = Convert.ToBase64String(PrivateKey);
            }
            else
            {
                builder.AppendLine("-----BEGIN RSA PUBLIC KEY-----");
                base64String = Convert.ToBase64String(PublicKey);
            }

            var offset = 0;
            const int PEM_LINE_LENGTH = 64;

            while (offset < base64String.Length)
            {
                var lineEnd = Math.Min(offset + PEM_LINE_LENGTH, base64String.Length);
                builder.AppendLine(base64String.Substring(offset, lineEnd - offset));
                offset = lineEnd;
            }

            if (type == KeyType.Private) builder.AppendLine("-----END RSA PRIVATE KEY-----");
            else builder.AppendLine("-----END RSA PUBLIC KEY-----");

            return builder.ToString();
        }
    }
}
