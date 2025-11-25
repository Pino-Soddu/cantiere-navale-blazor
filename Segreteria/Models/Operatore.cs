namespace CNP.Segreteria.Models
{
    public class Operatore
    {
        public int Id { get; set; }
        public string Cognome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public int Codice { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public string Ruolo { get; set; } = string.Empty;
    }
}
