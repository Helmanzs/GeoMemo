namespace GeoMemo.Models
{
    [Table("Obce")]
    public class Obec
    {
        [PrimaryKey]
        public int Id { get; set; }

        [Indexed, NotNull]
        public string Nazev { get; set; }
    }
}
