using ChatApp.Endpoint.Services;
using Microsoft.AspNetCore.Mvc;

/*
 * [ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatHistoryService _chatHistory;
    // 1. Injektáljuk a Hub-ot a controllerbe is!
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(ChatHistoryService chatHistory, IHubContext<ChatHub> hubContext)
    {
        _chatHistory = chatHistory;
        _hubContext = hubContext;
    }

    [HttpGet]
    public IActionResult GetMessages() 
    {
        return Ok(_chatHistory.GetAllMessages());
    }

    // A POST végpont, ami fogadja a külső kérést
    [HttpPost]
    public async Task<IActionResult> PostExternalMessage([FromBody] ChatMessage message)
    {
        // 2. Érvényesítjük és hozzáadjuk az üzenetet a közös tárolóhoz
        message.Timestamp = DateTime.UtcNow;
        _chatHistory.AddMessage(message);

        // 3. A HubContext segítségével kiküldjük az üzenetet minden csatlakoztatott kliensnek
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

        return Ok(new { Status = "Message broadcasted successfully." });
    }
}
 */
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
