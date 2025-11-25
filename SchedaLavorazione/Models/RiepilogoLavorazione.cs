namespace CNP.SchedaLavorazione.Models;

public class RiepilogoLavorazione
{
    public int PreventivoId { get; set; }
    public required string NomeBarca { get; set; }
    public int AttivitaTotali { get; set; }
    public int AttivitaCompletate { get; set; }
    public required string StatoGenerale { get; set; }
    public bool IsExpanded { get; set; }
}