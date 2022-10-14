using Avtotest.Web.Models;
using Avtotest.Web.Repositories;
using Avtotest.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avtotest.Web.Controllers;

[Route("[action]")]
public class UsersController : Controller
{
    private readonly UsersRepository _usersRepository;
    private readonly CookiesService _cookiesService;
    private readonly UsersService _usersService;
    private readonly TicketsRepository _ticketsRepository;
    private readonly QuestionsRepository _questionsRepository;

    public UsersController(UsersRepository usersRepository,
        CookiesService cookiesService, 
        UsersService usersService,
        TicketsRepository ticketsRepository,
        QuestionsRepository questionsRepository)
    {
        _usersRepository = usersRepository;
        _cookiesService = cookiesService;
        _usersService = usersService;
        _ticketsRepository = ticketsRepository;
        _questionsRepository = questionsRepository;
    }

    public IActionResult Index()
    {
        var user = _usersService.GetUserFromCookie(HttpContext);
        if (user != null)
        {
            return View(user);
        }

        return RedirectToAction("SignIn");
    }

    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SignUp(User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        user.Image ??= "sdadsa.png";
        _usersRepository.InsertUser(user);
        var _user = _usersRepository.GetUserByPhoneNumber(user.Phone!);

        _ticketsRepository.InsertUserTrainingTickets(_user.Index, _questionsRepository.GetQuestionsCount() / 20, 20);
        _cookiesService.SendUserPhoneToCookie(user.Phone!, HttpContext);
        return RedirectToAction("Index");
    }

    public IActionResult SignIn()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SignIn(UserDto user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var _user = _usersRepository.GetUserByPhoneNumber(user.Phone!);

        if (_user.Password == user.Password)
        {
            _cookiesService.SendUserPhoneToCookie(user.Phone!, HttpContext);
            return RedirectToAction("Index");
        }

        return RedirectToAction("SignIn");
    }

    public IActionResult Edit()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Edit([FromForm] User user)
    {
        var _user = _usersService.GetUserFromCookie(HttpContext);
        if (_user == null)
            return RedirectToAction("SignIn");
            
        user.Index = _user.Index;
        user.Image = SaveUserImage(user.ImageFile);
        _usersRepository.UpdateUser(user);
        return RedirectToAction("Index");
    }

    private string? SaveUserImage(IFormFile? userImageFile)
    {
        if (userImageFile == null)
        {
            return "avatar.png";
        }

        var imagePath = Guid.NewGuid().ToString("N") + Path.GetExtension(userImageFile.FileName);

        var ms = new MemoryStream();
        userImageFile.CopyTo(ms);
        System.IO.File.WriteAllBytes(Path.Combine("wwwroot", "Profile", imagePath), ms.ToArray());

        return imagePath;
    }
}