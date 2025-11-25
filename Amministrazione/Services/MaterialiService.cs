using CNP.Segreteria.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CNP.Amministrazione.Services
{
    public class MaterialiService
    {
        private readonly string _connectionString;

        public MaterialiService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string not found.");
        }

        // GET TUTTI I MATERIALI PER COMBO
        public async Task<List<Materiale>> GetMaterialiPerCombo()
        {
            var materiali = new List<Materiale>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT IDMateriale, CodiceMateriale, Descrizione, UnitaMisura, PrezzoUnitario
                         FROM Materiali 
                         ORDER BY CodiceMateriale";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                materiali.Add(new Materiale
                {
                    IDMateriale = reader.GetInt32("IDMateriale"),
                    CodiceMateriale = reader.GetString("CodiceMateriale"),
                    Descrizione = reader.IsDBNull("Descrizione") ? null : reader.GetString("Descrizione"),
                    UnitaMisura = reader.IsDBNull("UnitaMisura") ? null : reader.GetString("UnitaMisura"),
                    PrezzoUnitario = reader.IsDBNull("PrezzoUnitario") ? null : reader.GetDecimal("PrezzoUnitario")
                });
            }

            return materiali;
        }

        // GET MATERIALE BY CODICE
        public async Task<Materiale?> GetMaterialeByCodice(string codiceMateriale)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT IDMateriale, CodiceMateriale, Descrizione, UnitaMisura, PrezzoUnitario
                 FROM Materiali 
                 WHERE CodiceMateriale = @CodiceMateriale";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CodiceMateriale", codiceMateriale);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Materiale
                {
                    IDMateriale = reader.GetInt32("IDMateriale"),
                    CodiceMateriale = reader.GetString("CodiceMateriale"),
                    Descrizione = reader.IsDBNull("Descrizione") ? null : reader.GetString("Descrizione"),
                    UnitaMisura = reader.IsDBNull("UnitaMisura") ? null : reader.GetString("UnitaMisura"),
                    PrezzoUnitario = reader.IsDBNull("PrezzoUnitario") ? null : reader.GetDecimal("PrezzoUnitario")
                };
            }

            return null;
        }
    }
}