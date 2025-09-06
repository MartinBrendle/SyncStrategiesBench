using System.Windows;
using DevExpress.Spreadsheet;
using DevExpress.Spreadsheet.Charts;

namespace AIO.Utils
{
  public class FunktionsMessung
  {
    public int Task { get; set; }
    public string Device { get; set; }
    public int AnzahlZyklen { get; set; }
    public string Aktion { get; set; }
    public double DauerNs { get; set; }        // hier bereits t_sync speichern
    public int AnzahlTasks { get; set; }     // x-Achse im Diagramm
    public int DmSize { get; set; }
  }



  public class ExcelExporter
  {
    private Dictionary<int, int> tOperaProDmSize = new()
    {
      { 1, 1162 },
      { 2, 1340 },
      { 3, 1340 },
      { 4, 1510 },
      { 5, 1678 },
      { 6, 1444 },
      { 7, 1456 },
      { 8, 1726 },
      { 9, 1444 },
      { 10, 1822 },
      { 11, 1736 },
      { 12, 1928 },
      { 13, 1696 },
      { 14, 1850 },
      { 15, 1724 },
      { 16, 1668 },
      { 17, 1784 },
      { 18, 2190 },
      { 19, 1606 },
      { 20, 1662 },
      { 21, 1748 },
      { 22, 1892 },
      { 23, 1804 },
      { 24, 1798 },
      { 25, 1828 },
      { 26, 1954 },
      { 27, 1926 },
      { 28, 2312 },
      { 29, 2008 },
      { 30, 2050 },
      { 31, 2086 },
      { 32, 2092 },
      { 33, 2176 },
      { 34, 2296 },
      { 35, 2244 },
      { 36, 2328 },
      { 37, 2326 },
      { 38, 2406 },
      { 39, 2448 },
      { 40, 2380 },
      { 41, 2422 },
      { 42, 2420 },
      { 43, 2428 },
      { 44, 2364 },
      { 45, 2484 },
      { 46, 2558 },
      { 47, 2660 },
      { 48, 2664 },
      { 49, 2678 },
      { 50, 2700 }
    };

    private Dictionary<string, double> tOperaProAktion = new Dictionary<string, double>() //DM 1000
    {
      { "AddGerät", 80 },
      { "GetGerät", 16228 },
      { "GetGerätListe", 55 },

      { "AddAufzeichner", 69 },
      { "GetAufzeichner", 16429 },
      { "GetAufzeichnerListe",87 },

      { "AddDarstellungsObjekt", 73 },
      { "GetDarstellungsObjekt", 191 },
      { "GetDarstellungsObjektListe", 65 },

      { "VerknüpfeAufzeichnerMitDO", 129 },

      { "AddDatenBasisObjekt", 38 },
      { "GetDatenBasisObjekt", 185 },
      { "GetDatenBasis", 59 },

      { "LöscheDatenBasis", 109 },
      { "LöscheDarstellungsObjekt", 87 },
      { "LöscheAufzeichner", 2113 },
      { "LöscheGerät", 2677 },

      {"Performance_Overview", 34380 }
    };

    //private Dictionary<string, double> tOperaProAktion = new Dictionary<string, double>() //DM 10
    //{
    //  { "AddGerät", 37 },
    //  { "GetGerät", 333 },
    //  { "GetGerätListe", 93 },

    //  { "AddAufzeichner", 57 },
    //  { "GetAufzeichner", 489 },
    //  { "GetAufzeichnerListe",77 },

    //  { "AddDarstellungsObjekt", 58 },
    //  { "GetDarstellungsObjekt", 149 },
    //  { "GetDarstellungsObjektListe", 61 },

    //  { "VerknüpfeAufzeichnerMitDO", 107 },

    //  { "AddDatenBasisObjekt", 26 },
    //  { "GetDatenBasisObjekt", 151 },
    //  { "GetDatenBasis", 55 },

    //  { "LöscheDatenBasis", 59 },
    //  { "LöscheDarstellungsObjekt", 61 },
    //  { "LöscheAufzeichner", 73 },
    //  { "LöscheGerät", 80 },

    //  {"Performance_Overview", 2240 }
  //};

