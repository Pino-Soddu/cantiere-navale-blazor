using CNP.SchedaLavorazione.Models;
using CNP.Amministrazione.Models;
using CNP.Models.Configurazione;
using System.Globalization;
using Microsoft.Extensions.Logging;


namespace CNP.Amministrazione.Services
{
    public class CalcoloService
    {
        private readonly TariffePreventivi _tariffe;
        private readonly MaterialiService _materialiService;
        private readonly ILogger<CalcoloService> _logger;
        private readonly DettaglioPreventiviService _dettaglioService; // AGGIUNGI

        public CalcoloService(IConfiguration configuration, MaterialiService materialiService,
                             ILogger<CalcoloService> logger, DettaglioPreventiviService dettaglioService) // AGGIUNGI
        {
            _materialiService = materialiService;
            _logger = logger;
            _dettaglioService = dettaglioService;

            // CARICA LE TARIFFE DALLA CONFIGURAZIONE
            var configCompleta = configuration.GetSection("ConfigurazionePreventivi").Get<ConfigurazionePreventivi>()
                              ?? new ConfigurazionePreventivi();

            _tariffe = new TariffePreventivi
            {
                ImportoOrarioBase = 50.00m,
                IndiceMoltiplicatore = 1.8m,
                TariffaSostaGiornaliera = 120.00m,
                AliquotaIva = 22m
            };
        }

        #region CALCOLO SINGOLA VOCE PREVENTIVO

        public async Task<DettaglioPreventivo> CalcolaTotaleVoce(
            DettaglioPreventivo voce,
            float lunghezzaBarca = 0)
        {
            if (voce == null)
                throw new ArgumentNullException(nameof(voce), "La voce non può essere nulla");

            try
            {
                // CONVERTI I VALORI FLOAT IN DECIMAL PER I CALCOLI DI PRECISIONE
                decimal importoVoce = CalcolaImportoAttivita(
                    voce.N_Operatori,
                    voce.UnitaMisura,
                    (decimal)voce.Qta_Voce,
                    (decimal)lunghezzaBarca);

                decimal importoMateriale = await CalcolaImportoMateriale(
                    voce.CodiceMateriale,
                    voce.UM_Mat,
                    (decimal)voce.Qta_Mat,
                    (decimal)lunghezzaBarca);

                // CONVERTI I RISULTATI DECIMAL IN FLOAT PER IL MODELLO DATABASE
                voce.ImportoVoce = (float)importoVoce;
                voce.ImportoMateriale = (float)importoMateriale;
                voce.TotaleVoce = (float)(importoVoce + importoMateriale);

                return voce;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRORE calcolo voce {voce.DescrizioneVoce}: {ex.Message}");
                return voce;
            }
        }

        #endregion

        #region CALCOLO ATTIVITÀ (MANODOPERA)

        private decimal CalcolaImportoAttivita(
            int numeroOperai,
            string unitaMisura,
            decimal quantitaVoce,
            decimal lunghezzaBarca)
        {
            decimal importo = 0;

            if (string.IsNullOrWhiteSpace(unitaMisura))
                return importo;

            string um = unitaMisura.ToLower().Trim();

            // GESTIONE SOSTA (GG)
            if (um == "gg")
            {
                importo = quantitaVoce * _tariffe.TariffaSostaGiornaliera;
            }
            // MINUTI
            else if (um == "min")
            {
                decimal tariffaAlMinuto = _tariffe.TariffaOraria / 60;
                importo = tariffaAlMinuto * numeroOperai * quantitaVoce;
            }
            // ORE
            else if (um == "ore")
            {
                importo = _tariffe.TariffaOraria * numeroOperai * quantitaVoce;
            }
            // NUMERO (lavori a cottimo)
            else if (um == "num")
            {
                importo = _tariffe.TariffaOraria * numeroOperai * quantitaVoce;
            }
            // METRI LINEARI (proporzionale alla lunghezza barca)
            else if (um == "ml" && lunghezzaBarca > 0)
            {
                decimal tariffaAlMinuto = _tariffe.TariffaOraria / 60;
                importo = tariffaAlMinuto * numeroOperai * quantitaVoce * lunghezzaBarca;
            }
            // ALTRE UNITÀ DI MISURA - calcolo base
            else
            {
                importo = _tariffe.TariffaOraria * numeroOperai * quantitaVoce;
            }

            return Math.Round(importo, 2);
        }

        #endregion

        #region CALCOLO MATERIALI

        private async Task<decimal> CalcolaImportoMateriale(
            string codiceMateriale,
            string umMat,
            decimal quantitaMat,
            decimal lunghezzaBarca)
        {
            decimal costoUnitarioMateriale = await OttieniCostoMateriale(codiceMateriale);
            decimal importo = 0;

            if (string.IsNullOrWhiteSpace(umMat) || costoUnitarioMateriale == 0)
                return importo;

            string um = umMat.ToLower().Trim();

            // MATERIALI PROPORZIONALI ALLA LUNGHEZZA BARCA
            if (um == "kg/ml" && lunghezzaBarca > 0)
            {
                importo = costoUnitarioMateriale * quantitaMat * lunghezzaBarca;
            }
            // MATERIALI STANDARD
            else
            {
                importo = costoUnitarioMateriale * quantitaMat;
            }

            return Math.Round(importo, 2);
        }

