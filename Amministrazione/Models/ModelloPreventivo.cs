namespace CNP.Amministrazione.Models;

public class ModelloPreventivo
{
    public int Id { get; set; }
    public string TitoloPreventivo { get; set; } = string.Empty;
    public int VoceId { get; set; }
    public string DescrizioneAttivita { get; set; } = string.Empty;
    public string DescrizioneVoce { get; set; } = string.Empty;
    public int N_Operatori { get; set; }
    public string UnitaMisura { get; set; } = string.Empty;
    public double Qta_Voce { get; set; }
    public decimal ImportoVoce { get; set; }
    public string CodiceMateriale { get; set; } = string.Empty;
    public string UM_Mat { get; set; } = string.Empty;
    public decimal Costo_Mat { get; set; }
    public double Qta_Mat { get; set; }
    public decimal ImportoMateriale { get; set; }
    public decimal TotaleVoce { get; set; }
}