using AIO.DM;
using AIO.Utils;
using System.Collections.Concurrent;

namespace AIO.Services
{
  public class Middleware_Session_Copy
  {
    public async Task AddGerätDirect(Gerät gerät)
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

    public async Task AddAufzeichnerDirect(Aufzeichner auf)
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


    private GeräteListe mGeräte = new();
    private AufzeichnerList mAufzeichner = new();

    private readonly SemaphoreSlim mSemaphore = new(1, 1);
    private readonly ConcurrentDictionary<Guid, (GeräteListe Geräte, AufzeichnerList Aufzeichner)> mKopien = new();

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
    public Guid ErzeugeDatenmodellCopy()
    {
      var kopieG = new GeräteListe();
      var kopieA = new AufzeichnerList();

      foreach (var g in mGeräte)
        kopieG.Add(KopiereGerät(g));

      foreach (var a in mAufzeichner)
        kopieA.Add(KopiereAufzeichner(a));

      var id = Guid.NewGuid();
      mKopien[id] = (kopieG, kopieA);
      return id;
    }

    public async Task Synchronize(Guid id)
    {
      if (mKopien.TryGetValue(id, out var kopie))
      {
        await mSemaphore.WaitAsync();
        try
        {
          var neueGeräteListe = new GeräteListe();
          foreach (var g in kopie.Geräte)
          {
            neueGeräteListe.Add(KopiereGerät(g));
          }
          mGeräte = neueGeräteListe;//Aktuell Last Writer Wins 

          var neueAufzeichnerListe = new AufzeichnerList();
          foreach (var a in kopie.Aufzeichner)
          {
            neueAufzeichnerListe.Add(KopiereAufzeichner(a));
          }
          mAufzeichner = neueAufzeichnerListe;//Aktuell Last Writer Wins 
        }
        finally
        {
          mSemaphore.Release();
        }
      }
    }

    private Gerät KopiereGerät(Gerät original)
    {
      return new Gerät
      {
        Name = original.Name,
        DatenBasen = new DatenBasis(original.DatenBasen.Select(db => new DatenBasisObjekt(db.Name)).ToList()),
        DarstellungsObjekte = new DarstellungsObjektList(original.DarstellungsObjekte.Select(d => new DarstellungsObjekt(d.Name)).ToList())
      };
    }

    private Aufzeichner KopiereAufzeichner(Aufzeichner original)
    {
      return new Aufzeichner(original.Name);
    }


    // Verknüpfung
    public Task VerknüpfeAufzeichnerMitDO(Guid id, Aufzeichner auf, DarstellungsObjekt obj)
    {
      if (!auf.DarstellungsObjekte.Contains(obj))
        auf.DarstellungsObjekte.Add(obj);

      if (!obj.Aufzeichner.Contains(auf))
        obj.Aufzeichner.Add(auf);

      return Task.CompletedTask;
    }

    // Gerät
    public Task AddGerät(Guid id, Gerät gerät)
    {
      if (mKopien.TryGetValue(id, out var k))
        k.Geräte.Add(gerät);
      return Task.CompletedTask;
    }

    public Task<Gerät?> GetGerät(Guid id, string name)
    {
      if (mKopien.TryGetValue(id, out var k))
        return Task.FromResult(k.Geräte.FirstOrDefault(g => g.Name == name));
      return Task.FromResult<Gerät?>(null);
    }

    public Task<GeräteListe?> GetGerätListe(Guid id)
    {
      if (mKopien.TryGetValue(id, out var k))
        return Task.FromResult(k.Geräte);
      return Task.FromResult<GeräteListe?>(null);
    }

    public Task LöscheGerät(Guid id, Gerät gerät)
    {
      if (mKopien.TryGetValue(id, out var k))
        k.Geräte.Remove(gerät);
      return Task.CompletedTask;
    }




    // Darstellungsobjekt
    public Task AddDarstellungsObjekt(Guid id, Gerät gerät, DarstellungsObjekt obj)
    {
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          myGerät.DarstellungsObjekte.Add(obj);
        }
      }
      return Task.CompletedTask;
    }

    public Task<DarstellungsObjekt?> GetDarstellungsObjekt(Guid id, Gerät gerät, string name)
    {
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          return Task.FromResult(myGerät.DarstellungsObjekte.FirstOrDefault(d => d.Name == name));
        }
      }
      return Task.FromResult<DarstellungsObjekt?>(null);
    }

    public Task<DarstellungsObjektList> GetDarstellungsObjektListe(Guid id, Gerät gerät)
    {
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          return Task.FromResult(myGerät.DarstellungsObjekte);
        }
      }

      return Task.FromResult<DarstellungsObjektList?>(null);
    }

    public Task LöscheDarstellungsObjekt(Guid id, Gerät gerät, DarstellungsObjekt obj)
    {
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          myGerät.DarstellungsObjekte.Remove(obj);
        }
      }

      return Task.CompletedTask;
    }



    // Aufzeichner
    public Task AddAufzeichner(Guid id, Aufzeichner auf)
    {
      if (mKopien.TryGetValue(id, out var k))
        k.Aufzeichner.Add(auf);
      return Task.CompletedTask;
    }

    public Task<Aufzeichner?> GetAufzeichner(Guid id, string name)
    {
      if (mKopien.TryGetValue(id, out var k))
        return Task.FromResult(k.Aufzeichner.FirstOrDefault(a => a.Name == name));
      return Task.FromResult<Aufzeichner?>(null);
    }

    public Task<AufzeichnerList> GetAufzeichnerListe(Guid id)
    {
      if (mKopien.TryGetValue(id, out var k))
        return Task.FromResult(k.Aufzeichner);
      return Task.FromResult<AufzeichnerList?>(null);
    }

    public Task LöscheAufzeichner(Guid id, Aufzeichner auf)
    {
      if (mKopien.TryGetValue(id, out var k))
        k.Aufzeichner.Remove(auf);
      return Task.CompletedTask;
    }




    // DatenBasis
    public Task AddDatenBasisObjekt(Guid id, Gerät gerät, DatenBasisObjekt dbo)
    {
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          myGerät.DatenBasen.Add(dbo);
        }
      }
      return Task.CompletedTask;
    }

    public Task<DatenBasisObjekt?> GetDatenBasisObjekt(Guid id, Gerät gerät, string name)
    {
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          return Task.FromResult(myGerät.DatenBasen.FirstOrDefault(d => d.Name == name));
        }
      }
      return Task.FromResult<DatenBasisObjekt?>(null);
    }

    public Task<DatenBasis?> GetDatenBasis(Guid id, Gerät gerät)
    {
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          return Task.FromResult(myGerät.DatenBasen);
        }
      }

      return Task.FromResult<DatenBasis?>(null);
    }

    public Task LöscheDatenBasis(Guid id, Gerät gerät, DatenBasisObjekt dbo)
    {
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          myGerät.DatenBasen.Remove(dbo);
        }
      }

      return Task.CompletedTask;
    }
  }
}
