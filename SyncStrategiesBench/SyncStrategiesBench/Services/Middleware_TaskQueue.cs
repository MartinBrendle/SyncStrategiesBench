using AIO.DM;
using AIO.Utils;
using System.Threading.Channels;

namespace AIO.Services
{
  public class Middleware_TaskQueue : IMiddleware
  {
    public async Task AddGerätDirect(Gerät gerät)
    {
    }

    public async Task AddAufzeichnerDirect(Aufzeichner auf)
    {
    }

    private GeräteListe mGeräte = new();
    private AufzeichnerList mAufzeichner = new();
    private readonly Channel<Func<Task>> mTaskChannel = Channel.CreateUnbounded<Func<Task>>();

    public Middleware_TaskQueue()
    {
      _ = Task.Run(ProcessQueueAsync);
    }

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

    private async Task ProcessQueueAsync()
    {
      await foreach (var taskFunc in mTaskChannel.Reader.ReadAllAsync())
      {
        try
        {
          await taskFunc();
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Fehler bei Task-Verarbeitung: {ex.Message}");
        }
      }
    }

    //private Task Enqueue(Func<Task> action)
    //{
    //  mTaskChannel.Writer.TryWrite(action);
    //  return Task.CompletedTask;
    //}

    private Task Enqueue(Func<Task> action)
    {
      var tcs = new TaskCompletionSource<object?>();
      mTaskChannel.Writer.TryWrite(async () =>
      {
        try
        {
          await action();
          tcs.SetResult(null);
        }
        catch (Exception ex)
        {
          tcs.SetException(ex);
        }
      });
      return tcs.Task;
    }



    public Task VerknüpfeAufzeichnerMitDO(Aufzeichner auf, DarstellungsObjekt obj)
    {
      return Enqueue(() =>
      {
        if (!auf.DarstellungsObjekte.Contains(obj))
          auf.DarstellungsObjekte.Add(obj);
        if (!obj.Aufzeichner.Contains(auf))
          obj.Aufzeichner.Add(auf);
        return Task.CompletedTask;
      });
    }

    // Gerät
    public Task AddGerät(Gerät gerät)
    {
      return Enqueue(() =>
      {
        mGeräte.Add(gerät);
        return Task.CompletedTask;
      });
    }

    public Task<Gerät?> GetGerät(string name)
    {
      var tcs = new TaskCompletionSource<Gerät?>();
      Enqueue(() =>
      {
        var result = mGeräte.FirstOrDefault(g => g.Name == name);
        tcs.SetResult(result);
        return Task.CompletedTask;
      });
      return tcs.Task;
    }


    public Task<GeräteListe> GetGerätListe()
    {
      var tcs = new TaskCompletionSource<GeräteListe>();
      Enqueue(() =>
      {
        var result = mGeräte;
        tcs.SetResult(result);
        return Task.CompletedTask;
      });

      return tcs.Task;
    }

    public Task LöscheGerät(Gerät gerät)
    {
      return Enqueue(() =>
      {
        mGeräte.Remove(gerät);
        return Task.CompletedTask;
      });
    }


    //  DarstellungsObjekt
    public Task AddDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      return Enqueue(() =>
      {
        gerät.DarstellungsObjekte.Add(obj);
        return Task.CompletedTask;
      });
    }

    public Task<DarstellungsObjekt?> GetDarstellungsObjekt(Gerät gerät, string name)
    {
      var tcs = new TaskCompletionSource<DarstellungsObjekt?>();
      Enqueue(() =>
      {
        var result = gerät.DarstellungsObjekte.FirstOrDefault(d => d.Name == name);
        tcs.SetResult(result);
        return Task.CompletedTask;
      });
      return tcs.Task;
    }

    public Task<DarstellungsObjektList> GetDarstellungsObjektListe(Gerät gerät)
    {
      var tcs = new TaskCompletionSource<DarstellungsObjektList>();
      Enqueue(() =>
      {
        tcs.SetResult(gerät.DarstellungsObjekte);
        return Task.CompletedTask;
      });
      return tcs.Task;
    }

    public Task LöscheDarstellungsObjekt(Gerät gerät, DarstellungsObjekt obj)
    {
      return Enqueue(() =>
      {
        gerät.DarstellungsObjekte.Remove(obj);
        return Task.CompletedTask;
      });
    }


    // Aufzeichner
    public Task AddAufzeichner(Aufzeichner auf)
    {
      return Enqueue(() =>
      {
        mAufzeichner.Add(auf);
        return Task.CompletedTask;
      });
    }

    public Task<Aufzeichner?> GetAufzeichner(string name)
    {
      var tcs = new TaskCompletionSource<Aufzeichner?>();
      Enqueue(() =>
      {
        var result = mAufzeichner.FirstOrDefault(d => d.Name == name);
        tcs.SetResult(result);
        return Task.CompletedTask;
      });
      return tcs.Task;
    }

    public Task<AufzeichnerList> GetAufzeichnerListe()
    {
      var tcs = new TaskCompletionSource<AufzeichnerList>();
      Enqueue(() =>
      {
        tcs.SetResult(mAufzeichner);
        return Task.CompletedTask;
      });
      return tcs.Task;
    }

    public Task LöscheAufzeichner(Aufzeichner auf)
    {
      return Enqueue(() =>
      {
        mAufzeichner.Remove(auf);
        return Task.CompletedTask;
      });
    }


    // DatenBasis
    public Task AddDatenBasisObjekt(Gerät gerät, DatenBasisObjekt dbo)
    {
      return Enqueue(() =>
      {
        gerät.DatenBasen.Add(dbo);
        return Task.CompletedTask;
      });
    }

    public Task<DatenBasisObjekt?> GetDatenBasisObjekt(Gerät gerät, string name)
    {
      var tcs = new TaskCompletionSource<DatenBasisObjekt?>();
      Enqueue(() =>
      {
        var result = gerät.DatenBasen.FirstOrDefault(d => d.Name == name);
        tcs.SetResult(result);
        return Task.CompletedTask;
      });
      return tcs.Task;
    }

    public Task<DatenBasis?> GetDatenBasis(Gerät gerät)
    {
      var tcs = new TaskCompletionSource<DatenBasis>();
      Enqueue(() =>
      {
        tcs.SetResult(gerät.DatenBasen);
        return Task.CompletedTask;
      });
      return tcs.Task;
    }

    public Task LöscheDatenBasis(Gerät gerät, DatenBasisObjekt dbo)
    {
      return Enqueue(() =>
      {
        gerät.DatenBasen.Remove(dbo);
        return Task.CompletedTask;
      });
    }

  }
}
