namespace FarmXpert.Models.Cattle_Activity_Mass
{
    public class CattleEventMass
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public string? Notes { get; set; }
        public string? Medicine { get; set; }
        public string? Dosage { get; set; }
        public DateTime Date { get; set; }
        public int FarmID { get; set; }


    }
}
