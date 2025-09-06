using AIO.DM;
using AIO.Utils;
using System.Threading;

namespace AIO.Services
{
  public class Middleware_Baseline : IMiddleware
  {
    public async Task AddGerätDirect(Gerät gerät)
    {
    }

    public async Task AddAufzeichnerDirect(Aufzeichner auf)
    {
    }

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

        for (int d = 0; d < config.DarstellungsObjekte; d++)
          g.DarstellungsObjekte.Add(new DarstellungsObjekt($"Init_DO_{i}_{d}"));

        mGeräte.Add(g);
      }

      for (int a = 0; a < config.Aufzeichner; a++)
      {
        var auf = new Aufzeichner($"Init_Aufzeichner_{a}");
        mAufzeichner.Add(auf);
      }
    }

    public Task VerknüpfeAufzeichnerMitDO(Aufzeichner auf, DarstellungsObjekt obj)
    {
      if (!auf.DarstellungsObjekte.Contains(obj))
        auf.DarstellungsObjekte.Add(obj);
      if (!obj.Aufzeichner.Contains(auf))
        obj.Aufzeichner.Add(auf);
      return Task.CompletedTask;
    }

    // Gerät

    public Task AddGerät(Gerät gerät)
    {
      mGeräte.Add(gerät);
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
      mGeräte.Remove(gerät);
      return Task.CompletedTask;
    }


    //  DarstellungsObjekt
    public Task AddDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      gerät.DarstellungsObjekte.Add(obj);
      return Task.CompletedTask;
    }

    public Task<DarstellungsObjekt?> GetDarstellungsObjekt(Gerät gerät, string name)
    {
      return Task.FromResult(gerät.DarstellungsObjekte.FirstOrDefault(d => d.Name == name));
    }

    public Task<DarstellungsObjektList> GetDarstellungsObjektListe(Gerät gerät)
    {
      return Task.FromResult(gerät.DarstellungsObjekte);
    }

    public Task LöscheDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      gerät.DarstellungsObjekte.Remove(obj);
      return Task.CompletedTask;
    }

    // Aufzeichner
    public Task AddAufzeichner(Aufzeichner auf)
    {
      mAufzeichner.Add(auf);
      return Task.CompletedTask;
    }

    public Task<Aufzeichner?> GetAufzeichner(string name)
    {
      return Task.FromResult(mAufzeichner.FirstOrDefault(d => d.Name == name));
    }

    public Task<AufzeichnerList> GetAufzeichnerListe()
    {
      return Task.FromResult(mAufzeichner);
    }

    public Task LöscheAufzeichner(Aufzeichner auf)
    {
      mAufzeichner.Remove(auf);
      return Task.CompletedTask;
    }

    // DatenBasis
    public Task AddDatenBasisObjekt(Gerät gerät, DatenBasisObjekt dbo)
    {
      gerät.DatenBasen.Add(dbo);
      return Task.CompletedTask;
    }

    public Task<DatenBasisObjekt?> GetDatenBasisObjekt(Gerät gerät, string name)
    {
      return Task.FromResult(gerät.DatenBasen.FirstOrDefault(d => d.Name == name));
    }

    public Task<DatenBasis?> GetDatenBasis(Gerät gerät)
    {
      return Task.FromResult(gerät.DatenBasen);
    }

    public Task LöscheDatenBasis(Gerät gerät, DatenBasisObjekt dbo)
    {
      gerät.DatenBasen.Remove(dbo);
      return Task.CompletedTask;
    }
  }
}
