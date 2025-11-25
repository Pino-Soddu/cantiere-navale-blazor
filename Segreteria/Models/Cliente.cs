namespace CNP.Segreteria.Models
{
    public class Cliente
    {
        public int ClienteID { get; set; }
        public int CodiceCliente { get; set; }
        public string RagioneSociale { get; set; } = string.Empty;
        public string? NomeContatto { get; set; }
        public string? RuoloContatto { get; set; }
        public string? TipoCliente { get; set; }
        public string? Indirizzo { get; set; }
        public string? Comune { get; set; }
        public string? CAP { get; set; }
        public string? SiglaProvincia { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? PartitaIVA { get; set; }
        public string? CodiceFiscale { get; set; }
        public string? IBAN { get; set; }
        public string? BancaRiferimento { get; set; }
        public string? TipologiaServizi { get; set; }
        public string? PortoRiferimento { get; set; }
        public string? TerminiPagamento { get; set; }
        public DateTime? DataPrimoContatto { get; set; }
        public string? ValutazioneCliente { get; set; }
        public string? Note { get; set; }
    }
}