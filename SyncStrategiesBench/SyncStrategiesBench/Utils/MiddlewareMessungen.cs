using AIO.DM;
using AIO.Services;

namespace AIO.Utils
{
    public class MiddlewareMessungen
    {
        private readonly IMiddleware middleware;
        private readonly Func<string, Func<Task>, Task> MesseAsync;

        public MiddlewareMessungen(IMiddleware middleware, Func<string, Func<Task>, Task> messeAsync)
        {
            this.middleware = middleware;
            MesseAsync = messeAsync;
        }

        public async Task Messe_AddGerät(string name)
        {
            var g = await ErzeugeGerätAsync(name);
            await MesseAsync("AddGerät", () => middleware.AddGerät(g));
        }

        public async Task Messe_GetGerät(string name)
        {
            await MesseAsync("GetGerät", () => middleware.GetGerät(name));
        }

        public async Task Messe_GetGerätListe()
        {
            await MesseAsync("GetGerätListe", () => middleware.GetGerätListe());
        }

        public async Task Messe_LöscheGerät(string name)
        {
            var g = await middleware.GetGerät(name);
            if (g != null)
                await MesseAsync("LöscheGerät", () => middleware.LöscheGerät(g));
            else
            {
              Gerät tempGerät = new Gerät();
              await middleware.AddGerätDirect(tempGerät);
              await MesseAsync("LöscheGerät", () => middleware.LöscheGerät(tempGerät));
            }
        }

        public async Task Messe_AddDarstellungsobjekt(string gerätName, string doName)
        {
            var g = await middleware.GetGerät(gerätName);
            var d = new DarstellungsObjekt(doName);
            if (g != null)
                await MesseAsync("AddDarstellungsObjekt", () => middleware.AddDarstellungsObjekt(g, d));
            else
            {
              Gerät tempGerät = new Gerät() { DarstellungsObjekte = new DarstellungsObjektList() };
              DarstellungsObjekt tempDarstellungsObjekt = new DarstellungsObjekt("DummyDO");
              await middleware.AddGerätDirect(tempGerät);
              await MesseAsync("AddDarstellungsObjekt", () => middleware.AddDarstellungsObjekt(tempGerät, tempDarstellungsObjekt));
            }
        }

        public async Task Messe_GetDarstellungsobjekt(string gerätName, string doName)
        {
          var g = await middleware.GetGerät(gerätName);
          if (g != null)
            await MesseAsync("GetDarstellungsObjekt", () => middleware.GetDarstellungsObjekt(g, doName));
          else
          {
            Gerät tempGerät = new Gerät() { DarstellungsObjekte = new DarstellungsObjektList() };
            DarstellungsObjekt tempDarstellungsObjekt = new DarstellungsObjekt("DummyDO");
            tempGerät.DarstellungsObjekte.Add(tempDarstellungsObjekt);
            await middleware.AddGerätDirect(tempGerät);
            await MesseAsync("GetDarstellungsObjekt", () => middleware.GetDarstellungsObjekt(tempGerät, "DummyDO"));
          }
        }

        public async Task Messe_GetDarstellungsobjektListe(string gerätName)
        {
          var g = await middleware.GetGerät(gerätName);
          if (g != null)
            await MesseAsync("GetDarstellungsObjektListe", () => middleware.GetDarstellungsObjektListe(g));
          else
          {
            Gerät tempGerät = new Gerät() { DarstellungsObjekte = new DarstellungsObjektList() };
            DarstellungsObjekt tempDarstellungsObjekt = new DarstellungsObjekt("DummyDO");
            tempGerät.DarstellungsObjekte.Add(tempDarstellungsObjekt);
            await middleware.AddGerätDirect(tempGerät);
            await MesseAsync("GetDarstellungsObjektListe", () => middleware.GetDarstellungsObjektListe(tempGerät));
          }
        }

        public async Task Messe_LöscheDarstellungsobjekt(string gerätName, string doName)
        {
          var g = await middleware.GetGerät(gerätName);
          var d = await middleware.GetDarstellungsObjekt(g, doName);
          if (g != null && d != null)
          {
            await MesseAsync("LöscheDarstellungsobjekt", () => middleware.LöscheDarstellungsObjekt(g, d));
          }
          else
          {
            Gerät tempGerät = new Gerät() { DarstellungsObjekte = new DarstellungsObjektList() };
            DarstellungsObjekt tempDarstellungsObjekt = new DarstellungsObjekt("DummyDO");
            tempGerät.DarstellungsObjekte.Add(tempDarstellungsObjekt);
            await middleware.AddGerätDirect(tempGerät);
            await MesseAsync("LöscheDarstellungsobjekt", () => middleware.LöscheDarstellungsObjekt(tempGerät, tempDarstellungsObjekt));
          }
        }

        public async Task Messe_AddAufzeichner(string name)
        {
            var auf = new Aufzeichner(name);
            await MesseAsync("AddAufzeichner", () => middleware.AddAufzeichner(auf));
        }

        public async Task Messe_GetAufzeichner(string name)
        {
            await MesseAsync("GetAufzeichner", () => middleware.GetAufzeichner(name));
        }

