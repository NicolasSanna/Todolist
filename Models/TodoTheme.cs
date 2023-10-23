namespace Todolist.Models
{
    public class TodoTheme
    {
        public int TodoId { get; set; }
        public Todo? Todo { get; set; }

        public int ThemeId { get; set; }
        public Theme? Theme { get; set; }
    }
}
