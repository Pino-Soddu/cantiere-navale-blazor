using CNP.Segreteria.Models;
using Microsoft.Data.SqlClient;
using System.Data;
#nullable disable

namespace CNP.Segreteria.Services
{
    public class AnagraficheService
    {
        private readonly string _connectionString;

        public AnagraficheService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        #region GESTIONE CRUD CLIENTI
        // CLIENTI
        public async Task<List<Cliente>> GetClienti()
        {
            var clienti = new List<Cliente>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT ClienteID, CodiceCliente, RagioneSociale, NomeContatto, RuoloContatto, 
                         TipoCliente, Indirizzo, Comune, CAP, SiglaProvincia, Telefono, Email, 
                         PartitaIVA, CodiceFiscale, IBAN, BancaRiferimento, TipologiaServizi, 
                         PortoRiferimento, TerminiPagamento, DataPrimoContatto, ValutazioneCliente, Note 
                         FROM Clienti";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clienti.Add(new Cliente
                {
                    ClienteID = reader.GetInt32("ClienteID"),
                    CodiceCliente = reader.GetInt32("CodiceCliente"),
                    RagioneSociale = reader.GetString("RagioneSociale"),
                    NomeContatto = reader.IsDBNull("NomeContatto") ? null : reader.GetString("NomeContatto"),
                    RuoloContatto = reader.IsDBNull("RuoloContatto") ? null : reader.GetString("RuoloContatto"),
                    TipoCliente = reader.IsDBNull("TipoCliente") ? null : reader.GetString("TipoCliente"),
                    Indirizzo = reader.IsDBNull("Indirizzo") ? null : reader.GetString("Indirizzo"),
                    Comune = reader.IsDBNull("Comune") ? null : reader.GetString("Comune"),
                    CAP = reader.IsDBNull("CAP") ? null : reader.GetString("CAP"),
                    SiglaProvincia = reader.IsDBNull("SiglaProvincia") ? null : reader.GetString("SiglaProvincia"),
                    Telefono = reader.IsDBNull("Telefono") ? null : reader.GetString("Telefono"),
                    Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                    PartitaIVA = reader.IsDBNull("PartitaIVA") ? null : reader.GetString("PartitaIVA"),
                    CodiceFiscale = reader.IsDBNull("CodiceFiscale") ? null : reader.GetString("CodiceFiscale"),
                    IBAN = reader.IsDBNull("IBAN") ? null : reader.GetString("IBAN"),
                    BancaRiferimento = reader.IsDBNull("BancaRiferimento") ? null : reader.GetString("BancaRiferimento"),
                    TipologiaServizi = reader.IsDBNull("TipologiaServizi") ? null : reader.GetString("TipologiaServizi"),
                    PortoRiferimento = reader.IsDBNull("PortoRiferimento") ? null : reader.GetString("PortoRiferimento"),
                    TerminiPagamento = reader.IsDBNull("TerminiPagamento") ? null : reader.GetString("TerminiPagamento"),
                    DataPrimoContatto = reader.IsDBNull("DataPrimoContatto") ? null : reader.GetDateTime("DataPrimoContatto"),
                    ValutazioneCliente = reader.IsDBNull("ValutazioneCliente") ? null : reader.GetString("ValutazioneCliente"),
                    Note = reader.IsDBNull("Note") ? null : reader.GetString("Note")
                });
            }

