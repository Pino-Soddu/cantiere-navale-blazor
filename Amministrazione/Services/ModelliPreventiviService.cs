using CNP.Amministrazione.Models;
using CNP.SchedaLavorazione.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization; // AGGIUNTA PER GESTIONE CULTURE

namespace CNP.Amministrazione.Services
{
    public class ModelliPreventiviService
    {
        private readonly string _connectionString;

        public ModelliPreventiviService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        #region FUNZIONI HELPER MIGLIORATE PER PARSING

        // FUNZIONI DI PARSING ROBUSTE CON GESTIONE CULTURA ITALIANA
        private int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            // PROVA PRIMA CON CULTURA ITALIANA, POI INVARIANT
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.GetCultureInfo("it-IT"), out int result))
                return result;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                return result;

            Console.WriteLine($"⚠️ Impossibile convertire in int: '{value}'");
            return 0;
        }

        private double ParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            // GESTISCE SIA "," CHE "." COME SEPARATORI DECIMALI
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.GetCultureInfo("it-IT"), out double result))
                return result;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;

            Console.WriteLine($"⚠️ Impossibile convertire in double: '{value}'");
            return 0;
        }

        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            // GESTISCE VALORI MONETARI E NUMERICI
            if (decimal.TryParse(value, NumberStyles.Currency, CultureInfo.GetCultureInfo("it-IT"), out decimal result))
                return result;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            Console.WriteLine($"⚠️ Impossibile convertire in decimal: '{value}'");
            return 0;
        }

        #endregion

        #region LETTURA MODELLI

        // GET TUTTI I MODELLI PREVENTIVO (PER COMBO) 
        public async Task<List<ModelloPreventivo>> GetModelliPreventivo()
        {
            var modelli = new List<ModelloPreventivo>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT DISTINCT 
                 Id, TitoloPreventivo
                 FROM ModelliPreventivi 
                 WHERE VoceId = 1
                 ORDER BY TitoloPreventivo";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                modelli.Add(new ModelloPreventivo
                {
                    Id = reader.GetInt32("Id"),
                    TitoloPreventivo = reader.GetString("TitoloPreventivo")
                });
            }

            return modelli;
        }

        // GET VOCI MODELLO PER TITOLO - VERSIONE CON GESTIONE NULL
        public async Task<List<ModelloPreventivo>> GetVociModelloByTitolo(string titoloModello)
        {
            var voci = new List<ModelloPreventivo>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT 
            Id, TitoloPreventivo, VoceId, DescrizioneAttivita, DescrizioneVoce,
            N_Operatori, UnitaMisura, Qta_Voce, ImportoVoce,
            CodiceMateriale, UM_Mat, Costo_Mat, Qta_Mat, ImportoMateriale, TotaleVoce
         FROM ModelliPreventivi 
         WHERE TitoloPreventivo = @TitoloModello
         ORDER BY VoceId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TitoloModello", titoloModello);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                try
                {
                    var voce = new ModelloPreventivo
                    {
                        // ✅ SOLO DATI DI CONFIGURAZIONE
                        Id = reader.GetInt32("Id"),
                        VoceId = reader.GetInt32("VoceId"),
                        TitoloPreventivo = reader.GetString("TitoloPreventivo"),
                        DescrizioneAttivita = reader.GetString("DescrizioneAttivita"),
                        DescrizioneVoce = reader.GetString("DescrizioneVoce"),
                        UnitaMisura = reader.GetString("UnitaMisura"),
                        CodiceMateriale = reader.IsDBNull("CodiceMateriale") ? string.Empty : reader.GetString("CodiceMateriale"),
                        UM_Mat = reader.IsDBNull("UM_Mat") ? string.Empty : reader.GetString("UM_Mat"),

                        // ✅ CONVERSIONI NUMERICHE SOLO PER CAMPI UTILIZZATI
                        N_Operatori = ParseInt(reader.GetString("N_Operatori")),
                        Qta_Voce = ParseDouble(reader.GetString("Qta_Voce")),

                        // ❌ IGNORA GLI IMPORTI - SARANNO RICALCOLATI
                        // ImportoVoce, ImportoMateriale, TotaleVoce, Costo_Mat, Qta_Mat
                        // vengono lasciati a valori di default (0)
                    };

                    voci.Add(voce);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERRORE conversione voce: {ex.Message}");
                }
            }

            return voci;
        }

        // GET VOCI SPECIFICHE DI UN MODELLO (PER APPLICAZIONE) 
        public async Task<List<ModelloPreventivo>> GetVociModello(int modelloId)
        {
            var voci = new List<ModelloPreventivo>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT 
                    Id, TitoloPreventivo, VoceId, DescrizioneAttivita, DescrizioneVoce,
                    N_Operatori, UnitaMisura, Qta_Voce, ImportoVoce,
                    CodiceMateriale, UM_Mat, Costo_Mat, Qta_Mat, ImportoMateriale, TotaleVoce
                 FROM ModelliPreventivi 
                 WHERE Id = @ModelloId
                 ORDER BY VoceId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ModelloId", modelloId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                try
                {
                    var voce = new ModelloPreventivo
                    {
                        Id = reader.GetInt32("Id"),
                        TitoloPreventivo = reader.GetString("TitoloPreventivo"),
                        VoceId = reader.GetInt32("VoceId"),
                        DescrizioneAttivita = reader.GetString("DescrizioneAttivita"),
                        DescrizioneVoce = reader.GetString("DescrizioneVoce"),
                        UnitaMisura = reader.GetString("UnitaMisura"),
                        CodiceMateriale = reader.IsDBNull("CodiceMateriale") ? string.Empty : reader.GetString("CodiceMateriale"),
                        UM_Mat = reader.IsDBNull("UM_Mat") ? string.Empty : reader.GetString("UM_Mat"),

                        // CONVERSIONE NVARCHAR → NUMERI
                        N_Operatori = ParseInt(reader.GetString("N_Operatori")),
                        Qta_Voce = ParseDouble(reader.GetString("Qta_Voce")),
                        ImportoVoce = ParseDecimal(reader.GetString("ImportoVoce")),
                        Costo_Mat = ParseDecimal(reader.GetString("Costo_Mat")),
                        Qta_Mat = ParseDouble(reader.GetString("Qta_Mat")),
                        ImportoMateriale = ParseDecimal(reader.GetString("ImportoMateriale")),
                        TotaleVoce = ParseDecimal(reader.GetString("TotaleVoce"))
                    };

                    voci.Add(voce);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERRORE conversione voce modello {modelloId}: {ex.Message}");
                }
            }

            return voci;
        }

        #endregion

        #region GESTIONE MODELLI (CRUD)

        // CREA NUOVO MODELLO PREVENTIVO
        public async Task<int> CreaModelloPreventivo(string titoloModello, List<ModelloPreventivo> voci)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // ✅ PRIMA VERIFICA CHE LA TABELLA ABBIA UNA IDENTITY COLUMN
            var checkIdentityQuery = @"
        SELECT COUNT(*) 
        FROM sys.identity_columns 
        WHERE object_id = OBJECT_ID('ModelliPreventivi')";

            using var checkCommand = new SqlCommand(checkIdentityQuery, connection);
            var hasIdentity = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

            int modelloId = 0;

            if (hasIdentity)
            {
                // ✅ USA SCOPE_IDENTITY() SOLO SE C'È UNA IDENTITY COLUMN
                var insertQuery = @"INSERT INTO ModelliPreventivi (
            TitoloPreventivo, VoceId, DescrizioneAttivita, DescrizioneVoce,
            N_Operatori, UnitaMisura, Qta_Voce, ImportoVoce,
            CodiceMateriale, UM_Mat, Qta_Mat, Costo_Mat, ImportoMateriale, TotaleVoce
         ) VALUES (
            @TitoloPreventivo, @VoceId, @DescrizioneAttivita, @DescrizioneVoce,
            @N_Operatori, @UnitaMisura, @Qta_Voce, @ImportoVoce,
            @CodiceMateriale, @UM_Mat, @Qta_Mat, @Costo_Mat, @ImportoMateriale, @TotaleVoce
         )";

                foreach (var voce in voci)
                {
                    using var command = new SqlCommand(insertQuery, connection);

                    // Aggiungi tutti i parametri...
                    command.Parameters.AddWithValue("@TitoloPreventivo", titoloModello);
                    command.Parameters.AddWithValue("@VoceId", voce.VoceId);
                    command.Parameters.AddWithValue("@DescrizioneAttivita", voce.DescrizioneAttivita);
                    command.Parameters.AddWithValue("@DescrizioneVoce", voce.DescrizioneVoce);
                    command.Parameters.AddWithValue("@N_Operatori", voce.N_Operatori);
                    command.Parameters.AddWithValue("@UnitaMisura", voce.UnitaMisura);
                    command.Parameters.AddWithValue("@Qta_Voce", voce.Qta_Voce);
                    command.Parameters.AddWithValue("@ImportoVoce", voce.ImportoVoce);
                    command.Parameters.AddWithValue("@CodiceMateriale",
                        string.IsNullOrEmpty(voce.CodiceMateriale) ? (object)DBNull.Value : voce.CodiceMateriale);
                    command.Parameters.AddWithValue("@UM_Mat",
                        string.IsNullOrEmpty(voce.UM_Mat) ? (object)DBNull.Value : voce.UM_Mat);
                    command.Parameters.AddWithValue("@Qta_Mat", voce.Qta_Mat);
                    command.Parameters.AddWithValue("@Costo_Mat", voce.Costo_Mat);
                    command.Parameters.AddWithValue("@ImportoMateriale", voce.ImportoMateriale);
                    command.Parameters.AddWithValue("@TotaleVoce", voce.TotaleVoce);

                    await command.ExecuteNonQueryAsync();
                }

                // ✅ RECUPERA L'ID IN MODO SICURO
                using var idCommand = new SqlCommand("SELECT SCOPE_IDENTITY()", connection);
                var result = await idCommand.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    modelloId = Convert.ToInt32(result);
                }
            }
            else
            {
                // ✅ FALLBACK: USA MAX(ID) + 1 SE NON C'È IDENTITY
                var maxIdQuery = "SELECT ISNULL(MAX(Id), 0) FROM ModelliPreventivi";
                using var maxCommand = new SqlCommand(maxIdQuery, connection);
                modelloId = Convert.ToInt32(await maxCommand.ExecuteScalarAsync()) + 1;

                var insertQuery = @"INSERT INTO ModelliPreventivi (
            Id, TitoloPreventivo, VoceId, DescrizioneAttivita, DescrizioneVoce,
            N_Operatori, UnitaMisura, Qta_Voce, ImportoVoce,
            CodiceMateriale, UM_Mat, Qta_Mat, Costo_Mat, ImportoMateriale, TotaleVoce
         ) VALUES (
            @Id, @TitoloPreventivo, @VoceId, @DescrizioneAttivita, @DescrizioneVoce,
            @N_Operatori, @UnitaMisura, @Qta_Voce, @ImportoVoce,
            @CodiceMateriale, @UM_Mat, @Qta_Mat, @Costo_Mat, @ImportoMateriale, @TotaleVoce
         )";

                foreach (var voce in voci)
                {
                    using var command = new SqlCommand(insertQuery, connection);

                    command.Parameters.AddWithValue("@Id", modelloId);
                    // ... altri parametri uguali a sopra
                    command.Parameters.AddWithValue("@TitoloPreventivo", titoloModello);
                    command.Parameters.AddWithValue("@VoceId", voce.VoceId);
                    command.Parameters.AddWithValue("@DescrizioneAttivita", voce.DescrizioneAttivita);
                    command.Parameters.AddWithValue("@DescrizioneVoce", voce.DescrizioneVoce);
                    command.Parameters.AddWithValue("@N_Operatori", voce.N_Operatori);
                    command.Parameters.AddWithValue("@UnitaMisura", voce.UnitaMisura);
                    command.Parameters.AddWithValue("@Qta_Voce", voce.Qta_Voce);
                    command.Parameters.AddWithValue("@ImportoVoce", voce.ImportoVoce);
                    command.Parameters.AddWithValue("@CodiceMateriale",
                        string.IsNullOrEmpty(voce.CodiceMateriale) ? (object)DBNull.Value : voce.CodiceMateriale);
                    command.Parameters.AddWithValue("@UM_Mat",
                        string.IsNullOrEmpty(voce.UM_Mat) ? (object)DBNull.Value : voce.UM_Mat);
                    command.Parameters.AddWithValue("@Qta_Mat", voce.Qta_Mat);
                    command.Parameters.AddWithValue("@Costo_Mat", voce.Costo_Mat);
                    command.Parameters.AddWithValue("@ImportoMateriale", voce.ImportoMateriale);
                    command.Parameters.AddWithValue("@TotaleVoce", voce.TotaleVoce);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return modelloId;
        }

        // ELIMINA MODELLO PREVENTIVO
        public async Task<bool> EliminaModelloPreventivo(int modelloId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM ModelliPreventivi WHERE Id = @ModelloId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ModelloId", modelloId);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // VERIFICA SE MODELLO ESISTE
        public async Task<bool> ModelloEsiste(string titoloModello)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(1) FROM ModelliPreventivi WHERE TitoloPreventivo = @TitoloModello";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TitoloModello", titoloModello);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        #endregion

        #region APPLICAZIONE MODELLO A PREVENTIVO

        // CONVERTE VOCI MODELLO IN VOCI PREVENTIVO REALE
        public List<DettaglioPreventivo> ConvertiModelloInPreventivo(List<ModelloPreventivo> vociModello, decimal lunghezzaBarca = 0)
        {
            var vociPreventivo = new List<DettaglioPreventivo>();
            var voceId = 1;

            foreach (var voceModello in vociModello)
            {
                var vocePreventivo = new DettaglioPreventivo
                {
                    Id = voceId++,
                    VoceId = voceModello.VoceId,
                    DescrizioneAttivita = voceModello.DescrizioneAttivita,
                    DescrizioneVoce = voceModello.DescrizioneVoce,
                    N_Operatori = voceModello.N_Operatori,
                    UnitaMisura = voceModello.UnitaMisura,
                    Qta_Voce = (float)voceModello.Qta_Voce,
                    CodiceMateriale = voceModello.CodiceMateriale,
                    UM_Mat = voceModello.UM_Mat,

                    // ✅ IMPORTI INIZIALIZZATI A ZERO - SARANNO CALCOLATI SUCCESSIVAMENTE
                    ImportoVoce = 0,
                    ImportoMateriale = 0,
                    TotaleVoce = 0,
                    Qta_Mat = 0,
                    Costo_Mat = 0
                };

                vociPreventivo.Add(vocePreventivo);
            }

            return vociPreventivo;
        }
        #endregion
    }
}