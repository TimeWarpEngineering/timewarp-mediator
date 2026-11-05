#region Purpose
// Handler-first discovery: walk member assemblies, bind request→handler, close behaviors, emit diagnostics.
#endregion

#region Design
// Closed concrete handlers only (no MaxTypesClosing combinatorics). Behaviors come exclusively from
// [assembly: MediatorBehavior] so pipelines do not pick up unrelated IPipelineBehavior types from
// referenced projects. Order matches MediatR GetServices().Reverse().Aggregate: first listed is outermost.
// Member assemblies are ordered by name before SourceIndex so tie-breaking is deterministic.
// Discovery walks member assemblies by name and sorts each handler list by display name, so
// notification handler order and the TWM002 handler list do not depend on hash-set order.
// A class binds every handler interface it implements; an interface already implemented by a
// concrete, discovered base handler is inherited, not a second binding (no TWM002, no double publish).
// TryCloseBehavior returns null when the constructed type is not IPipelineBehavior<TRequest, TResponse>.
// Pipeline membership is [MediatorScope(typeof(TScope))] on handler, request, containing type, or
// assembly. Unscoped (no attribute) is the default ISender/IPublisher pipeline. Behaviors with a
// Scope named argument weave only that pipeline; unscoped behaviors never run on scoped requests.
// Void dispatch comes from matching IRequestHandler<T>, never from TResponse == Unit: an explicit
// IRequestHandler<T, Unit> returns ValueTask<Unit> and must be registered as the two-arity interface.
// Responses are ITypeSymbol so arrays and other non-named types (T[]) bind like named ones.
// A handler, message, response, behavior, or pipeline scope marker the host cannot access
// (internal in a referenced member without InternalsVisibleTo) is not bound and is TWM007,
// never CS0122 in generated code; its message is then not TWM001. Generated senders and
// publishers name that marker. [MediatorBehavior] entries that are not pipeline behaviors or
// close over no request are TWM010/TWM011. Those land in GeneratorDiagnostics so the analyzer
// stays TWM001–TWM004.
// Diagnostic locations are limited to this compilation's syntax trees: analyzers and generators
// may not report into another project's source.
// GeneratedHandlerAttribute on a concrete request names a handler. A symbol that exists is bound
// like any other handler. A missing name is TWM012 in a host and still suppresses TWM001, because
// another generator may have been going to emit it and, in a library, the host is where it appears.
// An unreadable name is TWM012 from this walk only when the caller asks: the generator reports
// that case itself, and reporting it here too would double it.
#endregion

namespace TimeWarp.Mediator.Analyzers;

public static partial class MessageGraphBuilder
{
  private const string RequestHandler1 = "TimeWarp.Mediator.IRequestHandler`1";
  private const string RequestHandler2 = "TimeWarp.Mediator.IRequestHandler`2";
  private const string NotificationHandler1 = "TimeWarp.Mediator.INotificationHandler`1";
  private const string PipelineBehavior2 = "TimeWarp.Mediator.IPipelineBehavior`2";
  private const string Request1 = "TimeWarp.Mediator.IRequest`1";
  private const string Request0 = "TimeWarp.Mediator.IRequest";
  private const string Notification0 = "TimeWarp.Mediator.INotification";
  private const string UnitTypeName = "TimeWarp.Mediator.Unit";

