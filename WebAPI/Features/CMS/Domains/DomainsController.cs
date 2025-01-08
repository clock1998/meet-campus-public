using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.CMS.Domains
{
    [Route("api/cms/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class DomainsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DomainsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Domains
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Domain>>> GetDomains()
        {
            return await _context.Domains.ToListAsync();
        }

        // GET: api/Domains/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Domain>> GetDomain(Guid id)
        {
            var domain = await _context.Domains.FindAsync(id);

            if (domain == null)
            {
                return NotFound();
            }

            return domain;
        }

        // PUT: api/Domains/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDomain(Guid id, Domain domain)
        {
            if (id != domain.Id)
            {
                return BadRequest();
            }

            _context.Entry(domain).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DomainExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Domains
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Domain>> PostDomain(Domain domain)
        {
            _context.Domains.Add(domain);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDomain", new { id = domain.Id }, domain);
        }

        // DELETE: api/Domains/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDomain(Guid id)
        {
            var domain = await _context.Domains.FindAsync(id);
            if (domain == null)
            {
                return NotFound();
            }

            _context.Domains.Remove(domain);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DomainExists(Guid id)
        {
            return _context.Domains.Any(e => e.Id == id);
        }
    }
}
