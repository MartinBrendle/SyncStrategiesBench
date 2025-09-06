using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AIO.DM;
using AIO.Services;
using AIO.Utils;


namespace ParallelDeviceUI
{
  public partial class CopyTest : Window
  {
    private List<FunktionsMessung> mLogEinträge = new List<FunktionsMessung>();
    private object mLogLock = new object();

    public CopyTest(List<ExperimentConfig> experimente)
    {
      InitializeComponent();
      foreach (ExperimentConfig experiment in experimente)
      {
        //StarteAufgaben(experiment);
        StarteAufgaben();
      }
    }


    public async Task StarteAufgaben2()
    {
      // Parameter für die Grid-Studie
      int[] dmSizes = Enumerable.Range(1, 50).ToArray();
      int[] taskSizes = Enumerable.Range(1, 50).ToArray();
      const int testZyklen = 5; // pro Task (konservativ halten – 50x50 Kombis sind teuer)

      foreach (int dmSize in dmSizes)
      {
        var experiment = new ExperimentConfig
        {
          Name = $"DM_{dmSize}",
          TestZyklen = testZyklen,
          Geraete = dmSize,
          DarstellungsObjekte = dmSize,
          Aufzeichner = dmSize,
          DatenBasisObjekte = dmSize
        };

        foreach (int anzahlTasks in taskSizes)
        {
          // Für jede Kombination eigenes Middleware-Exemplar + DM erzeugen
          var middleware = new Middleware_Copy();
          middleware.ErzeugeDatenmodell(experiment);

          var tasks = new List<Task>(anzahlTasks);
          for (int i = 0; i < anzahlTasks; i++)
          {
            int taskNummer = i;

            tasks.Add(Task.Run(async () =>
            {
              for (int j = 0; j < experiment.TestZyklen; j++)
              {
                string gerätName = $"Gerät_T{taskNummer}_#{j}";
                string aufzeichnerName = $"Auf_T{taskNummer}_#{j}";
                string darstellungsobjName = $"DO_T{taskNummer}_#{j}";
                string datenbasisName = $"DB_T{taskNummer}_#{j}";

                var (g, auf, d, db) = ErzeugeLAufzeitObjekte(
                  gerätName, aufzeichnerName, darstellungsobjName, datenbasisName);

                long start = Stopwatch.GetTimestamp();
                await MesseGesamtzeit(middleware, g, auf, d, db);
                long end = Stopwatch.GetTimestamp();

                double ns = (end - start) * (1_000_000_000.0 / Stopwatch.Frequency);
                lock (mLogLock)
                {
                  mLogEinträge.Add(new FunktionsMessung
                  {
                    Aktion = "Performance_3D",
                    DauerNs = ns,
                    AnzahlTasks = anzahlTasks,
                    DmSize = dmSize   // << wichtig für 3D-Export
                  });
                }
              }
            }));
          }

          await Task.WhenAll(tasks);
        }
      }

      // Export
      string basis = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "Messungen");
      Directory.CreateDirectory(basis);

      string pfad = Path.Combine(basis, "CopyTest_Tasks_x_DM_3D.xlsx");
      var exporter = new ExcelExporter();
      exporter.Exportiere3D(mLogEinträge, pfad);
    }


    public async Task StarteAufgaben()
    {
      int[] dmSteps = Enumerable.Range(1, 200).ToArray();

      foreach (int dmSize in dmSteps)
      {
        ExperimentConfig experiment = new ExperimentConfig()
        {
          Name = "DM_" + dmSize,
          TestZyklen = 5,
          Geraete = dmSize,
          DarstellungsObjekte = dmSize,
          Aufzeichner = dmSize,
          DatenBasisObjekte = dmSize
        };

        Middleware_Copy middleware = new Middleware_Copy();
        middleware.ErzeugeDatenmodell(experiment);
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < 20; i++) // 20 Tasks 
        {
          int taskNummer = i;
          TextBox textBox = null;

          tasks.Add(Task.Run(async () =>
          {
            for (int j = 0; j < experiment.TestZyklen; j++)
            {
              string gerätName = $"Gerät_T{taskNummer}_#{j}";
              string aufzeichnerName = $"Auf_T{taskNummer}_#{j}";
              string darstellungsobjektName = $"DO_T{taskNummer}_#{j}";
              string datenbasisName = $"DB_T{taskNummer}_#{j}";

              var (g, auf, d, db) = ErzeugeLAufzeitObjekte(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName);

              long start = Stopwatch.GetTimestamp();
              //await MesseAktionDauer(dmSize, middleware, g, auf, d, db);
              await MesseGesamtzeit(middleware, g, auf, d, db);
              long end = Stopwatch.GetTimestamp();

              long ns = (long)((end - start) * (1_000_000_000.0 / Stopwatch.Frequency));
              lock (mLogLock)
              {
                mLogEinträge.Add(new FunktionsMessung
                {
                  Aktion = "Performance_Overview",
                  DauerNs = ns,
                  AnzahlTasks = dmSize,
                });
              }
            }
          }));
        }
        await Task.WhenAll(tasks);
      }

