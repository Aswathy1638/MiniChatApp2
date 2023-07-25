using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniChatApp2.Data;
using MiniChatApp2.Model;
using static MiniChatApp2.Model.MessageResponseDto;

namespace MiniChatApp2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MiniChatApp2Context _context;
        private static readonly List<Message> _messages = new List<Message>();
        public MessagesController(MiniChatApp2Context context)
        {
            _context = context;
        }

        // GET: api/Messages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessage()
        {
          if (_context.Message == null)
          {
              return NotFound();
          }
            return await _context.Message.ToListAsync();
        }

        // GET: api/Messages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
          if (_context.Message == null)
          {
              return NotFound();
          }
            var message = await _context.Message.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return message;
        }

      
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(int id, MessageEditDto message)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = await _context.Message.FirstOrDefaultAsync(u => u.Id == id);

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid Credentials" });
            }

            if (Convert.ToInt32(userId) != messages.senderId)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }



            if (messages == null)
            {
                return NotFound(new { message = "message not found" });
            }

            messages.Content = message.Content;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message edited successfully" });
        }

        /*
        // PUT: api/Messages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754


        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(int id, Message message)
        {
            if (id != message.Id)
            {
                return BadRequest();
            }

            _context.Entry(message).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessageExists(id))
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

        // POST: api/Messages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /* [HttpPost]
         public async Task<ActionResult<Message>> PostMessage(Message message)
         {
           if (_context.Message == null)
           {
               return Problem("Entity set 'MiniChatApp2Context.Message'  is null.");
           }
             _context.Message.Add(message);
             await _context.SaveChangesAsync();

             return CreatedAtAction("GetMessage", new { id = message.Id }, message);
         }*/




        [HttpPost]
        public async Task<ActionResult<MessageResponse>> PostMessage(MessageCreateDto message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Message sending failed due to validation errors." });
            }

            var currentUser = HttpContext.User;
            var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine(userId);

            // Check if the sender user exists
            var senderUser = await _context.User.FindAsync(Convert.ToInt32(userId));
            if (senderUser == null)
            {
                return NotFound(new { error = "Sender user not found." });
            }

            // Check if the receiver user exists
            var receiverUser = await _context.User.FindAsync(message.ReceiverId);
            if (receiverUser == null)
            {
                return NotFound(new { error = "Receiver user not found." });
            }

            var messageEntity = new Message
            {
                senderId = Convert.ToInt32(userId),
                receiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = DateTime.Now
            };

            _context.Message.Add(messageEntity);
            await _context.SaveChangesAsync();

            var messageResponse = new MessageResponse
            {
                MessageId = messageEntity.Id,
                SenderId = messageEntity.senderId,
                ReceiverId = messageEntity.receiverId,
                Content = messageEntity.Content,
                Timestemp = messageEntity.Timestamp,
            };

            return Ok(messageResponse);
        }



        // DELETE: api/Messages/5
        [HttpDelete("{id}")]
     
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the message exists
            var message = await _context.Message.FindAsync(id);
            if (message == null)
            {
                return NotFound(new { error = "Message not found" });
            }

            // Check if the current user is the sender of the message
            if (Convert.ToInt32(userId) != message.senderId)
            {
                return Unauthorized(new { error = "You are not authorized to delete this message" });
            }

            // Delete the message and save changes
            _context.Message.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message deleted successfully" });
        }


        private bool MessageExists(int id)
        {
            return (_context.Message?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
