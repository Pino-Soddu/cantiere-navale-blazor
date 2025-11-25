namespace CNP.Amministrazione.Models
{
    public class StatoPreventivoConfig
    {
        public int Codice { get; set; }
        public string Colore { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
    }

    public class StatiPreventivoConfig
    {
        public StatoPreventivoConfig BOZZA { get; set; } = new();
        public StatoPreventivoConfig INVIATO { get; set; } = new();
        public StatoPreventivoConfig ACCETTATO { get; set; } = new();
        public StatoPreventivoConfig IN_LAVORAZIONE { get; set; } = new();
        public StatoPreventivoConfig RIFIUTATO { get; set; } = new();
        public StatoPreventivoConfig IN_REVISIONE { get; set; } = new();
        public StatoPreventivoConfig SCADUTO { get; set; } = new();
        public StatoPreventivoConfig CONVERTITO_ORDINE { get; set; } = new();
        public StatoPreventivoConfig ANNULLATO { get; set; } = new();
        public StatoPreventivoConfig ATTESA_PAGAMENTO { get; set; } = new();
    }
}