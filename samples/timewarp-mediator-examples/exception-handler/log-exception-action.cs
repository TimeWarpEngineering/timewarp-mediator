// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.ExceptionHandler;

public class LogExceptionAction : IRequestExceptionAction<Ping, Exception>
{
  private readonly TextWriter TextWriter;

  public LogExceptionAction(TextWriter writer) => TextWriter = writer;

  public Task Execute(Ping request, Exception exception, CancellationToken cancellationToken)
      => TextWriter.WriteLineAsync($"--- Exception: '{exception.GetType().FullName}'");
}