        private async Task<decimal> OttieniCostoMateriale(string codiceMateriale)
        {
            if (string.IsNullOrWhiteSpace(codiceMateriale))
                return 0;

            try
            {
                var materiale = await _materialiService.GetMaterialeByCodice(codiceMateriale);

                if (materiale != null && materiale.PrezzoUnitario.HasValue)
                {
                    Console.WriteLine($"✅ Prezzo materiale {codiceMateriale}: {materiale.PrezzoUnitario.Value}€");
                    return materiale.PrezzoUnitario.Value;
                }

                Console.WriteLine($"⚠️ Materiale non trovato: {codiceMateriale}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore caricamento materiale {codiceMateriale}: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region CALCOLO TOTALI PREVENTIVO

        public (decimal TotaleAttivita, decimal TotaleMateriali, decimal Imponibile, decimal Iva, decimal TotaleGenerale)
            CalcolaTotalePreventivo(List<DettaglioPreventivo> vociPreventivo)
        {
            decimal totaleAttivita = 0;
            decimal totaleMateriali = 0;
            decimal imponibile = 0;
            decimal iva = 0;
            decimal totaleGenerale = 0;

            if (vociPreventivo == null || !vociPreventivo.Any())
                return (totaleAttivita, totaleMateriali, imponibile, iva, totaleGenerale);

            try
            {
                foreach (var voce in vociPreventivo)
                {
                    // CONVERTI DA FLOAT A DECIMAL PER I TOTALI
                    totaleAttivita += (decimal)voce.ImportoVoce;
                    totaleMateriali += (decimal)voce.ImportoMateriale;
                }

                imponibile = totaleAttivita + totaleMateriali;
                iva = CalcolaIva(imponibile);
                totaleGenerale = imponibile + iva;

                totaleAttivita = Math.Round(totaleAttivita, 2);
                totaleMateriali = Math.Round(totaleMateriali, 2);
                imponibile = Math.Round(imponibile, 2);
                iva = Math.Round(iva, 2);
                totaleGenerale = Math.Round(totaleGenerale, 2);

                return (totaleAttivita, totaleMateriali, imponibile, iva, totaleGenerale);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRORE calcolo totale preventivo: {ex.Message}");
                return (0, 0, 0, 0, 0);
            }
        }

        private decimal CalcolaIva(decimal imponibile)
        {
            return imponibile * (_tariffe.AliquotaIva / 100);
        }

        #endregion

        #region CALCOLO AUTOMATICO ALLA MODIFICA

        public async Task<DettaglioPreventivo> RicalcolaVoceModificata(
            DettaglioPreventivo voceOriginale,
            int? nuoviOperai = null,
            string? nuovaUnitaMisura = null,
            float? nuovaQuantita = null,
            string? nuovoCodiceMateriale = null,
            float lunghezzaBarca = 0)
        {
            if (voceOriginale == null)
                throw new ArgumentNullException(nameof(voceOriginale), "La voce originale non può essere nulla");

            var voceCalcolo = new DettaglioPreventivo
            {
                Id = voceOriginale.Id,
                PreventivoId = voceOriginale.PreventivoId,
                VoceId = voceOriginale.VoceId,
                DescrizioneAttivita = voceOriginale.DescrizioneAttivita,
                DescrizioneVoce = voceOriginale.DescrizioneVoce,
                N_Operatori = nuoviOperai ?? voceOriginale.N_Operatori,
                UnitaMisura = nuovaUnitaMisura ?? voceOriginale.UnitaMisura,
                Qta_Voce = nuovaQuantita ?? voceOriginale.Qta_Voce,
                CodiceMateriale = nuovoCodiceMateriale ?? voceOriginale.CodiceMateriale,
                ImportoVoce = voceOriginale.ImportoVoce,
                ImportoMateriale = voceOriginale.ImportoMateriale,
                TotaleVoce = voceOriginale.TotaleVoce
            };

            return await CalcolaTotaleVoce(voceCalcolo, lunghezzaBarca);
        }

        #endregion

        // AGGIUNGI QUESTA REGIONE A CalcoloService.cs
        #region ESTRAZIONE MACRO-ATTIVITÀ PER PREVENTIVO

        public async Task<List<MacroAttivita>> GetMacroAttivitaPreventivo(int preventivoId)
        {
            var macroAttivita = new List<MacroAttivita>();

            try
            {
                // USA IL SERVIZIO ESISTENTE PER RECUPERARE I DETTAGLI
                var vociPreventivo = await _dettaglioService.GetDettagliByPreventivoId(preventivoId);

                // Raggruppa per attività e somma gli importi
                var attivitaRaggruppate = vociPreventivo
                    .Where(v => v.ImportoVoce > 0)
                    .GroupBy(v => v.DescrizioneAttivita?.Trim() ?? "Varie")
                    .Select(g => new MacroAttivita
                    {
                        NomeAttivita = g.Key,
                        ImportoTotale = g.Sum(v => (decimal)v.ImportoVoce)
                    })
                    .OrderBy(a => a.NomeAttivita)
                    .ToList();

                return attivitaRaggruppate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore estrazione macro-attività per preventivo {preventivoId}");
                return macroAttivita;
            }
        }


        #endregion

        #region FORMATTAZIONE ITALIANA

        public string FormattaImporto(decimal importo)
        {
            return importo.ToString("C", new CultureInfo("it-IT"));
        }

        public decimal ParseImportoItaliano(string valore)
        {
            if (string.IsNullOrWhiteSpace(valore))
                return 0;

            if (decimal.TryParse(valore, NumberStyles.Currency, CultureInfo.GetCultureInfo("it-IT"), out decimal risultato))
                return risultato;

            if (decimal.TryParse(valore, NumberStyles.Any, CultureInfo.InvariantCulture, out risultato))
                return risultato;

            return 0;
        }

        #endregion
    }
}