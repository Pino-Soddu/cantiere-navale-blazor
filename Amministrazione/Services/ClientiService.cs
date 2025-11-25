using CNP.Segreteria.Models;
using Microsoft.Data.SqlClient;
using System.Data;
#nullable disable

namespace CNP.Amministrazione.Services
{
    public class ClientiService
    {
        private readonly string _connectionString;

        public ClientiService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET CLIENTI PER PREVENTIVI (SOLO DATI ESSENZIALI)
        public async Task<List<Cliente>> GetClientiPerPreventivo()
        {
            var clienti = new List<Cliente>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT ClienteID, RagioneSociale, Indirizzo, CAP, Comune, 
                         SiglaProvincia, Telefono, Email 
                         FROM Clienti ORDER BY RagioneSociale";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clienti.Add(new Cliente
                {
                    ClienteID = reader.GetInt32("ClienteID"),
                    RagioneSociale = reader.GetString("RagioneSociale"),
                    Indirizzo = reader.IsDBNull("Indirizzo") ? null : reader.GetString("Indirizzo"),
                    CAP = reader.IsDBNull("CAP") ? null : reader.GetString("CAP"),
                    Comune = reader.IsDBNull("Comune") ? null : reader.GetString("Comune"),
                    SiglaProvincia = reader.IsDBNull("SiglaProvincia") ? null : reader.GetString("SiglaProvincia"),
                    Telefono = reader.IsDBNull("Telefono") ? null : reader.GetString("Telefono"),
                    Email = reader.IsDBNull("Email") ? null : reader.GetString("Email")
                });
            }

            return clienti;
        }

        // GET CLIENTE BY ID (PER DETTAGLI COMPLETI)
        public async Task<Cliente> GetClienteById(int clienteId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT ClienteID, RagioneSociale, Indirizzo, CAP, Comune, 
                         SiglaProvincia, Telefono, Email, PartitaIVA, CodiceFiscale
                         FROM Clienti WHERE ClienteID = @ClienteID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClienteID", clienteId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Cliente
                {
                    ClienteID = reader.GetInt32("ClienteID"),
                    RagioneSociale = reader.GetString("RagioneSociale"),
                    Indirizzo = reader.IsDBNull("Indirizzo") ? null : reader.GetString("Indirizzo"),
                    CAP = reader.IsDBNull("CAP") ? null : reader.GetString("CAP"),
                    Comune = reader.IsDBNull("Comune") ? null : reader.GetString("Comune"),
                    SiglaProvincia = reader.IsDBNull("SiglaProvincia") ? null : reader.GetString("SiglaProvincia"),
                    Telefono = reader.IsDBNull("Telefono") ? null : reader.GetString("Telefono"),
                    Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                    PartitaIVA = reader.IsDBNull("PartitaIVA") ? null : reader.GetString("PartitaIVA"),
                    CodiceFiscale = reader.IsDBNull("CodiceFiscale") ? null : reader.GetString("CodiceFiscale")
                };
            }

            return null;
        }

        // AGGIUNGI CLIENTE ESSENZIALE (PER PREVENTIVI)
        // METODO AGGIUNGI CLIENTE ESSENZIALE - VERSIONE CORRETTA
        public async Task<int> AggiungiClienteEssenziale(Cliente cliente)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Genera codice cliente progressivo
            var codiceQuery = "SELECT ISNULL(MAX(CodiceCliente), 0) + 1 FROM Clienti";
            using var codiceCommand = new SqlCommand(codiceQuery, connection);
            var nuovoCodice = Convert.ToInt32(await codiceCommand.ExecuteScalarAsync());

            var query = @"INSERT INTO Clienti (
            CodiceCliente, RagioneSociale, Indirizzo, Comune, CAP, 
            SiglaProvincia, Telefono, Email, DataPrimoContatto
             ) 
             OUTPUT INSERTED.ClienteID
             VALUES (
                @CodiceCliente, @RagioneSociale, @Indirizzo, @Comune, @CAP,
                @SiglaProvincia, @Telefono, @Email, @DataPrimoContatto
             )";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@CodiceCliente", nuovoCodice);
            command.Parameters.AddWithValue("@RagioneSociale", cliente.RagioneSociale.Trim().ToUpper()); 
            command.Parameters.AddWithValue("@Indirizzo", (object)cliente.Indirizzo ?? DBNull.Value);
            command.Parameters.AddWithValue("@Comune", (object)cliente.Comune ?? DBNull.Value);
            command.Parameters.AddWithValue("@CAP", (object)cliente.CAP ?? DBNull.Value);
            command.Parameters.AddWithValue("@SiglaProvincia", (object)cliente.SiglaProvincia ?? DBNull.Value);
            command.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)cliente.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@DataPrimoContatto", DateTime.Today);

            // Restituisce l'ID del cliente appena inserito
            var nuovoId = Convert.ToInt32(await command.ExecuteScalarAsync());
            return nuovoId;
        }

        // VERIFICA ESISTENZA CLIENTE (PER EVITARE DUPLICATI)
        public async Task<bool> ClienteEsiste(string ragioneSociale)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(1) FROM Clienti WHERE RagioneSociale = @RagioneSociale";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RagioneSociale", ragioneSociale);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<int?> GetCodiceClienteById(int clienteId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT CodiceCliente FROM Clienti WHERE ClienteID = @ClienteID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClienteID", clienteId);

            var result = await command.ExecuteScalarAsync();

            return result != null ? Convert.ToInt32(result) : null;
        }
    }
}