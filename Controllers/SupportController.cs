using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Curry.Models;
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
        [HttpPost("raiseRestaurantTicket")]
        public async Task<ActionResult<TicketDto>> RaiseRestaurantTicket([FromBody] RaiseTicketDto dto)
        {
            var ticket = new SupportTicket
            {
                RestId = dto.RestId,
                Email = dto.Email,
                Category = dto.Category,
                Description = dto.Description,
                CreatedDate = DateTimeOffset.Now,
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
        [HttpPost("raiseUserTicket")]
        public async Task<ActionResult<TicketDto>> RaiseUserTicket([FromBody] RaiseTicketDto dto)
        {
            var ticket = new SupportTicket
            {
                UserId = dto.UserId,
                Email = dto.Email,
                Category = dto.Category,
                Description = dto.Description,
                CreatedDate = DateTimeOffset.Now,
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
                    : "Restaurant"
            }).ToList();

            return Ok(result);
        }

        // 4) Assign Ticket
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
        [HttpPost("resolveTicket")]
        public async Task<IActionResult> ResolveTicket([FromBody] ResolveTicketDto dto)
        {
            var ticket = await _context.SupportTickets.FindAsync(dto.TicketId);
            if (ticket == null) return NotFound();

            ticket.TicketStatus = "Resolved";
            ticket.ResolvedDate = DateTimeOffset.Now;
            ticket.AdminMessage = dto.AdminMessage;

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
                AdminMessage = ticket.AdminMessage,
                ResolvedDate = ticket.ResolvedDate
            };
            return Ok(response);
        }

        // 6) View Closed Tickets - ✅ FIXED: Added AdminMessage
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
                ResolvedDate = ticket.ResolvedDate,
                AdminMessage = ticket.AdminMessage, // ✅ ADDED THIS LINE!
                Role = ticket.UserId != null
                    ? _context.Users.FirstOrDefault(u => u.UserId == ticket.UserId)?.Role
                    : "Restaurant"
            }).ToList();

            return Ok(result);
        }

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
                                         Role = "Restaurant"
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
                                             Role = "Restaurant"
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
                                             Role = "Restaurant",
                                             AdminMessage = ticket.AdminMessage,
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

        // 8) Get ALL Open Tickets (Admin View)
        [HttpGet("viewAllOpenTickets")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> ViewAllOpenTickets()
        {
            var tickets = await _context.SupportTickets
                .Where(t => t.TicketStatus == "Open" || t.TicketStatus == "Assigned")
                .OrderByDescending(t => t.CreatedDate)
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
                AssignedAdminId = ticket.AssignedAdminId,
                Role = ticket.UserId != null
                    ? _context.Users.FirstOrDefault(u => u.UserId == ticket.UserId)?.Role
                    : (ticket.RestId != null ? "Restaurant" : "Unknown")
            }).ToList();

            return Ok(result);
        }

        // 9) Get ALL Resolved Tickets (Admin View)
        [HttpGet("viewAllResolvedTickets")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> ViewAllResolvedTickets()
        {
            var tickets = await _context.SupportTickets
                .Where(t => t.TicketStatus == "Resolved")
                .OrderByDescending(t => t.ResolvedDate)
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
                ResolvedDate = ticket.ResolvedDate,
                TicketStatus = ticket.TicketStatus,
                AdminMessage = ticket.AdminMessage,
                Role = ticket.UserId != null
                    ? _context.Users.FirstOrDefault(u => u.UserId == ticket.UserId)?.Role
                    : (ticket.RestId != null ? "Restaurant" : "Unknown")
            }).ToList();

            return Ok(result);
        }

        // 10) Get User's All Tickets (for Past Tickets in Settings)
        [HttpGet("viewMyTickets/{userId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> ViewMyTickets(int userId)
        {
            var tickets = await _context.SupportTickets
                .Where(t => t.UserId == userId || t.RestId == userId)
                .OrderByDescending(t => t.CreatedDate)
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
                ResolvedDate = ticket.ResolvedDate,
                TicketStatus = ticket.TicketStatus,
                AdminMessage = ticket.AdminMessage,
                Role = ticket.UserId != null
                    ? _context.Users.FirstOrDefault(u => u.UserId == ticket.UserId)?.Role
                    : "Restaurant"
            }).ToList();

            return Ok(result);
        }

        // 11) Assign Ticket to Admin (for My Tickets)
        [HttpPost("assignToMe")]
        public async Task<IActionResult> AssignToMe([FromQuery] int ticketId, [FromQuery] int adminId)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found" });

            ticket.AssignedAdminId = adminId;
            ticket.TicketStatus = "Assigned";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ticket assigned successfully",
                ticketId = ticket.TicketId,
                assignedAdminId = ticket.AssignedAdminId,
                ticketStatus = ticket.TicketStatus
            });
        }

        // 12) Get Admin's Assigned Tickets (My Tickets)
        [HttpGet("viewMyAssignedTickets/{adminId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> ViewMyAssignedTickets(int adminId)
        {
            var tickets = await _context.SupportTickets
                .Where(t => t.AssignedAdminId == adminId && (t.TicketStatus == "Assigned" || t.TicketStatus == "Open"))
                .OrderByDescending(t => t.CreatedDate)
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
                    : (ticket.RestId != null ? "Restaurant" : "Unknown")
            }).ToList();

            return Ok(result);
        }
    }
}