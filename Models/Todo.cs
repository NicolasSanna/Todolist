using Microsoft.AspNetCore.Identity;

namespace Todolist.Models
{
    public class Todo
    {
        public int TodoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public int CategoryId { get; set; }

        // Propriété de navigation
        public Category? Category { get; set; }

        // Ajoutez cette propriété pour stocker l'ID de l'utilisateur
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}
