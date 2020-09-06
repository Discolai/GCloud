using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GCloud.Dtos.Auth
{
    public class SignupDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 6)]
        public string Password1 { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 6)]
        public string Password2 { get; set; }
    }
}
