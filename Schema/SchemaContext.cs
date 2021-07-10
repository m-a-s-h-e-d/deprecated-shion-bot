using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace Schema
{
    public class SchemaContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Balance> Balances { get; set; }
        public DbSet<Experience> Experiences { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionString = "server=localhost;user=root;database=shionbot;port=3306;Connect Timeout = 5;";
            var serverVersion = ServerVersion.AutoDetect(connectionString);

            options.UseMySql(connectionString, serverVersion);
        }
    }

    public class Server
    {
        [Key]
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }

    public class User
    {
        [Key]
        public ulong UserId { get; set; }
        public string EmbedColor { get; set; }
        //TODO Rep Count and Timestamp from last Rep
    }

    public class Balance
    {
        [Key]
        public ulong UserId { get; set; }
        public long Bal { get; set; }
        //TODO Timestamp from Last Daily Claim
    }

    public class Experience
    {
        [Key]
        public ulong UserId { get; set; }
        public int Level { get; set; }
        public long Exp { get; set; }
        //TODO Timestamp from Last Message
    }
}
