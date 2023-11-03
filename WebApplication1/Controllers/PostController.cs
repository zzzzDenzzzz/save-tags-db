using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogDbContext blogDbContext;

        public PostController(BlogDbContext blogDbContext)
        {
            this.blogDbContext = blogDbContext;
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.categories = new SelectList(blogDbContext.Categories, "Id", "Name");
            ViewBag.tags = new MultiSelectList(blogDbContext.Tags, "Id", "Name");
            return View();
        }

        [HttpGet]
        public IActionResult Index()
        {
            var posts = blogDbContext.Posts
                .Include(x => x.PostTags)
                .ThenInclude(x => x.Tag)
                .Include(x => x.Category);
            var categories = blogDbContext.Categories;
            var tags = blogDbContext.Tags;

            var model = new IndexViewModel() { Categories = categories, Posts = posts, Tags = tags };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Post post, IFormFile Image, int[] tags)
        {
            await UploadImageAsync(post, Image);

            TempData["status"] = "Новый пост добавлен!";
            post.Date = DateTime.Now;

            await SavePostAsync(post);

            await AddPostTagsAsync(post.Id, tags);

            return RedirectToAction("Index");
        }

        static async Task UploadImageAsync(Post post, IFormFile Image)
        {
            if (Image != null)
            {
                var filename = $"{Guid.NewGuid()}{Path.GetExtension(Image.FileName)}";
                using var fs = new FileStream(@$"wwwroot/uploads/{filename}", FileMode.Create);
                await Image.CopyToAsync(fs);
                post.ImageUrl = @$"/uploads/{filename}";
            }
        }

        async Task SavePostAsync(Post post)
        {
            await blogDbContext.Posts.AddAsync(post);
            await blogDbContext.SaveChangesAsync();
        }

        async Task AddPostTagsAsync(int postId, int[] tags)
        {
            var postTags = tags.Select(t =>
                new PostTag()
                {
                    PostId = postId,
                    TagId = t
                });
            await blogDbContext.PostTags.AddRangeAsync(postTags);
            await blogDbContext.SaveChangesAsync();
        }
    }
}
