using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.AppDbContext
{
    /// <summary>
    /// 配置数据库上下文
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Admin> Admin { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //显示处理User表（避免SQL server关键字冲突）
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<BorrowRecord>()
                .ToTable("BorrowRecord")
                .HasOne(br => br.Book)
                .WithMany(b => b.BorrowRecords)
                .HasForeignKey(br => br.BookId)
                .IsRequired(false);
                
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Book>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Publisher>().ToTable("Publisher");
            modelBuilder.Entity<Admin>().ToTable("Admin");
        }
    }
}
