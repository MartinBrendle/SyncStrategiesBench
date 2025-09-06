using AIO.DM;
using AIO.Utils;

namespace AIO.Services
{
  public class Middleware_DomainCopy : IMiddleware
  {
    public async Task AddGerätDirect(Gerät gerät)
    {
    }

    public async Task AddAufzeichnerDirect(Aufzeichner auf)
    {
    }


    private readonly SemaphoreSlim mSemaphore = new(1, 1);

    private (GeräteListe Geräte, AufzeichnerList Aufzeichner) GeräteDomain;
    private readonly SemaphoreSlim mGeräteDomainSemaphore = new(1, 1);

    private (GeräteListe Geräte, AufzeichnerList Aufzeichner) AufzeichnerDomain;
    private readonly SemaphoreSlim mAufzeichnerDomainSemaphore = new(1, 1);

    private (GeräteListe Geräte, AufzeichnerList Aufzeichner) DatenBasisDomain;
    private readonly SemaphoreSlim mDatenBasisDomainSemaphore = new(1, 1);

    public void Initialize(ExperimentConfig config)
    {
      GeräteDomain = ErzeugeInstanz(0, config);
      AufzeichnerDomain = ErzeugeInstanz(1, config);
      DatenBasisDomain = ErzeugeInstanz(2, config);
    }


    private (GeräteListe, AufzeichnerList) ErzeugeInstanz(int index, ExperimentConfig config)
    {
      var geraete = new GeräteListe();
      for (int g = 0; g < config.Geraete; g++)
      {
        var gerät = new Gerät
        {
          Name = $"Gerät_{index}_{g}",
          DatenBasen = new DatenBasis(),
          DarstellungsObjekte = new DarstellungsObjektList()
        };

        for (int d = 0; d < config.DatenBasisObjekte; d++)
          gerät.DatenBasen.Add(new DatenBasisObjekt($"DB_{index}_{g}_{d}"));

        geraete.Add(gerät);
      }

      var aufzeichner = new AufzeichnerList();
      for (int a = 0; a < config.Aufzeichner; a++)
      {
        aufzeichner.Add(new Aufzeichner($"Aufzeichner_{index}_{a}"));
      }

      return (geraete, aufzeichner);
    }



    #region API

    // Verknüpfung
    public Task VerknüpfeAufzeichnerMitDO(Aufzeichner auf, DarstellungsObjekt obj)
    {
      if (!auf.DarstellungsObjekte.Contains(obj))
        auf.DarstellungsObjekte.Add(obj);

      if (!obj.Aufzeichner.Contains(auf))
        obj.Aufzeichner.Add(auf);

      return Task.CompletedTask;
    }



    //Gerät
    public async Task AddGerät(Gerät gerät)
    {
      await mGeräteDomainSemaphore.WaitAsync();
      try
      {
        GeräteDomain.Geräte.Add(gerät);
      }
      finally
      {
        mGeräteDomainSemaphore.Release();
      }
    }

    public async Task<Gerät?> GetGerät(string name)
    {
      await mGeräteDomainSemaphore.WaitAsync();
      try
      {
        return GeräteDomain.Geräte.FirstOrDefault(g => g.Name == name);
      }
      finally
      {
        mGeräteDomainSemaphore.Release();
      }
    }

    public async Task<GeräteListe> GetGerätListe()
    {
      await mGeräteDomainSemaphore.WaitAsync();
      try
      {
        return GeräteDomain.Geräte;
      }
      finally
      {
        mGeräteDomainSemaphore.Release();
      }
    }

    public async Task LöscheGerät(Gerät gerät)
    {
      await mGeräteDomainSemaphore.WaitAsync();
      try
      {
        GeräteDomain.Geräte.Remove(gerät);
      }
      finally
      {
        mGeräteDomainSemaphore.Release();
      }
    }


    //Darstellungsobjekt
    public async Task AddDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      await mGeräteDomainSemaphore.WaitAsync();
      try
      {
        GeräteDomain.Geräte.First(g => g.Name == gerät.Name).DarstellungsObjekte.Add(obj);
      }
      finally
      {
        mGeräteDomainSemaphore.Release();
      }
    }

