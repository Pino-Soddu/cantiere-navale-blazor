using CNP.SchedaLavorazione.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CNP.Amministrazione.Services
{
    public class SchedaLavorazioneService
    {
        private readonly string _connectionString;

        public SchedaLavorazioneService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // GENERA SCHEDE LAVORAZIONE DA PREVENTIVO
        // MODIFICA SchedaLavorazioneService.cs
        public async Task<bool> GeneraSchedeLavorazioneDaPreventivo(int preventivoId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                // 1. ✅ ELIMINA SCHEDE ESISTENTI
                var eliminato = await EliminaSchedeEsistenti(preventivoId, connection, transaction);
                if (!eliminato)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 2. RECUPERA VOCI PREVENTIVO
                var vociPreventivo = await RecuperaVociPreventivo(preventivoId, connection, transaction);

                if (!vociPreventivo.Any())
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 3. GENERA NUOVE SCHEDE
                var voceIndex = 1;
                foreach (var voce in vociPreventivo)
                {
                    var success = await CreaSchedaLavorazione(connection, transaction, preventivoId, voce, voceIndex);
                    if (!success)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                    voceIndex++;
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"❌ Errore generazione schede lavorazione: {ex.Message}");
                return false;
            }
        }

        // AGGIUNGI METODO PER ELIMINARE SCHEDE ESISTENTI
        private async Task<bool> EliminaSchedeEsistenti(int preventivoId, SqlConnection connection, SqlTransaction transaction)
        {
            var query = "DELETE FROM SchedaLavorazioni WHERE PreventivoId = @PreventivoId";

            using var command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@PreventivoId", preventivoId);

            await command.ExecuteNonQueryAsync();
            return true; // Ritorna sempre true, anche se non c'erano schede
        }

        // RECUPERA VOCI PREVENTIVO
        private async Task<List<DettaglioPreventivo>> RecuperaVociPreventivo(int preventivoId, SqlConnection connection, SqlTransaction transaction)
        {
            var voci = new List<DettaglioPreventivo>();

            var query = @"SELECT Id, DescrizioneAttivita, DescrizioneVoce, CodiceMateriale
                         FROM DettaglioPreventivi 
                         WHERE PreventivoId = @PreventivoId
                         ORDER BY Id";

            using var command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@PreventivoId", preventivoId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                voci.Add(new DettaglioPreventivo
                {
                    Id = reader.GetInt32("Id"),
                    DescrizioneAttivita = reader.GetString("DescrizioneAttivita"),
                    DescrizioneVoce = reader.GetString("DescrizioneVoce"),
                    CodiceMateriale = reader.IsDBNull("CodiceMateriale") ? string.Empty : reader.GetString("CodiceMateriale")
                });
            }

            return voci;
        }

        // CREA SINGOLA SCHEDA LAVORAZIONE
        private async Task<bool> CreaSchedaLavorazione(SqlConnection connection, SqlTransaction transaction,
                                                      int preventivoId, DettaglioPreventivo voce, int idVoce)
        {
            var query = @"INSERT INTO SchedaLavorazioni (
                        PreventivoId, IdVoce, Attività, Voce, Stato, 
                        CodiceMateriale, QtaMateriale
                     ) VALUES (
                        @PreventivoId, @IdVoce, @Attivita, @Voce, @Stato,
                        @CodiceMateriale, @QtaMateriale
                     )";

            using var command = new SqlCommand(query, connection, transaction);

            command.Parameters.AddWithValue("@PreventivoId", preventivoId);
            command.Parameters.AddWithValue("@IdVoce", idVoce);
            command.Parameters.AddWithValue("@Attivita", voce.DescrizioneAttivita);
            command.Parameters.AddWithValue("@Voce", voce.DescrizioneVoce);
            command.Parameters.AddWithValue("@Stato", "Da Iniziare");
            command.Parameters.AddWithValue("@CodiceMateriale", (object)voce.CodiceMateriale ?? DBNull.Value);
            command.Parameters.AddWithValue("@QtaMateriale", 0.0); // Inizializza a 0

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // VERIFICA SE SCHEDE ESISTONO GIÀ
        public async Task<bool> SchedeLavorazioneEsistono(int preventivoId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(1) FROM SchedaLavorazioni WHERE PreventivoId = @PreventivoId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PreventivoId", preventivoId);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }
}