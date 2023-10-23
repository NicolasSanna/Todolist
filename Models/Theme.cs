namespace Todolist.Models
{
    public class Theme
    {
        public int ThemeId { get; set; }
        public string? Name { get; set; }

        // Propriété de navigation
        public ICollection<TodoTheme>? TodoThemes { get; set; }
    }
    
}
