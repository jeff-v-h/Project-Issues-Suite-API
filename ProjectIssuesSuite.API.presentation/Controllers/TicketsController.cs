using ProjectIssuesSuite.API.domain.Managers;
using ProjectIssuesSuite.API.domain.Models;
using LazyCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.presentation.Controllers
{
    [Route("api/tickets")]
    public class TicketsController : Controller
    {
        private readonly ILogger<TicketsController> _logger;
        private ITicketManager _manager { get; set; }
        private readonly IAppCache _cache;

        public TicketsController(ILogger<TicketsController> logger, ITicketManager manager, IAppCache cache)
        {
            _logger = logger;
            _manager = manager;
            _cache = cache;
        }

        [HttpGet("")]
        public IActionResult GetTickets()
        {
            ICollection<TicketViewModel> tickets = _manager.GetAllTickets();

            return Ok(tickets);
        }

        [HttpGet("{ticketId}", Name = "GetTicket")]
        public IActionResult GetTicket(string ticketId)
        {
            Func<TicketViewModel> ticketGetter = () => _manager.GetTicket(ticketId);

            var cacheExpiry = new TimeSpan(0, 0, 1);

            TicketViewModel ticketVMCached = _cache.GetOrAdd(
                "TicketsController.GetTicket." + ticketId,
                ticketGetter,
                cacheExpiry);

            if (ticketVMCached == null)
            {
                return NotFound($"Ticket with id '{ticketId}' could not be found.");
            }

            return Ok(ticketVMCached);
        }

        // Project name is included to allow for same ticket names for different projects
        [HttpGet("{projectName}/{ticketName}", Name = "GetTicketByName")]
        public IActionResult GetTicketByName(string projectName, string ticketName)
        {
            Func<TicketViewModel> ticketGetter = () => _manager.GetTicketByName(projectName, ticketName);

            var cacheExpiry = new TimeSpan(0, 0, 1);

            TicketViewModel ticketVMCached = _cache.GetOrAdd(
                "TicketsController.GetTicketByName." + ticketName,
                ticketGetter,
                cacheExpiry);

            if (ticketVMCached == null)
            {
                return NotFound($"Ticket with name '{ticketName}' could not be found in project '{projectName}'.");
            }

            return Ok(ticketVMCached);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateTicket([FromBody] TicketViewModel newTicket)
        {
            if (newTicket == null)
            {
                return BadRequest("Please provide details to create a ticket.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TicketViewModel ticketVM = await _manager.CreateTicket(newTicket);

            if (ticketVM == null)
            {
                return StatusCode(500, "An error occured while creating your ticket");
            }

            // return a URI to the newly created resource
            return CreatedAtRoute("GetTicket", new { ticketId = ticketVM.Id }, ticketVM);
        }

        [HttpPost("withvideos")] // Max limit web request is set to 300MB in Web.config and Startup
        public async Task<IActionResult> CreateTicketWithVideos(TicketViewModel newTicket)
        {
            if (newTicket == null)
            {
                return BadRequest("Please provide details to create a ticket.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TicketViewModel ticketVM = await _manager.CreateTicket(newTicket);

            if (ticketVM == null)
            {
                return StatusCode(500, "An error occured while creating your ticket");
            }

            // return a URI to the newly created resource
            return CreatedAtRoute("GetTicket", new { ticketId = ticketVM.Id }, ticketVM);
        }

        [HttpPost("{ticketId}")]
        public IActionResult UpdateTicket(string ticketId, [FromBody] TicketViewModel newTicketObject)
        {
            if (newTicketObject == null)
            {
                return BadRequest("Please provide details to update ticket.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (ticketId != newTicketObject.Id)
            {
                return BadRequest("The new ticket's id does not match the old id.");
            }

            bool ticketIsUpdated = _manager.ReplaceTicket(newTicketObject).Result;
            if (!ticketIsUpdated)
            {
                return NotFound($"Ticket with id '{ticketId}' was not found. No update was executed.");
            }

            return NoContent();
        }

        [HttpPost("{ticketId}/withvideos")]
        public IActionResult UpdateTicketWithVideos(string ticketId, [FromForm] TicketViewModel newTicketObject)
        {
            if (newTicketObject == null)
            {
                return BadRequest("Please provide details to update ticket.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (ticketId != newTicketObject.Id)
            {
                return BadRequest("The new ticket's id does not match the old id.");
            }

            bool ticketIsUpdated = _manager.ReplaceTicket(newTicketObject).Result;
            if (!ticketIsUpdated)
            {
                return NotFound($"Either video name is taken or ticket with id '{ticketId}' was not found. No update was executed.");
            }

            return NoContent();
        }

        [HttpDelete("{ticketId}")]
        public IActionResult DeleteTicket(string ticketId)
        {
            // Must wait to finish before completing this function, otherwise called methods won't finish due to async
            _manager.DeleteTicket(ticketId).Wait();

            return NoContent();
        }
    }
}
