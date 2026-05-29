using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LibraryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LibraryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Books ---

        [HttpPost("Book")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Librarian")]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        [HttpGet("Book/{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return book;
        }

        [HttpGet("Book")]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            return await _context.Books.ToListAsync();
        }

        // --- Book Issues ---

        [HttpPost("Issue")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Librarian")]
        public async Task<ActionResult<BookIssue>> IssueBook(BookIssue issue)
        {
            var book = await _context.Books.FindAsync(issue.BookId);
            if (book == null || book.AvailableCopies <= 0)
                return BadRequest("Book not available");

            book.AvailableCopies--;
            issue.IssueDate = DateTime.UtcNow;
            
            _context.BookIssues.Add(issue);
            await _context.SaveChangesAsync();
            return Ok(issue);
        }

        [HttpPut("Return/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Librarian")]
        public async Task<IActionResult> ReturnBook(int id, [FromBody] decimal fineAmount = 0)
        {
            var issue = await _context.BookIssues.Include(i => i.Book).FirstOrDefaultAsync(i => i.Id == id);
            if (issue == null) return NotFound();

            if (issue.Status == "Returned")
                return BadRequest("Book already returned");

            issue.ReturnDate = DateTime.UtcNow;
            issue.Status = "Returned";
            issue.FineAmount = fineAmount;
            
            if (issue.Book != null)
            {
                issue.Book.AvailableCopies++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Book returned successfully" });
        }
    }
}
