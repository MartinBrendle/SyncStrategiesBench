using AIO.DM;
using AIO.Services;

namespace AIO.Utils
{
  public class MiddlewareMessungenSessionCopy
  {
    private readonly Middleware_Session_Copy middleware;
    private readonly Func<string, Func<Task>, Task> MesseAsync;

    public MiddlewareMessungenSessionCopy(Middleware_Session_Copy middleware, Func<string, Func<Task>, Task> messeAsync)
    {
      this.middleware = middleware;
      MesseAsync = messeAsync;
    }

    public async Task Messe_AddGerät(Guid id, string name)
    {
      var g = await ErzeugeGerätAsync(name);
      await MesseAsync("AddGerät", () => middleware.AddGerät(id, g));
    }

    public async Task Messe_GetGerät(Guid id, string name)
    {
      await MesseAsync("GetGerät", () => middleware.GetGerät(id, name));
    }

    public async Task Messe_GetGerätListe(Guid id)
    {
      await MesseAsync("GetGerätListe", () => middleware.GetGerätListe(id));
    }

    public async Task Messe_LöscheGerät(Guid id, string name)
    {
      var g = await middleware.GetGerät(id, name);
      if (g != null)
        await MesseAsync("LöscheGerät", () => middleware.LöscheGerät(id, g));
    }

    public async Task Messe_AddDarstellungsobjekt(Guid id, string gerätName, string doName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      var d = new DarstellungsObjekt(doName);
      if (g != null)
        await MesseAsync("AddDarstellungsObjekt", () => middleware.AddDarstellungsObjekt(id, g, d));
    }

    public async Task Messe_GetDarstellungsobjekt(Guid id, string gerätName, string doName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      if (g != null)
        await MesseAsync("GetDarstellungsObjekt", () => middleware.GetDarstellungsObjekt(id, g, doName));
    }

    public async Task Messe_GetDarstellungsobjektListe(Guid id, string gerätName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      if (g != null)
        await MesseAsync("GetDarstellungsObjektListe", () => middleware.GetDarstellungsObjektListe(id, g));
    }

    public async Task Messe_LöscheDarstellungsobjekt(Guid id, string gerätName, string doName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      if (g != null)
      {
        var d = await middleware.GetDarstellungsObjekt(id, g, doName);
        if (d != null)
          await MesseAsync("LöscheDarstellungsobjekt", () => middleware.LöscheDarstellungsObjekt(id, g, d));
      }
    }

    public async Task Messe_AddAufzeichner(Guid id, string name)
    {
      var auf = new Aufzeichner(name);
      await MesseAsync("AddAufzeichner", () => middleware.AddAufzeichner(id, auf));
    }

    public async Task Messe_GetAufzeichner(Guid id, string name)
    {
      await MesseAsync("GetAufzeichner", () => middleware.GetAufzeichner(id, name));
    }

    public async Task Messe_GetAufzeichnerListe(Guid id)
    {
      await MesseAsync("GetAufzeichnerListe", () => middleware.GetAufzeichnerListe(id));
    }

    public async Task Messe_LöscheAufzeichner(Guid id, string name)
    {
      var auf = await middleware.GetAufzeichner(id, name);
      if (auf != null)
        await MesseAsync("LöscheAufzeichner", () => middleware.LöscheAufzeichner(id, auf));
    }

    public async Task Messe_AddDatenbasis(Guid id, string gerätName, string dbName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      if (g != null)
      {
        var db = new DatenBasisObjekt(dbName);
        await MesseAsync("AddDatenBasisObjekt", () => middleware.AddDatenBasisObjekt(id, g, db));
      }
    }

    public async Task Messe_GetDatenbasisObjekt(Guid id, string gerätName, string dbName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      if (g != null)
        await MesseAsync("GetDatenBasisObjekt", () => middleware.GetDatenBasisObjekt(id, g, dbName));
    }

    public async Task Messe_GetDatenbasis(Guid id, string gerätName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      if (g != null)
        await MesseAsync("GetDatenBasis", () => middleware.GetDatenBasis(id, g));
    }

    public async Task Messe_LöscheDatenbasis(Guid id, string gerätName, string dbName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      if (g != null)
      {
        var db = await middleware.GetDatenBasisObjekt(id, g, dbName);
        if (db != null)
          await MesseAsync("LöscheDatenBasis", () => middleware.LöscheDatenBasis(id, g, db));
      }
    }

    public async Task Messe_VerknüpfeAufzeichnerMitDO(Guid id, string gerätName, string doName, string aufzeichnerName)
    {
      var g = await middleware.GetGerät(id, gerätName);
      var auf = await middleware.GetAufzeichner(id, aufzeichnerName);
      if (g != null)
      {
        var d = await middleware.GetDarstellungsObjekt(id, g, doName);
        if (d != null && auf != null)
          await MesseAsync("VerknüpfeAufzeichnerMitDO", () => middleware.VerknüpfeAufzeichnerMitDO(id, auf, d));
      }
    }

    private Task<Gerät> ErzeugeGerätAsync(string name)
    {
      var g = new Gerät
      {
        Name = name,
        DatenBasen = new DatenBasis
            {
                new DatenBasisObjekt("DB1")
            },
        DarstellungsObjekte = new DarstellungsObjektList
            {
                new DarstellungsObjekt("DO1")
            }
      };
      return Task.FromResult(g);
    }
  }

}
