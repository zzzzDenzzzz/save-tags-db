using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
    }
}
