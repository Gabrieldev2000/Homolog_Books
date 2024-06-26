using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public BooksController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchBooks(string query)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetStringAsync($"https://www.googleapis.com/books/v1/volumes?q={query}");
        return Ok(response);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddBook([FromBody] AddBookRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            GoogleBooksId = request.GoogleBooksId,
            UserId = request.UserId
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return Ok(book);
    }

    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> RemoveBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks(string id = null, string userName = null, string email = null)
    {
        IQueryable<Book> query = _context.Books;

        if (!string.IsNullOrEmpty(id))
        {
            query = query.Where(b => b.UserId == id);
        }
        else if (!string.IsNullOrEmpty(userName))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user != null)
            {
                query = query.Where(b => b.UserId == user.Id);
            }
        }
        else if (!string.IsNullOrEmpty(email))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                query = query.Where(b => b.UserId == user.Id);
            }
        }

        var books = await query.ToListAsync();
        if (!books.Any())
        {
            return NotFound("Nenhum livro encontrado para o usuário.");
        }
        return Ok(books);
    }
}

public class AddBookRequest
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string GoogleBooksId { get; set; }
    public string UserId { get; set; }
}
