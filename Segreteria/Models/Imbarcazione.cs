namespace CNP.Segreteria.Models
{
    public class Imbarcazione
    {
        public int ID_Imbarcazione { get; set; }
        public string Nome_Imbarcazione { get; set; } = string.Empty;
        public int? ID_Tipo { get; set; }
        public int? ID_Cliente { get; set; }
        public int? Anno_Costruzione { get; set; }
        public decimal? Lunghezza { get; set; }
        public decimal? Larghezza { get; set; }
        public decimal? Pescaggio_FB { get; set; }
        public decimal? Pescaggio_EB { get; set; }
        public decimal? Peso { get; set; }
        public string? Materiale_Scafo { get; set; }
        public DateTime? Data_Acquisto { get; set; }
        public string? Stato { get; set; }
        public string? PortoAttracco { get; set; }
        public string? Note { get; set; }

        // PROPRIETÀ DI NAVIGAZIONE CODIFICATE
        public string? Nome_Tipo { get; set; }
        public string? RagioneSociale_Cliente { get; set; }

        // ⚠️ PROPRIETÀ CALCOLATA per CodiceCliente
        public int? CodiceCliente { get; set; } // Sarà popolata dal servizio
    }
}