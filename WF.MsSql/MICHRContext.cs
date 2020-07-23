namespace WF.MsSql
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;


    public partial class MICHRContext : DbContext
    {
        private readonly IConfiguration _config;

        public MICHRContext(IConfiguration config)
        {
            _config = config;
        }

        public virtual DbSet<aspnet_Roles> aspnet_Roles { get; set; }
        public virtual DbSet<aspnet_Users> aspnet_Users { get; set; }
        public virtual DbSet<aspnet_Membership> aspnet_Memberships { get; set; }
        public virtual DbSet<aspnet_UsersInRoles> aspnet_UsersInRoles { get; set; }
        public virtual DbSet<DM_WorkflowCommand> DM_WorkflowCommands { get; set; }
        public virtual DbSet<DM_WorkflowState> DM_WorkflowStates { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_config.GetConnectionString("MICHRConnection"));
            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<aspnet_UsersInRoles>()
                .HasKey(x => new { x.RoleId, x.UserId, x.ApplicationId });
        }
    }
}