  public void ExportiereBaseLine(List<FunktionsMessung> logEinträge, string dateiname)
    {
      var workbook = new Workbook();
      workbook.BeginUpdate();
      WorkbookCalculationMode prevMode = workbook.Options.CalculationMode;
      workbook.Options.CalculationMode = WorkbookCalculationMode.Manual;

      try
      {
        // einmal gruppieren
        var gruppiert = logEinträge.GroupBy(e => e.Aktion).ToList();

        foreach (var gruppe in gruppiert)
        {
          Worksheet sheet = workbook.Worksheets.Add();
          string sheetName = gruppe.Key.Length > 31 ? gruppe.Key[..31] : gruppe.Key;
          sheet.Name = sheetName;

          // ---------- 1) Rohdaten ----------
          // A: Anzahl Zyklen, B: t_opera (ns)
          var ordered = gruppe.OrderBy(e => e.AnzahlZyklen).ToList();
          int n = ordered.Count;

          var raw = new object[n + 1, 2];
          raw[0, 0] = "Anzahl Zyklen";
          raw[0, 1] = "t_opera (ns)";
          for (int i = 0; i < n; i++)
          {
            raw[i + 1, 0] = ordered[i].AnzahlZyklen;
            raw[i + 1, 1] = (double)ordered[i].DauerNs;
          }
          sheet.Import(raw, 0, 0);
          int lastDataRow = n + 1; // inkl. Header (Zeile 1)

          // ---------- 2) Scatter: Rohpunkte (nur Marker) ----------
          // Y = Anzahl Zyklen (A), X = t_opera (B)
          Chart chart = sheet.Charts.Add(ChartType.ScatterMarkers);
          var sRaw = chart.Series.Add(
            sheet.Range[$"A2:A{lastDataRow}"], // Y
            sheet.Range[$"B2:B{lastDataRow}"]  // X
          );
          sRaw.SeriesName.SetValue("t_opera");
          chart.TopLeftCell = sheet.Cells["D1"];
          chart.BottomRightCell = sheet.Cells["L20"];
          chart.Title.Visible = true;
          chart.Title.SetValue($"⏱️ {gruppe.Key} t_opera");

          // ---------- 3) Mittelwerte/SE je Zyklusgruppe ----------
          // O: Anzahl Zyklen, P: Mittelwert t_opera, Q: Standardfehler t_opera
          sheet.Cells[0, 14].Value = "Anzahl Zyklen";           // O1
          sheet.Cells[0, 15].Value = "Mittelwert t_opera (ns)"; // P1
          sheet.Cells[0, 16].Value = "Standardfehler (ns)";     // Q1

          // Gruppenlängen berechnen (einmal, in Datenreihenfolge)
          var groups = ordered
            .GroupBy(x => x.AnzahlZyklen)
            .OrderBy(g => g.Key)
            .Select(g => new { Zyklus = g.Key, Count = g.Count() })
            .ToList();

          // Zielbereich nicht als Text formatieren
          sheet.Range.FromLTRB(14, 1, 16, groups.Count).NumberFormat = "General";

          int dataStartRow = 2; // erste Datenzeile in A/B
          int meanRow = 2;      // erste Mittelwertzeile in O/P/Q

          foreach (var gInfo in groups)
          {
            int first = dataStartRow;
            int last = dataStartRow + gInfo.Count - 1;

            // O: Anzahl Zyklen
            sheet.Cells[meanRow - 1, 14].Value = gInfo.Zyklus;

            // P: Mittelwert t_opera
            sheet.Cells[meanRow - 1, 15].Formula = $"=AVERAGE(B{first}:B{last})";
            sheet.Cells[meanRow - 1, 15].NumberFormat = "#,##0";

            // Q: Standardfehler = STDEV.S / SQRT(COUNT)
            sheet.Cells[meanRow - 1, 16].Formula = $"=STDEV.S(B{first}:B{last})/SQRT(COUNT(B{first}:B{last}))";
            //sheet.Cells[meanRow - 1, 16].Formula = $"=STDEV.P(B{first}:B{last})/SQRT(COUNT(B{first}:B{last}))";


            sheet.Cells[meanRow - 1, 16].NumberFormat = "#,##0";

            dataStartRow += gInfo.Count;
            meanRow++;
          }
          int lastMeanRow = 1 + groups.Count;

          // ---------- 4) Scatter: Standardfehler je Zyklus ----------
          // Y = Anzahl Zyklen (O), X = Standardfehler (Q)
          Chart meanChart = sheet.Charts.Add(ChartType.ScatterMarkers);
          var sMean = meanChart.Series.Add(
            sheet.Range[$"O3:O{lastMeanRow}"], // Y
            sheet.Range[$"Q3:Q{lastMeanRow}"]  // X
          );
          sMean.SeriesName.SetValue("Standardfehler");
          meanChart.TopLeftCell = sheet.Cells["S1"];
          meanChart.BottomRightCell = sheet.Cells["AA20"];
          meanChart.Title.Visible = true;
          meanChart.Title.SetValue($"⏱️ {gruppe.Key} Standardfehler je Wiederholung");

          // Trendlinie (optional)
          sMean.Trendlines.Add(ChartTrendlineType.Exponential);
          sMean.Trendlines[0].DisplayEquation = true;
        }

        // Entferne evtl. leeres Default-Sheet
        if (workbook.Worksheets.Count > 0 && workbook.Worksheets[0].Name == "Sheet1")
          workbook.Worksheets.RemoveAt(0);

        // einmal berechnen & speichern
        workbook.Calculate();
        workbook.SaveDocument(dateiname, DocumentFormat.Xlsx);
      }
      finally
      {
        workbook.Options.CalculationMode = prevMode;
        //MessageBox.Show($"📊 Excel-Datei wurde gespeichert: {dateiname}",
        //  "Export fertig", MessageBoxButton.OK, MessageBoxImage.Information);
      }
    }

    
    public void Exportiere(List<FunktionsMessung> logEinträge, string dateiname)
    {
      var workbook = new Workbook();
      workbook.BeginUpdate();
      WorkbookCalculationMode prevCalc = workbook.Options.CalculationMode;
      workbook.Options.CalculationMode = WorkbookCalculationMode.Manual;

      try
      {
        // Gruppierung nur einmal
        var gruppiert = logEinträge
            .GroupBy(e => e.Aktion)
            .ToList();

        var meanCharts = new List<(string Aktion, Chart ChartRef)>();
        //var trendLines = new List<(string Aktion, string Function)>(); // optional befüllen (hier Dummy)

        foreach (var gruppe in gruppiert)
        {
          Worksheet sheet = workbook.Worksheets.Add();
          string sheetName = gruppe.Key.Length > 31 ? gruppe.Key.Substring(0, 31) : gruppe.Key;
          sheet.Name = sheetName;

          // -------------------------
          // 1) Rohdaten: A=AnzahlTasks, B=t_gesamt, C=t_opera, D=t_sync
          // t_opera hier als 0 (oder reell, wenn du es schon hast). Falls du t_gesamt->t_sync später abziehst, setze C entsprechend.
          // Tipp: gib t_sync bereits im Log mit; dann sind B/C entbehrlich.
          // -------------------------
          var ordered = gruppe.OrderBy(e => e.AnzahlTasks).ToList();
          int n = ordered.Count;

          // Bulk-Array vorbereiten
          var data = new object[n + 1, 4];
          data[0, 0] = "AnzahlTasks";
          data[0, 1] = "t_gesamt (ns)";
          data[0, 2] = "t_opera (ns)";
          data[0, 3] = "t_sync (ns)";

          // Falls du t_opera pro Aktion separat hast, hier einsetzen; sonst 0 und t_sync=t_gesamt.
          double baselineWert = tOperaProAktion.ContainsKey(gruppe.Key) ? tOperaProAktion[gruppe.Key] : 0;

          for (int i = 0; i < n; i++)
          {
            var e = ordered[i];
            double tGes = e.DauerNs;
            double tOper = baselineWert;
            double tSync = tGes - tOper;

            data[i + 1, 0] = e.AnzahlTasks;
            data[i + 1, 1] = tGes;
            data[i + 1, 2] = tOper;
            data[i + 1, 3] = tSync;
          }

          sheet.Import(data, 0, 0);
          
          //Mittelwert-Tabelle in Q/R, berechnet in Excel
          sheet.Cells[0, 16].Value = "AnzahlTasks";  // Spalte Q
          sheet.Cells[0, 17].Value = "Mittelwert t_sync";  // Spalte R
          sheet.Cells[0, 18].Value = "Standardfehler t_opera";  // Spalte S


          var gruppenNachTask = gruppe
            .OrderBy(x => x.AnzahlTasks)
            .GroupBy(e => e.AnzahlTasks)
            .OrderBy(g => g.Key)
            .ToList();

          int meanRow = 1;
          int excelRowStart = 2; // da 1-basiert und erste Zeile Überschrift
          int dataStartRow = 2;

          foreach (IGrouping<int, FunktionsMessung> g in gruppenNachTask)
          {
            int taskCount = g.Key;
            int skipCount = taskCount * 3; // so viele Einträge überspringen
            int groupSize = g.Count();     // Anzahl Zeilen für diese Taskgröße

            // Excel-Zeilenbereich bestimmen
            int firstDataRow = dataStartRow;                  // erste Datenzeile dieser Gruppe
            int firstUsedRow = firstDataRow + skipCount;      // ab hier wird gemittelt
            if (firstUsedRow > firstDataRow + groupSize - 1)
              firstUsedRow = firstDataRow + groupSize - 1;  // fallback falls zu viel skip

            int lastDataRow = firstDataRow + groupSize - 1;   // letzte Zeile dieser Gruppe

            // AnzahlTasks in Spalte Q
            sheet.Cells[meanRow, 16].Value = taskCount;

            // Mittelwert-Formel in Spalte R
            sheet.Cells[meanRow, 17].Formula = $"=AVERAGE(D{firstUsedRow}:D{lastDataRow})";
            sheet.Cells[meanRow, 17].NumberFormat = "#,##0";

            // Standardfehler in Spalte S
            sheet.Cells[meanRow, 18].Formula = $"=STDEV.P(D{firstUsedRow}:D{lastDataRow})/SQRT(COUNT(D{firstUsedRow}:D{lastDataRow}))"; ;
            sheet.Cells[meanRow, 18].NumberFormat = "#,##0";

            meanRow++;

            // Nächste Gruppe startet hier
            dataStartRow += groupSize;
          }

          int lastMeanRow = meanRow;


          // -------------------------
          // 3) Diagramme (Scatter – nur Punkte)
          //    a) Rohpunkte: X = t_sync (D2:Dlast), Y = AnzahlTasks (A2:Alast)
          // -------------------------
          Chart chart = sheet.Charts.Add(ChartType.ScatterMarkers);
          var series = chart.Series.Add(
              sheet.Range[$"A2:A{n +1}"], // Y
              sheet.Range[$"D2:D{n +1}"]  // X
          );
          series.SeriesName.SetValue("t_sync");

          chart.TopLeftCell = sheet.Cells["F1"];
          chart.BottomRightCell = sheet.Cells["N20"];
          chart.Title.Visible = true;
          chart.Title.SetValue($"⏱️ {gruppe.Key} t_sync über Tasks");

          // -------------------------
          //    b) Mittelwerte: X = R2:RlastMean, Y = Q2:QlastMean
          // -------------------------
          Chart meanChart = sheet.Charts.Add(ChartType.ScatterMarkers);
          var meanSeries = meanChart.Series.Add(
              sheet.Range[$"Q2:Q{lastMeanRow}"], // Y
              sheet.Range[$"R2:R{lastMeanRow}"]  // X
          );
          meanSeries.SeriesName.SetValue("Mean t_sync");
          meanChart.TopLeftCell = sheet.Cells["U1"];
          meanChart.BottomRightCell = sheet.Cells["AC20"];
          meanChart.Title.Visible = true;
          meanChart.Title.SetValue($"⏱️ {gruppe.Key} Mean t_sync über Tasks");

          // (Optional) eigene Trendgerade per C# rechnen und Gleichung als Text anzeigen
          // var (m,b) = LinearRegression( ... ); trendLines.Add((gruppe.Key, $"y={m:F3}x+{b:F3}"));

          meanCharts.Add((gruppe.Key, meanChart));
        }

        // -------------------------
        // 4) Übersicht: Charts gesammelt (ohne teure Neuberechnung)
        // -------------------------
        Worksheet übersicht = workbook.Worksheets.Add();
        übersicht.Name = "Übersicht";
        int diagrammTopRow = 1;
        foreach (var (aktion, meanChart) in meanCharts)
        {
          var overviewChart = meanChart.CopyTo(übersicht);
          overviewChart.TopLeftCell = übersicht.Cells[$"A{diagrammTopRow}"];
          overviewChart.BottomRightCell = übersicht.Cells[$"J{diagrammTopRow + 19}"];
          overviewChart.Title.SetValue($"⏱️ {aktion} Mean t_sync über Tasks");
          diagrammTopRow += 22;
        }

        // Leeres Standard-Worksheet entfernen, falls vorhanden
        if (workbook.Worksheets.Count > 0 && workbook.Worksheets[0].Name == "Sheet1")
          workbook.Worksheets.RemoveAt(0);

        // Kein Recalc nötig, da keine Formeln erzeugt wurden.
        workbook.SaveDocument(dateiname, DocumentFormat.Xlsx);
      }
      finally
      {
        workbook.Options.CalculationMode = prevCalc;
        workbook.Calculate();
        workbook.EndUpdate();

        MessageBox.Show($"📊 Excel-Datei wurde gespeichert: {dateiname}",
          "Export fertig", MessageBoxButton.OK, MessageBoxImage.Information);
      }
    }






