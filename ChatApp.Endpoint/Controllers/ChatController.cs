using ChatApp.Endpoint.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Endpoint.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatHistoryService _chatHistory;

        public ChatController(ChatHistoryService chatHistory)
        {
            _chatHistory = chatHistory;
        }

        // GET végpont a chat előzmények lekérésére
        [HttpGet]
        public IActionResult GetMessages()
        {
            return Ok(_chatHistory.GetAllMessages());
        }

        // POST végpont, ami ugyanazt csinálja, mint a Hub SendMessage.
        // Ezt pl. egy külső rendszer használhatná, ami nem SignalR-en kommunikál.
        // A vizsgán valószínűleg a Hub-os megoldás is elég, de így teljes a feladat.
        // A valós idejű frissítéshez a Hub-ra itt is szükség lenne.
    }
}
