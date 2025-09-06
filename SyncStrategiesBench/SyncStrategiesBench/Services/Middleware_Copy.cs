using AIO.DM;
using AIO.Utils;
using System.Collections.Concurrent;

namespace AIO.Services
{
  public class Middleware_Copy : IMiddleware
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

    public async Task<Guid> ErzeugeDatenmodellCopy()
    {
      var kopieG = new GeräteListe();
      var kopieA = new AufzeichnerList();
      await mSemaphore.WaitAsync();
      try
      {

        foreach (var g in mGeräte)
          kopieG.Add(KopiereGerät(g));

        foreach (var a in mAufzeichner)
          kopieA.Add(KopiereAufzeichner(a));

        var id = Guid.NewGuid();
        mKopien[id] = (kopieG, kopieA);
        return id;
      }
      finally
      {
        mSemaphore.Release();
      }
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

          mGeräte = neueGeräteListe; //Aktuell Last Writer Wins 

          var neueAufzeichnerListe = new AufzeichnerList();
          foreach (var a in kopie.Aufzeichner)
          {
            neueAufzeichnerListe.Add(KopiereAufzeichner(a));
          }

          mAufzeichner = neueAufzeichnerListe; //Aktuell Last Writer Wins 
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
        DarstellungsObjekte =
          new DarstellungsObjektList(original.DarstellungsObjekte.Select(d => new DarstellungsObjekt(d.Name)).ToList())
      };
    }

    private Aufzeichner KopiereAufzeichner(Aufzeichner original)
    {
      return new Aufzeichner(original.Name);
    }


    // Verknüpfung
    public Task VerknüpfeAufzeichnerMitDO(Aufzeichner auf, DarstellungsObjekt obj)
    {
      if (!auf.DarstellungsObjekte.Contains(obj))
        auf.DarstellungsObjekte.Add(obj);

      if (!obj.Aufzeichner.Contains(auf))
        obj.Aufzeichner.Add(auf);

      return Task.CompletedTask;
    }

    // Gerät
    public async Task AddGerät(Gerät gerät)
    {
      Guid id = await ErzeugeDatenmodellCopy();

      if (mKopien.TryGetValue(id, out var k))
        k.Geräte.Add(gerät);

      await Synchronize(id);
    }

    public async Task<Gerät?> GetGerät(string name)
    {
      Guid id = await ErzeugeDatenmodellCopy();

      if (mKopien.TryGetValue(id, out var k))
      {
        await Synchronize(id);
        return k.Geräte.FirstOrDefault(g => g.Name == name);
      }
      await Synchronize(id);
      return null;
    }

    public async Task<GeräteListe?> GetGerätListe()
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        await Synchronize(id);
        return k.Geräte;
      }

      await Synchronize(id);
      return null;
    }

    public async Task LöscheGerät(Gerät gerät)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        await Synchronize(id);
        k.Geräte.Remove(gerät);
      }
    }



    // Darstellungsobjekt
    public async Task AddDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          myGerät.DarstellungsObjekte.Add(obj);
        }
      }
      await Synchronize(id);
    }

    public async Task<DarstellungsObjekt?> GetDarstellungsObjekt(Gerät gerät, string name)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          await Synchronize(id);
          return myGerät.DarstellungsObjekte.FirstOrDefault(d => d.Name == name);
        }
      }
      await Synchronize(id);
      return null;
    }

    public async Task<DarstellungsObjektList> GetDarstellungsObjektListe(Gerät gerät)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          await Synchronize(id);
          return myGerät.DarstellungsObjekte;
        }
      }
      await Synchronize(id);
      return null;
    }

    public async Task LöscheDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          myGerät.DarstellungsObjekte.Remove(obj);
        }
      }
      await Synchronize(id);
    }



    // Aufzeichner
    public async Task AddAufzeichner(Aufzeichner auf)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
        k.Aufzeichner.Add(auf);

      await Synchronize(id);
    }

    public async Task<Aufzeichner?> GetAufzeichner(string name)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        await Synchronize(id);
        return k.Aufzeichner.FirstOrDefault(a => a.Name == name);
      }

      await Synchronize(id);
      return null;
    }

    public async Task<AufzeichnerList> GetAufzeichnerListe()
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        await Synchronize(id);
        return k.Aufzeichner;
      }
      await Synchronize(id);
      return null;
    }

    public async Task LöscheAufzeichner(Aufzeichner auf)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
        k.Aufzeichner.Remove(auf);

      await Synchronize(id);
    }




    // DatenBasis
    public async Task AddDatenBasisObjekt(Gerät gerät, DatenBasisObjekt dbo)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          myGerät.DatenBasen.Add(dbo);
        }
      }

      await Synchronize(id);
    }

    public async Task<DatenBasisObjekt?> GetDatenBasisObjekt(Gerät gerät, string name)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          await Synchronize(id);
          return myGerät.DatenBasen.FirstOrDefault(d => d.Name == name);
        }
      }
      await Synchronize(id);
      return null;
    }

    public async Task<DatenBasis?> GetDatenBasis(Gerät gerät)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          await Synchronize(id);
          return myGerät.DatenBasen;
        }
      }
      await Synchronize(id);
      return null;
    }

    public async Task LöscheDatenBasis(Gerät gerät, DatenBasisObjekt dbo)
    {
      Guid id = await ErzeugeDatenmodellCopy();
      if (mKopien.TryGetValue(id, out var k))
      {
        Gerät myGerät = k.Geräte.FirstOrDefault(g => g == gerät);
        if (myGerät != null)
        {
          myGerät.DatenBasen.Remove(dbo);
        }
      }
      await Synchronize(id);
    }
  }
}
