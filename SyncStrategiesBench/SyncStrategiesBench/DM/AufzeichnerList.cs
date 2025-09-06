namespace AIO.DM
{
    public class AufzeichnerList : List<Aufzeichner> 
    {
      public AufzeichnerList()
      {
      }

      public AufzeichnerList(List<Aufzeichner> quelle)
      {
        foreach (var aufzeichner in quelle)
        {
          Add(new Aufzeichner(aufzeichner.Name));
        }
      }

  }
}
