using System.Collections;
using APBD_D_CW7.Exceptions;
using APBD_D_CW7.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;

namespace APBD_D_CW7.Services;

public interface IDbService
{
    public Task<IEnumerable<TripGetDTO>> GetTripsAsync();
    public Task<IEnumerable<TripRegistrationDTO>> GetTripsByClientId(int idClient);
    public Task<ClientCreateDTO> CreateClientAsync(ClientCreateDTO client);
    public Task<bool> ClientAddToTripAsync(int idTrip, int idClient);
    public Task<bool> ClientRemoveFromTripAsync(int idTrip, int idClient);
}

public class DbService(IConfiguration config) : IDbService
{
    public async Task<IEnumerable<TripGetDTO>> GetTripsAsync()
    {
        var result = new List<TripGetDTO>();
        var connectionString = config.GetConnectionString("Default");
        var sql = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM trip";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new TripGetDTO
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5)
            });
        }

        return result;
    }

    public async Task<IEnumerable<TripRegistrationDTO>> GetTripsByClientId(int idClient)
    {
        var connectionString = config.GetConnectionString("Default");
        var sql =
            "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, ct.RegisteredAt, ct.PaymentDate FROM Trip t JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip WHERE ct.IdClient = @idClient;";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@idClient", idClient);

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            throw new NotFoundException($"Client with ID {idClient} not found");
        }

        var result = new List<TripRegistrationDTO>();
        result.Add(new TripRegistrationDTO()
        {
            IdTrip = reader.GetInt32(0),
            Name = reader.GetString(1),
            Description = reader.GetString(2),
            DateFrom = reader.GetDateTime(3),
            DateTo = reader.GetDateTime(4),
            MaxPeople = reader.GetInt32(5),
            RegisteredAt = reader.GetInt32(6),
            PaymentDate = reader.IsDBNull(7) ? null : reader.GetInt32(7)
        });

        while (await reader.ReadAsync())
        {
            result.Add(new TripRegistrationDTO()
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                RegisteredAt = reader.GetInt32(6),
                PaymentDate = reader.IsDBNull(7) ? null : reader.GetInt32(7)
            });
        }

        if (result.Count == 0)
        {
            throw new NotFoundException($"Could not find a Trip for client {idClient}");
        }

        return result;
    }

    public async Task<ClientCreateDTO> CreateClientAsync(ClientCreateDTO client)
    {
        try
        {
            var connectionString = config.GetConnectionString("Default");
            var sql = @"Insert Into Client (FirstName,LastName,Email,Telephone,Pesel)
            values (@FirstName,@LastName,@Email,@Telephone,@Pesel);
Select SCOPE_IDENTITY();";

            await using var connection = new SqlConnection(connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@FirstName", client.FirstName);
            command.Parameters.AddWithValue("@LastName", client.LastName);
            command.Parameters.AddWithValue("@Email", client.Email);
            command.Parameters.AddWithValue("@Telephone", client.Telephone);
            command.Parameters.AddWithValue("@Pesel", client.Pesel);

            await connection.OpenAsync();
            var id = Convert.ToInt32(await command.ExecuteScalarAsync());

            return new ClientCreateDTO()
            {
                Id = id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Telephone = client.Telephone,
                Pesel = client.Pesel,
            };
        }
        catch (SqlException ex)
        {
            throw new Exception("Blad bazy danych" + ex.Message);
        }
        catch (Exception ex)
        {
            throw new Exception("Blad aplikacji" + ex.Message);
        }
    }

    public async Task<bool> ClientAddToTripAsync(int idClient, int idTrip)
    {
        try
        {
            var connectionString = config.GetConnectionString("Default");
            var sql =
                "Select 1 From Client where IdClient = @idClient AND EXISTS(SELECT 1 FROM Client_Trip WHERE IdTrip = @idTrip);";
            await using var connection = new SqlConnection(connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@IdClient", idClient);
            command.Parameters.AddWithValue("@IdTrip", idTrip);

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            if (result != null)
            {
                var howManyPeople = "Select count(IdClient) from Client_Trip where IdTrip = @idTrip";
                command.CommandText = howManyPeople;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdTrip", idTrip);
                var howMany = Convert.ToInt32(await command.ExecuteScalarAsync());
                var sql2 = "Select MaxPeople from Trip where IdTrip = @idTrip";
                command.CommandText = sql2;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdTrip", idTrip);
                var maxPeople = Convert.ToInt32(await command.ExecuteScalarAsync());
                if (howMany < maxPeople)
                {
                    var sql3 = "Insert into Client_Trip (IdClient, IdTrip,RegisteredAt) values (@IdClient, @IdTrip,1)";
                    command.CommandText = sql3;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@IdClient", idClient);
                    command.Parameters.AddWithValue("@IdTrip", idTrip);
                    await command.ExecuteScalarAsync();
                    return true;
                }
                else
                {
                    throw new Exception($"Too many people signed to trip with id: {idTrip}");
                }
            }
            else
            {
                throw new NotFoundException($"Client with ID {idClient} not found or Trip with ID {idTrip} not exists");
            }
        }
        catch (SqlException ex)
        {
            throw new Exception("Blad bazy danych: " + ex.Message);
        }
    }

    public async Task<bool> ClientRemoveFromTripAsync(int idClient, int idTrip)
    {
        try
        {
            var connectionString = config.GetConnectionString("Default");
            var sql =
                "Select 1 From Client where IdClient = @idClient AND EXISTS(SELECT 1 FROM Client_Trip WHERE IdTrip = @idTrip);";
            await using var connection = new SqlConnection(connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@IdClient", idClient);
            command.Parameters.AddWithValue("@IdTrip", idTrip);

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            if (result != null)
            {
                sql = "Delete From Client_Trip where IdTrip = @idTrip and IdClient = @idClient";
                command.CommandText = sql;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@idTrip", idTrip);
                command.Parameters.AddWithValue("@idClient", idClient);
                await command.ExecuteScalarAsync();
                return true;
            }
            else
            {
                throw new Exception($"Client with ID {idClient}, or Trip with ID {idTrip} not found");
            }
        }
        catch (SqlException ex)
        {
            throw new Exception("Blad bazy danych " + ex.Message);
        }
    }
}