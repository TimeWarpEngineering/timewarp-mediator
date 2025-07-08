// Modified by Steven T. Cramer
namespace TimeWarp.Mediator.Examples;

public class Ping : IRequest<Pong>
{
    public string Message { get; set; }
}