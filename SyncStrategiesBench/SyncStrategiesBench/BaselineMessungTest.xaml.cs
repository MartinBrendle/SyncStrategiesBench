using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Input;
using AIO.DM;
using AIO.Services;
using AIO.Utils;

namespace ParallelDeviceUI
{
  public partial class BaselineMessungTest : Window
  {
    private List<FunktionsMessung> mLogEinträge = new List<FunktionsMessung>();
    private object mLogLock = new object();

    public BaselineMessungTest(List<ExperimentConfig> experimente)
    {
      InitializeComponent();
      foreach (var experiment in experimente)
      {

        string name = experiment.Name;
        for (int n = 1; n <= 1; n++)
        {
          // Experiment-Name anpassen
          experiment.Name = $"{n}{name}";

          // StarteAufgaben ausführen
          //StarteAufgaben(experiment);
          StarteAufgaben();
        }
      }
    }



    public async Task StarteAufgaben()
    {
      int[] dmSteps = Enumerable.Range(1, 200).ToArray();

      foreach (int dmSize in dmSteps)
      {
        ExperimentConfig experiment = new ExperimentConfig()
        {
          Name = "DM_" + dmSize,
          TestZyklen = 10,
          Geraete = dmSize,
          DarstellungsObjekte = dmSize,
          Aufzeichner = dmSize,
          DatenBasisObjekte = dmSize
        };
        Middleware_Baseline middleware = new Middleware_Baseline();
        middleware.ErzeugeDatenmodell(experiment);

        for (int j = 0; j < 50; j++)
        {
          string gerätName = $"Gerät_T1_#{j}";
          string aufzeichnerName = $"Auf_T1#{j}";
          string darstellungsobjektName = $"DO_T1_#{j}";
          string datenbasisName = $"DB_T1_#{j}";

          aufwaermen(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName, middleware);
          aufwaermen(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName, middleware);
          aufwaermen(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName, middleware);

          var (g, auf, d, db) = ErzeugeLAufzeitObjekte(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName);

          long start = Stopwatch.GetTimestamp();
          //await MesseAktionDauer(anzahlZyklen, middleware, g, auf, d, db);
          await MesseGesamtzeit(middleware, g, auf, d, db);
          long end = Stopwatch.GetTimestamp();


          long ns = (long)((end - start) * (1_000_000_000.0 / Stopwatch.Frequency));
          lock (mLogLock)
          {
            mLogEinträge.Add(new FunktionsMessung
            {
              Aktion = "Performance_Overview",
              DauerNs = ns,
              AnzahlZyklen = dmSize,
            });
          }

        }
      }

      string pfad = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Messungen","DM_1_Bis_50_Baseline.xlsx");
      ExcelExporter excelExporter = new ExcelExporter();
      excelExporter.ExportiereBaseLine(mLogEinträge, pfad);
    }
    
    public async Task StarteAufgaben(ExperimentConfig experiment)
    {

      Middleware_Baseline middleware = new Middleware_Baseline();
      middleware.ErzeugeDatenmodell(experiment);
      // int[] zyklenSteps = Enumerable.Range(1, 50).ToArray();
      int[] zyklenSteps = { 50 };

      foreach (int anzahlZyklen in zyklenSteps)
      {
        for (int j = 0; j < anzahlZyklen; j++)
        {
          string gerätName = $"Gerät_T1_#{j}";
          string aufzeichnerName = $"Auf_T1#{j}";
          string darstellungsobjektName = $"DO_T1_#{j}";
          string datenbasisName = $"DB_T1_#{j}";

          aufwaermen(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName, middleware );
          aufwaermen(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName, middleware );
          aufwaermen(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName, middleware );

          var (g, auf, d, db) = ErzeugeLAufzeitObjekte(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName);

          long start = Stopwatch.GetTimestamp();
          //await MesseAktionDauer(anzahlZyklen, middleware, g, auf, d, db);
          await MesseGesamtzeit(middleware, g, auf, d, db);
          long end = Stopwatch.GetTimestamp();


          long ns = (long)((end - start) * (1_000_000_000.0 / Stopwatch.Frequency));
          lock (mLogLock)
          {
            mLogEinträge.Add(new FunktionsMessung
            {
              Aktion = "Performance_Overview",
              DauerNs = ns,
              AnzahlZyklen = anzahlZyklen,
            });
          }

        }
      }

      string pfad = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Messungen", experiment.Name + "_Baseline.xlsx");
      ExcelExporter excelExporter = new ExcelExporter();
      excelExporter.ExportiereBaseLine(mLogEinträge, pfad);
    }




