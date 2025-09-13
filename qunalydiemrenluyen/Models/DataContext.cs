using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Models;


namespace QUANLYDIEMRENLUYEN.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<AdminMenu> AdminMenus { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<StudentEvaluation> StudentEvaluations { get; set; }
        public DbSet<EvaluationDetail> EvaluationDetails { get; set; }
        public DbSet<Evidence> Evidences { get; set; }
        public DbSet<EvaluationConfig> EvaluationConfigs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EvaluationSummary> EvaluationSummaries { get; set; }
        public DbSet<CriteriaCategory> CriteriaCategories { get; set; }
        public DbSet<Criteria> Criterias { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<AccountProfile> AccountProfiles { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<MeetingNotification> MeetingNotifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ 1-nhiều giữa Class và Account
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Class)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.SetNull); // hoặc .Restrict nếu không muốn xóa cascade
            // Cấu hình quan hệ 1-nhiều giữa Account và EvaluationSummary
            modelBuilder.Entity<Account>()
                .HasMany(a => a.EvaluationSummaries)
                .WithOne(es => es.Account)
                .HasForeignKey(es => es.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cấu hình quan hệ 1-nhiều giữa Faculty và Class
            modelBuilder.Entity<AcademicYear>()
                .HasMany(a => a.Semesters)
                .WithOne(s => s.AcademicYear)
                .HasForeignKey(s => s.AcademicYearId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ 1-nhiều giữa Semester và StudentEvaluation
            modelBuilder.Entity<CriteriaCategory>()
                .HasMany(c => c.Criterias)
                .WithOne(cr => cr.Category) 
                .HasForeignKey(cr => cr.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cấu hình quan hệ 1-nhiều giữa Account và StudentEvaluation
            modelBuilder.Entity<Account>()
                .HasMany(a => a.StudentEvaluations)
                .WithOne(se => se.Account)
                .HasForeignKey(se => se.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ 1-nhiều giữa Account và EvaluationSummary
            modelBuilder.Entity<Account>()
                .HasMany(a => a.Notifications)
                .WithOne(n => n.Account)
                .HasForeignKey(n => n.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ 1-nhiều giữa Account và EvaluationSummary
            modelBuilder.Entity<Account>()
                .HasMany(a => a.MeetingNotifications)
                .WithOne(mn => mn.Account)
                .HasForeignKey(mn => mn.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cấu hình quan hệ 1-nhiều giữa Semester và EvaluationConfig
            modelBuilder.Entity<Class>()
                .HasMany(c => c.Accounts)
                .WithOne(a => a.Class)
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ 1-nhiều giữa Class và Attendance
            modelBuilder.Entity<Faculty>()
                .HasMany(f => f.Classes)
                .WithOne(c => c.Faculty)
                .HasForeignKey(c => c.FacultyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ 1-nhiều giữa Class và Attendance
            modelBuilder.Entity<Semester>()
                .HasMany(s => s.StudentEvaluations)
                .WithOne(se => se.Semester)
                .HasForeignKey(se => se.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cấu hình quan hệ 1-nhiều giữa Semester và EvaluationConfig
            modelBuilder.Entity<Semester>()
                .HasMany(s => s.EvaluationConfigs)
                .WithOne(ec => ec.Semester)
                .HasForeignKey(ec => ec.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cấu hình quan hệ 1-nhiều giữa Semester và EvaluationSummary
            modelBuilder.Entity<Semester>()
                .HasMany<EvaluationSummary>()
                .WithOne(es => es.Semester)
                .HasForeignKey(es => es.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cấu hình quan hệ 1-nhiều giữa Semester và Evidence
            modelBuilder.Entity<Semester>()
                .HasMany<MeetingNotification>()
                .WithOne(mn => mn.Semester)
                .HasForeignKey(mn => mn.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cấu hình quan hệ 1-nhiều giữa Semester và Evidence
            modelBuilder.Entity<AcademicYear>()
                .HasMany<MeetingNotification>()
                .WithOne(mn => mn.AcademicYear)
                .HasForeignKey(mn => mn.AcademicYearId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}