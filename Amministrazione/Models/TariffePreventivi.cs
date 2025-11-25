public class TariffePreventivi
{
    public decimal ImportoOrarioBase { get; set; }
    public decimal IndiceMoltiplicatore { get; set; }
    public decimal TariffaSostaGiornaliera { get; set; }
    public decimal AliquotaIva { get; set; }

    public decimal TariffaOraria => ImportoOrarioBase * IndiceMoltiplicatore; // 90€/ora
}
