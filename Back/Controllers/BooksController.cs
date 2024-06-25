using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
    public async Task<IActionResult> AddBook([FromBody] Book book)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
        if (userIdClaim == null)
        {
            return Unauthorized("Reivindicação de ID de usuário não encontrada.");
        }

        book.UserId = userIdClaim.Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return Ok(book);
    }

    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> RemoveBook(int id)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
        if (userIdClaim == null)
        {
            return Unauthorized("Reivindicação de ID de usuário não encontrada.");
        }

        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound("Livro não encontrado.");
        }

        if (book.UserId != userIdClaim.Value)
        {
            return Forbid("Você não está autorizado a excluir este livro.");
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
        if (userIdClaim == null)
        {
            return Unauthorized("Reivindicação de ID de usuário não encontrada.");
        }

        var userId = userIdClaim.Value;
        var books = await _context.Books.Where(b => b.UserId == userId).ToListAsync();
        if (!books.Any())
        {
            return NotFound("Nenhum livro encontrado para o usuário.");
        }
        return Ok(books);
    }
}
