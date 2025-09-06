using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AIO.Services;
using AIO.Utils;


namespace ParallelDeviceUI
{
  public partial class SessionCopyTest : Window
  {
    private int AnzahlTasks = 5;
    private int TestZyklen = 5;

    public SessionCopyTest(List<ExperimentConfig> experimente)
    {
      InitializeComponent();
      foreach (ExperimentConfig experiment in experimente)
      {
        StarteAufgaben(experiment);
      }
    }

    private async void StarteAufgaben(ExperimentConfig experiment)
    {
      AnzahlTasks = experiment.AnzahlTasks;
      TestZyklen = experiment.TestZyklen;
      Middleware_Session_Copy middleware = new();
      middleware.ErzeugeDatenmodell(experiment);

      List<FunktionsMessung> logEinträge = new();
      object logLock = new();
      List<Task> tasks = new();

      for (int i = 0; i < AnzahlTasks; i++)
      {
        int taskNummer = i;
        TextBox textBox = null;

        await Dispatcher.InvokeAsync(() =>
        {
          textBox = ErzeugeTextBox(taskNummer);
          TaskPanel.Children.Add(textBox);
        });

        tasks.Add(Task.Run(async () =>
        {
          for (int j = 0; j < TestZyklen; j++)
          {
            string gerätName = $"Gerät_T{taskNummer}_#{j}";
            string aufzeichnerName = $"Auf_T{taskNummer}_#{j}";
            string darstellungsobjektName = $"DO_T{taskNummer}_#{j}";
            string datenbasisName = $"DB_T{taskNummer}_#{j}";

            bool ignoriereMessung = experiment.MesswertvorläufeIgnorieren && j < 1;


            async Task MesseAsync(string aktion, Func<Task> action)
            {
              if (ignoriereMessung)
              {
                await action();
                return;
              }

              long start = Stopwatch.GetTimestamp();
              await action();
              long end = Stopwatch.GetTimestamp();
              long ns = (long)((end - start) * (1_000_000_000.0 / Stopwatch.Frequency));

              lock (logLock)
              {
                logEinträge.Add(new FunktionsMessung
                {
                  Task = taskNummer,
                  Device = gerätName,
                  Aktion = aktion,
                  DauerNs = ns
                });
              }
            }

            MiddlewareMessungenSessionCopy messung = new MiddlewareMessungenSessionCopy(middleware, MesseAsync);


            Guid guid = middleware.ErzeugeDatenmodellCopy();

            //Geräte
            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_AddGerät"));
            await messung.Messe_AddGerät(guid, gerätName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GetGerät"));
            await messung.Messe_GetGerät(guid, gerätName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GetGerätListe"));
            await messung.Messe_GetGerätListe(guid);

            //Aufzeichner
            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_AddAufzeichner"));
            await messung.Messe_AddAufzeichner(guid, aufzeichnerName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GetAufzeichner"));
            await messung.Messe_GetAufzeichner(guid, aufzeichnerName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GetAufzeichnerListe"));
            await messung.Messe_GetAufzeichnerListe(guid);


            //Darstellungsobjekt
            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_AddDarstellungsobjekt"));
            await messung.Messe_AddDarstellungsobjekt(guid, gerätName, darstellungsobjektName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GetDarstellungsobjekt"));
            await messung.Messe_GetDarstellungsobjekt(guid, gerätName, darstellungsobjektName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GetDarstellungsobjektListe"));
            await messung.Messe_GetDarstellungsobjektListe(guid, gerätName);


            //Aufzeichner x DarstellungsObjekt
            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_VerknüpfeAufzeichnerMitDO"));
            await messung.Messe_VerknüpfeAufzeichnerMitDO(guid, gerätName, darstellungsobjektName, aufzeichnerName);


            //DatenBasis
            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_AddDatenbasis"));
            await messung.Messe_AddDatenbasis(guid, gerätName, datenbasisName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GetDatenbasisObjekt"));
            await messung.Messe_GetDatenbasisObjekt(guid, gerätName, datenbasisName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GetDatenbasis"));
            await messung.Messe_GetDatenbasis(guid, gerätName);

            //Löschen
            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_LöscheDatenbasis"));
            await messung.Messe_LöscheDatenbasis(guid, gerätName, datenbasisName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_GMesse_LöscheDarstellungsobjektetGerät"));
            await messung.Messe_LöscheDarstellungsobjekt(guid, gerätName, darstellungsobjektName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_LöscheAufzeichner"));
            await messung.Messe_LöscheAufzeichner(guid, aufzeichnerName);

            await Dispatcher.InvokeAsync(() => Append(textBox, "Messe_LöscheGerät"));
            await messung.Messe_LöscheGerät(guid, gerätName);
          }
        }));
      }

      await Task.WhenAll(tasks);


      string pfad = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        experiment.Name + "_AIO_SessionCopyTest.xlsx");
      ExcelExporter excelExporter = new ExcelExporter();
      excelExporter.Exportiere(logEinträge, pfad);
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
