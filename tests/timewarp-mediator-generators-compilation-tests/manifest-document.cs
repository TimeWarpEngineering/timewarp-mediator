#region Purpose
// Parses the generated MediatorManifest.Json so tests assert on fields, not exact substrings.
#endregion

#region Design
// Linked into the generator runtime tests as well. Missing fields throw (JsonElement.GetProperty),
// so a renamed or dropped key fails the test instead of silently matching nothing.
#endregion

using System.Text.Json;

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

internal sealed class ManifestDocument
{
  private ManifestDocument(
      int version,
      string profile,
      string generatedNamespace,
      IReadOnlyList<ManifestRequest> requests,
      IReadOnlyList<ManifestNotification> notifications,
      IReadOnlyList<string> membership)
  {
    Version = version;
    Profile = profile;
    Namespace = generatedNamespace;
    Requests = requests;
    Notifications = notifications;
    Membership = membership;
  }

  public int Version { get; }

  public string Profile { get; }

  public string Namespace { get; }

  public IReadOnlyList<ManifestRequest> Requests { get; }

  public IReadOnlyList<ManifestNotification> Notifications { get; }

  public IReadOnlyList<string> Membership { get; }

  public IEnumerable<string> Handlers =>
      Requests.Select(r => r.Handler).Concat(Notifications.SelectMany(n => n.Handlers));

  public static ManifestDocument Parse(string json)
  {
    using JsonDocument document = JsonDocument.Parse(json);
    JsonElement root = document.RootElement;
    return new ManifestDocument(
        root.GetProperty("version").GetInt32(),
        root.GetProperty("profile").GetString()!,
        root.GetProperty("namespace").GetString()!,
        root.GetProperty("requests").EnumerateArray().Select(ReadRequest).ToArray(),
        root.GetProperty("notifications").EnumerateArray().Select(ReadNotification).ToArray(),
        Strings(root.GetProperty("membership")));
  }

  public ManifestRequest Request(string requestType)
  {
    ManifestRequest[] matches = Requests.Where(r => r.Request == requestType).ToArray();
    if (matches.Length != 1)
    {
      throw new InvalidOperationException(
          "Expected one manifest request '" + requestType + "' but found " + matches.Length + ": "
          + string.Join(", ", Requests.Select(r => r.Request)));
    }

    return matches[0];
  }

  public ManifestNotification Notification(string notificationType)
  {
    ManifestNotification[] matches = Notifications.Where(n => n.Notification == notificationType).ToArray();
    if (matches.Length != 1)
    {
      throw new InvalidOperationException(
          "Expected one manifest notification '" + notificationType + "' but found " + matches.Length + ": "
          + string.Join(", ", Notifications.Select(n => n.Notification)));
    }

    return matches[0];
  }

  private static ManifestRequest ReadRequest(JsonElement element)
  {
    return new ManifestRequest(
        element.GetProperty("request").GetString()!,
        element.GetProperty("response").GetString()!,
        element.GetProperty("handler").GetString()!,
        element.GetProperty("kind").GetString()!,
        element.TryGetProperty("scope", out JsonElement scope) ? scope.GetString() : null,
        Strings(element.GetProperty("behaviors")));
  }

  private static ManifestNotification ReadNotification(JsonElement element)
  {
    return new ManifestNotification(
        element.GetProperty("notification").GetString()!,
        element.TryGetProperty("scope", out JsonElement scope) ? scope.GetString() : null,
        Strings(element.GetProperty("handlers")));
  }

  private static string[] Strings(JsonElement array)
  {
    return array.EnumerateArray().Select(e => e.GetString()!).ToArray();
  }
}

internal sealed class ManifestRequest
{
  public ManifestRequest(
      string request,
      string response,
      string handler,
      string kind,
      string? scope,
      IReadOnlyList<string> behaviors)
  {
    Request = request;
    Response = response;
    Handler = handler;
    Kind = kind;
    Scope = scope;
    Behaviors = behaviors;
  }

  public string Request { get; }

  public string Response { get; }

  public string Handler { get; }

  public string Kind { get; }

  public string? Scope { get; }

  public IReadOnlyList<string> Behaviors { get; }
}

internal sealed class ManifestNotification
{
  public ManifestNotification(string notification, string? scope, IReadOnlyList<string> handlers)
  {
    Notification = notification;
    Scope = scope;
    Handlers = handlers;
  }

  public string Notification { get; }

  public string? Scope { get; }

  public IReadOnlyList<string> Handlers { get; }
}