      string pfad = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Messungen", "Only_Geraete_DM_Size_AIO_CopyTest.xlsx");
      ExcelExporter excelExporter = new ExcelExporter();
      excelExporter.Exportiere2(mLogEinträge, pfad);
    }



    public async Task StarteAufgaben(ExperimentConfig experiment)
    {
      int[] taskSteps = Enumerable.Range(1, 50).ToArray();

      foreach (int anzahlTasks in taskSteps)
      {
        List<Task> tasks = new List<Task>();
        Middleware_Copy middleware = new Middleware_Copy();
        middleware.ErzeugeDatenmodell(experiment);

        for (int i = 0; i < anzahlTasks; i++)
        {
          int taskNummer = i;
          TextBox textBox = null;

          tasks.Add(Task.Run(async () =>
          {
            for (int j = 0; j < experiment.TestZyklen; j++)
            {
              string gerätName = $"Gerät_T{taskNummer}_#{j}";
              string aufzeichnerName = $"Auf_T{taskNummer}_#{j}";
              string darstellungsobjektName = $"DO_T{taskNummer}_#{j}";
              string datenbasisName = $"DB_T{taskNummer}_#{j}";

              var (g, auf, d, db) = ErzeugeLAufzeitObjekte(gerätName, aufzeichnerName, darstellungsobjektName, datenbasisName);

              long start = Stopwatch.GetTimestamp();
              //await MesseAktionDauer(anzahlTasks, middleware, g, auf, d, db);
              await MesseGesamtzeit(middleware, g, auf, d, db);

              long end = Stopwatch.GetTimestamp();

              long ns = (long)((end - start) * (1_000_000_000.0 / Stopwatch.Frequency));
              lock (mLogLock)
              {
                mLogEinträge.Add(new FunktionsMessung
                {
                  Aktion = "Performance_Overview",
                  DauerNs = ns,
                  AnzahlTasks = anzahlTasks,
                });
              }

            }
          }));
        }
        await Task.WhenAll(tasks);
      }


      string pfad = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Messungen", experiment.Name + "_AIO_CopyTest.xlsx");
      ExcelExporter excelExporter = new ExcelExporter();
      excelExporter.Exportiere(mLogEinträge, pfad);
    }


    private async Task MesseAktionDauer(int anzahlTasks, IMiddleware middleware, Gerät g, Aufzeichner auf, DarstellungsObjekt d, DatenBasisObjekt db)
    {
      async Task MesseAsync(string aktion, Func<Task> action)
      {
        long start = Stopwatch.GetTimestamp();
        await action();
        long end = Stopwatch.GetTimestamp();
        long ns = (long)((end - start) * (1_000_000_000.0 / Stopwatch.Frequency));

        lock (mLogLock)
        {
          mLogEinträge.Add(new FunktionsMessung
          {
            Aktion = aktion,
            DauerNs = ns,
            AnzahlTasks = anzahlTasks,
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


    private void Append(TextBox box, string text)
    {
      Dispatcher.Invoke(() =>
      {
        box.AppendText($"{DateTime.Now:HH:mm:ss.fff} {text}\n");
        box.ScrollToEnd();
      });
    }

    private TextBox ErzeugeTextBox(int nummer)
    {
      return new TextBox
      {
        Name = $"TaskTextBox{nummer}",
        Height = 150,
        Margin = new Thickness(5),
        IsReadOnly = true,
        TextWrapping = TextWrapping.Wrap,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
      };
    }
  }
}
