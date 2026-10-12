// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class SingHandler : IStreamRequestHandler<Sing, Song>
{
  private readonly TextWriter TextWriter;

  public SingHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public async IAsyncEnumerable<Song> Handle(Sing request, [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    await TextWriter.WriteLineAsync($"--- Handled Sing: {request.Message}, Song");
    yield return await Task.Run(() => new Song { Message = request.Message + "ing do" });
    yield return await Task.Run(() => new Song { Message = request.Message + "ing re" });
    yield return await Task.Run(() => new Song { Message = request.Message + "ing mi" });
    yield return await Task.Run(() => new Song { Message = request.Message + "ing fa" });
    yield return await Task.Run(() => new Song { Message = request.Message + "ing so" });
    yield return await Task.Run(() => new Song { Message = request.Message + "ing la" });
    yield return await Task.Run(() => new Song { Message = request.Message + "ing ti" });
    yield return await Task.Run(() => new Song { Message = request.Message + "ing do" });
  }
}