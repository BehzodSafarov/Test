using System.Runtime.InteropServices;
using Avtotest.Web.Models;
using Microsoft.Data.Sqlite;

namespace Avtotest.Web.Repositories;

public class QuestionsRepository
{
    private readonly string _connectionString = "Data source=avtotest.db";
    private readonly SqliteConnection _connection;

    public QuestionsRepository()
    {
        _connection = new SqliteConnection(_connectionString);
    }

    public int GetQuestionsCount()
    {
        _connection.Open();
        var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM questions;";
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

    public QuestionEntity GetQuestionById(int id)
    {
        _connection.Open();
        var question = new QuestionEntity();

        var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * from questions where id = @id;";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Prepare();

        var data = cmd.ExecuteReader();
        while (data.Read())
        {
            question.Id = data.GetInt32(0);
            question.Question = data.GetString(1);
            question.Description = data.GetString(2);
            question.Image = data.IsDBNull(3) ? null : data.GetString(3);
        }

        question.Choices = GetQuestionChoices(id);

        _connection.Close();
        return question;
    }
    
    private List<Choice> GetQuestionChoices(int questionId)
    {
        _connection.Open();
        var choices = new List<Choice>();

        var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * from choices WHERE questionId = @questionId";
        cmd.Parameters.AddWithValue("@questionId", questionId);
        cmd.Prepare();

        var data = cmd.ExecuteReader();
        while (data.Read())
        {
            var choice = new Choice
            {
                Id = data.GetInt32(0),
                Text = data.GetString(1),
                Answer = data.GetBoolean(2)
            };

            choices.Add(choice);
        }

        _connection.Close();
        return choices;
    }
}