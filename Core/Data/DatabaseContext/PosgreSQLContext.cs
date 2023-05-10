using Core.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Data.DatabaseContext
{
    public class PosgreSQLContext : DbContext
    {
        public PosgreSQLContext() : base() { }
        public PosgreSQLContext(DbContextOptions<PosgreSQLContext> options) : base(options) { }

        public DbSet<User> User { get; set; } = null!;
        public DbSet<UserGroup> UserGroup { get; set; } = null!;
        public DbSet<UserState> UserState { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            UserModelConfiguration(modelBuilder);
            UserGroupModelConfiguration(modelBuilder);
            UserStateModelConfiguration(modelBuilder);


            modelBuilder.Entity<User>()
                .HasOne(u => u.UserGroup)
                .WithOne(ug => ug.User)
                .HasForeignKey<UserGroup>(ug => ug.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserState)
                .WithOne(us => us.User)
                .HasForeignKey<UserState>(us => us.UserId);
        }

        /// <summary>
        /// Sets the column names for the User model
        /// </summary>
        private static void UserModelConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(p => p.Id)
                .HasColumnName("id");

            modelBuilder.Entity<User>()
                .Property(p => p.Login)
                .HasColumnName("login");

            modelBuilder.Entity<User>()
                .Property(p => p.Password)
                .HasColumnName("password");

            modelBuilder.Entity<User>()
                .Property(p => p.CreatedDate)
                .HasColumnName("created_date");
        }

        /// <summary>
        /// Sets the column names for the UserGroup model
        /// </summary>
        private static void UserGroupModelConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserGroup>()
                .Property(p => p.Id)
                .HasColumnName("id");

            modelBuilder.Entity<UserGroup>()
                .Property(p => p.Code)
                .HasColumnName("code");

            modelBuilder.Entity<UserGroup>()
                .Property(p => p.Description)
                .HasColumnName("description");
        }

        /// <summary>
        /// Sets the column names for the UserState model
        /// </summary>
        private static void UserStateModelConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserState>()
                .Property(p => p.Id)
                .HasColumnName("id");

            modelBuilder.Entity<UserState>()
               .Property(p => p.Code)
               .HasColumnName("code");

            modelBuilder.Entity<UserState>()
               .Property(p => p.Description)
               .HasColumnName("description");
        }
    }
}
