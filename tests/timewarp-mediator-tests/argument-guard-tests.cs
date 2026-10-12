// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class ArgumentGuardTests
{
  [Fact]
  public void Mediator_constructor_should_reject_null_arguments()
  {
    IServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();

    Should.Throw<ArgumentNullException>(() => new Mediator(null!)).ParamName.ShouldBe("serviceProvider");
    Should.Throw<ArgumentNullException>(() => new Mediator(serviceProvider, null!)).ParamName.ShouldBe("publisher");
  }

  [Fact]
  public void AddMediator_should_reject_null_arguments()
  {
    ServiceCollection services = new();

    Should.Throw<ArgumentNullException>(() => ((IServiceCollection) null!).AddMediator(new MediatorServiceConfiguration())).ParamName.ShouldBe("services");
    Should.Throw<ArgumentNullException>(() => services.AddMediator((MediatorServiceConfiguration) null!)).ParamName.ShouldBe("configuration");
    Should.Throw<ArgumentNullException>(() => services.AddMediator((Action<MediatorServiceConfiguration>) null!)).ParamName.ShouldBe("configuration");
  }

  [Fact]
  public void Configuration_methods_should_reject_null_arguments()
  {
    MediatorServiceConfiguration configuration = new();

    Should.Throw<ArgumentNullException>(() => configuration.RegisterServicesFromAssemblyContaining(null!)).ParamName.ShouldBe("type");
    Should.Throw<ArgumentNullException>(() => configuration.RegisterServicesFromAssembly(null!)).ParamName.ShouldBe("assembly");
    Should.Throw<ArgumentNullException>(() => configuration.RegisterServicesFromAssemblies(null!)).ParamName.ShouldBe("assemblies");
    Should.Throw<ArgumentNullException>(() => configuration.AddBehavior((Type) null!)).ParamName.ShouldBe("implementationType");
    Should.Throw<ArgumentNullException>(() => configuration.AddStreamBehavior((Type) null!)).ParamName.ShouldBe("implementationType");
    Should.Throw<ArgumentNullException>(() => configuration.AddRequestPreProcessor((Type) null!)).ParamName.ShouldBe("implementationType");
    Should.Throw<ArgumentNullException>(() => configuration.AddRequestPostProcessor((Type) null!)).ParamName.ShouldBe("implementationType");
    Should.Throw<ArgumentNullException>(() => configuration.AddOpenBehaviors((IEnumerable<Type>) null!)).ParamName.ShouldBe("openBehaviorTypes");
    Should.Throw<ArgumentNullException>(() => configuration.AddOpenBehaviors((IEnumerable<TimeWarp.Mediator.Entities.OpenBehavior>) null!)).ParamName.ShouldBe("openBehaviors");
  }

  [Fact]
  public void OpenBehavior_should_use_parameter_name_for_null_type()
  {
    Should.Throw<ArgumentNullException>(() => new TimeWarp.Mediator.Entities.OpenBehavior(null!)).ParamName.ShouldBe("openBehaviorType");
  }

  [Fact]
  public void ObjectDetails_should_compare_two_nulls_as_equal()
  {
    Type objectDetailsType = typeof(Mediator).Assembly.GetType("TimeWarp.Mediator.Internal.ObjectDetails", throwOnError: true)!;
    object comparer = Activator.CreateInstance(objectDetailsType, new object())!;
    MethodInfo compare = objectDetailsType.GetMethod("Compare")!;

    compare.Invoke(comparer, new object?[] { null, null }).ShouldBe(0);
  }
}