    public void Exportiere2(List<FunktionsMessung> logEinträge, string dateiname)
    {
      var workbook = new Workbook();
      workbook.BeginUpdate();
      WorkbookCalculationMode prevCalc = workbook.Options.CalculationMode;
      workbook.Options.CalculationMode = WorkbookCalculationMode.Manual;

      try
      {
        // Gruppierung nur einmal
        var gruppiert = logEinträge
            .GroupBy(e => e.Aktion)
            .ToList();

        var meanCharts = new List<(string Aktion, Chart ChartRef)>();
        //var trendLines = new List<(string Aktion, string Function)>(); // optional befüllen (hier Dummy)

        foreach (var gruppe in gruppiert)
        {
          Worksheet sheet = workbook.Worksheets.Add();
          string sheetName = gruppe.Key.Length > 31 ? gruppe.Key.Substring(0, 31) : gruppe.Key;
          sheet.Name = sheetName;

          // -------------------------
          // 1) Rohdaten: A=AnzahlTasks, B=t_gesamt, C=t_opera, D=t_sync
          // t_opera hier als 0 (oder reell, wenn du es schon hast). Falls du t_gesamt->t_sync später abziehst, setze C entsprechend.
          // Tipp: gib t_sync bereits im Log mit; dann sind B/C entbehrlich.
          // -------------------------
          var ordered = gruppe.OrderBy(e => e.AnzahlTasks).ToList();
          int n = ordered.Count;

          // Bulk-Array vorbereiten
          var data = new object[n + 1, 4];
          data[0, 0] = "Groese DM";
          data[0, 1] = "t_gesamt (ns)";
          data[0, 2] = "t_opera (ns)";
          data[0, 3] = "t_sync (ns)";


          for (int i = 0; i < n; i++)
          {
            var e = ordered[i];
            double tGes = e.DauerNs;
            double tOper = tOperaProDmSize.ContainsKey(e.AnzahlTasks) ? tOperaProDmSize[e.AnzahlTasks] : 0;
            double tSync = tGes - tOper;

            data[i + 1, 0] = e.AnzahlTasks;
            data[i + 1, 1] = tGes;
            data[i + 1, 2] = tOper;
            data[i + 1, 3] = tSync;
          }

          sheet.Import(data, 0, 0);

          //Mittelwert-Tabelle in Q/R, berechnet in Excel
          sheet.Cells[0, 16].Value = "Groese DM";  // Spalte Q
          sheet.Cells[0, 17].Value = "Mittelwert t_sync";  // Spalte R
          sheet.Cells[0, 18].Value = "Standardfehler t_opera";  // Spalte S


          var gruppenNachTask = gruppe
            .OrderBy(x => x.AnzahlTasks)
            .GroupBy(e => e.AnzahlTasks)
            .OrderBy(g => g.Key)
            .ToList();

          int meanRow = 1;
          int excelRowStart = 2; // da 1-basiert und erste Zeile Überschrift
          int dataStartRow = 2;

          foreach (IGrouping<int, FunktionsMessung> g in gruppenNachTask)
          {
            int taskCount = 200;
            int skipCount = 10; // so viele Einträge überspringen
            int groupSize = g.Count();     // Anzahl Zeilen für diese Taskgröße

            // Excel-Zeilenbereich bestimmen
            int firstDataRow = dataStartRow;                  // erste Datenzeile dieser Gruppe
            int firstUsedRow = firstDataRow + skipCount;      // ab hier wird gemittelt
            if (firstUsedRow > firstDataRow + groupSize - 1)
              firstUsedRow = firstDataRow + groupSize - 1;  // fallback falls zu viel skip

            int lastDataRow = firstDataRow + groupSize - 1;   // letzte Zeile dieser Gruppe

            // AnzahlTasks in Spalte Q
            sheet.Cells[meanRow, 16].Value = g.Key;

            // Mittelwert-Formel in Spalte R
            sheet.Cells[meanRow, 17].Formula = $"=AVERAGE(D{firstUsedRow}:D{lastDataRow})";
            sheet.Cells[meanRow, 17].NumberFormat = "#,##0";

            // Standardfehler in Spalte S
            sheet.Cells[meanRow, 18].Formula = $"=STDEV.P(D{firstUsedRow}:D{lastDataRow})/SQRT(COUNT(D{firstUsedRow}:D{lastDataRow}))"; ;
            sheet.Cells[meanRow, 18].NumberFormat = "#,##0";

            meanRow++;

            // Nächste Gruppe startet hier
            dataStartRow += groupSize;
          }

          int lastMeanRow = meanRow;


          // -------------------------
          // 3) Diagramme (Scatter – nur Punkte)
          //    a) Rohpunkte: X = t_sync (D2:Dlast), Y = AnzahlTasks (A2:Alast)
          // -------------------------
          Chart chart = sheet.Charts.Add(ChartType.ScatterMarkers);
          var series = chart.Series.Add(
              sheet.Range[$"A2:A{n + 1}"], // Y
              sheet.Range[$"D2:D{n + 1}"]  // X
          );
          series.SeriesName.SetValue("t_sync");

          chart.TopLeftCell = sheet.Cells["F1"];
          chart.BottomRightCell = sheet.Cells["N20"];
          chart.Title.Visible = true;
          chart.Title.SetValue($"⏱️ {gruppe.Key} t_sync über Tasks");

          // -------------------------
          //    b) Mittelwerte: X = R2:RlastMean, Y = Q2:QlastMean
          // -------------------------
          Chart meanChart = sheet.Charts.Add(ChartType.ScatterMarkers);
          var meanSeries = meanChart.Series.Add(
              sheet.Range[$"Q2:Q{lastMeanRow}"], // Y
              sheet.Range[$"R2:R{lastMeanRow}"]  // X
          );
          meanSeries.SeriesName.SetValue("Mean t_sync");
          meanChart.TopLeftCell = sheet.Cells["U1"];
          meanChart.BottomRightCell = sheet.Cells["AC20"];
          meanChart.Title.Visible = true;
          meanChart.Title.SetValue($"⏱️ {gruppe.Key} Mean t_sync über Tasks");

          // (Optional) eigene Trendgerade per C# rechnen und Gleichung als Text anzeigen
          // var (m,b) = LinearRegression( ... ); trendLines.Add((gruppe.Key, $"y={m:F3}x+{b:F3}"));

          meanCharts.Add((gruppe.Key, meanChart));
        }

        // -------------------------
        // 4) Übersicht: Charts gesammelt (ohne teure Neuberechnung)
        // -------------------------
        Worksheet übersicht = workbook.Worksheets.Add();
        übersicht.Name = "Übersicht";
        int diagrammTopRow = 1;
        foreach (var (aktion, meanChart) in meanCharts)
        {
          var overviewChart = meanChart.CopyTo(übersicht);
          overviewChart.TopLeftCell = übersicht.Cells[$"A{diagrammTopRow}"];
          overviewChart.BottomRightCell = übersicht.Cells[$"J{diagrammTopRow + 19}"];
          overviewChart.Title.SetValue($"⏱️ {aktion} Mean t_sync über Tasks");
          diagrammTopRow += 22;
        }

        // Leeres Standard-Worksheet entfernen, falls vorhanden
        if (workbook.Worksheets.Count > 0 && workbook.Worksheets[0].Name == "Sheet1")
          workbook.Worksheets.RemoveAt(0);

        // Kein Recalc nötig, da keine Formeln erzeugt wurden.
        workbook.SaveDocument(dateiname, DocumentFormat.Xlsx);
      }
      finally
      {
        workbook.Options.CalculationMode = prevCalc;
        workbook.Calculate();
        workbook.EndUpdate();

        MessageBox.Show($"📊 Excel-Datei wurde gespeichert: {dateiname}",
          "Export fertig", MessageBoxButton.OK, MessageBoxImage.Information);
      }
    }




