namespace Todolist.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Label { get; set; }

        // Propriété de navigation
        public List<Todo>? Todos { get; set; }
    }
}
