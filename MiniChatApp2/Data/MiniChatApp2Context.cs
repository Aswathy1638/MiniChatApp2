using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniChatApp2.Model;

namespace MiniChatApp2.Data
{
    public class MiniChatApp2Context : DbContext
    {
        public MiniChatApp2Context (DbContextOptions<MiniChatApp2Context> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>().ToTable("User");
            base.OnModelCreating(modelBuilder);
        }
    }
}
