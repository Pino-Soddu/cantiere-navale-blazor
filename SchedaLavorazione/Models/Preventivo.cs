namespace CNP.SchedaLavorazione.Models;
public class Preventivo
{
    public int PreventivoId { get; set; }
    public DateTime DataCreazione { get; set; }
    public string Descrizione { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string RagioneSociale { get; set; } = string.Empty;
    public decimal ImportoTotale { get; set; }
    public string Stato { get; set; } = string.Empty;
    public string NomeBarca { get; set; } = string.Empty;
    public string NomePreventivo { get; set; } = string.Empty;
    public DateTime DataScadenza { get; set; } 

    // Credenziali Cliente per accesso allo stato avanzamento lavori
    public string UserNameCliente { get; set; } = string.Empty;
    public string PasswordTemporanea { get; set; } = string.Empty;
    public string PasswordCambiata { get; set; } = string.Empty;

    // Per gestione invio Email Preventivo
    public DateTime? DataInvio { get; set; }
    public string? EmailCliente { get; set; }
}