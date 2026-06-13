using grupomathias.Models;
using grupomathias.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace grupomathias.Controllers.Api;

[ApiController]
[Route("api/membership")]
[Authorize]
public class MembershipApiController : ControllerBase
{
    private readonly IMercadoPagoMembershipService _membershipService;
    private readonly UserManager<IdentityUser> _userManager;

    public MembershipApiController(
        IMercadoPagoMembershipService membershipService,
        UserManager<IdentityUser> userManager)
    {
        _membershipService = membershipService;
        _userManager = userManager;
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult Status()
    {
        var options = _membershipService.GetOptions();
        return Ok(new
        {
            service = "Mercado Pago Membership",
            configured = !string.IsNullOrWhiteSpace(options.AccessToken),
            publicKeyLoaded = !string.IsNullOrWhiteSpace(options.PublicKey)
        });
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<MembershipCheckoutResult>> Checkout([FromBody] MembershipCheckoutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var result = await _membershipService.CreateCheckoutAsync(user.Id, request, cancellationToken);
        return Ok(result);
    }
}