    public void Exportiere3D(List<FunktionsMessung> logEinträge, string dateiname)
    {
      var workbook = new Workbook();
      workbook.BeginUpdate();

      try
      {
        // 1) Nur unsere 3D-Messungen einsammeln
        var logs = logEinträge
          .Where(e => e.Aktion == "Performance_3D")
          .ToList();

        if (logs.Count == 0)
        {
          var wsEmpty = workbook.Worksheets[0];
          wsEmpty.Name = "Raw+Summary";
          workbook.SaveDocument(dateiname, DocumentFormat.Xlsx);
          return;
        }

        // ---------- Hilfsfunktionen ----------
        double Mean(IReadOnlyList<double> xs) => xs.Count == 0 ? double.NaN : xs.Average();
        double StdSample(IReadOnlyList<double> xs)
        {
          int n = xs.Count;
          if (n <= 1) return double.NaN;
          double mu = xs.Average();
          double s2 = xs.Sum(v => (v - mu) * (v - mu)) / (n - 1);
          return Math.Sqrt(s2);
        }
        double SE(IReadOnlyList<double> xs)
        {
          int n = xs.Count;
          if (n <= 1) return double.NaN;
          return StdSample(xs) / Math.Sqrt(n);
        }

        // 2) "Raw+Summary" Blatt vorbereiten
        var wsRaw = workbook.Worksheets[0];
        wsRaw.Name = "Raw+Summary";

        // 2a) ROHDATEN links: 4 Spalten [DM Size, Tasks, t_ges (ns), t_sync (ns)]
        // t_sync = t_ges - t_opera(DM)
        var rawRows = new List<(int DM, int Tasks, double tges, double tsync)>(logs.Count);
        foreach (var e in logs)
        {
          double tOpera = tOperaProDmSize.TryGetValue(e.DmSize, out var op) ? op : 0.0;
          double tges = e.DauerNs;
          double tsync = tges - tOpera;
          rawRows.Add((e.DmSize, e.AnzahlTasks, tges, tsync));
        }

        // Header + Daten in 2D-Array
        int rawRowsCount = rawRows.Count;
        var rawGrid = new object[rawRowsCount + 1, 4];
        rawGrid[0, 0] = "DM Size";
        rawGrid[0, 1] = "Tasks";
        rawGrid[0, 2] = "t_ges (ns)";
        rawGrid[0, 3] = "t_sync (ns)";

        for (int i = 0; i < rawRowsCount; i++)
        {
          rawGrid[i + 1, 0] = rawRows[i].DM;
          rawGrid[i + 1, 1] = rawRows[i].Tasks;
          rawGrid[i + 1, 2] = rawRows[i].tges;
          rawGrid[i + 1, 3] = rawRows[i].tsync;
        }

        wsRaw.Import(rawGrid, 0, 0); // ab A1

        // 2b) ZUSAMMENFASSUNG rechts DANEBEN:
        // Gruppiert nach (DM, Tasks): n, mean(t_sync), SE(t_sync)  (KEIN t_ges-Mean/SE)
        var grouped = rawRows
          .GroupBy(r => new { r.DM, r.Tasks })
          .Select(g =>
          {
            var tsyncList = g.Select(x => x.tsync).ToList();
            return new
            {
              DM = g.Key.DM,
              Tasks = g.Key.Tasks,
              N = tsyncList.Count,
              Mean_tsync = Mean(tsyncList),
              SE_tsync = SE(tsyncList)
            };
          })
          .OrderBy(x => x.DM)
          .ThenBy(x => x.Tasks)
          .ToList();

        int summaryCount = grouped.Count;
        int summaryColStart = 6; // Spalte G
        var sumGrid = new object[summaryCount + 1, 5];
        sumGrid[0, 0] = "DM Size";
        sumGrid[0, 1] = "Tasks";
        sumGrid[0, 2] = "n";
        sumGrid[0, 3] = "Mean t_sync (ns)";
        sumGrid[0, 4] = "SE t_sync (ns)";

        for (int i = 0; i < summaryCount; i++)
        {
          var s = grouped[i];
          sumGrid[i + 1, 0] = s.DM;
          sumGrid[i + 1, 1] = s.Tasks;
          sumGrid[i + 1, 2] = s.N;
          sumGrid[i + 1, 3] = s.Mean_tsync;
          sumGrid[i + 1, 4] = s.SE_tsync;
        }

        wsRaw.Import(sumGrid, 0, summaryColStart); // ab G1

        // 3) AUS DEN AGGREGATEN die Matrix der Mittelwerte (t_sync) aufbauen
        int maxTasks = Math.Max(1, grouped.Max(x => x.Tasks));
        int maxDm = Math.Max(1, grouped.Max(x => x.DM));

        var meanByPair = grouped.ToDictionary(
          k => (Task: k.Tasks, DM: k.DM),
          v => v.Mean_tsync
        );

        // Matrix: Zeilen=Tasks (1..maxTasks), Spalten=DM (1..maxDm)
        var grid = new object[maxTasks + 1, maxDm + 1];
        grid[0, 0] = "Tasks \\ DM";
        for (int dm = 1; dm <= maxDm; dm++) grid[0, dm] = dm;
        for (int t = 1; t <= maxTasks; t++) grid[t, 0] = t;

        for (int t = 1; t <= maxTasks; t++)
          for (int dm = 1; dm <= maxDm; dm++)
            grid[t, dm] = meanByPair.TryGetValue((t, dm), out var mu) ? (object)mu : null;

        // 4) "SurfaceData" Blatt + 3D-Chart auf Basis der Mittelwerte
        var wsSurface = workbook.Worksheets.Add();
        wsSurface.Name = "SurfaceData";
        wsSurface.Import(grid, 0, 0);

        var chart = wsSurface.Charts.Add(ChartType.Surface3D);
        var dataRange = wsSurface.Range.FromLTRB(0, 0, maxDm, maxTasks); // kompletter Bereich inkl. Header
        chart.SelectData(dataRange, ChartDataDirection.Row);
        chart.TopLeftCell = wsSurface.Cells["A55"];
        chart.BottomRightCell = wsSurface.Cells["T95"];
        chart.Title.Visible = true;
        chart.Title.SetValue("t_sync Mittelwerte: Tasks × DM-Größe (3D)");

        // 5) Heatmap der Mittelwerte
        var wsHeat = workbook.Worksheets.Add();
        wsHeat.Name = "Heatmap";
        wsHeat.Import(grid, 0, 0);
        var rng = wsHeat.Range.FromLTRB(1, 1, maxDm, maxTasks);

        // 6) Speichern
        workbook.SaveDocument(dateiname, DocumentFormat.Xlsx);
      }
      finally
      {
        workbook.EndUpdate();
        MessageBox.Show($"📊 Excel-Datei wurde gespeichert: {dateiname}",
          "Export fertig", MessageBoxButton.OK, MessageBoxImage.Information);
      }
    }









