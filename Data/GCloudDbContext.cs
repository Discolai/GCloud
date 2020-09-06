using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCloud.Data
{
    public class GCloudDbContext : IdentityDbContext<IdentityUser>
    {
        public GCloudDbContext(DbContextOptions options) : base(options) { }
    }
}
