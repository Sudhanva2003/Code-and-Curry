using Code_Curry.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Code_Curry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportController : ControllerBase
    {
        private readonly CodeCurryContext _context;

        public SupportController(CodeCurryContext context)
        {
            _context = context;
        }

        // 1) Raise Restaurant Ticket
        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPost("raiseRestaurantTicket")]
        public async Task<ActionResult<TicketDto>> RaiseRestaurantTicket([FromBody] RaiseTicketDto dto)
        {
            var ticket = new SupportTicket
            {
                RestId = dto.RestId,
                Email = dto.Email,
                Category = dto.Category,
                Description = dto.Description,
                CreatedDate = DateTimeOffset.Now,  // Using system time (current UTC time)
                TicketStatus = "Open"
            };
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            var response = new TicketDto
            {
                TicketId = ticket.TicketId,
                RestId = ticket.RestId,
                Email = ticket.Email,
                Category = ticket.Category,
                Description = ticket.Description,
                CreatedDate = ticket.CreatedDate,
                TicketStatus = ticket.TicketStatus
            };
            return Ok(response);
        }

        // 2) Raise User Ticket
        [Authorize(Roles = "Admin,Customer,Deliverer")]
        [HttpPost("raiseUserTicket")]
        public async Task<ActionResult<TicketDto>> RaiseUserTicket([FromBody] RaiseTicketDto dto)
        {
            var ticket = new SupportTicket
            {
                UserId = dto.UserId,
                Email = dto.Email,
                Category = dto.Category,
                Description = dto.Description,
                CreatedDate = DateTimeOffset.Now,  // Using system time (current UTC time)
                TicketStatus = "Open"
            };
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);
            var response = new TicketDto
            {
                TicketId = ticket.TicketId,
                UserId = ticket.UserId,
                Email = ticket.Email,
                Category = ticket.Category,
                Description = ticket.Description,
                CreatedDate = ticket.CreatedDate,
                TicketStatus = ticket.TicketStatus,
                Role = user?.Role
            };
            return Ok(response);
        }
        [Authorize(Roles = "Admin")]

        // 3) View Open Tickets
        [HttpGet("viewOpenTickets/{userId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> ViewOpenTickets(int userId)
        {
            var tickets = await _context.SupportTickets
                .Where(t => t.TicketStatus == "Open" && (t.UserId == userId || t.RestId == userId))
                .ToListAsync();

            var result = tickets.Select(ticket => new TicketDto
            {
                TicketId = ticket.TicketId,
                RestId = ticket.RestId,
                UserId = ticket.UserId,
                Email = ticket.Email,
                Category = ticket.Category,
                Description = ticket.Description,
                CreatedDate = ticket.CreatedDate,
                TicketStatus = ticket.TicketStatus,
                Role = ticket.UserId != null
                    ? _context.Users.FirstOrDefault(u => u.UserId == ticket.UserId)?.Role
                    : "Deliverer"
            }).ToList();

            return Ok(result);
        }

        // 4) Assign Ticket
        [Authorize(Roles = "Admin")]
        [HttpPost("assignTicket")]
        public async Task<IActionResult> AssignTicket([FromQuery] int ticketId, [FromQuery] int userId)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            ticket.AssignedAdminId = userId;
            ticket.TicketStatus = "Assigned";
            await _context.SaveChangesAsync();

            return Ok();
        }

        // 5) Resolve Ticket
        [Authorize(Roles = "Admin")]
        [HttpPost("resolveTicket")]
        public async Task<IActionResult> ResolveTicket([FromBody] ResolveTicketDto dto)
        {
            var ticket = await _context.SupportTickets.FindAsync(dto.TicketId);
            if (ticket == null) return NotFound();

            ticket.TicketStatus = "Resolved";
            ticket.ResolvedDate = DateTimeOffset.Now;  // Using system time (current UTC time)
            // Add admin message
            var adminUser = await _context.Users.FindAsync(dto.UserId);
            // Here you can store the admin message as well
            ticket.Description += "\nResolved by: " + adminUser?.FullName + "\nMessage: " + dto.AdminMessage;
            await _context.SaveChangesAsync();

            var response = new TicketDto
            {
                TicketId = ticket.TicketId,
                RestId = ticket.RestId,
                UserId = ticket.UserId,
                Email = ticket.Email,
                Category = ticket.Category,
                Description = ticket.Description,
                CreatedDate = ticket.CreatedDate,
                TicketStatus = ticket.TicketStatus,
                Role = adminUser?.Role
            };
            return Ok(response);
        }

        // 6) View Closed Tickets
        [Authorize(Roles = "Admin")]
        [HttpGet("viewClosedTickets/{userId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> ViewClosedTickets(int userId)
        {
            var tickets = await _context.SupportTickets
                .Where(t => t.TicketStatus == "Resolved" && (t.UserId == userId || t.RestId == userId))
                .ToListAsync();

            var result = tickets.Select(ticket => new TicketDto
            {
                TicketId = ticket.TicketId,
                RestId = ticket.RestId,
                UserId = ticket.UserId,
                Email = ticket.Email,
                Category = ticket.Category,
                Description = ticket.Description,
                CreatedDate = ticket.CreatedDate,
                TicketStatus = ticket.TicketStatus,
                Role = ticket.UserId != null
                    ? _context.Users.FirstOrDefault(u => u.UserId == ticket.UserId)?.Role
                    : "Deliverer"
            }).ToList();

            return Ok(result);
        }
        [Authorize(Roles = "Admin,Restaurant")]
        // 7) View My Restaurant Tickets
        [HttpGet("viewMyRestTickets/{restId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> ViewMyRestTickets(int restId)
        {
            var tickets = await _context.SupportTickets
                .Where(t => t.RestId == restId)
                .ToListAsync();

            var openTickets = tickets.Where(t => t.TicketStatus == "Open")
                                     .Select(ticket => new TicketDto
                                     {
                                         TicketId = ticket.TicketId,
                                         RestId = ticket.RestId,
                                         UserId = ticket.UserId,
                                         Email = ticket.Email,
                                         Category = ticket.Category,
                                         Description = ticket.Description,
                                         CreatedDate = ticket.CreatedDate,
                                         TicketStatus = ticket.TicketStatus,
                                         Role = "Deliverer"
                                     }).ToList();

            var assignedTickets = tickets.Where(t => t.TicketStatus == "Assigned")
                                         .Select(ticket => new TicketDto
                                         {
                                             TicketId = ticket.TicketId,
                                             RestId = ticket.RestId,
                                             UserId = ticket.UserId,
                                             Email = ticket.Email,
                                             Category = ticket.Category,
                                             Description = ticket.Description,
                                             CreatedDate = ticket.CreatedDate,
                                             TicketStatus = ticket.TicketStatus,
                                             Role = "Deliverer"
                                         }).ToList();

            var resolvedTickets = tickets.Where(t => t.TicketStatus == "Resolved")
                                         .Select(ticket => new TicketDto
                                         {
                                             TicketId = ticket.TicketId,
                                             RestId = ticket.RestId,
                                             UserId = ticket.UserId,
                                             Email = ticket.Email,
                                             Category = ticket.Category,
                                             Description = ticket.Description,
                                             CreatedDate = ticket.CreatedDate,
                                             TicketStatus = ticket.TicketStatus,
                                             Role = "Deliverer",
                                             AdminMessage = ticket.Description,
                                             ResolvedDate = ticket.ResolvedDate
                                         }).ToList();

            var response = new
            {
                OpenTickets = openTickets,
                AssignedTickets = assignedTickets,
                ResolvedTickets = resolvedTickets
            };

            return Ok(response);
        }
    }
}
