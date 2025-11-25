namespace CNP.Segreteria.Models
{
    public class Fornitore
    {
        public int FornitoreID { get; set; }
        public string RagioneSociale { get; set; } = string.Empty;
        public string? Indirizzo { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? PartitaIVA { get; set; }
        public string? CodiceSDI { get; set; }
        public string? Note { get; set; }
    }
}
