using CNP.SchedaLavorazione.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CNP.Amministrazione.Services
{
    public class DettaglioPreventiviService
    {
        private readonly string _connectionString;

        public DettaglioPreventiviService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // AGGIUNGI SINGOLO DETTAGLIO PREVENTIVO
        public async Task<bool> AggiungiDettaglioPreventivo(DettaglioPreventivo dettaglio)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO DettaglioPreventivi (
                    PreventivoId, VoceId, DescrizioneAttivita, DescrizioneVoce,
                    N_Operatori, UnitaMisura, Qta_Voce, ImportoVoce,
                    CodiceMateriale, UM_Mat, Costo_Mat, Qta_Mat, ImportoMateriale, TotaleVoce
                 ) VALUES (
                    @PreventivoId, @VoceId, @DescrizioneAttivita, @DescrizioneVoce,
                    @N_Operatori, @UnitaMisura, @Qta_Voce, @ImportoVoce,
                    @CodiceMateriale, @UM_Mat, @Costo_Mat, @Qta_Mat, @ImportoMateriale, @TotaleVoce
                 )";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PreventivoId", dettaglio.PreventivoId);
            command.Parameters.AddWithValue("@VoceId", dettaglio.VoceId);
            command.Parameters.AddWithValue("@DescrizioneAttivita", dettaglio.DescrizioneAttivita);
            command.Parameters.AddWithValue("@DescrizioneVoce", dettaglio.DescrizioneVoce);
            command.Parameters.AddWithValue("@N_Operatori", dettaglio.N_Operatori);
            command.Parameters.AddWithValue("@UnitaMisura", dettaglio.UnitaMisura);
            command.Parameters.AddWithValue("@Qta_Voce", dettaglio.Qta_Voce);
            command.Parameters.AddWithValue("@ImportoVoce", dettaglio.ImportoVoce);
            command.Parameters.AddWithValue("@CodiceMateriale", dettaglio.CodiceMateriale);
            command.Parameters.AddWithValue("@UM_Mat", dettaglio.UM_Mat);
            command.Parameters.AddWithValue("@Costo_Mat", dettaglio.Costo_Mat);
            command.Parameters.AddWithValue("@Qta_Mat", dettaglio.Qta_Mat);
            command.Parameters.AddWithValue("@ImportoMateriale", dettaglio.ImportoMateriale);
            command.Parameters.AddWithValue("@TotaleVoce", dettaglio.TotaleVoce);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // GET DETTAGLI PER PREVENTIVO
        public async Task<List<DettaglioPreventivo>> GetDettagliByPreventivoId(int preventivoId)
        {
            var dettagli = new List<DettaglioPreventivo>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT Id, PreventivoId, VoceId, DescrizioneAttivita, DescrizioneVoce,
                         N_Operatori, UnitaMisura, Qta_Voce, ImportoVoce,
                         CodiceMateriale, UM_Mat, Costo_Mat, Qta_Mat, ImportoMateriale, TotaleVoce
                 FROM DettaglioPreventivi 
                 WHERE PreventivoId = @PreventivoId
                 ORDER BY VoceId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PreventivoId", preventivoId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                dettagli.Add(new DettaglioPreventivo
                {
                    Id = reader.GetInt32("Id"),
                    PreventivoId = reader.GetInt32("PreventivoId"),
                    VoceId = reader.GetInt32("VoceId"),
                    DescrizioneAttivita = reader.GetString("DescrizioneAttivita"),
                    DescrizioneVoce = reader.GetString("DescrizioneVoce"),
                    N_Operatori = reader.GetInt32("N_Operatori"),
                    UnitaMisura = reader.GetString("UnitaMisura"),
                    Qta_Voce = reader.GetFloat("Qta_Voce"),
                    ImportoVoce = reader.GetFloat("ImportoVoce"),
                    CodiceMateriale = reader.GetString("CodiceMateriale"),
                    UM_Mat = reader.GetString("UM_Mat"),
                    Costo_Mat = reader.GetFloat("Costo_Mat"),
                    Qta_Mat = reader.GetFloat("Qta_Mat"),
                    ImportoMateriale = reader.GetFloat("ImportoMateriale"),
                    TotaleVoce = reader.GetFloat("TotaleVoce")
                });
            }

            return dettagli;
        }

        // ELIMINA DETTAGLI PREVENTIVO
        public async Task<bool> EliminaDettagliPreventivo(int preventivoId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM DettaglioPreventivi WHERE PreventivoId = @PreventivoId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PreventivoId", preventivoId);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
    }
}