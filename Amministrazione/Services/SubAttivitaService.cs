using CNP.SchedaLavorazione.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CNP.Amministrazione.Services
{
    public class SubAttivitaService
    {
        private readonly string _connectionString;

        public SubAttivitaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string not found.");
        }

        // GET TUTTE LE SUBATTIVITÀ PER ATTIVITÀ SPECIFICA
        public async Task<List<SubAttività>> GetSubAttivitaByAttivita(int codiceAttivita)
        {
            var subAttivita = new List<SubAttività>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT ID, CodiceAttivita, CodiceSubattivita, Voce, 
                         N_Operatori, UnitaMisura, Qta_Voce, CodiceMateriale
                 FROM SubAttivita 
                 WHERE CodiceAttivita = @CodiceAttivita
                 ORDER BY CodiceSubattivita";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CodiceAttivita", codiceAttivita);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                subAttivita.Add(new SubAttività
                {
                    ID = reader.GetInt32("ID"),
                    CodiceAttivita = reader.GetInt32("CodiceAttivita"),
                    CodiceSubattivita = reader.GetInt32("CodiceSubattivita"),
                    Voce = reader.GetString("Voce"),
                    N_Operatori = reader.GetInt32("N_Operatori"),
                    UnitaMisura = reader.GetString("UnitaMisura"),
                    Qta_Voce = reader.GetFloat("Qta_Voce"),
                    CodiceMateriale = reader.IsDBNull("CodiceMateriale") ? string.Empty : reader.GetString("CodiceMateriale")
                });
            }

            return subAttivita;
        }

        // LEGGE LE SUBATTIVITA' DELLA ATTIVITA' SELEZIONATA PER CARICARE IL COMBO DELLE VOCI
        // MODIFICA IL METODO PER RESTITUIRE List<SubAttività> MA SOLO CON ID E VOCE
        public async Task<List<SubAttività>> CaricaComboVociPerAttivita(int codiceAttivita)
        {
            var voci = new List<SubAttività>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT ID, Voce
                 FROM SubAttivita 
                 WHERE CodiceAttivita = @CodiceAttivita
                 ORDER BY CodiceSubattivita";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CodiceAttivita", codiceAttivita);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                voci.Add(new SubAttività
                {
                    ID = reader.GetInt32("ID"),
                    Voce = reader.GetString("Voce")
                    // ALTRI CAMPI RESTANO A VALORI DI DEFAULT
                });
            }

            return voci;
        }
    }
}