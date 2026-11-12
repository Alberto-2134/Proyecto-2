using Microsoft.EntityFrameworkCore;
using EcosistemaAcademico.Models.Entidades;

namespace EcosistemaAcademico.Models.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Tutor> Tutores { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Asignatura> Asignaturas { get; set; }
        public DbSet<TutorAsignatura> TutorAsignaturas { get; set; }
        public DbSet<DiagnosticoAcademico> Diagnosticos { get; set; }
        public DbSet<Emparejamiento> Emparejamientos { get; set; }
        public DbSet<Repositorio> Repositorios { get; set; }
        public DbSet<RecursoAcademico> Recursos { get; set; }
        public DbSet<SeguimientoAcademico> Seguimientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TPT: cada subclase tiene su propia tabla
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Estudiante>().ToTable("Estudiantes");
            modelBuilder.Entity<Tutor>().ToTable("Tutores");
            modelBuilder.Entity<Administrador>().ToTable("Administradores");

            // Evita el error "multiple cascade paths" de SQL Server
            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys())
                         .Where(f => !f.IsOwnership))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(5, 2);
        }
    }
}
