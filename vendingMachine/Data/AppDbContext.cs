using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using vendingMachine.Models;

namespace vendingMachine.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public DbSet<TransactionModel> Transaction { get; set; }
        public DbSet<ProductModel> Product { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            {
                //foreign key relationship between Products and Transations
                modelBuilder.Entity<TransactionModel>()
                    .HasOne(i => i.Product)
                    .WithMany()
                    .HasForeignKey(i => i.productId);

                modelBuilder.Entity<ProductModel>()
                    .Property(p => p.productPrice)
                    .HasColumnType("decimal(18,2)");

                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
