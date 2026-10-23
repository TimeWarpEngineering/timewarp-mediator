#region Purpose
// IRequest of a two-arm result, with one behavior, for the Architecture-shaped row.
#endregion

#region Design
// The result is a benchmark-local two-arm struct (reference success, value problem) so this
// project does not take a OneOf package reference. The shape matches IRequest<OneOf<Response, ProblemDetails>>.
// The behavior is closed over UpdateProfile, so it does not wrap Ping/Pong.
#endregion

using TimeWarp.Mediator;

namespace MediatorBenchmarks;

public readonly struct OneOf<T0, T1>
    where T0 : class
    where T1 : struct
{
  private readonly T0? Zero;
  private readonly T1 One;
  private readonly bool HasZero;

  private OneOf(T0? zero, T1 one, bool hasZero)
  {
    Zero = zero;
    One = one;
    HasZero = hasZero;
  }

  public static OneOf<T0, T1> FromT0(T0 value) => new(value, default, true);

  public bool IsT0 => HasZero;

  public T0 AsT0 => Zero!;
}

public readonly struct ProblemDetails
{
  public ProblemDetails(int status)
  {
    Status = status;
  }

  public int Status { get; }
}

public sealed class ProfileResponse
{
  public ProfileResponse(string name)
  {
    Name = name;
  }

  public string Name { get; }
}

public sealed class UpdateProfile : IRequest<OneOf<ProfileResponse, ProblemDetails>>
{
  public string Name { get; set; } = "ada";
}

public sealed class UpdateProfileHandler : IRequestHandler<UpdateProfile, OneOf<ProfileResponse, ProblemDetails>>
{
  public ValueTask<OneOf<ProfileResponse, ProblemDetails>> Handle(UpdateProfile request, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(OneOf<ProfileResponse, ProblemDetails>.FromT0(new ProfileResponse(request.Name)));
  }
}

public sealed class ArchitectureValidationBehavior : IPipelineBehavior<UpdateProfile, OneOf<ProfileResponse, ProblemDetails>>
{
  public async ValueTask<OneOf<ProfileResponse, ProblemDetails>> Handle(
      UpdateProfile request,
      MessageHandlerDelegate<UpdateProfile, OneOf<ProfileResponse, ProblemDetails>> next,
      CancellationToken cancellationToken)
  {
    return await next(request, cancellationToken).ConfigureAwait(false);
  }
}
