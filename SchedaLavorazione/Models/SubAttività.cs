namespace CNP.SchedaLavorazione.Models;
public class SubAttività
{
    public int ID { get; set; }
    public int CodiceAttivita { get; set; }
    public int CodiceSubattivita { get; set; }
    public string Voce { get; set; } = string.Empty;
    public int N_Operatori { get; set; }
    public string UnitaMisura { get; set; } = string.Empty;
    public float Qta_Voce { get; set; }
    public string CodiceMateriale { get; set; } = string.Empty;
}