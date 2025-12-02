using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrimsonBookStore.Api.Controllers;

[ApiController]
[Route("api/sell-submissions")]
public class SellSubmissionsController : ControllerBase
{
    private readonly ISellSubmissionService _submissionService;

    public SellSubmissionsController(ISellSubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubmission([FromBody] SellSubmissionRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body is required" });
        }
        
        if (request.UserID <= 0)
        {
            return BadRequest(new { message = "UserID is required" });
        }
        
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Title is required" });
        }
        
        if (string.IsNullOrWhiteSpace(request.ISBN))
        {
            return BadRequest(new { message = "ISBN is required" });
        }
        
        try
        {
            var submission = await _submissionService.CreateSubmissionAsync(request.UserID, request);
            return CreatedAtAction(nameof(GetSubmission), new { id = submission.SubmissionID }, submission);
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging
            Console.WriteLine($"Error creating submission: {ex}");
            Console.WriteLine($"Inner exception: {ex.InnerException}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Return the actual exception message
            var errorMessage = ex.Message ?? "An error occurred while creating the submission";
            if (ex.InnerException != null)
            {
                errorMessage += $" ({ex.InnerException.Message})";
            }
            
            return StatusCode(500, new { message = errorMessage });
        }
    }

    [HttpGet("customer/{userId}")]
    public async Task<IActionResult> GetCustomerSubmissions(int userId)
    {
        var submissions = await _submissionService.GetSubmissionsByUserIdAsync(userId);
        return Ok(submissions);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSubmissions()
    {
        // TODO: Add admin authorization check
        var submissions = await _submissionService.GetAllSubmissionsAsync();
        return Ok(submissions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubmission(int id)
    {
        var submissions = await _submissionService.GetAllSubmissionsAsync();
        var submission = submissions.FirstOrDefault(s => s.SubmissionID == id);
        if (submission == null)
        {
            return NotFound();
        }
        return Ok(submission);
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveSubmission(int id)
    {
        // TODO: Add admin authorization check
        try
        {
            var success = await _submissionService.ApproveSubmissionAsync(id);
            if (!success)
            {
                return BadRequest(new { message = "Submission not found or already processed" });
            }
            return Ok(new { message = "Submission approved" });
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging
            Console.WriteLine($"Error approving submission: {ex}");
            Console.WriteLine($"Inner exception: {ex.InnerException}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Return the actual exception message
            var errorMessage = ex.Message ?? "An error occurred while approving the submission";
            if (ex.InnerException != null)
            {
                errorMessage += $" ({ex.InnerException.Message})";
            }
            
            return StatusCode(500, new { message = errorMessage });
        }
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectSubmission(int id)
    {
        // TODO: Add admin authorization check
        try
        {
            var success = await _submissionService.RejectSubmissionAsync(id);
            if (!success)
            {
                return BadRequest(new { message = "Submission not found or already processed" });
            }
            return Ok(new { message = "Submission rejected" });
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging
            Console.WriteLine($"Error rejecting submission: {ex}");
            Console.WriteLine($"Inner exception: {ex.InnerException}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Return the actual exception message
            var errorMessage = ex.Message ?? "An error occurred while rejecting the submission";
            if (ex.InnerException != null)
            {
                errorMessage += $" ({ex.InnerException.Message})";
            }
            
            return StatusCode(500, new { message = errorMessage });
        }
    }
}

