namespace GeoMemo.Models
{
    [Table("GeoVzpominky")]
    public class GeoVzpominka
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // GPS
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Obec
        [Indexed]
        public string NazevObce { get; set; }

        // Hodnocení 1–5
        public int Hodnoceni { get; set; }

        // Krátký popis
        [MaxLength(100)]
        public string Popis { get; set; }

        // Cesta k obrázku v úložišti zařízení
        public string ImagePath { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
