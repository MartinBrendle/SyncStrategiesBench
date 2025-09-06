namespace AIO.DM
{
    public class DatenBasisObjekt
    {
        // Immutable Objekt
        public string Name { get; set; }

        public DatenBasisObjekt(string name)
        {
            Name = name;
        }
    }
}