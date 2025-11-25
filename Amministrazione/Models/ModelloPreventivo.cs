using System.Text.Json.Serialization;

namespace CNP.Amministrazione.Models
{
    public class ModelloPreventivo
    {
        // ✅ CAMPI SEMPRE UTILIZZATI
        public int Id { get; set; }
        public string TitoloPreventivo { get; set; } = string.Empty;
        public int VoceId { get; set; }
        public string DescrizioneAttivita { get; set; } = string.Empty;
        public string DescrizioneVoce { get; set; } = string.Empty;
        public int N_Operatori { get; set; }
        public string UnitaMisura { get; set; } = string.Empty;
        public double Qta_Voce { get; set; }

        // ✅ MATERIALI - SEMPRE PRESENTI
        public string CodiceMateriale { get; set; } = string.Empty;
        public string UM_Mat { get; set; } = string.Empty;

        // ✅ MATERIALI FISSI (KG, LT, NUM) - SOLO SE APPLICABILE
        public double Qta_Mat { get; set; }
        public decimal Costo_Mat { get; set; }
        public decimal ImportoMateriale { get; set; }

        // ✅ TOTALI - SEMPRE PRESENTI
        public decimal ImportoVoce { get; set; }
        public decimal TotaleVoce { get; set; }

        // ✅ METODO PER VERIFICARE SE È MATERIALE VARIABILE
        [JsonIgnore]
        public bool IsMaterialeVariabile =>
            !string.IsNullOrEmpty(UM_Mat) &&
            (UM_Mat.Contains("/ML") || UM_Mat.Contains("/MQ"));
    }
}