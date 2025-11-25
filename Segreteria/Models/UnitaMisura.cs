namespace CNP.Segreteria.Models
{
    public class UnitaMisura
    {
        public int IdUnitaMisura { get; set; }
        public string UM { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
        public string Tipo { get; set; } = string.Empty; // "ATTIVITA", "MATERIALE", "ENTRAMBI"
    }
}