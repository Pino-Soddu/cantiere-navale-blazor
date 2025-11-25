namespace CNP.SchedaLavorazione.Models;
public class DettaglioPreventivo
{
    public int Id { get; set; }
    public int PreventivoId { get; set; }
    public int VoceId { get; set; }
    public string DescrizioneAttivita { get; set; } = string.Empty;
    public string DescrizioneVoce { get; set; } = string.Empty;

    // CAMPI PER ATTIVITA' 
    public int N_Operatori { get; set; }
    public string UnitaMisura { get; set; } = string.Empty;
    public float Qta_Voce { get; set; } 
    public float ImportoVoce { get; set; }

    // CAMPI PER MATERIALI 
    public string CodiceMateriale { get; set; } = string.Empty;
    public string UM_Mat { get; set; } = string.Empty;
    public float Qta_Mat { get; set; } 
    public float Costo_Mat { get; set; }
    public float ImportoMateriale { get; set; } 

    public float TotaleVoce { get; set; } 
}