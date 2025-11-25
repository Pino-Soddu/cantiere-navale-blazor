namespace CNP.Segreteria.Models
{
    public class TipoImbarcazione
    {
        public int ID_Tipo { get; set; }
        public string Nome_Tipo { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
    }
}