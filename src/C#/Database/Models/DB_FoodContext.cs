using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Food_Database.Descriptors;

namespace Food_Database.Models;

public partial class DB_FoodContext : DbContext
{
    public DB_FoodContext(DbContextOptions<DB_FoodContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Food_DBEntity> Food { get; set; }

    public virtual DbSet<FoodIngredients_DBEntity> FoodIngredients { get; set; }

    public virtual DbSet<UserCreatedFood_DBEntity> UserCreatedFood { get; set; }

    public virtual DbSet<UserFoodOptions_DBEntity> UserFoodOptions { get; set; }

    public virtual DbSet<UserFoodRemarks_DBEntity> UserFoodRemark { get; set; }

    public virtual DbSet<Users_DBEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Food_DBEntity>(entity =>
        {
            entity.Property(e => e.Carbs_Sugar).HasColumnType("decimal(16, 2)");
            entity.Property(e => e.Carbs_Total).HasColumnType("decimal(16, 2)");
            entity.Property(e => e.Fat_Saturated).HasColumnType("decimal(16, 2)");
            entity.Property(e => e.Fat_Total).HasColumnType("decimal(16, 2)");
            entity.Property(e => e.Food_Description).HasMaxLength(4000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Protein_Total).HasColumnType("decimal(16, 2)");
            entity.Property(e => e.Salt_Total).HasColumnType("decimal(16, 2)");
        });

        modelBuilder.Entity<FoodIngredients_DBEntity>(entity =>
        {
            entity.HasKey(e => new { e.Food_Id, e.Ingredient_Food_Id });

            entity.Property(e => e.Weight_Ingredient_Normalised).HasColumnType("decimal(16, 2)");

            entity.HasOne(d => d.Food).WithMany(p => p.FoodIngredientsFood)
                .HasForeignKey(d => d.Food_Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FoodIngredients_Food");

            entity.HasOne(d => d.Ingredient_Food).WithMany(p => p.FoodIngredientsIngredient_Food)
                .HasForeignKey(d => d.Ingredient_Food_Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FoodIngredients_IngredientFood");
        });

        modelBuilder.Entity<UserCreatedFood_DBEntity>(entity =>
        {
            entity.HasKey(e => e.Food_Id);

            entity.HasIndex(e => e.User_Id, "IX_UserCreatedFood_User_Id");

            entity.Property(e => e.Food_Id).ValueGeneratedNever();

            entity.HasOne(d => d.Food).WithOne(p => p.UserCreatedFood)
                .HasForeignKey<UserCreatedFood_DBEntity>(d => d.Food_Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserCreatedFood_Food");

            entity.HasOne(d => d.User).WithMany(p => p.UserCreatedFood)
                .HasForeignKey(d => d.User_Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserCreatedFood_User");
        });

        modelBuilder.Entity<UserFoodOptions_DBEntity>(entity =>
        {
            entity.HasKey(e => new { e.User_Id, e.Food_Id });

            entity.Property(e => e.Price_Eur).HasColumnType("decimal(16, 4)");
            entity.Property(e => e.Weight_Total).HasColumnType("decimal(16, 4)");

            entity.HasOne(d => d.Food).WithMany(p => p.UserFoodOptions)
                .HasForeignKey(d => d.Food_Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFoodOptions_Food");

            entity.HasOne(d => d.User).WithMany(p => p.UserFoodOptions)
                .HasForeignKey(d => d.User_Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFoodOptions_User");
        });

        modelBuilder.Entity<UserFoodRemarks_DBEntity>(entity =>
        {
            entity.HasKey(e => new { e.User_Id, e.Food_Id });

            entity.Property(e => e.Food_Remark).HasMaxLength(4000);

            entity.HasOne(d => d.Food).WithMany(p => p.UserFoodRemark)
                .HasForeignKey(d => d.Food_Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFoodRemark_Food");

            entity.HasOne(d => d.User).WithMany(p => p.UserFoodRemark)
                .HasForeignKey(d => d.User_Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFoodRemark_User");
        });

        modelBuilder.Entity<Users_DBEntity>(entity =>
        {
            entity.HasMany(d => d.Food).WithMany(p => p.User)
                .UsingEntity<Dictionary<string, object>>(
                    "UserFoodFavorites",
                    r => r.HasOne<Food_DBEntity>().WithMany()
                        .HasForeignKey("Food_Id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserFoodFavorites_Food"),
                    l => l.HasOne<Users_DBEntity>().WithMany()
                        .HasForeignKey("User_Id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserFoodFavorites_User"),
                    j =>
                    {
                        j.HasKey("User_Id", "Food_Id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}