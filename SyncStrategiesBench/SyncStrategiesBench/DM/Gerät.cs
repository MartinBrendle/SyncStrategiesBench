namespace AIO.DM
{
    public class Gerät
    {
        public DarstellungsObjektList DarstellungsObjekte { get; set; } = new();
        public DatenBasis DatenBasen { get; set; } = new();

        public string Name { get; set; }
    }
}