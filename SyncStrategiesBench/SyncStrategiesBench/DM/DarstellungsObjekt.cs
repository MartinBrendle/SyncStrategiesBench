namespace AIO.DM
{
    public class DarstellungsObjekt
    {
        public DarstellungsObjekt(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public AufzeichnerList Aufzeichner { get; set; } = new();
    }
}