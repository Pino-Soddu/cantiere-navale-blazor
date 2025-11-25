using CNP.Amministrazione.Services;
using CNP.SchedaLavorazione.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace CNP.Amministrazione.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly ClientiService _clientiService;
        private readonly PreventiviService _preventiviService;
        private readonly CalcoloService _calcoloService;
        private readonly PdfService _pdfService;
        private readonly SchedaLavorazioneService _schedaLavorazioneService;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger,
                          ClientiService clientiService, PreventiviService preventiviService, 
                          CalcoloService calcoloService, PdfService pdfService, SchedaLavorazioneService schedaLavorazioneService)
        {
            _configuration = configuration;
            _logger = logger;
            _clientiService = clientiService;
            _preventiviService = preventiviService;
            _calcoloService = calcoloService;
            _pdfService = pdfService;
            _schedaLavorazioneService = schedaLavorazioneService;
        }

        public async Task<bool> InviaPreventivoCliente(int preventivoId)
        {
            try
            {
                _logger.LogInformation($"Inizio invio preventivo {preventivoId}");

                // 1. Recupera dati preventivo
                var preventivo = await _preventiviService.GetPreventivoById(preventivoId);
                if (preventivo == null)
                {
                    _logger.LogError($"Preventivo {preventivoId} non trovato");
                    return false;
                }

                // 2. VERIFICA SE CREDENZIALI ESISTONO - SE NO, GENERALE
                if (string.IsNullOrEmpty(preventivo.UserNameCliente))
                {
                    await _preventiviService.GeneraCredenzialiCliente(preventivoId, preventivo.NomeBarca);
                    // RICARICA IL PREVENTIVO CON LE CREDENZIALI AGGIORNATE
                    preventivo = await _preventiviService.GetPreventivoById(preventivoId);
                }

                // 3. Recupera email cliente
                #pragma warning disable CS8602
                var emailCliente = await RecuperaEmailCliente(preventivo.ClienteId);
                #pragma warning restore CS8602
                if (string.IsNullOrEmpty(emailCliente))
                {
                    _logger.LogError($"Email non trovata per cliente {preventivo.ClienteId}");
                    return false;
                }

                // 4. Genera corpo email HTML - AGGIUNGI AWAIT
                var corpoHtml = await GeneraCorpoEmailPreventivo(preventivo); // AGGIUNGI AWAIT

                // 5. Genera PDF (per ora null, implementeremo dopo)
                byte[]? allegatoPdf = await _pdfService.GeneraPdfPreventivo(preventivo.PreventivoId);

                // 6. Invia email
                var risultato = await InviaEmail(
                    emailCliente,
                    $"Preventivo N. {preventivo.PreventivoId} - Cantiere Navale Sa Perdixedda",
                    corpoHtml,
                    allegatoPdf
                );

                if (risultato)
                {
                    // 7. Aggiorna stato preventivo
                    await _preventiviService.CambiaStatoInviato(preventivoId, emailCliente);
                    _logger.LogInformation($"Preventivo {preventivoId} inviato con successo a {emailCliente}");
                }

                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'invio del preventivo {preventivoId}");
                return false;
            }
        }
        private async Task<string?> RecuperaEmailCliente(int clienteId)
        {
            try
            {
                var cliente = await _clientiService.GetClienteById(clienteId);
                if (cliente != null && !string.IsNullOrEmpty(cliente.Email))
                {
                    return cliente.Email;
                }

                _logger.LogWarning($"Cliente {clienteId} non trovato o email non presente");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore recupero email per cliente {clienteId}");
                return null;
            }
        }
        private async Task<string> GeneraCorpoEmailPreventivo(Preventivo preventivo)
        {
            // RECUPERA LE MACRO-ATTIVITÀ
            var macroAttivita = await _calcoloService.GetMacroAttivitaPreventivo(preventivo.PreventivoId);
            var totaleAttivita = macroAttivita.Sum(a => a.ImportoTotale);
            var iva = totaleAttivita * 0.22m;
            var totaleGenerale = totaleAttivita + iva;

            var righeAttivita = new StringBuilder();

            var sezioneCredenziali = $@"
                <div style='margin-top: 30px; padding: 15px; background: #f8f9fa; border-radius: 5px; border-left: 4px solid #0066cc;'>
                    <h4 style='margin-top: 0; color: #0066cc;'>📱 ACCESSO AREA CLIENTI</h4>
                    <p>Per monitorare lo stato delle lavorazioni della tua imbarcazione <strong>{preventivo.NomeBarca}</strong>:</p>
                    <p><strong>🔗 Link:</strong> http://localhost:5050/areaclienti</p>
                    <p><strong>👤 Username:</strong> {preventivo.UserNameCliente}</p>
                    <p><strong>🔐 Password:</strong> {preventivo.PasswordTemporanea}</p>
                    <p style='font-size: 12px; color: #666;'><em>Conserva queste credenziali per accedere all'area clienti</em></p>
                </div>";

            foreach (var attivita in macroAttivita)
            {
                righeAttivita.AppendLine($@"
                <tr>
                    <td>{attivita.NomeAttivita}</td>
                    <td class='text-end'>€ {attivita.ImportoTotale.ToString("N2")}</td>
                </tr>");
            }

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 20px; color: #333; }}
                        .header {{ border-bottom: 2px solid #0066cc; padding-bottom: 10px; margin-bottom: 20px; }}
                        .company-name {{ font-size: 18px; font-weight: bold; color: #0066cc; }}
                        .client-info {{ background: #f9f9f9; padding: 15px; margin: 15px 0; }}
                        .preventivo-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                        .preventivo-table th {{ background: #0066cc; color: white; padding: 10px; text-align: left; }}
                        .preventivo-table td {{ padding: 10px; border-bottom: 1px solid #ddd; }}
                        .total-row {{ font-weight: bold; background: #f0f0f0; }}
                        .footer {{ margin-top: 30px; font-size: 12px; color: #666; }}
                        .text-end {{ text-align: right; }}
                    </style>
                </head>
                <body>

                <div class='header'>
                    <div class='company-name'>CANTIERE NAVALE SA PERDIXEDDA</div>
                    <div>DI GIUSEPPE FULGHESU</div>
                    <div>Calata dei Mercedari, snc - Loc. Su Siccu, 09123 - Cagliari (CA)</div>
                    <div>P.IVA 02392400921 - CF 02392400921</div>
                </div>

                <div class='client-info'>
                    <strong>Spett.le</strong><br>
                    {preventivo.RagioneSociale}<br>
                    Preventivo per l'imbarcazione: {preventivo.NomeBarca}
                </div>

                <div>
                    <h3>PREVENTIVO N. {preventivo.PreventivoId}</h3>
                    <p><strong>Data Emissione:</strong> {preventivo.DataCreazione:dd/MM/yyyy}</p>
                    <p><strong>Data Scadenza:</strong> {preventivo.DataScadenza:dd/MM/yyyy}</p>
                    <p><strong>Riferimenti:</strong> {preventivo.Descrizione}</p>
                </div>

                <table class='preventivo-table'>
                    <tr>
                        <th>ATTIVITÀ</th>
                        <th class='text-end'>IMPORTO</th>
                    </tr>
    
                    {righeAttivita}
    
                    <tr class='total-row'>
                        <td>TOTALE ATTIVITÀ</td>
                        <td class='text-end'>€ {totaleAttivita.ToString("N2")}</td>
                    </tr>
                    <tr class='total-row'>
                        <td>IVA 22%</td>
                        <td class='text-end'>€ {iva.ToString("N2")}</td>
                    </tr>
                    <tr class='total-row' style='background: #e6f7ff;'>
                        <td><strong>TOTALE PREVENTIVO</strong></td>
                        <td class='text-end'><strong>€ {totaleGenerale.ToString("N2")}</strong></td>
                    </tr>
                </table>

                {sezioneCredenziali}

                <div class='footer'>
                    <p><strong>Note:</strong> Per il dettaglio completo delle voci, contattare il cantiere.</p>
                    <p><strong>Validità:</strong> 30 giorni dalla data di emissione.</p>
                    <p><strong>Condizioni di pagamento:</strong> 50% all'accettazione, 50% al ritiro.</p>
                    <p><em>Questo preventivo è stato generato automaticamente dal sistema del cantiere.</em></p>
                </div>

        </body>
        </html>";
        }
        public async Task<bool> InviaEmail(string destinatario, string oggetto, string corpoHtml, byte[]? allegatoPdf = null)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                // RISOLVI AVVISO CS8604 - USA GetValue() CON VALORI DI DEFAULT
                using var client = new SmtpClient(emailSettings["ServerSMTP"] ?? "smtp.gmail.com")
                {
                    Port = emailSettings.GetValue<int>("Porta", 587), // VALORE DI DEFAULT 587
                    Credentials = new NetworkCredential(
                        emailSettings["NomeUtente"] ?? string.Empty,
                        emailSettings["Password"] ?? string.Empty
                    ),
                    EnableSsl = emailSettings.GetValue<bool>("AbilitaSSL", true) // VALORE DI DEFAULT true
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(
                        emailSettings["EmailMittente"] ?? string.Empty,
                        emailSettings["NomeMittente"] ?? "Cantiere Navale"
                    ),
                    Subject = oggetto,
                    Body = corpoHtml,
                    IsBodyHtml = true
                };

                message.To.Add(destinatario);

                // Allega PDF se presente
                if (allegatoPdf != null)
                {
                    var attachment = new Attachment(
                        new MemoryStream(allegatoPdf),
                        $"Preventivo_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                        MediaTypeNames.Application.Pdf
                    );
                    message.Attachments.Add(attachment);
                }

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email inviata a {destinatario}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore invio email a {destinatario}");
                return false;
            }
        }

        // METODO CORRETTO PER InviaNotificaInizioLavori
        public async Task<bool> InviaNotificaInizioLavori(int preventivoId)
        {
            try
            {
                var preventivo = await _preventiviService.GetPreventivoById(preventivoId);
                if (preventivo == null) return false;

                var emailCliente = await RecuperaEmailCliente(preventivo.ClienteId);
                if (string.IsNullOrEmpty(emailCliente)) return false;

                // STRINGA HTML CORRETTA - USA @$ PER MULTILINEA
                var corpoEmail = @$"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; margin: 20px; color: #333; }}
                            .header {{ border-bottom: 2px solid #0066cc; padding-bottom: 10px; margin-bottom: 20px; }}
                            .company-name {{ font-size: 18px; font-weight: bold; color: #0066cc; }}
                            .notification-box {{ background: #f9f9f9; padding: 15px; margin: 15px 0; border-radius: 5px; }}
                        </style>
                    </head>
                    <body>

                    <div class='header'>
                        <div class='company-name'>CANTIERE NAVALE SA PERDIXEDDA</div>
                        <div>DI GIUSEPPE FULGHESU</div>
                    </div>

                    <div class='notification-box'>
                        <strong>Gentile Cliente,</strong><br><br>
    
                        I lavori per la sua imbarcazione <strong>""{preventivo.NomeBarca}""</strong> sono ufficialmente iniziati!<br><br>

                        📋 <strong>Potrà seguire l'avanzamento in tempo reale dalla sua Area Clienti:</strong><br>
                        🔗 <strong>Link:</strong> https://tuosito.com/clienti<br>
                        👤 <strong>Username:</strong> {preventivo.UserNameCliente}<br>
                        🔐 <strong>Password:</strong> {preventivo.PasswordTemporanea}<br><br>

                        Cordiali saluti,<br>
                        <strong>Cantiere Navale Sa Perdixedda</strong>
                    </div>

                    </body>
                    </html>";

                return await InviaEmail(
                    emailCliente,
                    $"Lavori iniziati per {preventivo.NomeBarca}",
                    corpoEmail,
                    null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica inizio lavori per preventivo {preventivoId}");
                return false;
            }
        }


    }
}








