using CNP.Segreteria.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CNP.Amministrazione.Services
{
    public class UnitaMisuraService
    {
        private readonly string _connectionString;

        public UnitaMisuraService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // GET TUTTE LE UNITÀ DI MISURA
        public async Task<List<UnitaMisura>> GetUnitaMisura()
        {
            var unitaMisura = new List<UnitaMisura>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT IdUnitaMisura, UnitaMisura, Descrizione, Tipo
                         FROM UnitaMisura 
                         ORDER BY UnitaMisura";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                unitaMisura.Add(new UnitaMisura
                {
                    IdUnitaMisura = reader.GetInt32("IdUnitaMisura"),
                    UM = reader.GetString("UnitaMisura"),
                    Descrizione = reader.IsDBNull("Descrizione") ? null : reader.GetString("Descrizione"),
                    Tipo = reader.GetString("Tipo")
                });
            }

            return unitaMisura;
        }

        // GET UNITÀ DI MISURA PER ATTIVITÀ
        public async Task<List<UnitaMisura>> GetUnitaMisuraAttivita()
        {
            var unitaMisura = await GetUnitaMisura();
            return unitaMisura
                .Where(u => u.Tipo == "ATTIVITA" || u.Tipo == "ENTRAMBI")
                .OrderBy(u => u.UM)
                .ToList();
        }

        // GET UNITÀ DI MISURA PER MATERIALI
        public async Task<List<UnitaMisura>> GetUnitaMisuraMateriali()
        {
            var unitaMisura = await GetUnitaMisura();
            return unitaMisura
                .Where(u => u.Tipo == "MATERIALE" || u.Tipo == "ENTRAMBI")
                .OrderBy(u => u.UM)
                .ToList();
        }

        // GET UNITÀ DI MISURA BY ID
        public async Task<UnitaMisura?> GetUnitaMisuraById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT IdUnitaMisura, UnitaMisura, Descrizione, Tipo
                         FROM UnitaMisura 
                         WHERE IdUnitaMisura = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UnitaMisura
                {
                    IdUnitaMisura = reader.GetInt32("IdUnitaMisura"),
                    UM = reader.GetString("UnitaMisura"),
                    Descrizione = reader.IsDBNull("Descrizione") ? null : reader.GetString("Descrizione"),
                    Tipo = reader.GetString("Tipo")
                };
            }

            return null;
        }
    }
}