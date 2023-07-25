using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniChatApp2.Data;
using MiniChatApp2.Model;

namespace MiniChatApp2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly MiniChatApp2Context _context;

        public LogsController(MiniChatApp2Context context)
        {
            _context = context;
        }

        // GET: api/Logs
        
        [HttpGet]
        public async Task<IActionResult> GetLogs(DateTime? endTime = null, DateTime? startTime = null)
        {
            if (endTime == null)
                endTime = DateTime.UtcNow;

            if (startTime == null)
                startTime = DateTime.UtcNow.AddMinutes(-5);

            if (startTime >= endTime)
            {
                return BadRequest(new { error = "StartTime must be earlier than EndTime" });
            }

            var logs = await _context.Logs
               
                .Select(log => new
                {
                    Id = log.id,
                    Ip = log.ip,
                    Username = log.Username,
                    RequestBody = log.RequestBody.Replace("\n", "").Replace("\"", "").Replace("\r", ""),
                    TimeStamp = log.TimeStamp,
                })
                .ToListAsync();

            if (logs.Count == 0)
            {
                return NotFound(new { message = "Logs not found" });
            }

            return Ok(new { Logs = logs });
        }


        // GET: api/Logs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Logs>> GetLogs(int id)
        {
          if (_context.Logs == null)
          {
              return NotFound();
          }
            var logs = await _context.Logs.FindAsync(id);

            if (logs == null)
            {
                return NotFound();
            }

            return logs;
        }

        // PUT: api/Logs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogs(int id, Logs logs)
        {
            if (id != logs.id)
            {
                return BadRequest();
            }

            _context.Entry(logs).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogsExists(id))
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

        // POST: api/Logs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Logs>> PostLogs(Logs logs)
        {
          if (_context.Logs == null)
          {
              return Problem("Entity set 'MiniChatApp2Context.Logs'  is null.");
          }
            _context.Logs.Add(logs);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogs", new { id = logs.id }, logs);
        }

        // DELETE: api/Logs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogs(int id)
        {
            if (_context.Logs == null)
            {
                return NotFound();
            }
            var logs = await _context.Logs.FindAsync(id);
            if (logs == null)
            {
                return NotFound();
            }

            _context.Logs.Remove(logs);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogsExists(int id)
        {
            return (_context.Logs?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
