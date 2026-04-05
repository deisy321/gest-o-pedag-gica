using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Controllers; // Adicionado namespace para organização

[Route("api/[controller]")]
[ApiController]
public class PushController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PushController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] string payload)
    {
        // 1. Obtém o utilizador atual
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // 2. Verifica se este dispositivo (payload) já está registado
        // Alterado para PushSubscriptions para condizer com o seu DbContext
        var existing = await _context.PushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.Payload == payload);

        if (existing == null)
        {
            // 3. Cria o registo (A classe continua a ser NotificationSubscription, 
            // mas a tabela no contexto chama-se PushSubscriptions)
            var newSub = new NotificationSubscription
            {
                UserId = user.Id,
                Payload = payload
            };

            _context.PushSubscriptions.Add(newSub);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }
}