    //public void Exportiere3D(List<FunktionsMessung> logEinträge, string dateiname)
    //{
    //  var workbook = new Workbook();
    //  workbook.BeginUpdate();

    //  try
    //  {
    //    // 1) Daten einsammeln: nur unsere 3D-Messungen
    //    var logs = logEinträge
    //      .Where(e => e.Aktion == "Performance_3D")
    //      .ToList();

    //    if (logs.Count == 0)
    //    {
    //      // Fallback: falls nichts da ist, trotzdem leere Datei schreiben
    //      workbook.Worksheets[0].Name = "SurfaceData";
    //      workbook.SaveDocument(dateiname, DocumentFormat.Xlsx);
    //      return;
    //    }

    //    int maxTasks = Math.Max(1, logs.Max(x => x.AnzahlTasks));
    //    int maxDm = Math.Max(1, logs.Max(x => x.DmSize));

    //    // 2) Matrix [Zeilen: Tasks 1..maxTasks, Spalten: DM 1..maxDm]
    //    // A1: "Tasks \\ DM", B1..: DM-Header, A2..: Task-Header
    //    Worksheet sheet = workbook.Worksheets[0];
    //    sheet.Name = "SurfaceData";

    //    int rows = maxTasks + 1; // +Headerzeile
    //    int cols = maxDm + 1;    // +Headerspalte
    //    var grid = new object[rows, cols];

