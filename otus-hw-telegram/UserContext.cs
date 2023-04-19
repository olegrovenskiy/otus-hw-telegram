using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
class UserContext : DbContext
{


    public DbSet<Customer> Customers { get; set; }

    public DbSet<TicketDB> Tickets { get; set; }


    public UserContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=PostgreeTelegram;Username=postgres;Password=12345");
    }

}

