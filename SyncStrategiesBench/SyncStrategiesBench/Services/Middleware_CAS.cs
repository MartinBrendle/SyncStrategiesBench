using AIO.DM;
using AIO.Utils;
using System.Threading;

namespace AIO.Services
{
  public class Middleware_CAS : IMiddleware
  {
    public async Task AddGerätDirect(Gerät gerät) { }

    public async Task AddAufzeichnerDirect(Aufzeichner auf) { }


    private GeräteListe mGeräte = new();
    private AufzeichnerList mAufzeichner = new();

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

    public Task AddGerät(Gerät gerät)
    {
      GeräteListe alt, neu;
      do
      {
        alt = mGeräte;
        neu = new GeräteListe(alt) { gerät };
      }
      while (Interlocked.CompareExchange(ref mGeräte, neu, alt) != alt);

      return Task.CompletedTask;
    }

    public Task<Gerät?> GetGerät(string name)
    {
      return Task.FromResult(mGeräte.FirstOrDefault(g => g.Name == name));
    }

    public Task<GeräteListe> GetGerätListe()
    {
      return Task.FromResult(mGeräte);
    }

    public Task LöscheGerät(Gerät gerät)
    {
      GeräteListe alt, neu;
      do
      {
        alt = mGeräte;
        neu = new GeräteListe(alt);
        neu.RemoveAll(g => g.Name == gerät.Name);
      }
      while (Interlocked.CompareExchange(ref mGeräte, neu, alt) != alt);

      return Task.CompletedTask;
    }

    public Task AddDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      GeräteListe alt, neu;
      do
      {
        alt = mGeräte;
        neu = new GeräteListe();
        foreach (var g in alt)
        {
          if (g.Name == gerät.Name)
          {
            var kopie = new Gerät
            {
              Name = g.Name,
              DatenBasen = new DatenBasis(g.DatenBasen),
              DarstellungsObjekte = new DarstellungsObjektList(g.DarstellungsObjekte)
            };
            kopie.DarstellungsObjekte.Add(obj);
            neu.Add(kopie);
          }
          else
          {
            neu.Add(g);
          }
        }
      }
      while (Interlocked.CompareExchange(ref mGeräte, neu, alt) != alt);

      return Task.CompletedTask;
    }

    public Task<DarstellungsObjekt?> GetDarstellungsObjekt(Gerät gerät, string name)
    {
      var echtesGerät = mGeräte.FirstOrDefault(g => g.Name == gerät.Name);
      return Task.FromResult(echtesGerät?.DarstellungsObjekte.FirstOrDefault(d => d.Name == name));
    }

    public Task<DarstellungsObjektList> GetDarstellungsObjektListe(Gerät gerät)
    {
      var echtesGerät = mGeräte.FirstOrDefault(g => g.Name == gerät.Name);
      return Task.FromResult(echtesGerät?.DarstellungsObjekte ?? new DarstellungsObjektList());
    }

    public Task LöscheDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      GeräteListe alt, neu;
      do
      {
        alt = mGeräte;
        neu = new GeräteListe();
        foreach (var g in alt)
        {
          if (g.Name == gerät.Name)
          {
            var kopie = new Gerät
            {
              Name = g.Name,
              DatenBasen = new DatenBasis(g.DatenBasen),
              DarstellungsObjekte = new DarstellungsObjektList(g.DarstellungsObjekte)
            };
            kopie.DarstellungsObjekte.RemoveAll(d => d.Name == obj.Name);
            neu.Add(kopie);
          }
          else
          {
            neu.Add(g);
          }
        }
      }
      while (Interlocked.CompareExchange(ref mGeräte, neu, alt) != alt);

      return Task.CompletedTask;
    }

    public Task AddDatenBasisObjekt(Gerät gerät, DatenBasisObjekt dbo)
    {
      GeräteListe alt, neu;
      do
      {
        alt = mGeräte;
        neu = new GeräteListe();
        foreach (var g in alt)
        {
          if (g.Name == gerät.Name)
          {
            var kopie = new Gerät
            {
              Name = g.Name,
              DatenBasen = new DatenBasis(g.DatenBasen),
              DarstellungsObjekte = new DarstellungsObjektList(g.DarstellungsObjekte)
            };
            kopie.DatenBasen.Add(dbo);
            neu.Add(kopie);
          }
          else
          {
            neu.Add(g);
          }
        }
      }
      while (Interlocked.CompareExchange(ref mGeräte, neu, alt) != alt);

      return Task.CompletedTask;
    }

    public Task<DatenBasisObjekt?> GetDatenBasisObjekt(Gerät gerät, string name)
    {
      var echtesGerät = mGeräte.FirstOrDefault(g => g.Name == gerät.Name);
      return Task.FromResult(echtesGerät?.DatenBasen.FirstOrDefault(d => d.Name == name));
    }

    public Task<DatenBasis> GetDatenBasis(Gerät gerät)
    {
      var echtesGerät = mGeräte.FirstOrDefault(g => g.Name == gerät.Name);
      return Task.FromResult(echtesGerät?.DatenBasen ?? new DatenBasis());
    }

    public Task LöscheDatenBasis(Gerät gerät, DatenBasisObjekt dbo)
    {
      GeräteListe alt, neu;
      do
      {
        alt = mGeräte;
        neu = new GeräteListe();
        foreach (var g in alt)
        {
          if (g.Name == gerät.Name)
          {
            var kopie = new Gerät
            {
              Name = g.Name,
              DatenBasen = new DatenBasis(g.DatenBasen),
              DarstellungsObjekte = new DarstellungsObjektList(g.DarstellungsObjekte)
            };
            kopie.DatenBasen.RemoveAll(d => d.Name == dbo.Name);
            neu.Add(kopie);
          }
          else
          {
            neu.Add(g);
          }
        }
      }
      while (Interlocked.CompareExchange(ref mGeräte, neu, alt) != alt);

      return Task.CompletedTask;
    }

    public Task AddAufzeichner(Aufzeichner auf)
    {
      AufzeichnerList alt, neu;
      do
      {
        alt = mAufzeichner;
        neu = new AufzeichnerList(alt) { auf };
      }
      while (Interlocked.CompareExchange(ref mAufzeichner, neu, alt) != alt);

      return Task.CompletedTask;
    }

    public Task<Aufzeichner?> GetAufzeichner(string name)
    {
      return Task.FromResult(mAufzeichner.FirstOrDefault(a => a.Name == name));
    }

    public Task<AufzeichnerList> GetAufzeichnerListe()
    {
      return Task.FromResult(mAufzeichner);
    }

    public Task LöscheAufzeichner(Aufzeichner auf)
    {
      AufzeichnerList alt, neu;
      do
      {
        alt = mAufzeichner;
        neu = new AufzeichnerList(alt);
        neu.RemoveAll(a => a.Name == auf.Name);
      }
      while (Interlocked.CompareExchange(ref mAufzeichner, neu, alt) != alt);

      return Task.CompletedTask;
    }

    public Task VerknüpfeAufzeichnerMitDO(Aufzeichner auf, DarstellungsObjekt obj)
    {
      if (!auf.DarstellungsObjekte.Contains(obj))
        auf.DarstellungsObjekte.Add(obj);
      if (!obj.Aufzeichner.Contains(auf))
        obj.Aufzeichner.Add(auf);
      return Task.CompletedTask;
    }
  }
}
