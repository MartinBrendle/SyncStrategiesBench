using AIO.DM;
using AIO.Utils;

namespace AIO.Services
{
  public interface IMiddleware
  {
    Task AddGerätDirect(Gerät gerät);
    Task AddAufzeichnerDirect(Aufzeichner auf);

    // Gerät
    Task AddGerät(Gerät gerät);
    Task<Gerät?> GetGerät(string name);
    Task<GeräteListe> GetGerätListe();
    Task LöscheGerät(Gerät gerät);

    // Darstellungsobjekt
    Task AddDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj);
    Task<DarstellungsObjekt?> GetDarstellungsObjekt(Gerät gerät, string name);
    Task<DarstellungsObjektList> GetDarstellungsObjektListe(Gerät gerät);
    Task LöscheDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj);

    // Aufzeichner
    Task AddAufzeichner(Aufzeichner auf);
    Task<Aufzeichner?> GetAufzeichner(string name);
    Task<AufzeichnerList> GetAufzeichnerListe();
    Task LöscheAufzeichner(Aufzeichner auf);

    // Verknüpfung
    Task VerknüpfeAufzeichnerMitDO(Aufzeichner auf, DarstellungsObjekt obj);

    // DatenBasis
    Task AddDatenBasisObjekt(Gerät gerät, DatenBasisObjekt dbo);
    Task<DatenBasisObjekt?> GetDatenBasisObjekt(Gerät gerät, string name);
    Task<DatenBasis?> GetDatenBasis(Gerät gerät);
    Task LöscheDatenBasis(Gerät gerät, DatenBasisObjekt dbo);
  }
}