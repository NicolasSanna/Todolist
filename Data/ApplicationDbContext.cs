using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Todolist.Models;

namespace Todolist.Data
{
    public class ApplicationDbContext : IdentityDbContext  // Définit la classe ApplicationDbContext qui hérite d'IdentityDbContext.
    {
        // DbSet permet au contexte de gérer les opérations CRUD pour chaque entité spécifiée.

        public DbSet<Todo> Todos { get; set; } // DbSet pour l'entité Todo.
        public DbSet<Category> Categories { get; set; } // DbSet pour l'entité Category.
        public DbSet<TodoTheme> TodoThemes { get; set; } // DbSet pour l'entité TodoTheme.
        public DbSet<Theme> Themes { get; set; } // DbSet pour l'entité Theme.

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            // Constructeur de la classe, prenant des options de contexte et appelant le constructeur de la classe de base.
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Appel de la méthode OnModelCreating de la classe de base.

            // Configuration des relations entre les entités.

            modelBuilder.Entity<Todo>()
                .HasOne(t => t.Category) // Un Todo a une Category associée.
                .WithMany(c => c.Todos) // Une Category peut avoir plusieurs Todos associés.
                .HasForeignKey(t => t.CategoryId); // La clé étrangère dans Todo est CategoryId.

            modelBuilder.Entity<Todo>()
                .HasOne(t => t.User) // Un Todo a un utilisateur associé.
                .WithMany() // Un utilisateur peut avoir plusieurs Todos associés.
                .HasForeignKey(t => t.UserId) // La clé étrangère dans Todo est UserId.
                .IsRequired(true); // Le champ UserId est obligatoire.

            modelBuilder.Entity<TodoTheme>()
                .HasKey(tt => new { tt.TodoId, tt.ThemeId }); // Configuration de la clé primaire composée.

            modelBuilder.Entity<TodoTheme>()
                .HasOne(tt => tt.Todo) // Un TodoTheme a un Todo associé.
                .WithMany(t => t.TodoThemes) // Un Todo peut avoir plusieurs TodoThemes associés.
                .HasForeignKey(tt => tt.TodoId); // La clé étrangère dans TodoTheme est TodoId.

            modelBuilder.Entity<TodoTheme>()
                .HasOne(tt => tt.Theme) // Un TodoTheme a un Theme associé.
                .WithMany(t => t.TodoThemes) // Un Theme peut avoir plusieurs TodoThemes associés.
                .HasForeignKey(tt => tt.ThemeId); // La clé étrangère dans TodoTheme est ThemeId.
        }


    }
}