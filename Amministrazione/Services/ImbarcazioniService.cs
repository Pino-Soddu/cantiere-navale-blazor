using CNP.Segreteria.Models;
using Microsoft.Data.SqlClient;
using System.Data;
#nullable disable

namespace CNP.Amministrazione.Services
{
    public class ImbarcazioniService
    {
        private readonly string _connectionString;

        public ImbarcazioniService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET IMBARCAZIONI PER PREVENTIVI (SOLO DATI ESSENZIALI)
        public async Task<List<Imbarcazione>> GetImbarcazioniPerPreventivo()
        {
            var imbarcazioni = new List<Imbarcazione>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT 
                    i.ID_Imbarcazione, i.Nome_Imbarcazione, i.Lunghezza, i.PortoAttracco,
                    t.Nome_Tipo, c.RagioneSociale
                 FROM Imbarcazioni i
                 LEFT JOIN Tipo_Imbarcazione t ON i.ID_Tipo = t.ID_Tipo
                 LEFT JOIN Clienti c ON i.ID_Cliente = c.ClienteID
                 ORDER BY i.Nome_Imbarcazione";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                imbarcazioni.Add(new Imbarcazione
                {
                    ID_Imbarcazione = reader.GetInt32("ID_Imbarcazione"),
                    Nome_Imbarcazione = reader.GetString("Nome_Imbarcazione"),
                    Lunghezza = reader.IsDBNull("Lunghezza") ? null : reader.GetDecimal("Lunghezza"),
                    PortoAttracco = reader.IsDBNull("PortoAttracco") ? null : reader.GetString("PortoAttracco"),
                    Nome_Tipo = reader.IsDBNull("Nome_Tipo") ? null : reader.GetString("Nome_Tipo"),
                    RagioneSociale_Cliente = reader.IsDBNull("RagioneSociale") ? null : reader.GetString("RagioneSociale")
                });
            }

            return imbarcazioni;
        }

        // GET IMBARCAZIONI BY CLIENTE (FILTRO PER CLIENTE SELEZIONATO)
        public async Task<List<Imbarcazione>> GetImbarcazioniByCliente(int codiceCliente)
        {
            var imbarcazioni = new List<Imbarcazione>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT 
                i.ID_Imbarcazione, i.Nome_Imbarcazione, i.Lunghezza, i.PortoAttracco,
                t.Nome_Tipo
                 FROM Imbarcazioni i
                 LEFT JOIN Tipo_Imbarcazione t ON i.ID_Tipo = t.ID_Tipo
                 WHERE i.ID_Cliente = @CodiceCliente 
                 ORDER BY i.Nome_Imbarcazione";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CodiceCliente", codiceCliente);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                imbarcazioni.Add(new Imbarcazione
                {
                    ID_Imbarcazione = reader.GetInt32("ID_Imbarcazione"),
                    Nome_Imbarcazione = reader.GetString("Nome_Imbarcazione"),
                    Lunghezza = reader.IsDBNull("Lunghezza") ? null : reader.GetDecimal("Lunghezza"),
                    PortoAttracco = reader.IsDBNull("PortoAttracco") ? null : reader.GetString("PortoAttracco"),
                    Nome_Tipo = reader.IsDBNull("Nome_Tipo") ? null : reader.GetString("Nome_Tipo")
                });
            }

            return imbarcazioni;
        }
        
        // AGGIUNGI IMBARCAZIONE ESSENZIALE (PER PREVENTIVI)
        public async Task<bool> AggiungiImbarcazioneEssenziale(Imbarcazione imbarcazione)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Imbarcazioni (
                    Nome_Imbarcazione, ID_Tipo, ID_Cliente, Lunghezza, PortoAttracco
                 ) VALUES (
                    @Nome_Imbarcazione, @ID_Tipo, @ID_Cliente, @Lunghezza, @PortoAttracco
                 )";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Nome_Imbarcazione", imbarcazione.Nome_Imbarcazione);
            command.Parameters.AddWithValue("@ID_Tipo", imbarcazione.ID_Tipo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ID_Cliente", imbarcazione.ID_Cliente ?? (object)DBNull.Value); // Questo è CodiceCliente
            command.Parameters.AddWithValue("@Lunghezza", imbarcazione.Lunghezza ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PortoAttracco", imbarcazione.PortoAttracco ?? (object)DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        // GET TIPI IMBARCAZIONE PER COMBO
        public async Task<List<TipoImbarcazione>> GetTipiImbarcazione()
        {
            var tipi = new List<TipoImbarcazione>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT ID_Tipo, Nome_Tipo FROM Tipo_Imbarcazione ORDER BY Nome_Tipo";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tipi.Add(new TipoImbarcazione
                {
                    ID_Tipo = reader.GetInt32("ID_Tipo"),
                    Nome_Tipo = reader.GetString("Nome_Tipo")
                });
            }

            return tipi;
        }

        // VERIFICA ESISTENZA IMBARCAZIONE (PER EVITARE DUPLICATI)
        public async Task<bool> ImbarcazioneEsiste(string nomeImbarcazione)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(1) FROM Imbarcazioni WHERE Nome_Imbarcazione = @Nome_Imbarcazione";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nome_Imbarcazione", nomeImbarcazione);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }
}