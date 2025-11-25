using CNP.Data;
using CNP.Segreteria.Models;
using CNP.SchedaLavorazione.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CNP.SchedaLavorazione.Services; // ← MODIFICA: Namespace corretto

public class PreventiviService
{
    private readonly IDatabaseService _databaseService;

    public PreventiviService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Recupera l'elenco delle schede in lavorazione con il relativo stato di riepilogo.
    /// Per ogni scheda lavorazione, calcola il numero di attività completate e totali.
    /// </summary>
    /// <returns>Una lista di oggetti anonimi con i dati di riepilogo per ogni scheda lavorazione.</returns>
    public async Task<IEnumerable<RiepilogoLavorazione>> GetRiepilogoLavorazioniAsync()
    {
        // Query identica ma userà i modelli corretti
        string query = @"
        SELECT 
            p.PreventivoId AS PreventivoId,
            p.NomeBarca,
            COUNT(s.Id) AS AttivitaTotali,
            SUM(CASE WHEN s.Stato = 'Completata' THEN 1 ELSE 0 END) AS AttivitaCompletate,
            CASE 
                WHEN SUM(CASE WHEN s.Stato IN ('In Corso', 'Completata') THEN 1 ELSE 0 END) = 0 THEN 'Da Iniziare'
                WHEN SUM(CASE WHEN s.Stato = 'Completata' THEN 1 ELSE 0 END) = COUNT(s.Id) THEN 'Completata'
                ELSE 'In Corso'
            END AS StatoGenerale
        FROM Preventivi p
        LEFT JOIN SchedaLavorazioni s ON p.PreventivoId = s.PreventivoId
        WHERE p.Stato = 'IN LAVORAZIONE'
        GROUP BY p.PreventivoId, p.NomeBarca
        ORDER BY p.NomeBarca";

        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query);

