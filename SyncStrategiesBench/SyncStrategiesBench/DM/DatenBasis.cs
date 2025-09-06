namespace AIO.DM
{
    public class DatenBasis : List<DatenBasisObjekt>
    {
        public DatenBasis()
        {
        }

        public DatenBasis(List<DatenBasisObjekt> toList)
        {
            foreach (DatenBasisObjekt datenBasisObjekt in toList)
            {
                Add(datenBasisObjekt);
            }
        }
    }
}