    private async Task MesseAktionDauer(int anzahlZyklen, IMiddleware middleware, Gerät g, Aufzeichner auf, DarstellungsObjekt d, DatenBasisObjekt db)
    {
      async Task MesseAsync(string aktion, Func<Task> action)
      {

        long start = Stopwatch.GetTimestamp();
        await action();
        long end = Stopwatch.GetTimestamp();
        long deltaTicks = end - start;
        double ns = deltaTicks * (1_000_000_000.0 / Stopwatch.Frequency);

        lock (mLogLock)
        {
          mLogEinträge.Add(new FunktionsMessung
          {
            Aktion = aktion,
            DauerNs = ns,
            AnzahlZyklen = anzahlZyklen,
          });
        }
      }

      await MesseAsync("AddGerät", () => middleware.AddGerät(g));
      await MesseAsync("GetGerät", () => middleware.GetGerät(g.Name));
      await MesseAsync("GetGerätListe", () => middleware.GetGerätListe());

      await MesseAsync("AddAufzeichner", () => middleware.AddAufzeichner(auf));
      await MesseAsync("GetAufzeichner", () => middleware.GetAufzeichner(auf.Name));
      await MesseAsync("GetAufzeichnerListe", () => middleware.GetAufzeichnerListe());

      await MesseAsync("AddDarstellungsObjekt", () => middleware.AddDarstellungsObjekt(g, d));
      await MesseAsync("GetDarstellungsObjekt", () => middleware.GetDarstellungsObjekt(g, d.Name));
      await MesseAsync("GetDarstellungsObjektListe", () => middleware.GetDarstellungsObjektListe(g));

      await MesseAsync("VerknüpfeAufzeichnerMitDO", () => middleware.VerknüpfeAufzeichnerMitDO(auf, d));

      await MesseAsync("AddDatenBasisObjekt", () => middleware.AddDatenBasisObjekt(g, db));
      await MesseAsync("GetDatenBasisObjekt", () => middleware.GetDatenBasisObjekt(g, db.Name));
      await MesseAsync("GetDatenBasis", () => middleware.GetDatenBasis(g));

      await MesseAsync("LöscheDatenBasis", () => middleware.LöscheDatenBasis(g, db));
      await MesseAsync("LöscheDarstellungsObjekt", () => middleware.LöscheDarstellungsObjekt(g, d));
      await MesseAsync("LöscheAufzeichner", () => middleware.LöscheAufzeichner(auf));
      await MesseAsync("LöscheGerät", () => middleware.LöscheGerät(g));
    }

    private async Task MesseGesamtzeit(IMiddleware middleware, Gerät g, Aufzeichner auf,
      DarstellungsObjekt d, DatenBasisObjekt db)
    {
      await middleware.AddGerät(g);
      await middleware.GetGerät(g.Name);
      await middleware.GetGerätListe();

      await middleware.AddAufzeichner(auf);
      await middleware.GetAufzeichner(auf.Name);
      await middleware.GetAufzeichnerListe();

      await middleware.AddDarstellungsObjekt(g, d);
      await middleware.GetDarstellungsObjekt(g, d.Name);
      await middleware.GetDarstellungsObjektListe(g);

      await middleware.VerknüpfeAufzeichnerMitDO(auf, d);

      await middleware.AddDatenBasisObjekt(g, db);
      await middleware.GetDatenBasisObjekt(g, db.Name);
      await middleware.GetDatenBasis(g);

      await middleware.LöscheDatenBasis(g, db);
      await middleware.LöscheDarstellungsObjekt(g, d);
      await middleware.LöscheAufzeichner(auf);
      await middleware.LöscheGerät(g);
    }

    (Gerät g, Aufzeichner auf, DarstellungsObjekt d, DatenBasisObjekt db) ErzeugeLAufzeitObjekte(string gerätName,
      string aufzeichnerName, string darstellungsobjektName, string datenbasisName)
    {
      var g = new Gerät
      {
        Name = gerätName,
        DatenBasen = new DatenBasis
        {
          new DatenBasisObjekt("DB1")
        },
        DarstellungsObjekte = new DarstellungsObjektList
        {
          new DarstellungsObjekt("DO1")
        }
      };
      var auf = new Aufzeichner(aufzeichnerName);
      var d = new DarstellungsObjekt(darstellungsobjektName);
      var db = new DatenBasisObjekt(datenbasisName);
      return (g, auf, d, db);
    }







    private void aufwaermen(string gerätName, string aufzeichnerName, string darstellungsobjektName, string datenbasisName, Middleware_Baseline middleware)
    {

      var g = new Gerät
      {
        Name = gerätName,
        DatenBasen = new DatenBasis
        {
          new DatenBasisObjekt("DB1")
        },
        DarstellungsObjekte = new DarstellungsObjektList
        {
          new DarstellungsObjekt("DO1")
        }
      };

      middleware.AddGerät(g);
      middleware.GetGerät(g.Name);
      middleware.GetGerätListe();

      var auf = new Aufzeichner(aufzeichnerName);
      middleware.AddAufzeichner(auf);
      middleware.GetAufzeichner(aufzeichnerName);
      middleware.GetAufzeichnerListe();

      var d = new DarstellungsObjekt(darstellungsobjektName);
      middleware.AddDarstellungsObjekt(g, d);
      middleware.GetDarstellungsObjekt(g, darstellungsobjektName);
      middleware.GetDarstellungsObjektListe(g);

      middleware.VerknüpfeAufzeichnerMitDO(auf, d);

      var db = new DatenBasisObjekt(datenbasisName);
      middleware.AddDatenBasisObjekt(g, db);
      middleware.GetDatenBasisObjekt(g, datenbasisName);
      middleware.GetDatenBasis(g);

      middleware.LöscheDatenBasis(g, db);
      middleware.LöscheDarstellungsObjekt(g, d);
      middleware.LöscheAufzeichner(auf);
      middleware.LöscheGerät(g);
    }

  }
  
}


