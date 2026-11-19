using IS_AdvokatskeKancelarije.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        // GET: api/Search?query=...&caseId=...&top=5
        [HttpGet]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] Guid? caseId,
            [FromQuery] int top = 5,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required");

            top = Math.Clamp(top, 1, 20);

            try
            {
                var hits = await _searchService.SearchAsync(query.Trim(), caseId, top, ct);
                return Ok(hits);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "The embedding service (Ollama) is not available. Make sure Ollama is running.");
            }
        }
    }
}
