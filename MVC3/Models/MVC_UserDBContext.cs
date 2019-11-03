namespace MVC3.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MVC_UserDBContext : DbContext
    {
        public MVC_UserDBContext()
            : base("name=MVC_UserDB")
        {
        }

        public virtual DbSet<UserTable> UserTables { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTable>()
                .Property(e => e.UserSex)
                .IsFixedLength();
        }
    }
}