    //    grid[0, 0] = "Tasks \\ DM";
    //    for (int dm = 1; dm <= maxDm; dm++) grid[0, dm] = dm;
    //    for (int t = 1; t <= maxTasks; t++) grid[t, 0] = t;

    //    // Mittelwerte je (Task, DM) für t_sync = t_gesamt - t_opera(DM)
    //    // t_opera wird aus deiner tOperaProDmSize-Map entnommen.
    //    var byPair = logs
    //      .GroupBy(e => new { e.AnzahlTasks, e.DmSize })
    //      .ToDictionary(
    //        g => (Task: g.Key.AnzahlTasks, DM: g.Key.DmSize),
    //        g =>
    //        {
    //          double tOpera = tOperaProDmSize.TryGetValue(g.Key.DmSize, out var op) ? op : 0.0;
    //          var vals = g.Select(x => x.DauerNs - tOpera).Where(v => v > 0).ToList();
    //          return vals.Count == 0 ? (double?)null : vals.Average();
    //        });

    //    for (int t = 1; t <= maxTasks; t++)
    //    {
    //      for (int dm = 1; dm <= maxDm; dm++)
    //      {
    //        if (byPair.TryGetValue((t, dm), out var mean) && mean.HasValue)
    //          grid[t, dm] = mean.Value;
    //        else
    //          grid[t, dm] = null; // leer lassen
    //      }
    //    }