            return clienti;
        }

        // AGGIUNGI CLIENTE
        public async Task<bool> AggiungiCliente(Cliente cliente)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Clienti (
                    CodiceCliente, RagioneSociale, NomeContatto, RuoloContatto, TipoCliente, 
                    Indirizzo, Comune, CAP, SiglaProvincia, Telefono, Email, PartitaIVA, 
                    CodiceFiscale, IBAN, BancaRiferimento, TipologiaServizi, PortoRiferimento, 
                    TerminiPagamento, DataPrimoContatto, ValutazioneCliente, Note
                 ) VALUES (
                    @CodiceCliente, @RagioneSociale, @NomeContatto, @RuoloContatto, @TipoCliente,
                    @Indirizzo, @Comune, @CAP, @SiglaProvincia, @Telefono, @Email, @PartitaIVA,
                    @CodiceFiscale, @IBAN, @BancaRiferimento, @TipologiaServizi, @PortoRiferimento,
                    @TerminiPagamento, @DataPrimoContatto, @ValutazioneCliente, @Note
                 )";

            using var command = new SqlCommand(query, connection);

            // Aggiungi parametri
            command.Parameters.AddWithValue("@CodiceCliente", cliente.CodiceCliente);
            command.Parameters.AddWithValue("@RagioneSociale", cliente.RagioneSociale);
            command.Parameters.AddWithValue("@NomeContatto", (object)cliente.NomeContatto ?? DBNull.Value);
            command.Parameters.AddWithValue("@RuoloContatto", (object)cliente.RuoloContatto ?? DBNull.Value);
            command.Parameters.AddWithValue("@TipoCliente", (object)cliente.TipoCliente ?? DBNull.Value);
            command.Parameters.AddWithValue("@Indirizzo", (object)cliente.Indirizzo ?? DBNull.Value);
            command.Parameters.AddWithValue("@Comune", (object)cliente.Comune ?? DBNull.Value);
            command.Parameters.AddWithValue("@CAP", (object)cliente.CAP ?? DBNull.Value);
            command.Parameters.AddWithValue("@SiglaProvincia", (object)cliente.SiglaProvincia ?? DBNull.Value);
            command.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)cliente.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@PartitaIVA", (object)cliente.PartitaIVA ?? DBNull.Value);
            command.Parameters.AddWithValue("@CodiceFiscale", (object)cliente.CodiceFiscale ?? DBNull.Value);
            command.Parameters.AddWithValue("@IBAN", (object)cliente.IBAN ?? DBNull.Value);
            command.Parameters.AddWithValue("@BancaRiferimento", (object)cliente.BancaRiferimento ?? DBNull.Value);
            command.Parameters.AddWithValue("@TipologiaServizi", (object)cliente.TipologiaServizi ?? DBNull.Value);
            command.Parameters.AddWithValue("@PortoRiferimento", (object)cliente.PortoRiferimento ?? DBNull.Value);
            command.Parameters.AddWithValue("@TerminiPagamento", (object)cliente.TerminiPagamento ?? DBNull.Value);
            command.Parameters.AddWithValue("@DataPrimoContatto", (object)cliente.DataPrimoContatto ?? DBNull.Value);
            command.Parameters.AddWithValue("@ValutazioneCliente", (object)cliente.ValutazioneCliente ?? DBNull.Value);
            command.Parameters.AddWithValue("@Note", (object)cliente.Note ?? DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // MODIFICA CLIENTE
        public async Task<bool> ModificaCliente(Cliente cliente)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE Clienti SET 
                    CodiceCliente = @CodiceCliente,
                    RagioneSociale = @RagioneSociale,
                    NomeContatto = @NomeContatto,
                    RuoloContatto = @RuoloContatto,
                    TipoCliente = @TipoCliente,
                    Indirizzo = @Indirizzo,
                    Comune = @Comune,
                    CAP = @CAP,
                    SiglaProvincia = @SiglaProvincia,
                    Telefono = @Telefono,
                    Email = @Email,
                    PartitaIVA = @PartitaIVA,
                    CodiceFiscale = @CodiceFiscale,
                    IBAN = @IBAN,
                    BancaRiferimento = @BancaRiferimento,
                    TipologiaServizi = @TipologiaServizi,
                    PortoRiferimento = @PortoRiferimento,
                    TerminiPagamento = @TerminiPagamento,
                    DataPrimoContatto = @DataPrimoContatto,
                    ValutazioneCliente = @ValutazioneCliente,
                    Note = @Note
                 WHERE ClienteID = @ClienteID";

            using var command = new SqlCommand(query, connection);

            // Aggiungi parametri
            command.Parameters.AddWithValue("@ClienteID", cliente.ClienteID);
            command.Parameters.AddWithValue("@CodiceCliente", cliente.CodiceCliente);
            command.Parameters.AddWithValue("@RagioneSociale", cliente.RagioneSociale);
            command.Parameters.AddWithValue("@NomeContatto", (object)cliente.NomeContatto ?? DBNull.Value);
            command.Parameters.AddWithValue("@RuoloContatto", (object)cliente.RuoloContatto ?? DBNull.Value);
            command.Parameters.AddWithValue("@TipoCliente", (object)cliente.TipoCliente ?? DBNull.Value);
            command.Parameters.AddWithValue("@Indirizzo", (object)cliente.Indirizzo ?? DBNull.Value);
            command.Parameters.AddWithValue("@Comune", (object)cliente.Comune ?? DBNull.Value);
            command.Parameters.AddWithValue("@CAP", (object)cliente.CAP ?? DBNull.Value);
            command.Parameters.AddWithValue("@SiglaProvincia", (object)cliente.SiglaProvincia ?? DBNull.Value);
            command.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)cliente.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@PartitaIVA", (object)cliente.PartitaIVA ?? DBNull.Value);
            command.Parameters.AddWithValue("@CodiceFiscale", (object)cliente.CodiceFiscale ?? DBNull.Value);
            command.Parameters.AddWithValue("@IBAN", (object)cliente.IBAN ?? DBNull.Value);
            command.Parameters.AddWithValue("@BancaRiferimento", (object)cliente.BancaRiferimento ?? DBNull.Value);
            command.Parameters.AddWithValue("@TipologiaServizi", (object)cliente.TipologiaServizi ?? DBNull.Value);
            command.Parameters.AddWithValue("@PortoRiferimento", (object)cliente.PortoRiferimento ?? DBNull.Value);
            command.Parameters.AddWithValue("@TerminiPagamento", (object)cliente.TerminiPagamento ?? DBNull.Value);
            command.Parameters.AddWithValue("@DataPrimoContatto", (object)cliente.DataPrimoContatto ?? DBNull.Value);
            command.Parameters.AddWithValue("@ValutazioneCliente", (object)cliente.ValutazioneCliente ?? DBNull.Value);
            command.Parameters.AddWithValue("@Note", (object)cliente.Note ?? DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // ELIMINA CLIENTE (Metodo sviluppato in CONTROLLI ELIMINAZIONE CLIENTI)
        /*
        public async Task<bool> EliminaCliente(int clienteID)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM Clienti WHERE ClienteID = @ClienteID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClienteID", clienteID);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        */

        // GET CLIENTI PER DROPDOWN (CON CODICE CLIENTE)
        public async Task<List<Cliente>> GetClientiPerDropdown()
        {
            var clienti = new List<Cliente>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // ⚠️ SELEZIONA ANCHE CODICECLIENTE
                var query = "SELECT ClienteID, CodiceCliente, RagioneSociale FROM Clienti ORDER BY RagioneSociale";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    clienti.Add(new Cliente
                    {
                        ClienteID = reader.GetInt32("ClienteID"),
                        CodiceCliente = reader.GetInt32("CodiceCliente"),  // ⚠️ IMPORTANTE
                        RagioneSociale = reader.GetString("RagioneSociale")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel caricamento clienti per dropdown: {ex.Message}");
            }

            return clienti;
        }

        #endregion

        #region GESTIONE CRUD FORNITORI
        // FORNITORI
        public async Task<List<Fornitore>> GetFornitori()
        {
            var fornitori = new List<Fornitore>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT FornitoreID, RagioneSociale, Indirizzo, Telefono, Email, 
                         PartitaIVA, CodiceSDI, Note FROM Fornitori";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                fornitori.Add(new Fornitore
                {
                    FornitoreID = reader.GetInt32("FornitoreID"),
                    RagioneSociale = reader.GetString("RagioneSociale"),
                    Indirizzo = reader.IsDBNull("Indirizzo") ? null : reader.GetString("Indirizzo"),
                    Telefono = reader.IsDBNull("Telefono") ? null : reader.GetString("Telefono"),
                    Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                    PartitaIVA = reader.IsDBNull("PartitaIVA") ? null : reader.GetString("PartitaIVA"),
                    CodiceSDI = reader.IsDBNull("CodiceSDI") ? null : reader.GetString("CodiceSDI"),
                    Note = reader.IsDBNull("Note") ? null : reader.GetString("Note")
                });
            }

            return fornitori;
        }
        // AGGIUNGI FORNITORE
        public async Task<bool> AggiungiFornitore(Fornitore fornitore)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Fornitori (
                    RagioneSociale, Indirizzo, Telefono, Email, 
                    PartitaIVA, CodiceSDI, Note
                 ) VALUES (
                    @RagioneSociale, @Indirizzo, @Telefono, @Email,
                    @PartitaIVA, @CodiceSDI, @Note
                 )";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@RagioneSociale", fornitore.RagioneSociale);
            command.Parameters.AddWithValue("@Indirizzo", fornitore.Indirizzo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Telefono", fornitore.Telefono ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", fornitore.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PartitaIVA", fornitore.PartitaIVA ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CodiceSDI", fornitore.CodiceSDI ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Note", fornitore.Note ?? (object)DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // MODIFICA FORNITORE
        public async Task<bool> ModificaFornitore(Fornitore fornitore)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE Fornitori SET 
                    RagioneSociale = @RagioneSociale,
                    Indirizzo = @Indirizzo,
                    Telefono = @Telefono,
                    Email = @Email,
                    PartitaIVA = @PartitaIVA,
                    CodiceSDI = @CodiceSDI,
                    Note = @Note
                 WHERE FornitoreID = @FornitoreID";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@FornitoreID", fornitore.FornitoreID);
            command.Parameters.AddWithValue("@RagioneSociale", fornitore.RagioneSociale);
            command.Parameters.AddWithValue("@Indirizzo", fornitore.Indirizzo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Telefono", fornitore.Telefono ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", fornitore.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PartitaIVA", fornitore.PartitaIVA ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CodiceSDI", fornitore.CodiceSDI ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Note", fornitore.Note ?? (object)DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // ELIMINA FORNITORE
        public async Task<bool> EliminaFornitore(int fornitoreID)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM Fornitori WHERE FornitoreID = @FornitoreID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FornitoreID", fornitoreID);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        #endregion

        #region GESTIONE CRUD MATERIALI
        // MATERIALI
        public async Task<List<Materiale>> GetMateriali()
        {
            var materiali = new List<Materiale>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT IDMateriale, CodiceMateriale, Descrizione, UnitaMisura, 
                         PrezzoUnitario, DataUltimoOrdine, QuantitaDisponibile, 
                         QuantitaMinima, Fornitore, Note FROM Materiali";

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
                    PrezzoUnitario = reader.IsDBNull("PrezzoUnitario") ? null : reader.GetDecimal("PrezzoUnitario"),
                    DataUltimoOrdine = reader.IsDBNull("DataUltimoOrdine") ? null : reader.GetDateTime("DataUltimoOrdine"),
                    QuantitaDisponibile = reader.IsDBNull("QuantitaDisponibile") ? null : reader.GetDecimal("QuantitaDisponibile"),
                    QuantitaMinima = reader.IsDBNull("QuantitaMinima") ? null : reader.GetDecimal("QuantitaMinima"),
                    Fornitore = reader.IsDBNull("Fornitore") ? null : reader.GetString("Fornitore"),
                    Note = reader.IsDBNull("Note") ? null : reader.GetString("Note")
                });
            }

            return materiali;
        }

        // AGGIUNGI MATERIALE
        public async Task<bool> AggiungiMateriale(Materiale materiale)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Materiali (
                    CodiceMateriale, Descrizione, UnitaMisura, PrezzoUnitario, 
                    DataUltimoOrdine, QuantitaDisponibile, QuantitaMinima, 
                    Fornitore, Note
                 ) VALUES (
                    @CodiceMateriale, @Descrizione, @UnitaMisura, @PrezzoUnitario,
                    @DataUltimoOrdine, @QuantitaDisponibile, @QuantitaMinima,
                    @Fornitore, @Note
                 )";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@CodiceMateriale", materiale.CodiceMateriale);
            command.Parameters.AddWithValue("@Descrizione", materiale.Descrizione ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitaMisura", materiale.UnitaMisura ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PrezzoUnitario", materiale.PrezzoUnitario ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DataUltimoOrdine", materiale.DataUltimoOrdine ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@QuantitaDisponibile", materiale.QuantitaDisponibile ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@QuantitaMinima", materiale.QuantitaMinima ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Fornitore", materiale.Fornitore ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Note", materiale.Note ?? (object)DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // MODIFICA MATERIALE
        public async Task<bool> ModificaMateriale(Materiale materiale)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE Materiali SET 
                    CodiceMateriale = @CodiceMateriale,
                    Descrizione = @Descrizione,
                    UnitaMisura = @UnitaMisura,
                    PrezzoUnitario = @PrezzoUnitario,
                    DataUltimoOrdine = @DataUltimoOrdine,
                    QuantitaDisponibile = @QuantitaDisponibile,
                    QuantitaMinima = @QuantitaMinima,
                    Fornitore = @Fornitore,
                    Note = @Note
                 WHERE IDMateriale = @IDMateriale";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@IDMateriale", materiale.IDMateriale);
            command.Parameters.AddWithValue("@CodiceMateriale", materiale.CodiceMateriale);
            command.Parameters.AddWithValue("@Descrizione", materiale.Descrizione ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitaMisura", materiale.UnitaMisura ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PrezzoUnitario", materiale.PrezzoUnitario ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DataUltimoOrdine", materiale.DataUltimoOrdine ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@QuantitaDisponibile", materiale.QuantitaDisponibile ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@QuantitaMinima", materiale.QuantitaMinima ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Fornitore", materiale.Fornitore ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Note", materiale.Note ?? (object)DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // ELIMINA MATERIALE
        public async Task<bool> EliminaMateriale(int materialeID)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM Materiali WHERE IDMateriale = @IDMateriale";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IDMateriale", materialeID);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        #endregion

        #region GESTIONE CRUD IMBARCAZIONI

        // IMBARCAZIONI CON JOIN E CODICE CLIENTE
        public async Task<List<Imbarcazione>> GetImbarcazioni()
        {
            var imbarcazioni = new List<Imbarcazione>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT 
                i.ID_Imbarcazione, i.Nome_Imbarcazione, i.ID_Tipo, i.ID_Cliente, 
                i.Anno_Costruzione, i.Lunghezza, i.Larghezza, i.Pescaggio_FB, i.Pescaggio_EB, 
                i.Peso, i.Materiale_Scafo, i.Data_Acquisto, i.Stato, i.PortoAttracco, i.Note,
                t.Nome_Tipo,
                c.RagioneSociale,
                c.CodiceCliente  
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
                    ID_Tipo = reader.IsDBNull("ID_Tipo") ? null : reader.GetInt32("ID_Tipo"),
                    ID_Cliente = reader.IsDBNull("ID_Cliente") ? null : reader.GetInt32("ID_Cliente"),
                    Anno_Costruzione = reader.IsDBNull("Anno_Costruzione") ? null : reader.GetInt32("Anno_Costruzione"),
                    Lunghezza = reader.IsDBNull("Lunghezza") ? null : reader.GetDecimal("Lunghezza"),
                    Larghezza = reader.IsDBNull("Larghezza") ? null : reader.GetDecimal("Larghezza"),
                    Pescaggio_FB = reader.IsDBNull("Pescaggio_FB") ? null : reader.GetDecimal("Pescaggio_FB"),
                    Pescaggio_EB = reader.IsDBNull("Pescaggio_EB") ? null : reader.GetDecimal("Pescaggio_EB"),
                    Peso = reader.IsDBNull("Peso") ? null : reader.GetDecimal("Peso"),
                    Materiale_Scafo = reader.IsDBNull("Materiale_Scafo") ? null : reader.GetString("Materiale_Scafo"),
                    Data_Acquisto = reader.IsDBNull("Data_Acquisto") ? null : reader.GetDateTime("Data_Acquisto"),
                    Stato = reader.IsDBNull("Stato") ? null : reader.GetString("Stato"),
                    PortoAttracco = reader.IsDBNull("PortoAttracco") ? null : reader.GetString("PortoAttracco"),
                    Note = reader.IsDBNull("Note") ? null : reader.GetString("Note"),
                    Nome_Tipo = reader.IsDBNull("Nome_Tipo") ? null : reader.GetString("Nome_Tipo"),
                    RagioneSociale_Cliente = reader.IsDBNull("RagioneSociale") ? null : reader.GetString("RagioneSociale"),
                    // ⚠️ POPOLA CodiceCliente dal join con Clienti
                    CodiceCliente = reader.IsDBNull("CodiceCliente") ? null : reader.GetInt32("CodiceCliente")
                });
            }

            return imbarcazioni;
        }

        // AGGIUNGI IMBARCAZIONE
        public async Task<bool> AggiungiImbarcazione(Imbarcazione imbarcazione)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Imbarcazioni (
                    Nome_Imbarcazione, ID_Tipo, ID_Cliente, Anno_Costruzione, 
                    Lunghezza, Larghezza, Pescaggio_FB, Pescaggio_EB, Peso, 
                    Materiale_Scafo, Data_Acquisto, Stato, PortoAttracco, Note
                 ) VALUES (
                    @Nome_Imbarcazione, @ID_Tipo, @ID_Cliente, @Anno_Costruzione,
                    @Lunghezza, @Larghezza, @Pescaggio_FB, @Pescaggio_EB, @Peso,
                    @Materiale_Scafo, @Data_Acquisto, @Stato, @PortoAttracco, @Note
                 )";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Nome_Imbarcazione", imbarcazione.Nome_Imbarcazione);
            command.Parameters.AddWithValue("@ID_Tipo", imbarcazione.ID_Tipo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ID_Cliente", imbarcazione.ID_Cliente ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Anno_Costruzione", imbarcazione.Anno_Costruzione ?? (object)DBNull.Value); // ✅ CORRETTO
            command.Parameters.AddWithValue("@Lunghezza", imbarcazione.Lunghezza ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Larghezza", imbarcazione.Larghezza ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Pescaggio_FB", imbarcazione.Pescaggio_FB ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Pescaggio_EB", imbarcazione.Pescaggio_EB ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Peso", imbarcazione.Peso ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Materiale_Scafo", imbarcazione.Materiale_Scafo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Data_Acquisto", imbarcazione.Data_Acquisto ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Stato", imbarcazione.Stato ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PortoAttracco", imbarcazione.PortoAttracco ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Note", imbarcazione.Note ?? (object)DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // MODIFICA IMBARCAZIONE
        public async Task<bool> ModificaImbarcazione(Imbarcazione imbarcazione)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE Imbarcazioni SET 
                    Nome_Imbarcazione = @Nome_Imbarcazione,
                    ID_Tipo = @ID_Tipo,
                    ID_Cliente = @ID_Cliente,
                    Anno_Costruzione = @Anno_Costruzione,
                    Lunghezza = @Lunghezza,
                    Larghezza = @Larghezza,
                    Pescaggio_FB = @Pescaggio_FB,
                    Pescaggio_EB = @Pescaggio_EB,
                    Peso = @Peso,
                    Materiale_Scafo = @Materiale_Scafo,
                    Data_Acquisto = @Data_Acquisto,
                    Stato = @Stato,
                    PortoAttracco = @PortoAttracco,
                    Note = @Note
                 WHERE ID_Imbarcazione = @ID_Imbarcazione";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID_Imbarcazione", imbarcazione.ID_Imbarcazione);
            command.Parameters.AddWithValue("@Nome_Imbarcazione", imbarcazione.Nome_Imbarcazione);
            command.Parameters.AddWithValue("@ID_Tipo", imbarcazione.ID_Tipo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ID_Cliente", imbarcazione.ID_Cliente ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Anno_Costruzione", imbarcazione.Anno_Costruzione ?? (object)DBNull.Value); // ✅ CORRETTO
            command.Parameters.AddWithValue("@Lunghezza", imbarcazione.Lunghezza ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Larghezza", imbarcazione.Larghezza ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Pescaggio_FB", imbarcazione.Pescaggio_FB ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Pescaggio_EB", imbarcazione.Pescaggio_EB ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Peso", imbarcazione.Peso ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Materiale_Scafo", imbarcazione.Materiale_Scafo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Data_Acquisto", imbarcazione.Data_Acquisto ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Stato", imbarcazione.Stato ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PortoAttracco", imbarcazione.PortoAttracco ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Note", imbarcazione.Note ?? (object)DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // ELIMINA IMBARCAZIONE
        public async Task<bool> EliminaImbarcazione(int imbarcazioneID)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM Imbarcazioni WHERE ID_Imbarcazione = @ID_Imbarcazione";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ID_Imbarcazione", imbarcazioneID);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // GET TIPI IMBARCAZIONE PER DROPDOWN
        public async Task<List<TipoImbarcazione>> GetTipiImbarcazione()
        {
            var tipi = new List<TipoImbarcazione>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT ID_Tipo, Nome_Tipo, Descrizione FROM Tipo_Imbarcazione ORDER BY Nome_Tipo";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    tipi.Add(new TipoImbarcazione
                    {
                        ID_Tipo = reader.GetInt32("ID_Tipo"),
                        Nome_Tipo = reader.GetString("Nome_Tipo"),
                        Descrizione = reader.IsDBNull("Descrizione") ? null : reader.GetString("Descrizione")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel caricamento tipi imbarcazione: {ex.Message}");
            }

            return tipi;
        }
        #endregion

        #region CONTROLLI ELIMINAZIONE CLIENTI

        // CONTROLLA QUANTE IMBARCAZIONI HA UN CLIENTE
        public async Task<int> ContaImbarcazioniCliente(int clienteID)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT COUNT(*) FROM Imbarcazioni WHERE ID_Cliente = @ClienteID";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ClienteID", clienteID);

                return (int)await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore conteggio imbarcazioni cliente: {ex.Message}");
                return 0;
            }
        }

        // ELIMINA CLIENTE E TUTTE LE SUE IMBARCAZIONI (ELIMINAZIONE A CASCATA)
        public async Task<bool> EliminaCliente(int clienteID)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. PRIMA ELIMINA TUTTE LE IMBARCAZIONI DEL CLIENTE
                var queryImbarcazioni = "DELETE FROM Imbarcazioni WHERE ID_Cliente = @ClienteID";
                using var commandImbarcazioni = new SqlCommand(queryImbarcazioni, connection, (SqlTransaction)transaction);
                commandImbarcazioni.Parameters.AddWithValue("@ClienteID", clienteID);
                await commandImbarcazioni.ExecuteNonQueryAsync();

                // 2. POI ELIMINA IL CLIENTE
                var queryCliente = "DELETE FROM Clienti WHERE ClienteID = @ClienteID";
                using var commandCliente = new SqlCommand(queryCliente, connection, (SqlTransaction)transaction);
                commandCliente.Parameters.AddWithValue("@ClienteID", clienteID);
                var result = await commandCliente.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Errore eliminazione cliente e imbarcazioni: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region CONTROLLI DUPLICATI IMBARCAZIONI

        // CONTROLLA SE ESISTE GIA' UN'IMBARCAZIONE CON LO STESSO NOME E CLIENTE
        public async Task<bool> EsisteImbarcazioneDuplicata(string nomeImbarcazione, int? idCliente, int idImbarcazioneEscludi = 0)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT COUNT(*) FROM Imbarcazioni 
                     WHERE Nome_Imbarcazione = @NomeImbarcazione 
                     AND ID_Cliente = @IDCliente 
                     AND ID_Imbarcazione != @IDImbarcazioneEscludi";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@NomeImbarcazione", nomeImbarcazione);
                command.Parameters.AddWithValue("@IDCliente", idCliente ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IDImbarcazioneEscludi", idImbarcazioneEscludi);

                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore controllo duplicati imbarcazione: {ex.Message}");
                return false;
            }
        }

        // CONTROLLA SE IL CLIENTE HA GIA' UN'IMBARCAZIONE CON LO STESSO NOME (PER LO STESSO CLIENTE)
        public async Task<bool> EsisteImbarcazioneStessoCliente(string nomeImbarcazione, int? idCliente, int idImbarcazioneEscludi = 0)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT COUNT(*) FROM Imbarcazioni 
                     WHERE Nome_Imbarcazione = @NomeImbarcazione 
                     AND ID_Cliente = @IDCliente 
                     AND ID_Imbarcazione != @IDImbarcazioneEscludi";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@NomeImbarcazione", nomeImbarcazione);
                command.Parameters.AddWithValue("@IDCliente", idCliente ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IDImbarcazioneEscludi", idImbarcazioneEscludi);

                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore controllo imbarcazione stesso cliente: {ex.Message}");
                return false;
            }
        }

        // CONTROLLA SE ESISTE GIA' UN'IMBARCAZIONE CON LO STESSO NOME (GLOBALE)
        public async Task<bool> EsisteImbarcazioneConNome(string nomeImbarcazione, int idImbarcazioneEscludi = 0)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT COUNT(*) FROM Imbarcazioni 
                     WHERE Nome_Imbarcazione = @NomeImbarcazione 
                     AND ID_Imbarcazione != @IDImbarcazioneEscludi";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@NomeImbarcazione", nomeImbarcazione);
                command.Parameters.AddWithValue("@IDImbarcazioneEscludi", idImbarcazioneEscludi);

                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore controllo nome imbarcazione: {ex.Message}");
                return false;
            }
        }

        #endregion

    }
}