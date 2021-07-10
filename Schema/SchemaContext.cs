using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Schema
{
    public class SchemaContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ServerUser> ServerUsers { get; set; }
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
        public long RepCount { get; set; }
        public DateTime? LastRep { get; set; }
    }

    [Keyless]
    public class ServerUser
    {
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
    }

    public class Balance
    {
        [Key]
        public ulong UserId { get; set; }
        public long Bal { get; set; }
        public DateTime? LastClaim { get; set; }
    }

    public class Experience
    {
        [Key]
        public ulong UserId { get; set; }
        public int Level { get; set; }
        public long Exp { get; set; }
        public DateTime? LastMessage { get; set; }
    }
}
