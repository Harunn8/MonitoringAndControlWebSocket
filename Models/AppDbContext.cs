using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}

        public DbSet<ScriptModels> Scripts { get; set; }
        public DbSet<SnmpDevice> Devices { get; set; }
        public DbSet<TcpDeviceV2> TcpDevices { get; set; }
        public DbSet<UserV2> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ScriptModels>().ToTable("Scripts");
            modelBuilder.Entity<ScriptModels>().HasKey(s => s.Id);
            modelBuilder.Entity<ScriptModels>().Property(s => s.ScriptName).IsRequired();
            modelBuilder.Entity<ScriptModels>().Property(s => s.Script).IsRequired();
            modelBuilder.Entity<ScriptModels>().Property(s => s.CreatedDate).IsRequired();
            modelBuilder.Entity<ScriptModels>().Property(s => s.UpdatedDate).IsRequired();
        }
    }
}
