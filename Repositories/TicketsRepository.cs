using Avtotest.Web.Models;
using Microsoft.Data.Sqlite;

namespace Avtotest.Web.Repositories;

public class TicketsRepository
{
    private readonly string _connectionString = "Data source=avtotest.db";
    private readonly SqliteConnection _connection;

    public TicketsRepository()
    {
        _connection = new SqliteConnection(_connectionString);
        CreateTicketTable();
    }

    private void CreateTicketTable()
    {
        _connection.Open();

        var cmd = _connection.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS tickets(" +
                          "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                          "user_id INTEGER, " +
                          "from_index INTEGER, " +
                          "questions_count INTEGER, " +
                          "correct_count INTEGER," +
                          "is_training BOOLEAN)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE TABLE IF NOT EXISTS tickets_data(" +
                          "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                          "ticket_id INTEGER, " +
                          "question_id INTEGER, " +
                          "choice_id INTEGER, " +
                          "answer BOOLEAN)";
        cmd.ExecuteNonQuery();

        _connection.Close();
    }

    public int GetLastRowId()
    {
        _connection.Open();

        int id = 0;
        var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT id from tickets ORDER BY id DESC LIMIT 1";
        var data = cmd.ExecuteReader();
        while (data.Read())
        {
            id = data.GetInt32(0);
        }

        _connection.Close();
        return id;
    }

    public void InsertTicket(Ticket ticket)
    {
        _connection.Open();
        var cmd = _connection.CreateCommand();

        cmd.CommandText = "INSERT INTO tickets(user_id, from_index, questions_count, correct_count, is_training) VALUES(@userId, @fromIndex, @questionsCount, @correctCount, @isTraining);";

        cmd.Parameters.AddWithValue("@userId", ticket.UserId);
        cmd.Parameters.AddWithValue("@fromIndex", ticket.FromIndex);
        cmd.Parameters.AddWithValue("@questionsCount", ticket.QuestionsCount);
        cmd.Parameters.AddWithValue("@correctCount", ticket.CorrectCount);
        cmd.Parameters.AddWithValue("@isTraining", ticket.IsTraining);

        cmd.Prepare();

        cmd.ExecuteNonQuery();

        _connection.Close();
    }

    public void InsertTicketData(TicketData ticketData)
    {
        _connection.Open();
        var cmd = _connection.CreateCommand();

        cmd.CommandText = "INSERT INTO tickets_data(ticket_id, question_id, choice_id, answer) VALUES(@ticketId, @questionId, @choiceId, @answer)";
        cmd.Parameters.AddWithValue("@ticketId", ticketData.TicketId);
        cmd.Parameters.AddWithValue("@questionId", ticketData.QuestionId);
        cmd.Parameters.AddWithValue("@choiceId", ticketData.ChoiceId);
        cmd.Parameters.AddWithValue("@answer", ticketData.Answer);
        cmd.Prepare();

        cmd.ExecuteNonQuery();
        _connection.Close();
    }

    public TicketData? GetTicketDataByQuestionId(int ticketId, int questionId)
    {
        _connection.Open();
        var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT * from tickets_data where ticket_id = {ticketId} and question_id = {questionId}";
        var data = cmd.ExecuteReader();

        var ticketData = new TicketData();

        while (data.Read())
        {
            ticketData.Id = data.GetInt32(0);
            ticketData.TicketId = data.GetInt32(1);
            ticketData.QuestionId = data.GetInt32(2);
            ticketData.ChoiceId = data.GetInt32(3);
            ticketData.Answer = data.GetBoolean(4);
        }

        if (ticketData.QuestionId == questionId)
        {
            return ticketData;
        }

        return null;
    }

    public int GetTicketAnswersCount(int ticketId)
    {
        _connection.Open();
        var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) from tickets_data WHERE ticket_id = {ticketId}";
        var data = cmd.ExecuteReader();

        while (data.Read())
        {
            var count = data.GetInt32(0);
            _connection.Close();
            data.Close();
            return count;
        }

        _connection.Close();
        return 0;
    }

    public void UpdateTicketCorrectCount(int ticketId)
    {
        _connection.Open();
        var cmd = _connection.CreateCommand();
        cmd.CommandText = $"UPDATE tickets set correct_count = correct_count + 1 WHERE id = {ticketId}";
        cmd.ExecuteNonQuery();
        _connection.Close();
    }

    public List<TicketData> GetTicketDataById(int ticketId)
    {
        var ticketDatas = new List<TicketData>();

        _connection.Open();
        var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT question_id, choice_id, answer FROM tickets_data WHERE ticket_id = {ticketId}";
        var data = cmd.ExecuteReader();
        while (data.Read())
        {
            var ticketData = new TicketData()
            {
                QuestionId = data.GetInt32(0),
                ChoiceId = data.GetInt32(1),
                Answer = data.GetBoolean(2)
            };
            ticketDatas.Add(ticketData);
        }

        _connection.Close();
        return ticketDatas;
    }

    public List<Ticket> GetTicketsByUserId(int userId)
    {
        var tickets = new List<Ticket>();

        _connection.Open();
        var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT id, from_index, questions_count, correct_count FROM tickets WHERE user_id = {userId} AND is_training = true";
        var data = cmd.ExecuteReader();
        while (data.Read())
        {
            var ticketData = new Ticket()
            {
                Id = data.GetInt32(0),
                FromIndex = data.GetInt32(1),
                QuestionsCount = data.GetInt32(2),
                CorrectCount = data.GetInt32(3),
                UserId = userId
            };
            tickets.Add(ticketData);
        }

        _connection.Close();
        return tickets;
    }
    
    public Ticket GetTicketById(int id, int userId)
    {
        _connection.Open();
        var ticket = new Ticket();

        var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM tickets WHERE id = @id AND user_id = @userId;";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Prepare();

        var data = cmd.ExecuteReader();
        while (data.Read())
        {
            ticket.Id = data.GetInt32(0);
            ticket.UserId = data.GetInt32(1);
            ticket.FromIndex = data.GetInt32(2);
            ticket.QuestionsCount = data.GetInt32(3);
            ticket.CorrectCount = data.GetInt32(4);
            ticket.IsTraining = data.GetBoolean(5);
        }

        _connection.Close();
        return ticket;
    }

    public void InsertUserTrainingTickets(int userId, int ticketsCount, int ticketQuestionsCount)
    {
        for (int i = 0; i < ticketsCount; i++)
        {
            InsertTicket(new Ticket()
            {
                UserId = userId,
                CorrectCount = 0,
                IsTraining = true,
                FromIndex = i * ticketQuestionsCount + 1,
                QuestionsCount = ticketQuestionsCount
            });
        }
    }
}