
using Supabase.Storage.Interfaces;
using TicketsSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketsSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ApiBaseController
    {
        private readonly IStorageService _storageService;
        public StorageController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet("getfileurl")]
        [Authorize]
        public async Task<IActionResult> GetFileUrl(string bucketName, string path)
        {
            var result = await _storageService.GetUrlAsync(bucketName, path);
            if (result.IsFailed)
            {
                return ProcessResult(result);
            }

            return Ok(new { url = result.Value });
        }

    }
}