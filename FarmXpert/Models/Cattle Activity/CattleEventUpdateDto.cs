namespace FarmXpert.Models.Cattle_Activity
{
    public class CattleEventUpdateDto
    {
        public string? EventType { get; set; }
        public int? TagNumber { get; set; }
        public string? Notes { get; set; }
        public string? Medicine { get; set; }
        public double? Weight { get; set; }
        public string? Dosage { get; set; }
        public string? WithdrawalTime { get; set; }
        public string? VaccineType { get; set; }
        public string? CalfGender { get; set; }
        public DateTime? Date { get; set; }
    }
}
