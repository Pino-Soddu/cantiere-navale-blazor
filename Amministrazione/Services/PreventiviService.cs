using CNP.SchedaLavorazione.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CNP.Amministrazione.Services
{
    public class PreventiviService
    {
        private readonly string _connectionString;
        private readonly SchedaLavorazioneService _schedaLavorazioneService;

        public PreventiviService(IConfiguration configuration, SchedaLavorazioneService schedaLavorazioneService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _schedaLavorazioneService = schedaLavorazioneService;
        }

        #region GESTIONE CRUD PREVENTIVI

        // GET PREVENTIVI
        public async Task<List<Preventivo>> GetPreventivi()
        {
            var preventivi = new List<Preventivo>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT Id, PreventivoId, DataCreazione, Descrizione, ClienteId, 
                 RagioneSociale, ImportoTotale, Stato, NomeBarca, NomePreventivo,
                 UserNameCliente, PasswordTemporanea, PasswordCambiata 
                 FROM Preventivi";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                preventivi.Add(new Preventivo
                {
                    PreventivoId = reader.GetInt32("PreventivoId"),
                    DataCreazione = reader.GetDateTime("DataCreazione"),
                    Descrizione = reader.IsDBNull("Descrizione") ? string.Empty : reader.GetString("Descrizione"), // ✅ string.Empty invece di null
                    ClienteId = reader.GetInt32("ClienteId"),
                    RagioneSociale = reader.IsDBNull("RagioneSociale") ? string.Empty : reader.GetString("RagioneSociale"), // ✅ string.Empty
                    ImportoTotale = reader.GetDecimal("ImportoTotale"),
                    Stato = reader.IsDBNull("Stato") ? string.Empty : reader.GetString("Stato"), // ✅ string.Empty
                    NomeBarca = reader.IsDBNull("NomeBarca") ? string.Empty : reader.GetString("NomeBarca"), // ✅ string.Empty
                    NomePreventivo = reader.IsDBNull("NomePreventivo") ? string.Empty : reader.GetString("NomePreventivo"), // ✅ string.Empty
                    UserNameCliente = reader.IsDBNull("UserNameCliente") ? string.Empty : reader.GetString("UserNameCliente"), // ✅ string.Empty
                    PasswordTemporanea = reader.IsDBNull("PasswordTemporanea") ? string.Empty : reader.GetString("PasswordTemporanea"), // ✅ string.Empty
                    PasswordCambiata = reader.IsDBNull("PasswordCambiata") ? string.Empty : reader.GetString("PasswordCambiata") // ✅ string.Empty
                });
            }

            return preventivi;
        }

        // AGGIUNGI PREVENTIVO
        public async Task<bool> AggiungiPreventivo(Preventivo preventivo)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Preventivi (
                    PreventivoId, DataCreazione, Descrizione, ClienteId, RagioneSociale,
                    ImportoTotale, Stato, NomeBarca, NomePreventivo,
                    UserNameCliente, PasswordTemporanea, PasswordCambiata
                 ) VALUES (
                    @PreventivoId, @DataCreazione, @Descrizione, @ClienteId, @RagioneSociale,
                    @ImportoTotale, @Stato, @NomeBarca, @NomePreventivo,
                    @UserNameCliente, @PasswordTemporanea, @PasswordCambiata
                 )";

            using var command = new SqlCommand(query, connection);

            // Aggiungi parametri
            command.Parameters.AddWithValue("@PreventivoId", preventivo.PreventivoId);
            command.Parameters.AddWithValue("@DataCreazione", preventivo.DataCreazione);
            command.Parameters.AddWithValue("@Descrizione", (object)preventivo.Descrizione ?? DBNull.Value);
            command.Parameters.AddWithValue("@ClienteId", preventivo.ClienteId);
            command.Parameters.AddWithValue("@RagioneSociale", (object)preventivo.RagioneSociale ?? DBNull.Value);
            command.Parameters.AddWithValue("@ImportoTotale", preventivo.ImportoTotale);
            command.Parameters.AddWithValue("@Stato", (object)preventivo.Stato ?? DBNull.Value);
            command.Parameters.AddWithValue("@NomeBarca", (object)preventivo.NomeBarca ?? DBNull.Value);
            command.Parameters.AddWithValue("@NomePreventivo", (object)preventivo.NomePreventivo ?? DBNull.Value);
            command.Parameters.AddWithValue("@UserNameCliente", (object)preventivo.UserNameCliente ?? DBNull.Value);
            command.Parameters.AddWithValue("@PasswordTemporanea", (object)preventivo.PasswordTemporanea ?? DBNull.Value);
            command.Parameters.AddWithValue("@PasswordCambiata", (object)preventivo.PasswordCambiata ?? DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();

            return result > 0;
        }

        // MODIFICA PREVENTIVO
        public async Task<bool> ModificaPreventivo(Preventivo preventivo)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE Preventivi SET 
                    PreventivoId = @PreventivoId,
                    DataCreazione = @DataCreazione,
                    Descrizione = @Descrizione,
                    ClienteId = @ClienteId,
                    RagioneSociale = @RagioneSociale,
                    ImportoTotale = @ImportoTotale,
                    Stato = @Stato,
                    NomeBarca = @NomeBarca,
                    NomePreventivo = @NomePreventivo,
                    UserNameCliente = @UserNameCliente,
                    PasswordTemporanea = @PasswordTemporanea,
                    PasswordCambiata = @PasswordCambiata
                 WHERE PreventivoId = @PreventivoId";

            using var command = new SqlCommand(query, connection);

            // Aggiungi parametri
            command.Parameters.AddWithValue("@PreventivoId", preventivo.PreventivoId);
            command.Parameters.AddWithValue("@DataCreazione", preventivo.DataCreazione);
            command.Parameters.AddWithValue("@Descrizione", (object)preventivo.Descrizione ?? DBNull.Value);
            command.Parameters.AddWithValue("@ClienteId", preventivo.ClienteId);
            command.Parameters.AddWithValue("@RagioneSociale", (object)preventivo.RagioneSociale ?? DBNull.Value);
            command.Parameters.AddWithValue("@ImportoTotale", preventivo.ImportoTotale);
            command.Parameters.AddWithValue("@Stato", (object)preventivo.Stato ?? DBNull.Value);
            command.Parameters.AddWithValue("@NomeBarca", (object)preventivo.NomeBarca ?? DBNull.Value);
            command.Parameters.AddWithValue("@NomePreventivo", (object)preventivo.NomePreventivo ?? DBNull.Value);
            command.Parameters.AddWithValue("@UserNameCliente", (object)preventivo.UserNameCliente ?? DBNull.Value);
            command.Parameters.AddWithValue("@PasswordTemporanea", (object)preventivo.PasswordTemporanea ?? DBNull.Value);
            command.Parameters.AddWithValue("@PasswordCambiata", (object)preventivo.PasswordCambiata ?? DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // ELIMINA PREVENTIVO COMPLETO (RIEPILOGO + DETTAGLI)
        public async Task<bool> EliminaPreventivoCompleto(int preventivoId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // 🔥 CORREZIONE: usa SqlTransaction invece di DbTransaction
            using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                // 1. PRIMA ELIMINA I DETTAGLI
                var deleteDettagliQuery = "DELETE FROM DettaglioPreventivi WHERE PreventivoId = @PreventivoId";
                using var cmdDettagli = new SqlCommand(deleteDettagliQuery, connection, transaction);
                cmdDettagli.Parameters.AddWithValue("@PreventivoId", preventivoId);
                await cmdDettagli.ExecuteNonQueryAsync();

                // 2. POI ELIMINA L'INTESTAZIONE
                var deletePreventivoQuery = "DELETE FROM Preventivi WHERE PreventivoId = @PreventivoId";
                using var cmdPreventivo = new SqlCommand(deletePreventivoQuery, connection, transaction);
                cmdPreventivo.Parameters.AddWithValue("@PreventivoId", preventivoId);
                var result = await cmdPreventivo.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return result > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // GET PROSSIMO PREVENTIVO ID (per nuovo preventivo)
        public async Task<int> GetProssimoPreventivoId()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT ISNULL(MAX(PreventivoId), 0) + 1 FROM Preventivi";

            using var command = new SqlCommand(query, connection);
            var result = await command.ExecuteScalarAsync();

            return result != null ? Convert.ToInt32(result) : 1;
        }

        // LETTURA PREVENTIVO PER MODIFICA
        public async Task<Preventivo?> GetPreventivoById(int preventivoId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT Id, PreventivoId, DataCreazione, DataScadenza, Descrizione, 
                 ClienteId, RagioneSociale, ImportoTotale, Stato, NomeBarca, NomePreventivo,
                 UserNameCliente, PasswordTemporanea, PasswordCambiata 
                 FROM Preventivi 
                 WHERE PreventivoId = @PreventivoId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PreventivoId", preventivoId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Preventivo
                {
                    PreventivoId = reader.GetInt32("PreventivoId"),
                    DataCreazione = reader.GetDateTime("DataCreazione"),
                    DataScadenza = reader.GetDateTime("DataScadenza"),
                    Descrizione = reader.IsDBNull("Descrizione") ? string.Empty : reader.GetString("Descrizione"),
                    ClienteId = reader.GetInt32("ClienteId"),
                    RagioneSociale = reader.IsDBNull("RagioneSociale") ? string.Empty : reader.GetString("RagioneSociale"),
                    ImportoTotale = reader.GetDecimal("ImportoTotale"),
                    Stato = reader.IsDBNull("Stato") ? string.Empty : reader.GetString("Stato"),
                    NomeBarca = reader.IsDBNull("NomeBarca") ? string.Empty : reader.GetString("NomeBarca"),
                    NomePreventivo = reader.IsDBNull("NomePreventivo") ? string.Empty : reader.GetString("NomePreventivo"),
                    UserNameCliente = reader.IsDBNull("UserNameCliente") ? string.Empty : reader.GetString("UserNameCliente"),
                    PasswordTemporanea = reader.IsDBNull("PasswordTemporanea") ? string.Empty : reader.GetString("PasswordTemporanea"),
                    PasswordCambiata = reader.IsDBNull("PasswordCambiata") ? string.Empty : reader.GetString("PasswordCambiata")
                };
            }

            return null; // Preventivo non trovato
        }

        #endregion

        #region GESTIONE INVIO PREVENTIVI

        // METODO PER CAMBIARE STATO A "INVIATO" E REGISTRARE DATA/EMAIL
        public async Task<bool> CambiaStatoInviato(int preventivoId, string emailCliente)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE Preventivi SET 
                    Stato = 'INVIATO',
                    DataInvio = @DataInvio,
                    EmailCliente = @EmailCliente
                 WHERE PreventivoId = @PreventivoId";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PreventivoId", preventivoId);
            command.Parameters.AddWithValue("@DataInvio", DateTime.Now);
            command.Parameters.AddWithValue("@EmailCliente", (object)emailCliente ?? DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // METODO PER VERIFICARE SE UN PREVENTIVO PUÒ ESSERE INVIATO
        public async Task<bool> PuoEssereInviato(int preventivoId)
        {
            var preventivo = await GetPreventivoById(preventivoId);
            return preventivo != null && preventivo.Stato?.ToUpper() == "BOZZA";
        }
        #endregion

        #region GESTIONE CREDENZIALI CLIENTE

        // GENERA USERNAME AUTOMATICO
        private string GeneraUsername(int preventivoId, string nomeBarca)
        {
            // GESTISCI STRINGHE NULL O VUOTE
            var nomeBarcaSicuro = nomeBarca ?? "BARCA";

            var inizialiBarca = new string(nomeBarcaSicuro
                .Where(char.IsLetterOrDigit)
                .Take(3)
                .ToArray())
                .ToUpper();

            if (string.IsNullOrEmpty(inizialiBarca) || inizialiBarca.Length < 2)
                inizialiBarca = "BAR";

            return $"CN{preventivoId}{inizialiBarca}";
        }

        // GENERA PASSWORD CASUALE
        private string GeneraPassword()
        {
            const string lettere = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numeri = "0123456789";
            var random = new Random();

            var password = new char[8];

            // 5 lettere
            for (int i = 0; i < 5; i++)
            {
                password[i] = lettere[random.Next(lettere.Length)];
            }

            // 3 numeri
            for (int i = 5; i < 8; i++)
            {
                password[i] = numeri[random.Next(numeri.Length)];
            }

            // Mescola
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }

        // GENERA E SALVA CREDENZIALI PER PREVENTIVO
        public async Task<bool> GeneraCredenzialiCliente(int preventivoId, string? nomeBarca)
        {
            try
            {
                // GESTISCI NOME BARCA NULL
                var nomeBarcaSicuro = nomeBarca ?? "BARCA";
                var username = GeneraUsername(preventivoId, nomeBarcaSicuro);
                var password = GeneraPassword();

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"UPDATE Preventivi SET 
                      UserNameCliente = @UserNameCliente,
                      PasswordTemporanea = @PasswordTemporanea
                      WHERE PreventivoId = @PreventivoId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserNameCliente", username);
                command.Parameters.AddWithValue("@PasswordTemporanea", password);
                command.Parameters.AddWithValue("@PreventivoId", preventivoId);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore generazione credenziali: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region
        // METODO DI GESTIONE CAMBIO STATO CON TRIGGER
        public async Task<bool> CambiaStatoConGenerazioneLavorazione(int preventivoId, string nuovoStato, string emailCliente)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                // 1. AGGIORNA STATO PREVENTIVO
                var query = @"UPDATE Preventivi SET 
                    Stato = @Stato,
                    DataInvio = CASE WHEN @Stato = 'INVIATO' THEN GETDATE() ELSE DataInvio END
                 WHERE PreventivoId = @PreventivoId";

                using var command = new SqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@PreventivoId", preventivoId);
                command.Parameters.AddWithValue("@Stato", nuovoStato);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0 && nuovoStato == "IN LAVORAZIONE")
                {
                    // 2. GENERA SCHEDE LAVORAZIONE SE STATO = "IN LAVORAZIONE"
                    var schedeGenerate = await _schedaLavorazioneService.GeneraSchedeLavorazioneDaPreventivo(preventivoId);
                    if (!schedeGenerate)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"❌ Errore cambio stato: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}

