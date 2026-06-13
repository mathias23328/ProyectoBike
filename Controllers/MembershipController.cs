using grupomathias.Models;
using grupomathias.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace grupomathias.Controllers;

[Authorize]
public class MembershipController : Controller
{
    private readonly IMercadoPagoMembershipService _membershipService;
    private readonly UserManager<IdentityUser> _userManager;

    public MembershipController(
        IMercadoPagoMembershipService membershipService,
        UserManager<IdentityUser> userManager)
    {
        _membershipService = membershipService;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var options = _membershipService.GetOptions();
        return View(new MembershipPageViewModel
        {
            PlanName = "Bike Plus",
            MonthlyPrice = 19.90m,
            Currency = "PEN",
            BenefitsSummary = "Acceso a rutas premium, historial ampliado, recomendaciones del agente ML y alertas de seguridad.",
            ApiHint = string.IsNullOrWhiteSpace(options.PublicKey)
                ? "Mercado Pago está listo para activarse con configuración local."
                : "Mercado Pago está configurado para generar checkouts de membresía."
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCheckout(MembershipCheckoutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", new MembershipPageViewModel
            {
                PlanName = request.PlanName,
                MonthlyPrice = request.MonthlyPrice == 0 ? 19.90m : request.MonthlyPrice,
                Currency = "PEN",
                BenefitsSummary = "Revisa los campos para continuar con la membresía.",
                ApiHint = "Faltan datos para crear el checkout."
            });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["MembershipMessage"] = "Debe estar autenticado para crear una membresía.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _membershipService.CreateCheckoutAsync(user.Id, request, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.CheckoutUrl))
        {
            TempData["MembershipMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["MembershipMessage"] = $"Checkout creado: {result.PreferenceId}.";
        return Redirect(result.CheckoutUrl);
    }

    [HttpGet]
    public IActionResult Result(string state)
    {
        ViewData["Title"] = "Resultado membresía";
        ViewBag.State = state;
        return View();
    }
}