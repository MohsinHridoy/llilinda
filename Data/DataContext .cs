using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using Backend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Backend.Dtos;

namespace Backend.DbContextBD
{
    public class DataContext : DbContext
    {


        public DataContext(DbContextOptions<DataContext> options)
   : base(options)
        { }

        /**************************************
               * 
               * Display All Modules
               * 
               * ***************/

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Access> Accesss { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        public DbSet<LicenseHistory> LicensesHistory { get; set; }


       
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {


            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;User " +
                "Id=postgres;Password=topadmin2023;Database=TP-PROVISIONING;");


        }

    }
}