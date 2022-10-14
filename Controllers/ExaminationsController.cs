using Avtotest.Web.Models;
using Avtotest.Web.Options;
using Avtotest.Web.Repositories;
using Avtotest.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Avtotest.Web.Controllers;

public class ExaminationsController : Controller
{
    private readonly QuestionsRepository _questionsRepository;
    private readonly UsersService _usersService;
    private readonly TicketsRepository _ticketsRepository;

    private readonly int TicketQuestionCount = 20;

    public ExaminationsController(
        QuestionsRepository questionsRepository,
        UsersService usersService,
        TicketsRepository ticketsRepository,
        IOptions<TicketSettings> option)
    {
        _questionsRepository = questionsRepository;
        _usersService = usersService;
        _ticketsRepository = ticketsRepository;
        TicketQuestionCount = option.Value.QuestionsCount;
    }

    public IActionResult Index()
    {
        var user = _usersService.GetUserFromCookie(HttpContext);
        if(user == null)
            return RedirectToAction("SignIn", "Users");

        var ticket = CreateRandomTicket(user);

        return View(ticket);
    }

    private Ticket CreateRandomTicket(User user)
    {
        //10 va 20 ni hisoblash kerak
        var ticketCount = _questionsRepository.GetQuestionsCount() / TicketQuestionCount;

        var random = new Random();
        var ticketIndex = random.Next(0, ticketCount);
        var from = ticketIndex * TicketQuestionCount;

        var ticket =  new Ticket(user.Index, from, TicketQuestionCount);

        _ticketsRepository.InsertTicket(ticket);

        var id = _ticketsRepository.GetLastRowId();
        ticket.Id = id;

        return ticket;
    }

    [Route("tickets/{ticketId}")]
    [Route("tickets/{ticketId}/questions/{questionId}")]
    [Route("tickets/{ticketId}/questions/{questionId}/choices/{choiceId}")]
    public IActionResult Exam(int ticketId, int? questionId = null, int? choiceId = null)
    {
        var user = _usersService.GetUserFromCookie(HttpContext);
        if (user == null)
            return RedirectToAction("SignIn", "Users");

        var ticket = _ticketsRepository.GetTicketById(ticketId, user.Index);

        questionId = questionId ?? ticket.FromIndex;

        if (ticket.FromIndex <= questionId && ticket.QuestionsCount + ticket.FromIndex > questionId)
        {
            var question = _questionsRepository.GetQuestionById(questionId.Value);

            ViewBag.TicketData = _ticketsRepository.GetTicketDataById(ticket.Id);

            //2, get from ticket_data by questionId and ticketId
            var _ticketData = _ticketsRepository.GetTicketDataByQuestionId(ticketId, questionId.Value);

            var _choiceId = (int?)null;
            var _answer = false;

            if (_ticketData != null)
            {
                _choiceId = _ticketData.ChoiceId;
                _answer = _ticketData.Answer;
            }
            else if (choiceId != null)
            {
                //1, insert into ticket_data ticketId, questionId, choiceId, answer;
                var answer = question.Choices!.First(choice => choice.Id == choiceId).Answer;
                
                var ticketData = new TicketData()
                {
                    TicketId = ticketId,
                    QuestionId = question.Id,
                    ChoiceId = choiceId.Value,
                    Answer = answer
                };
                _ticketsRepository.InsertTicketData(ticketData);
                
                _choiceId = choiceId;
                _answer = answer;

                //4, if answer true update ticket correct count
                if (_answer)
                {
                    _ticketsRepository.UpdateTicketCorrectCount(ticket.Id);
                }
                //3, agar hamma savolga javob bergan bolsa natijani korsatish
                if (ticket.QuestionsCount == _ticketsRepository.GetTicketAnswersCount(ticket.Id))
                {
                    return RedirectToAction("ExamResult", new {ticketId = ticket.Id});
                }
            }

            ViewBag.Ticket = ticket;
            ViewBag.ChoiceId = _choiceId;
            ViewBag.Answer = _answer;

            return View(question);
        }

        return NotFound();
    }

    public IActionResult GetQuestionById(int questionId)
    {
        var question = _questionsRepository.GetQuestionById(questionId);
        return View(question);
    }

    public IActionResult ExamResult(int ticketId)
    {
        var user = _usersService.GetUserFromCookie(HttpContext);
        if (user == null)
            return RedirectToAction("SignIn", "Users");

        var ticket = _ticketsRepository.GetTicketById(ticketId, user.Index);

        return View(ticket);
    }
}