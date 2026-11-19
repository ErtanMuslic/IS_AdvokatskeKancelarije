using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssistantController : ControllerBase
    {
        private readonly RagService _ragService;

        public AssistantController(RagService ragService)
        {
            _ragService = ragService;
        }

        // POST: api/Assistant/ask
        [HttpPost("ask")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public async Task<IActionResult> Ask([FromBody] AskDTO dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Question))
                return BadRequest("Question is required");

            if (dto.Question.Length > 1000)
                return BadRequest("Question is too long (max 1000 characters)");

            try
            {
                var answer = await _ragService.AskAsync(dto.Question.Trim(), dto.CaseId, ct);
                return Ok(answer);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "The AI service (Ollama) is not available. Make sure Ollama is running.");
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Ollama"))
            {
                // Npr. model nije preuzet (ollama pull ...)
                return StatusCode(502, ex.Message);
            }
        }
    }
}
