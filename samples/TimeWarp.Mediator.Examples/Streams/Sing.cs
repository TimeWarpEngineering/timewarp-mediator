// Modified by Steven T. Cramer
namespace TimeWarp.Mediator.Examples;

public class Sing : IStreamRequest<Song>
{
    public string Message { get; set; }
}