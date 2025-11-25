using CNP.SchedaLavorazione.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CNP.Amministrazione.Services
{
    public class AttivitaService
    {
        private readonly string _connectionString;

        public AttivitaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string not found.");
        }

        // GET TUTTE LE ATTIVITÀ
        public async Task<List<Attività>> GetAttivita()
        {
            var attivita = new List<Attività>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT ID, CodiceAttivita, NomeAttivita, Descrizione
                 FROM Attivita 
                 ORDER BY NomeAttivita";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                attivita.Add(new Attività
                {
                    ID = reader.GetInt32("ID"),
                    CodiceAttivita = reader.GetInt32("CodiceAttivita"),
                    NomeAttivita = reader.GetString("NomeAttivita"),
                    Descrizione = reader.IsDBNull("Descrizione") ? string.Empty : reader.GetString("Descrizione")
                });
            }

            return attivita;
        }
    }
}