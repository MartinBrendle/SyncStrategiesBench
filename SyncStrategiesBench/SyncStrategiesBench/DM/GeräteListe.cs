namespace AIO.DM
{
    public class GeräteListe : List<Gerät>
    {
        // Standardkonstruktor
        public GeräteListe() {}

        public GeräteListe(List<Gerät> quelle)
        {
          foreach (Gerät g in quelle)
          {
            Add(new Gerät
            {
              Name = g.Name,
              DatenBasen = new DatenBasis(g.DatenBasen), // Copy-Konstruktor von DatenBasis
              DarstellungsObjekte = new DarstellungsObjektList(g.DarstellungsObjekte) // Copy-Konstruktor
            });
          }
        }
  }
}