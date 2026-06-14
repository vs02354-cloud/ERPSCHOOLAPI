using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Models;

namespace SchoolERP.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Timetable> Timetables { get; set; }
        public DbSet<Homework> Homeworks { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }

        // Phase 3: Finance
        public DbSet<FeeStructure> FeeStructures { get; set; }
        public DbSet<FeePayment> FeePayments { get; set; }

        // Phase 3: Student Administration
        public DbSet<TransferCertificate> TransferCertificates { get; set; }

        // Phase 3: HR
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<SalarySlip> SalarySlips { get; set; }

        // Phase 4: Library, Transport, Communication
        public DbSet<Book> Books { get; set; }
        public DbSet<BookIssue> BookIssues { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<TransportRoute> TransportRoutes { get; set; }
        public DbSet<TransportRouteStop> TransportRouteStops { get; set; }
        public DbSet<TransportGatePass> TransportGatePasses { get; set; }
        public DbSet<TransportAttendance> TransportAttendances { get; set; }
        public DbSet<DriverLocation> DriverLocations { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<AdmissionInquiry> AdmissionInquiries { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Phase 5: Assignments
        public DbSet<AssignmentMaster> AssignmentMasters { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

        // Phase 6: Commission Management
        public DbSet<CommissionSetting> CommissionSettings { get; set; }
        public DbSet<TeacherCommission> TeacherCommissions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Phase 7: Transport Management - Prevent Cascade Delete Cycles
            builder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithMany()
                .HasForeignKey(v => v.DriverEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Attendant)
                .WithMany()
                .HasForeignKey(v => v.AttendantEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.AssignedRoute)
                .WithMany()
                .HasForeignKey(v => v.AssignedRouteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransportGatePass>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransportGatePass>()
                .HasOne(t => t.Route)
                .WithMany()
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransportGatePass>()
                .HasOne(t => t.Vehicle)
                .WithMany()
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransportAttendance>()
                .HasOne(ta => ta.Student)
                .WithMany()
                .HasForeignKey(ta => ta.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransportAttendance>()
                .HasOne(ta => ta.ScannedByEmployee)
                .WithMany()
                .HasForeignKey(ta => ta.ScannedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DriverLocation>()
                .HasOne(dl => dl.Vehicle)
                .WithMany()
                .HasForeignKey(dl => dl.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DriverLocation>()
                .HasOne(dl => dl.DriverEmployee)
                .WithMany()
                .HasForeignKey(dl => dl.DriverEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Phase 6: Commission Management - Prevent Cascade Delete Cycles
            builder.Entity<TeacherCommission>()
                .HasOne(tc => tc.Employee)
                .WithMany()
                .HasForeignKey(tc => tc.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherCommission>()
                .HasOne(tc => tc.Student)
                .WithMany()
                .HasForeignKey(tc => tc.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherCommission>()
                .HasOne(tc => tc.FeePayment)
                .WithMany()
                .HasForeignKey(tc => tc.FeePaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