  public static MessageGraph Build(
      Compilation compilation,
      Membership membership,
      CancellationToken cancellationToken,
      bool reportUnreadableDeclaredHandler = true)
  {
    INamedTypeSymbol? requestHandler1 = compilation.GetTypeByMetadataName(RequestHandler1);
    INamedTypeSymbol? requestHandler2 = compilation.GetTypeByMetadataName(RequestHandler2);
    INamedTypeSymbol? notificationHandler1 = compilation.GetTypeByMetadataName(NotificationHandler1);
    INamedTypeSymbol? pipelineBehavior2 = compilation.GetTypeByMetadataName(PipelineBehavior2);
    INamedTypeSymbol? request0 = compilation.GetTypeByMetadataName(Request0);
    INamedTypeSymbol? request1 = compilation.GetTypeByMetadataName(Request1);
    INamedTypeSymbol? notification0 = compilation.GetTypeByMetadataName(Notification0);
    INamedTypeSymbol? unitType = compilation.GetTypeByMetadataName(UnitTypeName);
    INamedTypeSymbol? generatedHandlerAttribute = compilation.GetTypeByMetadataName(HandlerTypeNameReader.MetadataName);

    List<Diagnostic> diagnostics = new();
    List<DeclaredHandler> declaredHandlers = new();
    List<Diagnostic> declaredDiagnostics = new();
    HashSet<INamedTypeSymbol> declaredCovered = new(SymbolEqualityComparer.Default);
    Dictionary<INamedTypeSymbol, List<string>> declaredNames = new(SymbolEqualityComparer.Default);
    List<Diagnostic> generatorDiagnostics = new();
    AccessGuard access = new(compilation, generatorDiagnostics);
    HashSet<INamedTypeSymbol> droppedMessages = new(SymbolEqualityComparer.Default);
    Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> handlersByRequest =
        new(SymbolEqualityComparer.Default);
    Dictionary<INamedTypeSymbol, ITypeSymbol> responseByRequest =
        new(SymbolEqualityComparer.Default);
    HashSet<INamedTypeSymbol> voidRequests = new(SymbolEqualityComparer.Default);
    Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> handlersByNotification =
        new(SymbolEqualityComparer.Default);
    HashSet<INamedTypeSymbol> requests = new(SymbolEqualityComparer.Default);

    foreach (IAssemblySymbol assembly in membership.MemberAssemblies.OrderBy(a => a.Name, System.StringComparer.Ordinal))
    {
      foreach (INamedTypeSymbol type in Membership.EnumerateTypes(assembly.GlobalNamespace))
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsDiscoverableType(type))
        {
          continue;
        }

        CollectRequest(compilation, type, request0, request1, requests);
        CollectDeclaredHandlers(
            compilation,
            type,
            request0,
            request1,
            unitType,
            generatedHandlerAttribute,
            membership,
            access,
            reportUnreadableDeclaredHandler,
            handlersByRequest,
            responseByRequest,
            voidRequests,
            requests,
            droppedMessages,
            declaredHandlers,
            declaredDiagnostics,
            declaredCovered,
            declaredNames);

        foreach (RequestHandlerMatch match in GetRequestHandlers(type, requestHandler1, requestHandler2, unitType, membership))
        {
          if (!access.Allows(type) || !access.Allows(match.RequestType) || !access.Allows(match.ResponseType))
          {
            droppedMessages.Add(match.RequestType);
            continue;
          }

          if (!handlersByRequest.TryGetValue(match.RequestType, out List<INamedTypeSymbol>? list))
          {
            list = new List<INamedTypeSymbol>();
            handlersByRequest[match.RequestType] = list;
          }

          // A GeneratedHandlerAttribute may already have named this class.
          if (!ContainsSymbol(list, type))
          {
            list.Add(type);
          }
          responseByRequest[match.RequestType] = match.ResponseType;
          if (match.IsVoid)
          {
            voidRequests.Add(match.RequestType);
          }

          requests.Add(match.RequestType);
        }

        foreach (INamedTypeSymbol notificationType in GetNotificationHandlers(type, notificationHandler1, membership))
        {
          if (!access.Allows(type) || !access.Allows(notificationType))
          {
            continue;
          }

          if (!handlersByNotification.TryGetValue(notificationType, out List<INamedTypeSymbol>? list))
          {
            list = new List<INamedTypeSymbol>();
            handlersByNotification[notificationType] = list;
          }

          list.Add(type);
        }
      }
    }

    foreach (List<INamedTypeSymbol> list in handlersByRequest.Values)
    {
      SortByDisplayName(list);
    }

    foreach (List<INamedTypeSymbol> list in handlersByNotification.Values)
    {
      SortByDisplayName(list);
    }

    ImmutableArray<BehaviorRegistration> behaviors = DiscoverBehaviors(
        compilation,
        membership,
        pipelineBehavior2,
        access,
        generatorDiagnostics);
    HashSet<BehaviorRegistration> wovenBehaviors = new();

    foreach (INamedTypeSymbol requestType in requests)
    {
      if (!IsDiscoverableType(requestType) || requestType.Arity > 0)
      {
        continue;
      }

      if (!membership.Includes(requestType) || droppedMessages.Contains(requestType))
      {
        continue;
      }

      handlersByRequest.TryGetValue(requestType, out List<INamedTypeSymbol>? handlerList);
      int handlerCount = handlerList?.Count ?? 0;

      if (handlerCount == 0)
      {
        if (declaredCovered.Contains(requestType))
        {
          if (declaredNames.TryGetValue(requestType, out List<string>? names) && DistinctCount(names) > 1)
          {
            diagnostics.Add(DuplicateDeclared(compilation, requestType, names));
          }

          continue;
        }

        diagnostics.Add(
            Diagnostic.Create(
                DiagnosticDescriptors.RequestHasNoHandler,
                GetSourceLocation(compilation, requestType),
                requestType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
      }
      else if (handlerCount == 1
          && declaredNames.TryGetValue(requestType, out List<string>? declaredNameList)
          && DistinctCount(declaredNameList) > 1)
      {
        diagnostics.Add(DuplicateDeclared(compilation, requestType, declaredNameList));
      }
      else if (handlerCount > 1 && handlerList is not null)
      {
        string names = string.Join(
            ", ",
            handlerList.Select(h => h.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
        diagnostics.Add(
            Diagnostic.Create(
                DiagnosticDescriptors.DuplicateHandler,
                GetSourceLocation(compilation, handlerList[1]),
                requestType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                names));
      }
    }

    ImmutableArray<RequestBinding>.Builder requestBindings = ImmutableArray.CreateBuilder<RequestBinding>();
    foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> pair in handlersByRequest.OrderBy(p => p.Key.ToDisplayString(), System.StringComparer.Ordinal))
    {
      if (pair.Value.Count != 1)
      {
        continue;
      }

      INamedTypeSymbol requestType = pair.Key;
      INamedTypeSymbol handlerType = pair.Value[0];
      ITypeSymbol responseType = responseByRequest[requestType];
      bool isUnit = voidRequests.Contains(requestType);

      INamedTypeSymbol? handlerScope = ResolveScope(handlerType);
      INamedTypeSymbol? requestScope = ResolveScope(requestType);
      ReportScopeMismatch(compilation, handlerType, handlerScope, requestType, requestScope, diagnostics);

      INamedTypeSymbol? scopeType = handlerScope ?? requestScope;
      if (!PipelineScopeAllowed(access, scopeType))
      {
        continue;
      }

      ImmutableArray<INamedTypeSymbol> closedBehaviors = CloseBehaviors(
          behaviors,
          requestType,
          responseType,
          pipelineBehavior2,
          scopeType,
          wovenBehaviors);

      requestBindings.Add(
          new RequestBinding(
              requestType,
              responseType,
              handlerType,
              isUnit,
              closedBehaviors,
              scopeType));
    }

    foreach (BehaviorRegistration registration in behaviors)
    {
      if (wovenBehaviors.Contains(registration))
      {
        continue;
      }

      generatorDiagnostics.Add(
          Diagnostic.Create(
              DiagnosticDescriptors.BehaviorAppliesToNoRequest,
              registration.Location,
              registration.BehaviorType.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
              registration.ScopeType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "unscoped"));
    }

    ImmutableArray<NotificationBinding>.Builder notificationBindings = ImmutableArray.CreateBuilder<NotificationBinding>();
    List<NotificationGroup> groupedNotifications = new();
    foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> pair in handlersByNotification)
    {
      INamedTypeSymbol? notificationScope = ResolveScope(pair.Key);
      foreach (INamedTypeSymbol handler in pair.Value)
      {
        INamedTypeSymbol? handlerScope = ResolveScope(handler);
        ReportScopeMismatch(compilation, handler, handlerScope, pair.Key, notificationScope, diagnostics);
        INamedTypeSymbol? scopeType = handlerScope ?? notificationScope;
        if (!PipelineScopeAllowed(access, scopeType))
        {
          continue;
        }

        NotificationGroup? bucket = null;
        foreach (NotificationGroup candidate in groupedNotifications)
        {
          if (SymbolEqualityComparer.Default.Equals(candidate.Notification, pair.Key)
              && SymbolEqualityComparer.Default.Equals(candidate.Scope, scopeType))
          {
            bucket = candidate;
            break;
          }
        }

        if (bucket is null)
        {
          bucket = new NotificationGroup(pair.Key, scopeType);
          groupedNotifications.Add(bucket);
        }

        bucket.Handlers.Add(handler);
      }
    }

    foreach (NotificationGroup grouped in groupedNotifications
        .OrderBy(g => g.Notification.ToDisplayString(), System.StringComparer.Ordinal)
        .ThenBy(g => g.Scope?.ToDisplayString() ?? string.Empty, System.StringComparer.Ordinal))
    {
      notificationBindings.Add(
          new NotificationBinding(
              grouped.Notification,
              grouped.Handlers.ToImmutableArray(),
              grouped.Scope));
    }

    return new MessageGraph(
        requestBindings.ToImmutable(),
        notificationBindings.ToImmutable(),
        diagnostics.ToImmutableArray(),
        generatorDiagnostics.ToImmutableArray(),
        membership,
        declaredHandlers.ToImmutableArray(),
        declaredDiagnostics.ToImmutableArray());
  }

}
