#region Purpose
// Handler-list identity, scope, and accessibility for the emission graph.
#endregion

#region Design
// The same type, by key or fully qualified name, is one handler. A request fact can be visited before its handler fact.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class CandidateGraph
{
  private static TypeModel RequestModel(
      string key,
      Dictionary<string, TypeModel> requests,
      Dictionary<string, TypeModel> responseByRequest)
  {
    if (requests.TryGetValue(key, out TypeModel? request))
    {
      return request;
    }

    throw new InvalidOperationException("Request '" + key + "' has a handler and no request model.");
  }

  private static string RequestDisplay(string key, Dictionary<string, TypeModel> requests)
  {
    return requests.TryGetValue(key, out TypeModel? request) ? request.Display : key;
  }

  private static TypeModel NotificationModel(string key, Dictionary<string, TypeFact> facts, List<TypeModel> handlers)
  {
    if (facts.TryGetValue(key, out TypeFact? fact))
    {
      return fact.Type;
    }

    foreach (TypeFact candidate in facts.Values)
    {
      foreach (HandlerMatch match in candidate.Matches)
      {
        if (match.IsNotification && match.Message.Key == key)
        {
          return match.Message;
        }
      }
    }

    return handlers[0];
  }

  private static TypeModel Prefer(TypeModel model, Dictionary<string, TypeFact> facts)
  {
    return facts.TryGetValue(model.Key, out TypeFact? fact) ? fact.Type : model;
  }

  private static bool AlreadyListed(Dictionary<string, List<TypeModel>> handlers, string messageKey, TypeModel handler)
  {
    if (!handlers.TryGetValue(messageKey, out List<TypeModel>? list))
    {
      return false;
    }

    foreach (TypeModel existing in list)
    {
      if (existing.Key == handler.Key || existing.FullyQualified == handler.FullyQualified)
      {
        return true;
      }
    }

    return false;
  }

  private static void AddHandler(Dictionary<string, List<TypeModel>> handlers, TypeModel message, TypeModel handler)
  {
    if (!handlers.TryGetValue(message.Key, out List<TypeModel>? list))
    {
      list = new List<TypeModel>();
      handlers.Add(message.Key, list);
    }

    foreach (TypeModel existing in list)
    {
      if (existing.Key == handler.Key || existing.FullyQualified == handler.FullyQualified)
      {
        return;
      }
    }

    list.Add(handler);
  }

  private static bool BoundByBase(
      TypeModel handler,
      HandlerMatch match,
      Dictionary<string, TypeFact> facts,
      HashSet<string> members)
  {
    foreach (string baseKey in handler.BaseKeys)
    {
      if (!facts.TryGetValue(baseKey, out TypeFact? baseFact))
      {
        continue;
      }

      TypeModel baseType = baseFact.Type;
      if (!baseType.IsClassOrStruct
          || baseType.IsAbstract
          || baseType.Arity != 0
          || !baseType.IsDiscoverable
          || !members.Contains(baseType.AssemblyName))
      {
        continue;
      }

      foreach (HandlerMatch baseMatch in baseFact.Matches)
      {
        string baseResponse = baseMatch.Response is null ? string.Empty : baseMatch.Response.Key;
        string matchResponse = match.Response is null ? string.Empty : match.Response.Key;
        if (baseMatch.IsNotification == match.IsNotification
            && baseMatch.IsVoid == match.IsVoid
            && baseMatch.Message.Key == match.Message.Key
            && baseResponse == matchResponse)
        {
          return true;
        }
      }
    }

    return false;
  }

  private static TypeModel? ScopeOf(
      TypeModel type,
      Dictionary<string, TypeFact> facts,
      Dictionary<string, TypeModel> scopeByMessage)
  {
    if (facts.TryGetValue(type.Key, out TypeFact? fact) && fact.Scope is not null)
    {
      return fact.Scope;
    }

    if (scopeByMessage.TryGetValue(type.Key, out TypeModel? scope))
    {
      return scope;
    }

    return null;
  }

  private static void ReportScopeMismatch(
      TypeModel handlerType,
      TypeModel? handlerScope,
      TypeModel messageType,
      TypeModel? messageScope,
      List<DiagnosticInfo> diagnostics)
  {
    if (handlerScope is null || messageScope is null || handlerScope.Key == messageScope.Key)
    {
      return;
    }

    diagnostics.Add(Info(
        DiagnosticDescriptors.ScopeMismatch,
        handlerType.Location,
        handlerType.ErrorDisplay,
        handlerScope.ErrorDisplay,
        messageType.ErrorDisplay,
        messageScope.ErrorDisplay));
  }

  private static bool PipelineScopeAllowed(
      TypeModel? scopeType,
      HashSet<string> reported,
      List<DiagnosticInfo> generatorDiagnostics,
      string hostAssemblyName)
  {
    return scopeType is null || Allows(scopeType, reported, generatorDiagnostics, hostAssemblyName);
  }

  private static bool Allows(
      TypeModel type,
      HashSet<string> reported,
      List<DiagnosticInfo> generatorDiagnostics,
      string hostAssemblyName)
  {
    TypeModel? inaccessible = type.InaccessiblePart ?? (type.Accessible ? null : type);
    if (inaccessible is null)
    {
      return true;
    }

    if (reported.Add(inaccessible.Key))
    {
      generatorDiagnostics.Add(Info(
          DiagnosticDescriptors.InaccessibleMemberType,
          inaccessible.Location,
          inaccessible.ErrorDisplay,
          inaccessible.AssemblyName,
          hostAssemblyName));
    }

    return false;
  }
}
