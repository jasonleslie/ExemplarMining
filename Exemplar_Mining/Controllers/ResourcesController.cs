using Exemplar_Mining.Models;
using Exemplar_Mining.Models.DTOs;
using Exemplar_Mining.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exemplar_Mining.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly DBContext _context;

        public ResourcesController(DBContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResourceDTO>>> GetResource()
        {
            return await _context.Resource.Select(x => new ResourceDTO()
            {
                Type = x.Type,
                Metric = x.Metric,
                Value = x.Value
            })
            .ToListAsync();
        }


        [HttpPost]
        public async Task<ActionResult<Resource>> PostResource(Resource resource)
        {
            _context.Resource.Add(resource);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ResourceExists(resource.Type))
                {
                    return Conflict(new APIResponse()
                    {
                        code = 409,
                        message = String.Format("Resource of type '{0}' already exists.", resource.Type)
                    });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new BaseAPIResponse() { message = String.Format("Resource '{0}' has been created.", resource.Type) });
        }


        [HttpDelete("name/{resourceType}")]
        public async Task<ActionResult<Resource>> DeleteResource(string resourceType)
        {
            var resource = await _context.Resource.FindAsync(resourceType);

            if (resource == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("Resource of type '{0}' does not exist.", resourceType)
                });
            }

            _context.Resource.Remove(resource);
            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Resource '{0}' has been deleted.", resource.Type) });
        }

        private bool ResourceExists(string id)
        {
            return _context.Resource.Any(e => e.Type == id);
        }
    }
}
