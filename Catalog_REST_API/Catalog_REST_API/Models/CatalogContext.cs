using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Catalog_REST_API.Models
{
    public partial class CatalogContext : DbContext
    {
        public CatalogContext()
        {
        }

        public CatalogContext(DbContextOptions<CatalogContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Attributes> Attributes { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CategoryAttributes> CategoryAttributes { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<Manufacturer> Manufacturer { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductValue> ProductValue { get; set; }
        public virtual DbSet<Qualification> Qualification { get; set; }
        public virtual DbSet<Values> Values { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=catalog-server.cbcwlzi7lckf.us-east-2.rds.amazonaws.com;database=Catalog;uid=admin;pwd=Dima1490;treattinyasboolean=true", x => x.ServerVersion("8.0.17-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attributes>(entity =>
            {
                entity.HasKey(e => e.IdAttributes)
                    .HasName("PRIMARY");

                entity.Property(e => e.IdAttributes)
                    .HasColumnName("idAttributes")
                    .HasColumnType("int(100)");

                entity.Property(e => e.NameAttribute)
                    .IsRequired()
                    .HasColumnName("nameAttribute")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.IdCategory)
                    .HasName("PRIMARY");

                entity.Property(e => e.IdCategory)
                    .HasColumnName("idCategory")
                    .HasColumnType("int(11)");

                entity.Property(e => e.NameCategory)
                    .IsRequired()
                    .HasColumnName("nameCategory")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<CategoryAttributes>(entity =>
            {
                entity.HasKey(e => new { e.IdCategory, e.IdAttribute })
                    .HasName("PRIMARY");

                entity.ToTable("Category_Attributes");

                entity.HasIndex(e => e.IdAttribute)
                    .HasName("fk_Attributes_categoryAttributes_idx");

                entity.Property(e => e.IdCategory)
                    .HasColumnName("idCategory")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IdAttribute)
                    .HasColumnName("idAttribute")
                    .HasColumnType("int(100)");

                entity.HasOne(d => d.IdAttributeNavigation)
                    .WithMany(p => p.CategoryAttributes)
                    .HasForeignKey(d => d.IdAttribute)
                    .HasConstraintName("fk_Attributes_categoryAttributes");

                entity.HasOne(d => d.IdCategoryNavigation)
                    .WithMany(p => p.CategoryAttributes)
                    .HasForeignKey(d => d.IdCategory)
                    .HasConstraintName("fk_category_categoryAttributes");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.IdCountry)
                    .HasName("PRIMARY");

                entity.Property(e => e.IdCountry)
                    .HasColumnName("idCountry")
                    .HasColumnType("int(11)");

                entity.Property(e => e.NameCountry)
                    .IsRequired()
                    .HasColumnName("nameCountry")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.HasKey(e => e.IdManufacturer)
                    .HasName("PRIMARY");

                entity.Property(e => e.IdManufacturer)
                    .HasColumnName("idManufacturer")
                    .HasColumnType("int(11)");

                entity.Property(e => e.NameManufacturer)
                    .IsRequired()
                    .HasColumnName("nameManufacturer")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.IdProduct)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.IdCategory)
                    .HasName("fk_category_product_idx");

                entity.HasIndex(e => e.IdCountry)
                    .HasName("fk_country_product_idx");

                entity.HasIndex(e => e.IdManufacturer)
                    .HasName("fk_firm_product_idx");

                entity.HasIndex(e => e.IdQualification)
                    .HasName("fk_qualification_product_idx");

                entity.Property(e => e.IdProduct)
                    .HasColumnName("idProduct")
                    .HasColumnType("int(100)");

                entity.Property(e => e.Description)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IdCategory)
                    .HasColumnName("idCategory")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IdCountry)
                    .HasColumnName("idCountry")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IdManufacturer)
                    .HasColumnName("idManufacturer")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IdQualification)
                    .HasColumnName("idQualification")
                    .HasColumnType("int(11)");

                entity.Property(e => e.NameProduct)
                    .IsRequired()
                    .HasColumnName("nameProduct")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TySmd)
                    .HasColumnName("TY/SMD")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.IdCategoryNavigation)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.IdCategory)
                    .HasConstraintName("fk_category_product");

                entity.HasOne(d => d.IdCountryNavigation)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.IdCountry)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_country_product");

                entity.HasOne(d => d.IdManufacturerNavigation)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.IdManufacturer)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_firm_product");

                entity.HasOne(d => d.IdQualificationNavigation)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.IdQualification)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_qualification_product");
            });

            modelBuilder.Entity<ProductValue>(entity =>
            {
                entity.HasKey(e => new { e.IdProduct, e.IdValues })
                    .HasName("PRIMARY");

                entity.ToTable("Product_value");

                entity.HasIndex(e => e.IdValues)
                    .HasName("fk_value_Product_value_idx");

                entity.Property(e => e.IdProduct)
                    .HasColumnName("idProduct")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IdValues)
                    .HasColumnName("idValues")
                    .HasColumnType("int(100)");

                entity.HasOne(d => d.IdProductNavigation)
                    .WithMany(p => p.ProductValue)
                    .HasForeignKey(d => d.IdProduct)
                    .HasConstraintName("fk_product_productvalue");

                entity.HasOne(d => d.IdValuesNavigation)
                    .WithMany(p => p.ProductValue)
                    .HasForeignKey(d => d.IdValues)
                    .HasConstraintName("fk_value_Product_value");
            });

            modelBuilder.Entity<Qualification>(entity =>
            {
                entity.HasKey(e => e.IdQualification)
                    .HasName("PRIMARY");

                entity.Property(e => e.IdQualification)
                    .HasColumnName("idQualification")
                    .HasColumnType("int(11)");

                entity.Property(e => e.NameQualification)
                    .IsRequired()
                    .HasColumnName("nameQualification")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Values>(entity =>
            {
                entity.HasKey(e => e.IdValues)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.IdAttribute)
                    .HasName("fk_values_attributes_idx");

                entity.Property(e => e.IdValues)
                    .HasColumnName("idValues")
                    .HasColumnType("int(100)");

                entity.Property(e => e.IdAttribute)
                    .HasColumnName("idAttribute")
                    .HasColumnType("int(100)");

                entity.Property(e => e.Units)
                    .HasColumnName("units")
                    .HasColumnType("varchar(10)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Values1)
                    .IsRequired()
                    .HasColumnName("values")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.IdAttributeNavigation)
                    .WithMany(p => p.Values)
                    .HasForeignKey(d => d.IdAttribute)
                    .HasConstraintName("fk_values_attributes");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