        var riepilogo = new List<RiepilogoLavorazione>();
        foreach (DataRow row in dataTable.Rows)
        {
            riepilogo.Add(new RiepilogoLavorazione
            {
                PreventivoId = Convert.ToInt32(row["PreventivoId"]),
                NomeBarca = row["NomeBarca"].ToString() ?? "",
                AttivitaTotali = Convert.ToInt32(row["AttivitaTotali"]),
                AttivitaCompletate = Convert.ToInt32(row["AttivitaCompletate"]),
                StatoGenerale = row["StatoGenerale"].ToString() ?? "",
                IsExpanded = false
            });
        }
        return riepilogo;
    }

    /// <summary>
    /// Recupera le attività dettagliate per un specifico preventivo.
    /// </summary>
    /// <param name="preventivoId">L'ID del preventivo di cui recuperare le attività.</param>
    /// <returns>Una lista di attività dalla tabella SchedaLavorazioni.</returns>
    public async Task<IEnumerable<dynamic>> GetAttivitaPerPreventivoAsync(int preventivoId)
    {
        string query = @"SELECT 
            Id,
            PreventivoId, 
            IdVoce,
            Attività,
            Voce,
            Operatore,
            DataOraInizio,
            DataOraFine,
            OreImpiegate,
            CodiceMateriale,
            QtaMateriale,
            Note,
            Stato
        FROM SchedaLavorazioni 
        WHERE PreventivoId = @PreventivoId
        ORDER BY IdVoce";

        var parameters = new[] { new SqlParameter("@PreventivoId", preventivoId) };
        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

        var attivita = new List<dynamic>();
        try
        {
            foreach (DataRow row in dataTable.Rows)
            {
                // FORMATTAZIONE ORE IMPIEGATE (SICURA)
                string oreImpiegateFormattate = "—";
                if (row["OreImpiegate"] != DBNull.Value && !string.IsNullOrEmpty(row["OreImpiegate"]?.ToString()))
                {
                    try
                    {
                        int minutiTotali = Convert.ToInt32(row["OreImpiegate"]);
                        int oreTotali = minutiTotali / 60;
                        int minuti = minutiTotali % 60;
                        oreImpiegateFormattate = $"{oreTotali} ore {minuti} min";
                    }
                    catch
                    {
                        oreImpiegateFormattate = row["OreImpiegate"]?.ToString() ?? "—";
                    }
                }

                // INFO SOSPENSIONE SE APPLICABILE
                string infoSospensione = "";
                if (row["Stato"]?.ToString() == "Sospesa")
                {
                    var ultimaSospensione = await GetUltimaSospensioneAsync(Convert.ToInt32(row["Id"]));
                    if (ultimaSospensione != null)
                    {
                        infoSospensione = $"{ultimaSospensione.Operatore} • " +
                                        $"{ultimaSospensione.DataOraInizioSospensione:dd/MM HH:mm} • " +
                                        $"{ultimaSospensione.Motivazione}";
                    }
                }

                attivita.Add(new
                {
                    Id = Convert.ToInt32(row["Id"]),
                    PreventivoId = Convert.ToInt32(row["PreventivoId"]),
                    IdVoce = Convert.ToInt32(row["IdVoce"]),
                    Attivita = row["Attività"]?.ToString() ?? "",
                    Voce = row["Voce"]?.ToString() ?? "",
                    Operatore = row["Operatore"]?.ToString() ?? "",
                    DataOraInizio = row["DataOraInizio"] != DBNull.Value ? row["DataOraInizio"].ToString() : "",
                    DataOraFine = row["DataOraFine"] != DBNull.Value ? row["DataOraFine"].ToString() : "",
                    OreImpiegate = oreImpiegateFormattate,
                    CodiceMateriale = row["CodiceMateriale"]?.ToString() ?? "",
                    QtaMateriale = row["QtaMateriale"]?.ToString() ?? "",
                    Note = row["Note"]?.ToString() ?? "",
                    Stato = row["Stato"]?.ToString() ?? "",
                    InfoSospensione = infoSospensione // NUOVO CAMPO
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRORE nel mapping: {ex.Message}");
            throw;
        }
        return attivita;
    }
    /// <summary>
    /// (METODO DEFINITIVO per il Cruscotto)
    /// Recupera l'elenco delle schede di lavorazione attive con il nome della barca e il suo stato.
    /// Esegue una JOIN tra le tabelle SchedaLavorazione e Preventivi.
    /// </summary>
    /// <returns>Una lista di oggetti anonimi con NomeBarca e StatoLavorazione.</returns>
    public async Task<IEnumerable<dynamic>> GetSchedeLavorazioniAsync()
    {
        string query = @"
        SELECT 
            p.NomeBarca, 
            s.Stato AS StatoLavorazione 
        FROM SchedaLavorazioni s 
        INNER JOIN Preventivi p ON s.PreventivoId = p.PreventivoId 
        WHERE p.Stato = 'IN LAVORAZIONE' 
        ORDER BY p.NomeBarca";

        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query);

        var schedeLavorazione = new List<dynamic>();
        foreach (DataRow row in dataTable.Rows)
        {
            schedeLavorazione.Add(new
            {
                NomeBarca = row["NomeBarca"].ToString(),
                StatoLavorazione = row["StatoLavorazione"].ToString()
            });
        }
        return schedeLavorazione;
    }

    /// <summary>
    /// Aggiorna parzialmente una scheda di lavorazione esistente.
    /// Aggiorna solo i campi specificati senza modificare gli altri.
    /// </summary>
    /// <param name="scheda">Oggetto SchedaLavorazione con i campi da aggiornare</param>
    /// <returns>True se l'aggiornamento è avvenuto con successo</returns>
    public async Task<bool> AggiornaSchedaLavorazioneAsync(SchedaLavorazioneModel scheda)
    {
        string query = @"UPDATE SchedaLavorazioni 
                    SET Operatore = @Operatore, 
                        DataOraInizio = @DataOraInizio,
                        Stato = @Stato
                    WHERE Id = @Id";

        var parameters = new[]
        {
        new SqlParameter("@Operatore", scheda.Operatore ?? (object)DBNull.Value),
        new SqlParameter("@DataOraInizio", scheda.DataOraInizio ?? (object)DBNull.Value),
        new SqlParameter("@Stato", scheda.Stato),
        new SqlParameter("@Id", scheda.Id)
    };

        int result = await _databaseService.ExecuteNonQueryAsync(query, parameters);
        return result > 0;
    }

    /// <summary>
    /// Recupera i dettagli completi di un singolo preventivo specificato dal suo ID.
    /// Questo metodo è utile per visualizzare un overview del preventivo selezionato.
    /// </summary>
    /// <param name="idPreventivo">L'ID del preventivo da recuperare.</param>
    /// <returns>Un oggetto Preventivo popolato con tutti i suoi dati, o null se non trovato.</returns>
    public async Task<Preventivo?> GetDettaglioPreventivoAsync(int idPreventivo)
    {
        string query = @"SELECT p.Id, p.PreventivoId, p.DataCreazione, p.Descrizione, p.ClienteId, c.RagioneSociale, 
                                p.ImportoTotale, p.Stato, i.Nome_Imbarcazione as NomeBarca
                         FROM Preventivi p
                         INNER JOIN Clienti c ON p.ClienteId = c.ClienteID
                         INNER JOIN Imbarcazioni i ON p.ImbarcazioneId = i.ID_Imbarcazione
                         WHERE p.Id = @IdPreventivo";

        var parameters = new[] { new SqlParameter("@IdPreventivo", idPreventivo) };
        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

        if (dataTable.Rows.Count > 0)
        {
            return MapToPreventivo(dataTable.Rows[0]);
        }
        return null;
    }

    /// <summary>
    /// Recupera l'elenco di tutte le voci (attività e sub-attività) che compongono un preventivo.
    /// Questo metodo fornisce i dati necessari per la griglia di assegnazione delle lavorazioni agli operai.
    /// Ogni voce rappresenta una riga di lavoro distinta nel preventivo.
    /// </summary>
    /// <param name="idPreventivo">L'ID del preventivo di cui recuperare le voci.</param>
    /// <returns>Una lista di oggetti DettaglioPreventivo.</returns>
    public async Task<IEnumerable<DettaglioPreventivo>> GetVociPreventivoAsync(int idPreventivo)
    {
        string query = @"SELECT dp.Id, dp.PreventivoId, dp.VoceId, a.Descrizione AS DescrizioneAttivita, 
                                dp.DescrizioneVoce, dp.N_Operatori, dp.UnitaMisura, dp.Qta_Voce, dp.CodiceMateriale
                         FROM DettaglioPreventivo dp
                         INNER JOIN Attività a ON dp.AttivitaId = a.ID
                         WHERE dp.PreventivoId = @IdPreventivo
                         ORDER BY a.CodiceAttivita, dp.VoceId";

        var parameters = new[] { new SqlParameter("@IdPreventivo", idPreventivo) };
        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

        List<DettaglioPreventivo> voci = new();
        foreach (DataRow row in dataTable.Rows)
        {
            voci.Add(MapToDettaglioPreventivo(row));
        }
        return voci;
    }

    /// <summary>
    /// Recupera l'elenco di tutti gli operatori abilitati alle lavorazioni.
    /// Questo metodo fornisce i dati necessari per l'assegnazione delle attività agli operai.
    /// Restituisce l'elenco completo degli operai ordinato per cognome e nome.
    /// </summary>
    /// <returns>Una lista di oggetti Operatore con tutti i dati anagrafici.</returns>
    public async Task<IEnumerable<Operatore>> GetOperatoriAsync()
    {
        string query = "SELECT Id, Cognome, Nome, Codice FROM Operatori ORDER BY Cognome, Nome";
        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query);

        var operatori = new List<Operatore>();
        foreach (DataRow row in dataTable.Rows)
        {
            operatori.Add(new Operatore
            {
                Id = Convert.ToInt32(row["Id"]),
                Cognome = row["Cognome"].ToString() ?? "",
                Nome = row["Nome"].ToString() ?? "",
                Codice = Convert.ToInt32(row["Codice"])
            });
        }
        return operatori;
    }

    /// <summary>
    /// Completa un'attività e aggiorna lo stato a "Completata"
    /// </summary>
    public async Task<bool> CompletaAttivitaAsync(int idScheda)
    {
        // PRIMA recupera DataOraInizio per calcolo corretto
        string querySelect = "SELECT DataOraInizio FROM SchedaLavorazioni WHERE Id = @Id";
        var parametersSelect = new[] { new SqlParameter("@Id", idScheda) };

        DataTable dataTable = await _databaseService.ExecuteQueryAsync(querySelect, parametersSelect);
        if (dataTable.Rows.Count == 0) return false;

        DateTime dataInizio = Convert.ToDateTime(dataTable.Rows[0]["DataOraInizio"]);
        TimeSpan durata = DateTime.Now - dataInizio;

        // CALCOLA MINUTI (come stringa per nvarchar(10))
        int minutiTotali = (int)durata.TotalMinutes;
        string oreImpiegateString = minutiTotali.ToString();

        string queryUpdate = @"UPDATE SchedaLavorazioni 
                        SET Stato = 'Completata', 
                            DataOraFine = @DataOraFine,
                            OreImpiegate = @OreImpiegate
                        WHERE Id = @Id AND Stato = 'In Corso'";

        var parametersUpdate = new[]
        {
        new SqlParameter("@DataOraFine", DateTime.Now),
        new SqlParameter("@OreImpiegate", oreImpiegateString), // MINUTI COME STRINGA
        new SqlParameter("@Id", idScheda)
    };

        int result = await _databaseService.ExecuteNonQueryAsync(queryUpdate, parametersUpdate);
        return result > 0;
    }

    /// <summary>
    /// Verifica se una scheda è completamente completata (tutte le attività)
    /// </summary>
    public async Task<bool> IsSchedaCompletamenteCompletataAsync(int preventivoId)
    {
        string query = @"SELECT COUNT(*) 
                    FROM SchedaLavorazioni 
                    WHERE PreventivoId = @PreventivoId 
                    AND Stato != 'Completata'";

        var parameters = new[] { new SqlParameter("@PreventivoId", preventivoId) };
        var result = await _databaseService.ExecuteScalarAsync(query, parameters);

        return Convert.ToInt32(result) == 0;
    }

    /// <summary>
    /// Sospende un'attività e registra la sospensione
    /// </summary>
    public async Task<bool> SospendiAttivitaAsync(int schedaLavorazioneId, string operatore, string motivazione)
    {
        // Verifica che l'attività sia in corso
        string checkStatoQuery = "SELECT Stato FROM SchedaLavorazioni WHERE Id = @Id";
        var checkParameters = new[] { new SqlParameter("@Id", schedaLavorazioneId) };

        var checkResult = await _databaseService.ExecuteScalarAsync(checkStatoQuery, checkParameters);
        if (checkResult?.ToString() != "In Corso") return false;

        // Transazione
        var transaction = await _databaseService.BeginTransactionAsync();

        try
        {
            // 1. Aggiorna stato scheda a "Sospesa"
            string updateSchedaQuery = @"UPDATE SchedaLavorazioni 
                                    SET Stato = 'Sospesa' 
                                    WHERE Id = @Id";

            var parametersScheda = new[] { new SqlParameter("@Id", schedaLavorazioneId) };
            int resultScheda = await _databaseService.ExecuteNonQueryAsync(updateSchedaQuery, transaction, parametersScheda);

            if (resultScheda == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // 2. Inserisce record sospensione
            string insertSospensioneQuery = @"INSERT INTO Sospensioni 
                                        (SchedaLavorazioneId, DataOraInizioSospensione, Operatore, Motivazione) 
                                        VALUES (@SchedaLavorazioneId, @DataOraInizio, @Operatore, @Motivazione)";

            var parametersSospensione = new[]
            {
            new SqlParameter("@SchedaLavorazioneId", schedaLavorazioneId),
            new SqlParameter("@DataOraInizio", DateTime.Now),
            new SqlParameter("@Operatore", operatore),
            new SqlParameter("@Motivazione", motivazione ?? (object)DBNull.Value)
        };

            await _databaseService.ExecuteNonQueryAsync(insertSospensioneQuery, transaction, parametersSospensione);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Errore durante sospensione: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Chiude una sospensione, riprende e mette nuovamente in lavorazione una attività sospesa 
    /// </summary>
    public async Task<bool> RiprendiAttivitaAsync(int schedaLavorazioneId)
    {
        // Verifica che l'attività sia sospesa
        string checkStatoQuery = "SELECT Stato FROM SchedaLavorazioni WHERE Id = @Id";
        var checkParameters = new[] { new SqlParameter("@Id", schedaLavorazioneId) };

        var checkResult = await _databaseService.ExecuteScalarAsync(checkStatoQuery, checkParameters);
        if (checkResult?.ToString() != "Sospesa") return false;

        // Transazione
        var transaction = await _databaseService.BeginTransactionAsync();

        try
        {
            // 1. Chiude l'ultima sospensione attiva
            string updateSospensioneQuery = @"UPDATE Sospensioni 
                                        SET DataOraFineSospensione = @DataOraFine
                                        WHERE SchedaLavorazioneId = @SchedaLavorazioneId
                                        AND DataOraFineSospensione IS NULL
                                        AND Id = (SELECT TOP 1 Id FROM Sospensioni 
                                                WHERE SchedaLavorazioneId = @SchedaLavorazioneId 
                                                AND DataOraFineSospensione IS NULL
                                                ORDER BY DataOraInizioSospensione DESC)";

            var parametersSospensione = new[]
            {
            new SqlParameter("@DataOraFine", DateTime.Now),
            new SqlParameter("@SchedaLavorazioneId", schedaLavorazioneId)
        };

            int resultSospensione = await _databaseService.ExecuteNonQueryAsync(updateSospensioneQuery, transaction, parametersSospensione);
            if (resultSospensione == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // 2. Aggiorna stato scheda a "In Corso"
            string updateSchedaQuery = @"UPDATE SchedaLavorazioni 
                                    SET Stato = 'In Corso' 
                                    WHERE Id = @Id";

            var parametersScheda = new[] { new SqlParameter("@Id", schedaLavorazioneId) };
            int resultScheda = await _databaseService.ExecuteNonQueryAsync(updateSchedaQuery, transaction, parametersScheda);

            if (resultScheda == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Errore durante ripresa: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Recupera i dati della eventuale ultima sospensione della scheda di lavorazione
    /// </summary>
    public async Task<dynamic?> GetUltimaSospensioneAsync(int schedaLavorazioneId)
    {
        string query = @"SELECT TOP 1 Operatore, DataOraInizioSospensione, Motivazione
                    FROM Sospensioni 
                    WHERE SchedaLavorazioneId = @SchedaLavorazioneId 
                    AND DataOraFineSospensione IS NULL
                    ORDER BY DataOraInizioSospensione DESC";

        var parameters = new[] { new SqlParameter("@SchedaLavorazioneId", schedaLavorazioneId) };
        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

        if (dataTable.Rows.Count > 0)
        {
            return new
            {
                Operatore = dataTable.Rows[0]["Operatore"]?.ToString() ?? "",
                DataOraInizioSospensione = Convert.ToDateTime(dataTable.Rows[0]["DataOraInizioSospensione"]),
                Motivazione = dataTable.Rows[0]["Motivazione"]?.ToString() ?? ""
            };
        }

        return null;
    }

    /// <summary>
    /// Metodo per inviare notifiche (DA IMPLEMENTARE IN FUTURO)
    /// </summary>
    private async Task InviaNotificaCompletamentoSchedaAsync(int preventivoId, string nomeBarca)
    {
        // TODO: Implementare sistema notifiche
        Console.WriteLine($"NOTIFICA: Scheda completata - Preventivo {preventivoId}, {nomeBarca}");
        await Task.CompletedTask;
    }

    // --- Metodi (helper) di supporto per la mappatura (PRIVATI) ---

    /// <summary>
    /// Mappa una singola riga di un DataTable in un oggetto modello Imbarcazione.
    /// </summary>
    private Imbarcazione MapToImbarcazione(DataRow row)
    {
        return new Imbarcazione
        {
            ID_Imbarcazione = Convert.ToInt32(row["ID_Imbarcazione"]),
            Nome_Imbarcazione = row["Nome_Imbarcazione"].ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Mappa una singola riga di un DataTable in un oggetto modello Preventivo.
    /// </summary>
    private Preventivo MapToPreventivo(DataRow row)
    {
        return new Preventivo
        {
            PreventivoId = Convert.ToInt32(row["PreventivoId"]),
            DataCreazione = Convert.ToDateTime(row["DataCreazione"]),
            Descrizione = row["Descrizione"].ToString() ?? string.Empty,
            ClienteId = Convert.ToInt32(row["ClienteId"]),
            RagioneSociale = row["RagioneSociale"].ToString() ?? string.Empty,
            ImportoTotale = Convert.ToDecimal(row["ImportoTotale"]),
            Stato = row["Stato"].ToString() ?? string.Empty,
            NomeBarca = row["NomeBarca"].ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Mappa una singola riga di un DataTable in un oggetto modello DettaglioPreventivo.
    /// </summary>
    private DettaglioPreventivo MapToDettaglioPreventivo(DataRow row)
    {
        return new DettaglioPreventivo
        {
            Id = Convert.ToInt32(row["Id"]),
            PreventivoId = Convert.ToInt32(row["PreventivoId"]),
            VoceId = Convert.ToInt32(row["VoceId"]),
            DescrizioneAttivita = row["DescrizioneAttivita"].ToString() ?? string.Empty,
            DescrizioneVoce = row["DescrizioneVoce"].ToString() ?? string.Empty,
            N_Operatori = Convert.ToInt32(row["N_Operatori"]),
            UnitaMisura = row["UnitaMisura"].ToString() ?? string.Empty,
            Qta_Voce = (float)Convert.ToDouble(row["Qta_Voce"]),
            CodiceMateriale = row["CodiceMateriale"].ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Ottiene i preventivi per un cliente tramite UserNameCliente
    /// </summary>
    public async Task<IEnumerable<Preventivo>> GetPreventiviPerClienteAsync(string userNameCliente)
    {
        string query = @"
        SELECT Id, PreventivoId, DataCreazione, Descrizione, ClienteId, 
               RagioneSociale, ImportoTotale, Stato, NomeBarca,
               UserNameCliente, PasswordTemporanea, PasswordCambiata
        FROM Preventivi 
        WHERE UserNameCliente = @UserNameCliente";

        var parameters = new[] { new SqlParameter("@UserNameCliente", userNameCliente) };
        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

        var preventivi = new List<Preventivo>();
        foreach (DataRow row in dataTable.Rows)
        {
            preventivi.Add(new Preventivo
            {
                PreventivoId = Convert.ToInt32(row["PreventivoId"]),
                DataCreazione = Convert.ToDateTime(row["DataCreazione"]),
                Descrizione = row["Descrizione"].ToString() ?? "",
                ClienteId = Convert.ToInt32(row["ClienteId"]),
                RagioneSociale = row["RagioneSociale"].ToString() ?? "",
                ImportoTotale = Convert.ToDecimal(row["ImportoTotale"]),
                Stato = row["Stato"].ToString() ?? "",
                NomeBarca = row["NomeBarca"].ToString() ?? "",
                UserNameCliente = row["UserNameCliente"].ToString() ?? "",
                PasswordTemporanea = row["PasswordTemporanea"].ToString() ?? "",
                PasswordCambiata = row["PasswordCambiata"].ToString() ?? ""
            });
        }
        return preventivi;
    }

    /// <summary>
    /// Ottiene il riepilogo lavorazioni per un cliente specifico
    /// </summary>
    public async Task<IEnumerable<RiepilogoLavorazione>> GetRiepilogoLavorazioniPerClienteAsync(int preventivoId)
    {
        string query = @"
        SELECT 
            sl.PreventivoId,
            p.NomeBarca,
            COUNT(*) as AttivitaTotali,
            SUM(CASE WHEN sl.Stato = 'Completata' THEN 1 ELSE 0 END) as AttivitaCompletate,
            CASE 
                WHEN COUNT(*) = SUM(CASE WHEN sl.Stato = 'Completata' THEN 1 ELSE 0 END) THEN 'Completata'
                WHEN SUM(CASE WHEN sl.Stato = 'In Corso' THEN 1 ELSE 0 END) > 0 THEN 'In Corso'
                WHEN SUM(CASE WHEN sl.Stato = 'Sospesa' THEN 1 ELSE 0 END) > 0 THEN 'Sospesa'
                ELSE 'Da Iniziare'
            END as StatoGenerale
        FROM SchedaLavorazioni sl
        INNER JOIN Preventivi p ON sl.PreventivoId = p.PreventivoId
        WHERE sl.PreventivoId = @PreventivoId
        GROUP BY sl.PreventivoId, p.NomeBarca";

        var parameters = new[] { new SqlParameter("@PreventivoId", preventivoId) };
        DataTable dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

        var riepilogo = new List<RiepilogoLavorazione>();
        foreach (DataRow row in dataTable.Rows)
        {
            riepilogo.Add(new RiepilogoLavorazione
            {
                PreventivoId = Convert.ToInt32(row["PreventivoId"]),
                NomeBarca = row["NomeBarca"].ToString() ?? "",
                AttivitaTotali = Convert.ToInt32(row["AttivitaTotali"]),
                AttivitaCompletate = Convert.ToInt32(row["AttivitaCompletate"]),
                StatoGenerale = row["StatoGenerale"].ToString() ?? "Da Iniziare",
                IsExpanded = false
            });
        }
        return riepilogo;
    }
}
