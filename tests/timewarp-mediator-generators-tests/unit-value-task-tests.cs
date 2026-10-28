#region Purpose
// Unit.ValueTask must not declare a static field of type ValueTask<Unit>. Mono WebAssembly rejects that cycle.
#endregion

public class UnitValueTaskTests
{
  [Fact]
  public void Unit_DeclaresNoStaticFieldOfTypeValueTaskOfUnit()
  {
    const BindingFlags flags =
      BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    FieldInfo[] fields = typeof(Unit).GetFields(flags);
    fields.ShouldNotContain(field => field.FieldType == typeof(ValueTask<Unit>));

    PropertyInfo valueTask = typeof(Unit).GetProperty(nameof(Unit.ValueTask), flags).ShouldNotBeNull();
    valueTask.GetMethod.ShouldNotBeNull();
    valueTask.SetMethod.ShouldBeNull();
  }

  [Fact]
  public async Task ValueTask_ErgonomicsMatchTheMigrationPage()
  {
    Task<string> task = Task.FromResult("pong");
    ValueTask<string> wrapped = new(task);
    string wrappedValue = await wrapped;
    wrappedValue.ShouldBe("pong");

    Task<string> trimmed = task.ContinueWith(finished => finished.Result.Trim());
    string trimmedValue = await trimmed;
    trimmedValue.ShouldBe("pong");

    ValueTask<string> left = ValueTask.FromResult("a");
    ValueTask<string> right = ValueTask.FromResult("b");
    string[] both = await Task.WhenAll(left.AsTask(), right.AsTask());
    both.ShouldBe(new[] { "a", "b" });
  }

  [Fact]
  public async Task NuruStyleHandler_ReturnsQualifiedUnitValueTask()
  {
    NuruStyleUnitHandler handler = new();
    Unit unit = await handler.Handle(new NuruStylePing(), CancellationToken.None);
    unit.ShouldBe(Unit.Value);

    ValueTask<Unit> completed = Unit.ValueTask;
    completed.IsCompletedSuccessfully.ShouldBeTrue();
    Unit completedValue = await completed;
    completedValue.ShouldBe(Unit.Value);
  }
}
