namespace CNP.SchedaLavorazione.Models;

public class SchedaLavorazioneModel
{
    public int Id { get; set; }
    public int PreventivoId { get; set; }
    public int IdVoce { get; set; }
    public string Attività { get; set; } = string.Empty;
    public string Voce { get; set; } = string.Empty;
    public string Operatore { get; set; } = string.Empty;
    public DateTime? DataOraInizio { get; set; }
    public DateTime? DataOraFine { get; set; }
    public TimeSpan? OreImpiegate { get; set; }
    public string CodiceMateriale { get; set; } = string.Empty;
    public double QtaMateriale { get; set; }
    public string Note { get; set; } = string.Empty;
    public string Stato { get; set; } = string.Empty;   // DaIniziare/InCorso/Sospesa/Completata
}