namespace CNP.SchedaLavorazione.Models;
public class Attività
{
    public int ID { get; set; }
    public int CodiceAttivita { get; set; }
    public string NomeAttivita { get; set; } = string.Empty;
    public string Descrizione { get; set; } = string.Empty;
}