    //    sheet.Import(grid, 0, 0);

    //    // 3) 3D-Chart anlegen (Surface)
    //    // Hinweis: Falls deine DevExpress-Version 'Surface3D' nicht unterstützt,
    //    // verwende alternativ 'ChartType.Column3D' und chart.SelectData(...).
    //    var chart = sheet.Charts.Add(ChartType.Surface3D);
    //    // Bereich: komplette Matrix inkl. Header (A1 .. [cols-1, rows-1])
    //    var dataRange = sheet.Range.FromLTRB(0, 0, cols - 1, rows - 1);
    //    chart.SelectData(dataRange, ChartDataDirection.Row);

    //    chart.TopLeftCell = sheet.Cells["A55"];  // unter die Matrix
    //    chart.BottomRightCell = sheet.Cells["T95"];
    //    chart.Title.Visible = true;
    //    chart.Title.SetValue("t_sync-Mittelwerte: Tasks × DM-Größe (3D)");

    //    // 4) Zusätzlich: Heatmap in separatem Sheet (optional, hilfreich beim Lesen)
    //    var heat = workbook.Worksheets.Add();
    //    heat.Name = "Heatmap";
    //    heat.Import(grid, 0, 0);
    //    var rng = heat.Range.FromLTRB(1, 1, cols - 1, rows - 1);
    //    //var cf = heat.ConditionalFormattings.colors.AddColorScaleConditionalFormatting(rng, ColorScaleType.ColorScale3);
    //    //cf.IsDescending = false;

    //    // 5) Speichern
    //    workbook.SaveDocument(dateiname, DocumentFormat.Xlsx);
    //  }
    //  finally
    //  {
    //    workbook.EndUpdate();
    //    MessageBox.Show($"📊 Excel-Datei wurde gespeichert: {dateiname}",
    //      "Export fertig", MessageBoxButton.OK, MessageBoxImage.Information);
    //  }
    //}

  }
}
