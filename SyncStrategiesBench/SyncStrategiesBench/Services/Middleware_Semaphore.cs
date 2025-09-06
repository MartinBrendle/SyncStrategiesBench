using AIO.DM;
using AIO.Utils;
using DevExpress.CodeParser;


namespace AIO.Services
{
  public class Middleware_Semaphore : IMiddleware
  {
    public async Task AddGerätDirect(Gerät gerät)
    {
    }

    public async Task AddAufzeichnerDirect(Aufzeichner auf)
    {
    }

    private GeräteListe mGeräte = new();
    private AufzeichnerList mAufzeichner = new();
    private readonly SemaphoreSlim mSemaphore = new(1, 1);

    public void ErzeugeDatenmodell(ExperimentConfig config)
    {
      mGeräte = new();
      mAufzeichner = new();

      for (int i = 0; i < config.Geraete; i++)
      {
        var g = new Gerät
        {
          Name = $"Initial_Gerät_{i}",
          DatenBasen = new DatenBasis(),
          DarstellungsObjekte = new DarstellungsObjektList()
        };

        for (int d = 0; d < config.DatenBasisObjekte; d++)
          g.DatenBasen.Add(new DatenBasisObjekt($"Init_DB_{i}_{d}"));

        mGeräte.Add(g);
      }

      for (int a = 0; a < config.Aufzeichner; a++)
      {
        var auf = new Aufzeichner($"Init_Aufzeichner_{a}");
        mAufzeichner.Add(auf);
      }
    }


    //Gerät
    public async Task AddGerät(Gerät gerät)
    {
      await mSemaphore.WaitAsync();
      try
      {
        mGeräte.Add(gerät);
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task<Gerät?> GetGerät(string name)
    {
      await mSemaphore.WaitAsync();
      try
      {
        return mGeräte.FirstOrDefault(g => g.Name == name);
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task<GeräteListe> GetGerätListe()
    {
      await mSemaphore.WaitAsync();
      try
      {
        return mGeräte;
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task LöscheGerät(Gerät gerät)
    {
      await mSemaphore.WaitAsync();
      try
      {
        mGeräte.Remove(gerät);
      }
      finally
      {
        mSemaphore.Release();
      }
    }


    //Darstellungsobjekt
    public async Task AddDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      await mSemaphore.WaitAsync();
      try
      {
        mGeräte.First(g => g.Name == gerät.Name).DarstellungsObjekte.Add(obj);
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task<DarstellungsObjekt?> GetDarstellungsObjekt(Gerät gerät, string name)
    {
      await mSemaphore.WaitAsync();
      try
      {
        return mGeräte.First(g => g.Name == gerät.Name).DarstellungsObjekte.FirstOrDefault(d => d.Name == name);
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task<DarstellungsObjektList> GetDarstellungsObjektListe(Gerät gerät)
    {
      await mSemaphore.WaitAsync();
      try
      {
        return mGeräte.First(g => g.Name == gerät.Name).DarstellungsObjekte;
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task LöscheDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      await mSemaphore.WaitAsync();
      try
      {
        mGeräte.First(g => g.Name == gerät.Name).DarstellungsObjekte.Remove(obj);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Fehler beim Löschen der DarstellungsObjekte für Gerät '{gerät.Name}': {ex.Message}");
      }
      finally
      {
        mSemaphore.Release();
      }
    }


    //Aufzeichner
    public async Task AddAufzeichner(Aufzeichner auf)
    {
      await mSemaphore.WaitAsync();
      try
      {
        mAufzeichner.Add(auf);
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task<Aufzeichner?> GetAufzeichner(string name)
    {
      await mSemaphore.WaitAsync();
      try
      {
        return mAufzeichner.FirstOrDefault(d => d.Name == name);
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task<AufzeichnerList> GetAufzeichnerListe()
    {
      await mSemaphore.WaitAsync();
      try
      {
        return mAufzeichner;
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task LöscheAufzeichner(Aufzeichner auf)
    {
      await mSemaphore.WaitAsync();
      try
      {
        mAufzeichner.Remove(auf);
      }
      finally
      {
        mSemaphore.Release();
      }
    }


    //DatenBasisObjekt
    public async Task AddDatenBasisObjekt(Gerät gerät, DatenBasisObjekt dbo)
    {
      await mSemaphore.WaitAsync();
      try
      {
        mGeräte.First(g => g.Name == gerät.Name).DatenBasen.Add(dbo);
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task<DatenBasisObjekt> GetDatenBasisObjekt(Gerät gerät, string name)
    {
      await mSemaphore.WaitAsync();
      try
      {
        return gerät.DatenBasen.FirstOrDefault(d => d.Name == name);
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task<DatenBasis> GetDatenBasis(Gerät gerät)
    {
      await mSemaphore.WaitAsync();
      try
      {
        return mGeräte.First(g => g.Name == gerät.Name).DatenBasen;
      }
      finally
      {
        mSemaphore.Release();
      }
    }

    public async Task LöscheDatenBasis(Gerät gerät, DatenBasisObjekt dbo)
    {
      await mSemaphore.WaitAsync();
      try
      {
        mGeräte.First(g => g.Name == gerät.Name).DatenBasen.Remove(dbo);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Fehler beim Löschen der Datenbasis für Gerät '{gerät.Name}': {ex.Message}");
      }
      finally
      {
        mSemaphore.Release();
      }
    }


    //VerknüpfeAufzeichnerMitDO
    public async Task VerknüpfeAufzeichnerMitDO(Aufzeichner auf, DarstellungsObjekt obj)
    {
      await mSemaphore.WaitAsync();
      try
      {
        if (!auf.DarstellungsObjekte.Contains(obj))
          auf.DarstellungsObjekte.Add(obj);
        if (!obj.Aufzeichner.Contains(auf))
          obj.Aufzeichner.Add(auf);
      }
      finally
      {
        mSemaphore.Release();
      }
    }
  }
}
