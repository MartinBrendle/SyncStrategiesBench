namespace AIO.DM
{
    public class DarstellungsObjektList : List<DarstellungsObjekt>
    {
        public DarstellungsObjektList()
        {
        }

        public DarstellungsObjektList(List<DarstellungsObjekt> toList)
        {
            foreach (DarstellungsObjekt darstellungsObjekt in toList)
            {
                Add(darstellungsObjekt);
            }
        }
    }
}