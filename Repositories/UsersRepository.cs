using Avtotest.Web.Models;
using Microsoft.Data.Sqlite;

namespace Avtotest.Web.Repositories;

public class UsersRepository
{
    private const string ConnectionString = "Data source=users.db";
    private SqliteConnection _connection;
    private SqliteCommand _command;

    public UsersRepository()
    {
        OpenConnection();
        CreateUsersTable();
    }

    public void OpenConnection()
    {
        _connection = new SqliteConnection(ConnectionString);
        _connection.Open();
        _command = _connection.CreateCommand();
    }

    public void CreateUsersTable()
    {
        _command.CommandText = "create table if not exists users(id integer primary key autoincrement, name TEXT, phone TEXT, password TEXT, image TEXT)";
        _command.ExecuteNonQuery();
    }

    public void InsertUser(User user)
    {
        _command.CommandText = $"INSERT INTO users(name, phone, password, image) values(@name, @phone, @password, @image)";
        _command.Parameters.AddWithValue("@name", user.Name);
        _command.Parameters.AddWithValue("@phone", user.Phone);
        _command.Parameters.AddWithValue("@password", user.Password);
        _command.Parameters.AddWithValue("@image", user.Image);
        _command.Prepare();

        _command.ExecuteNonQuery();
    }

    public List<User> GetUsers()
    {
        var users = new List<User>();

        _command.CommandText = "SELECT * FROM users";
        var data = _command.ExecuteReader();

        while (data.Read())
        {
            var user = new User();

            user.Index = data.GetInt32(0);
            user.Name = data.GetString(1);
            user.Phone = data.GetString(2);
            user.Image = data.GetString(4);

            users.Add(user);
        }

        return users;
    }

    public User GetUserByIndex(int index)
    {
        var user = new User();

        _command.CommandText = $"SELECT * FROM users WHERE id = {index}";
        var data = _command.ExecuteReader();
        while (data.Read())
        {
            user.Index = data.GetInt32(0);
            user.Name = data.GetString(1);
            user.Phone = data.GetString(2);
            user.Image = data.GetString(4);
        }

        return user;
    }

    public User GetUserByPhoneNumber(string phoneNumber)
    {
        var user = new User();

        _command.CommandText = $"SELECT * FROM users WHERE phone = '{phoneNumber}'";
        var data = _command.ExecuteReader();
        while (data.Read())
        {
            user.Index = data.GetInt32(0);
            user.Name = data.GetString(1);
            user.Phone = data.GetString(2);
            user.Password = data.GetString(3);
            user.Image = data.GetString(4);
        }

        return user;
    }

    public void DeleteUser(int index)
    {
        _command.CommandText = $"DELETE FROM users WHERE id = {index}";
        _command.ExecuteNonQuery();
    }

    public void UpdateUser(User user)
    {
        _command.CommandText = "UPDATE users SET name = @name, phone = @phone, password = @password, image = @image WHERE id = @userId";
        _command.Parameters.AddWithValue("@name", user.Name);
        _command.Parameters.AddWithValue("@phone", user.Phone);
        _command.Parameters.AddWithValue("@password", user.Password);
        _command.Parameters.AddWithValue("@image", user.Image);
        _command.Parameters.AddWithValue("@userId", user.Index);
        _command.Prepare();

        _command.ExecuteNonQuery();
    }
}