    public async Task<DarstellungsObjekt?> GetDarstellungsObjekt(Gerät gerät, string name)
    {
      await mGeräteDomainSemaphore.WaitAsync();
      try
      {
        return GeräteDomain.Geräte.First(g => g.Name == gerät.Name).DarstellungsObjekte
          .FirstOrDefault(d => d.Name == name);
      }
      finally
      {
        mGeräteDomainSemaphore.Release();
      }
    }

    public async Task<DarstellungsObjektList> GetDarstellungsObjektListe(Gerät gerät)
    {
      await mGeräteDomainSemaphore.WaitAsync();
      try
      {
        return GeräteDomain.Geräte.First(g => g.Name == gerät.Name).DarstellungsObjekte;
      }
      finally
      {
        mGeräteDomainSemaphore.Release();
      }
    }

    public async Task LöscheDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      await mGeräteDomainSemaphore.WaitAsync();
      try
      {
        GeräteDomain.Geräte.First(g => g.Name == gerät.Name).DarstellungsObjekte.Remove(obj);
      }
      finally
      {
        mGeräteDomainSemaphore.Release();
      }
    }


    //Aufzeichner
    public async Task AddAufzeichner(Aufzeichner auf)
    {
      await mAufzeichnerDomainSemaphore.WaitAsync();
      try
      {
        AufzeichnerDomain.Aufzeichner.Add(auf);
      }
      finally
      {
        mAufzeichnerDomainSemaphore.Release();
      }
    }

    public async Task<Aufzeichner?> GetAufzeichner(string name)
    {
      await mAufzeichnerDomainSemaphore.WaitAsync();
      try
      {
        return AufzeichnerDomain.Aufzeichner.FirstOrDefault(d => d.Name == name);
      }
      finally
      {
        mAufzeichnerDomainSemaphore.Release();
      }
    }

    public async Task<AufzeichnerList> GetAufzeichnerListe()
    {
      await mAufzeichnerDomainSemaphore.WaitAsync();
      try
      {
        return AufzeichnerDomain.Aufzeichner;
      }
      finally
      {
        mAufzeichnerDomainSemaphore.Release();
      }
    }

    public async Task LöscheAufzeichner(Aufzeichner auf)
    {
      await mAufzeichnerDomainSemaphore.WaitAsync();
      try
      {
        AufzeichnerDomain.Aufzeichner.Remove(auf);
      }
      finally
      {
        mAufzeichnerDomainSemaphore.Release();
      }
    }


    //DatenBasisObjekt
    public async Task AddDatenBasisObjekt(Gerät gerät, DatenBasisObjekt dbo)
    {
      await mDatenBasisDomainSemaphore.WaitAsync();
      try
      {
        var zielGerät = DatenBasisDomain.Geräte.FirstOrDefault(g => g.Name == gerät.Name);
        if (zielGerät != null)
        {
          zielGerät.DatenBasen.Add(dbo);
        }

      }
      finally
      {
        mDatenBasisDomainSemaphore.Release();
      }
    }

    public async Task<DatenBasisObjekt> GetDatenBasisObjekt(Gerät gerät, string name)
    {
      await mDatenBasisDomainSemaphore.WaitAsync();
      try
      {
        return gerät.DatenBasen.FirstOrDefault(d => d.Name == name);
      }
      finally
      {
        mDatenBasisDomainSemaphore.Release();
      }
    }

    public async Task<DatenBasis> GetDatenBasis(Gerät gerät)
    {
      await mDatenBasisDomainSemaphore.WaitAsync();
      try
      {
        var zielGerät = DatenBasisDomain.Geräte.FirstOrDefault(g => g.Name == gerät.Name);
        if (zielGerät != null)
        {
          return zielGerät.DatenBasen;
        }

        return null;
      }
      finally
      {
        mDatenBasisDomainSemaphore.Release();
      }
    }

    public async Task LöscheDatenBasis(Gerät gerät, DatenBasisObjekt dbo)
    {
      await mDatenBasisDomainSemaphore.WaitAsync();
      try
      {
        var zielGerät = DatenBasisDomain.Geräte.FirstOrDefault(g => g.Name == gerät.Name);
        if (zielGerät != null)
        {
          zielGerät.DatenBasen.Remove(dbo);
        }
      }
      finally
      {
        mDatenBasisDomainSemaphore.Release();
      }
    }

    #endregion
  }
}
