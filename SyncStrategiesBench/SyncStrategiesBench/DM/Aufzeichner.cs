namespace AIO.DM
{
    public class Aufzeichner
    {
        public string Name { get; set; }

        public Aufzeichner(string name)
        {
            Name = name;
        }

        public DarstellungsObjektList DarstellungsObjekte { get; set; } = new();
    }
}