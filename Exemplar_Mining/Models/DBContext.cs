using Microsoft.EntityFrameworkCore;

namespace Exemplar_Mining.Models
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {

        }

        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<Leave> Leave { get; set; }
        public virtual DbSet<Performance> Performance { get; set; }
        public virtual DbSet<Mine> Mine { get; set; }
        public virtual DbSet<Resource> Resource { get; set; }
        public virtual DbSet<Production> Production { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepId)
                    .HasName("department_pk");

                entity.ToTable("department", "hr");

                entity.HasIndex(e => e.DepartmentName)
                    .HasName("departmentname_unq")
                    .IsUnique();

                entity.Property(e => e.DepId).HasColumnName("dep_id");

                entity.Property(e => e.DateEstablished)
                    .HasColumnName("date_established")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.DepartmentName)
                    .IsRequired()
                    .HasColumnName("department_name")
                    .HasColumnType("citext");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmpId)
                    .HasName("employee_pk");

                entity.ToTable("employee", "hr");

                entity.HasIndex(e => new { e.FirstName, e.LastName })
                .HasName("employeename_unq")
                .IsUnique();

                entity.Property(e => e.EmpId).HasColumnName("emp_id");

                entity.Property(e => e.DepId).HasColumnName("dep_id");

                entity.Property(e => e.EnrollmentDate)
                    .HasColumnName("enrollment_date")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name")
                    .HasColumnType("citext");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("last_name")
                    .HasColumnType("citext");

                entity.Property(e => e.ManagerId).HasColumnName("manager_id");

                entity.Property(e => e.Position)
                    .IsRequired()
                    .HasColumnName("position")
                    .HasColumnType("citext");

                entity.Property(e => e.Salary)
                    .HasColumnName("salary")
                    .HasColumnType("numeric");

                entity.HasOne(d => d.Dep)
                    .WithMany(p => p.Employee)
                    .HasForeignKey(d => d.DepId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("employee_dep_id_fkey");

                entity.HasOne(d => d.Manager)
                    .WithMany(p => p.InverseManager)
                    .HasForeignKey(d => d.ManagerId)
                    .HasConstraintName("employee_manager_id_fkey");
            });

            modelBuilder.Entity<Leave>(entity =>
            {
                entity.HasKey(e => new { e.EmpId, e.LeaveType })
                    .HasName("leave_pk");

                entity.ToTable("leave", "hr");

                entity.Property(e => e.EmpId).HasColumnName("emp_id");

                entity.Property(e => e.LeaveType)
                    .HasColumnName("leave_type")
                    .HasColumnType("character varying");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.HasOne(d => d.Emp)
                    .WithMany(p => p.Leave)
                    .HasForeignKey(d => d.EmpId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("leave_fk");
            });

            modelBuilder.Entity<Performance>(entity =>
            {
                entity.HasKey(e => new { e.EmpId, e.PerformanceType })
                    .HasName("performance_pk");

                entity.ToTable("performance", "hr");

                entity.Property(e => e.EmpId).HasColumnName("emp_id");

                entity.Property(e => e.PerformanceType)
                    .HasColumnName("performance_type")
                    .HasColumnType("character varying");

                entity.Property(e => e.Rating).HasColumnName("rating");

                entity.HasOne(d => d.Emp)
                    .WithMany(p => p.Performance)
                    .HasForeignKey(d => d.EmpId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("performance_fk");
            });

            modelBuilder.Entity<Mine>(entity =>
            {
                entity.HasKey(e => e.MineId)
                    .HasName("mine_pk");

                entity.ToTable("mine", "operations");

                entity.HasIndex(e => e.Name)
                    .HasName("minename_unq")
                    .IsUnique();

                entity.Property(e => e.MineId).HasColumnName("mineid");

                entity.Property(e => e.Lattitude)
                    .HasColumnName("lattitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("citext");

                entity.Property(e => e.OverseerId).HasColumnName("overseerid");

                entity.HasOne(e => e.Emp)
                    .WithMany(p => p.Mines)
                    .HasForeignKey(d => d.OverseerId)
                    .HasConstraintName("mine_overseer_fk");

                entity.HasOne(e => e.Res)
                   .WithMany(p => p.Mines)
                   .HasForeignKey(d => d.Type)
                   .HasConstraintName("mine_resource_fk");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasColumnType("citext");
            });

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(e => e.Type)
                    .HasName("resource_pk");

                entity.ToTable("resource", "operations");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("citext");

                entity.Property(e => e.Metric)
                    .IsRequired()
                    .HasColumnName("metric")
                    .HasColumnType("character varying")
                    .HasDefaultValueSql("'ton'::character varying");

                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasColumnType("numeric");
            });

            modelBuilder.Entity<Production>(entity =>
            {
                entity.ToTable("production", "operations");

                entity.HasIndex(e => new { e.MineId, e.Datelogged })
                .HasName("productiondaily_unq")
                .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("numeric")
                    .HasComment("In Metric Tons");

                entity.Property(e => e.Datelogged)
                    .HasColumnName("datelogged")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(now())::date");

                entity.Property(e => e.MineId).HasColumnName("mineid");

                entity.HasOne(d => d.Mine)
                    .WithMany(p => p.Production)
                    .HasForeignKey(d => d.MineId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("production_fk");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
