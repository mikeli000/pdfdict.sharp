using Microsoft.AspNetCore.Mvc;

namespace PDFDict.SDK.Sharp.Web.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public FileUploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            
            // Generate or retrieve the session ID
            string sessionId;
            if (Request.Cookies.ContainsKey("SessionId"))
            {
                sessionId = Request.Cookies["SessionId"];
            }
            else
            {
                sessionId = Guid.NewGuid().ToString();
                Response.Cookies.Append("SessionId", sessionId, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict });
            }

            // Create a unique directory based on the session ID
            var sessionFolder = Path.Combine(_environment.ContentRootPath, "Uploads", sessionId);
            Directory.CreateDirectory(sessionFolder); // Ensure the directory exists

            var filePath = Path.Combine(sessionFolder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new Dictionary<string, string> 
            {
                {  "sessionId", sessionId }
            });
        }
    }
}