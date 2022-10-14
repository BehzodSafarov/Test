using Avtotest.Web.Repositories;
using Avtotest.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avtotest.Web.Controllers;

public class TicketsController : Controller
{
    private readonly UsersService _usersService;
    private readonly TicketsRepository _ticketsRepository;

    public TicketsController(UsersService usersService, 
        TicketsRepository ticketsRepository)
    {
        _usersService = usersService;
        _ticketsRepository = ticketsRepository;
    }

    public IActionResult Index()
    {
        var user = _usersService.GetUserFromCookie(HttpContext);
        if (user == null)
            return RedirectToAction("SignIn", "Users");

        var tickets = _ticketsRepository.GetTicketsByUserId(user.Index);

        return View(tickets);
    }
}