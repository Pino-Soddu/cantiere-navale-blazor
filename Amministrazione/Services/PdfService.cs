using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CNP.SchedaLavorazione.Models;
using CNP.Amministrazione.Models;
using Microsoft.Extensions.Logging;

namespace CNP.Amministrazione.Services
{
    public class PdfService
    {
        private readonly ILogger<PdfService> _logger;
        private readonly PreventiviService _preventiviService;
        private readonly DettaglioPreventiviService _dettaglioService;
        private readonly CalcoloService _calcoloService;
        private readonly IConfiguration _configuration;

        public PdfService(ILogger<PdfService> logger,
                        PreventiviService preventiviService,
                        DettaglioPreventiviService dettaglioService,
                        CalcoloService calcoloService, IConfiguration configuration)
        {
            _logger = logger;
            _preventiviService = preventiviService;
            _dettaglioService = dettaglioService;
            _calcoloService = calcoloService;
            _configuration = configuration;

            // Imposta licenza (QuestPDF è gratis per progetti open source)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GeneraPdfPreventivo(int preventivoId)
        {
            try
            {
                // Recupera dati preventivo
                var preventivo = await _preventiviService.GetPreventivoById(preventivoId);
                if (preventivo == null)
                    throw new ArgumentException($"Preventivo {preventivoId} non trovato");

                // Recupera macro-attività
                var macroAttivita = await _calcoloService.GetMacroAttivitaPreventivo(preventivoId);
                var totaleAttivita = macroAttivita.Sum(a => a.ImportoTotale);
                var iva = totaleAttivita * 0.22m;
                var totaleGenerale = totaleAttivita + iva;

                // Genera PDF
                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // HEADER
                        page.Header().Element(ComponiHeader);

                        // CONTENUTO
                        page.Content().Element(c => ComponiContenuto(c, preventivo, macroAttivita, totaleAttivita, iva, totaleGenerale));

                        // FOOTER
                        page.Footer().AlignCenter().Text(t =>
                        {
                            t.Span("Pagina ");
                            t.CurrentPageNumber();
                            t.Span(" di ");
                            t.TotalPages();
                        });
                    });
                })
                .GeneratePdf();

                await SalvaPdfSuFileSystem(preventivoId, pdfBytes);

                _logger.LogInformation($"PDF generato per preventivo {preventivoId}");
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore generazione PDF per preventivo {preventivoId}");
                throw;
            }
        }

        private async Task SalvaPdfSuFileSystem(int preventivoId, byte[] pdfBytes)
        {
            try
            {
                var pdfPath = _configuration["PdfSettings:PdfStoragePath"] ?? "C:\\CN_Fulghesu\\Archivi\\PreventiviPDF\\";
                var fileName = $"Preventivo_{preventivoId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var fullPath = Path.Combine(pdfPath, fileName);

                // Crea directory se non esiste
                Directory.CreateDirectory(pdfPath);

                await File.WriteAllBytesAsync(fullPath, pdfBytes);
                _logger.LogInformation($"PDF salvato in: {fullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore salvataggio PDF su file system");
            }
        }

        private void ComponiHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("CANTIERE NAVALE SA PERDIXEDDA").Bold().FontSize(14);
                    column.Item().Text("DI GIUSEPPE FULGHESU");
                    column.Item().Text("Calata dei Mercedari, snc - Loc. Su Siccu, 09123 - Cagliari (CA)");
                    column.Item().Text("P.IVA 02392400921 - CF 02392400921");
                });

                row.ConstantItem(100).Height(50).Image("wwwroot/Immagini/logo.png");
            });
        }

        private void ComponiContenuto(IContainer container, Preventivo preventivo,
                                    List<MacroAttivita> macroAttivita, decimal totaleAttivita,
                                    decimal iva, decimal totaleGenerale)
        {
            container.Column(column =>
            {
                // DATI CLIENTE
                column.Item().PaddingBottom(15).Column(clienteColumn =>
                {
                    clienteColumn.Item().Text("Spett.le").Bold();
                    clienteColumn.Item().Text(preventivo.RagioneSociale);
                    clienteColumn.Item().Text($"Preventivo per l'imbarcazione: {preventivo.NomeBarca}");
                });

                // DATI PREVENTIVO
                column.Item().PaddingBottom(20).Column(preventivoColumn =>
                {
                    preventivoColumn.Item().Text($"PREVENTIVO N. {preventivo.PreventivoId}").Bold().FontSize(12);
                    preventivoColumn.Item().Text($"Data Emissione: {preventivo.DataCreazione:dd/MM/yyyy}");
                    preventivoColumn.Item().Text($"Data Scadenza: {preventivo.DataScadenza:dd/MM/yyyy}");
                    preventivoColumn.Item().Text($"Riferimenti: {preventivo.Descrizione}");
                });

                // TABELLA ATTIVITÀ
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // ATTIVITÀ
                        columns.ConstantColumn(100); // IMPORTO
                    });

                    // INTESTAZIONE TABELLA
                    table.Header(header =>
                    {
                        header.Cell().Background("#0066cc").Padding(5).Text("ATTIVITÀ").FontColor(Colors.White).Bold();
                        header.Cell().Background("#0066cc").Padding(5).Text("IMPORTO").FontColor(Colors.White).Bold();
                    });

                    // RIGHE ATTIVITÀ
                    foreach (var attivita in macroAttivita)
                    {
                        table.Cell().BorderBottom(1).BorderColor("#dddddd").Padding(5).Text(attivita.NomeAttivita);
                        table.Cell().BorderBottom(1).BorderColor("#dddddd").Padding(5).AlignRight().Text($"€ {attivita.ImportoTotale:N2}");
                    }

                    // TOTALI
                    table.Cell().BorderBottom(1).BorderColor("#333333").Padding(5).Text("TOTALE ATTIVITÀ").Bold();
                    table.Cell().BorderBottom(1).BorderColor("#333333").Padding(5).AlignRight().Text($"€ {totaleAttivita:N2}").Bold();

                    table.Cell().Padding(5).Text("IVA 22%");
                    table.Cell().Padding(5).AlignRight().Text($"€ {iva:N2}");

                    table.Cell().Background("#e6f7ff").Padding(5).Text("TOTALE PREVENTIVO").Bold().FontSize(11);
                    table.Cell().Background("#e6f7ff").Padding(5).AlignRight().Text($"€ {totaleGenerale:N2}").Bold().FontSize(11);
                });

                // NOTE E CONDIZIONI
                column.Item().PaddingTop(20).Column(noteColumn =>
                {
                    noteColumn.Item().Text("Note: Per il dettaglio completo delle voci, contattare il cantiere.").FontSize(9);
                    noteColumn.Item().Text("Validità: 30 giorni dalla data di emissione.").FontSize(9);
                    noteColumn.Item().Text("Condizioni di pagamento: 50% all'accettazione, 50% al ritiro.").FontSize(9);
                    noteColumn.Item().PaddingTop(10).Text("Questo preventivo è stato generato automaticamente dal sistema del cantiere.").Italic().FontSize(8);
                });
            });
        }
    }
}