        public async Task Messe_GetAufzeichnerListe()
        {
            await MesseAsync("GetAufzeichnerListe", () => middleware.GetAufzeichnerListe());
        }

        public async Task Messe_LöscheAufzeichner(string name)
        {
          var auf = await middleware.GetAufzeichner(name);
          if (auf != null)
            await MesseAsync("LöscheAufzeichner", () => middleware.LöscheAufzeichner(auf));
          else
          {
            Aufzeichner tempAufzeichner = new Aufzeichner("Dummy");
            await middleware.AddAufzeichnerDirect(tempAufzeichner);
            await MesseAsync("LöscheAufzeichner", () => middleware.LöscheAufzeichner(tempAufzeichner));
          }
        }

        public async Task Messe_AddDatenbasis(string gerätName, string dbName)
        {
          var g = await middleware.GetGerät(gerätName);
          if (g != null)
          {
            var db = new DatenBasisObjekt(dbName);
            await MesseAsync("AddDatenBasisObjekt", () => middleware.AddDatenBasisObjekt(g, db));
          }
          else
          {
            Gerät tempGerät = new Gerät() { DatenBasen = new DatenBasis() };
            DatenBasisObjekt tempDatenBasisObjekt = new DatenBasisObjekt("DummyDB");
            await middleware.AddGerätDirect(tempGerät);
            await MesseAsync("AddDatenBasisObjekt", () => middleware.AddDatenBasisObjekt(tempGerät, tempDatenBasisObjekt));
          }
        }

        public async Task Messe_GetDatenbasisObjekt(string gerätName, string dbName)
        {
            var g = await middleware.GetGerät(gerätName);
            if (g != null)
                await MesseAsync("GetDatenBasisObjekt", () => middleware.GetDatenBasisObjekt(g, dbName));
            else
            {
              Gerät tempGerät = new Gerät() { DatenBasen = new DatenBasis() };
              DatenBasisObjekt tempDatenBasisObjekt = new DatenBasisObjekt("DummyDB");
              tempGerät.DatenBasen.Add(tempDatenBasisObjekt);
              await middleware.AddGerätDirect(tempGerät);
              await MesseAsync("GetDatenBasisObjekt", () => middleware.GetDatenBasisObjekt(tempGerät, "DummyDB"));
            }
        }

        public async Task Messe_GetDatenbasis(string gerätName)
        {
          var g = await middleware.GetGerät(gerätName);
          if (g != null)
            await MesseAsync("GetDatenBasis", () => middleware.GetDatenBasis(g));
          else
          {
            Gerät tempGerät = new Gerät() { DatenBasen = new DatenBasis() };
            DatenBasisObjekt tempDatenBasisObjekt = new DatenBasisObjekt("DummyDB");
            tempGerät.DatenBasen.Add(tempDatenBasisObjekt);
            await middleware.AddGerätDirect(tempGerät);
            await MesseAsync("GetDatenBasis", () => middleware.GetDatenBasis(tempGerät));
          }
        }

        public async Task Messe_LöscheDatenbasis(string gerätName, string dbName)
        {
          var g = await middleware.GetGerät(gerätName);
          var db = await middleware.GetDatenBasisObjekt(g, dbName);
          if (g != null && db != null)
          {
            await MesseAsync("LöscheDatenBasis", () => middleware.LöscheDatenBasis(g, db));
          }
          else
          {
            Gerät tempGerät = new Gerät() { DatenBasen = new DatenBasis() };
            DatenBasisObjekt tempDatenBasisObjekt = new DatenBasisObjekt("DummyDB");
            tempGerät.DatenBasen.Add(tempDatenBasisObjekt);
            await middleware.AddGerätDirect(tempGerät);
            await MesseAsync("LöscheDatenBasis", () => middleware.LöscheDatenBasis(tempGerät, tempDatenBasisObjekt));
          }
        }

        public async Task Messe_VerknüpfeAufzeichnerMitDO(string gerätName, string doName, string aufzeichnerName)
        {
          var g = await middleware.GetGerät(gerätName);
          var auf = await middleware.GetAufzeichner(aufzeichnerName);
          var d = await middleware.GetDarstellungsObjekt(g, doName);
          if (g != null && auf != null && d != null)
          {
            await MesseAsync("VerknüpfeAufzeichnerMitDO", () => middleware.VerknüpfeAufzeichnerMitDO(auf, d));
          }
          else
          {
            Gerät tempGerät = new Gerät() { DarstellungsObjekte = new DarstellungsObjektList() };
            DarstellungsObjekt tempDarstellungsObjekt = new DarstellungsObjekt("DummyDO");
            tempGerät.DarstellungsObjekte.Add(tempDarstellungsObjekt);

            Aufzeichner tempAufzeichner = new Aufzeichner("Dummy");
            await middleware.AddAufzeichnerDirect(tempAufzeichner);

            await MesseAsync("VerknüpfeAufzeichnerMitDO", () => middleware.VerknüpfeAufzeichnerMitDO(tempAufzeichner, tempDarstellungsObjekt));
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
