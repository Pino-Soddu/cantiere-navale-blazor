namespace CNP.Segreteria.Models
{
    public class Materiale
    {
        public int IDMateriale { get; set; }
        public string CodiceMateriale { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
        public string? UnitaMisura { get; set; }
        public decimal? PrezzoUnitario { get; set; }
        public DateTime? DataUltimoOrdine { get; set; }
        public decimal? QuantitaDisponibile { get; set; }
        public decimal? QuantitaMinima { get; set; }
        public string? Fornitore { get; set; }
        public string? Note { get; set; }
    }
}
