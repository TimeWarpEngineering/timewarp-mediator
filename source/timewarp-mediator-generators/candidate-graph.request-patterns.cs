#region Purpose
// Adds GeneratedRequestPatternAttribute expansions to the request graph.
#endregion

#region Design
// A declared request binds like a request whose GeneratedHandlerAttribute names its handler: the
// declared handler is added beside any visible one (same type is one handler, a different type is
// TWM002), the declaration suppresses TWM001, and the same Allows checks report TWM007. The scope
// is the declaration's, unless a visible fact already gave the request one. Every binding then
// goes through the same behavior closing and emission as a discovered request.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class CandidateGraph
{
  private static void AddDeclaredRequests(
      EquatableArray<RequestPatternFact> patterns,
      Dictionary<string, TypeFact> byKey,
      string hostAssemblyName,
      List<DiagnosticInfo> diagnostics,
      List<DiagnosticInfo> generatorDiagnostics,
      HashSet<string> reported,
      HashSet<string> dropped,
      Dictionary<string, TypeModel> scopeByMessage,
      Dictionary<string, List<TypeModel>> handlersByRequest,
      Dictionary<string, TypeModel> responseByRequest,
      HashSet<string> voidRequests,
      Dictionary<string, TypeModel> requests,
      HashSet<string> declarationCovers)
  {
    foreach (RequestPatternFact pattern in patterns)
    {
      diagnostics.AddRange(pattern.Failures);
      foreach (DeclaredRequestFact declared in pattern.Requests)
      {
        TypeModel request = Prefer(declared.Request, byKey);
        declarationCovers.Add(request.Key);
        if (!requests.ContainsKey(request.Key))
        {
          requests[request.Key] = request;
        }

        if (declared.Scope is not null && !scopeByMessage.ContainsKey(request.Key))
        {
          scopeByMessage.Add(request.Key, declared.Scope);
        }

        if (!Allows(request, reported, generatorDiagnostics, hostAssemblyName)
            || !Allows(declared.Handler, reported, generatorDiagnostics, hostAssemblyName)
            || !Allows(declared.Response, reported, generatorDiagnostics, hostAssemblyName))
        {
          dropped.Add(request.Key);
          continue;
        }

        if (AlreadyListed(handlersByRequest, request.Key, declared.Handler))
        {
          continue;
        }

        AddHandler(handlersByRequest, request, declared.Handler);
        if (!responseByRequest.ContainsKey(request.Key))
        {
          responseByRequest[request.Key] = declared.Response;
          if (declared.IsVoid)
          {
            voidRequests.Add(request.Key);
          }
        }
      }
    }
  }
}
