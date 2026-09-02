// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class GenericRequestHandlerTests : BaseGenericRequestHandlerTests
{

    [Theory]
    [InlineData(9, 3, 3)]
    [InlineData(10, 4, 4)]
    [InlineData(1, 1, 1)]
    [InlineData(50, 3, 3)]
    public void ShouldResolveAllCombinationsOfGenericHandler(int numberOfClasses, int numberOfInterfaces, int numberOfTypeParameters)
    {
        ServiceCollection services = new();

        AssemblyBuilder dynamicAssembly = GenerateCombinationsTestAssembly(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);

        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(dynamicAssembly);
            cfg.RegisterGenericHandlers = true;
        });

        ServiceProvider provider = services.BuildServiceProvider();

        Type dynamicRequestType = dynamicAssembly.GetType("DynamicRequest")!;

        int expectedCombinations = CalculateTotalCombinations(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);

        Type[] testClasses = Enumerable.Range(1, numberOfClasses)
            .Select(i => dynamicAssembly.GetType($"TestClass{i}")!)
            .ToArray();

        List<Type[]> combinations = GenerateCombinations(testClasses, numberOfInterfaces);          

        foreach (Type[] combination in combinations)
        {
            Type concreteRequestType = dynamicRequestType.MakeGenericType(combination);
            Type requestHandlerInterface = typeof(IRequestHandler<>).MakeGenericType(concreteRequestType);

            object? handler = provider.GetService(requestHandlerInterface);
            handler.ShouldNotBeNull($"Handler for {concreteRequestType} should not be null");
        }            
    }

    [Theory]
    [InlineData(9, 3, 3)]
    [InlineData(10, 4, 4)]
    [InlineData(1, 1, 1)]
    [InlineData(50, 3, 3)]
    public void ShouldRegisterTheCorrectAmountOfHandlers(int numberOfClasses, int numberOfInterfaces, int numberOfTypeParameters)
    {
        AssemblyBuilder dynamicAssembly = GenerateCombinationsTestAssembly(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);          
        int expectedCombinations = CalculateTotalCombinations(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);
        Type[] testClasses = Enumerable.Range(1, numberOfClasses)
           .Select(i => dynamicAssembly.GetType($"TestClass{i}")!)
           .ToArray();
        List<Type[]> combinations = GenerateCombinations(testClasses, numberOfInterfaces);
        combinations.Count.ShouldBe(expectedCombinations, $"Should have tested all {expectedCombinations} combinations");
    }

    [Theory]
    [InlineData(9, 3, 3)]
    [InlineData(10, 4, 4)]
    [InlineData(1, 1, 1)]
    [InlineData(50, 3, 3)]
    public void ShouldNotRegisterDuplicateHandlers(int numberOfClasses, int numberOfInterfaces, int numberOfTypeParameters)
    {
        AssemblyBuilder dynamicAssembly = GenerateCombinationsTestAssembly(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);
        int expectedCombinations = CalculateTotalCombinations(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);
        Type[] testClasses = Enumerable.Range(1, numberOfClasses)
           .Select(i => dynamicAssembly.GetType($"TestClass{i}")!)
           .ToArray();
        List<Type[]> combinations = GenerateCombinations(testClasses, numberOfInterfaces);
        bool hasDuplicates = combinations
          .Select(x => string.Join(", ", x.Select(y => y.Name)))
          .GroupBy(x => x)
          .Any(g => g.Count() > 1);

        hasDuplicates.ShouldBeFalse();
    }

    [Fact]
    public void ShouldThrowExceptionWhenTypesClosingExceedsMaximum()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Logger());

        Assembly assembly = GenerateTypesClosingExceedsMaximumAssembly();

        Should.Throw<ArgumentException>(() =>
        {
            services.AddMediator(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.RegisterGenericHandlers = true;
            });
        })
        .Message.ShouldContain("One of the generic type parameter's count of types that can close exceeds the maximum length allowed");
    }

    [Fact]
    public void ShouldThrowExceptionWhenGenericHandlerRegistrationsExceedsMaximum()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Logger());

        Assembly assembly = GenerateHandlerRegistrationsExceedsMaximumAssembly();

        Should.Throw<ArgumentException>(() =>
        {
            services.AddMediator(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.RegisterGenericHandlers = true;
            });
        })
        .Message.ShouldContain("The total number of generic type registrations exceeds the maximum allowed");
    }

    [Fact]
    public void ShouldThrowExceptionWhenGenericTypeParametersExceedsMaximum()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Logger());

        Assembly assembly = GenerateGenericTypeParametersExceedsMaximumAssembly();

        Should.Throw<ArgumentException>(() =>
        {
            services.AddMediator(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.RegisterGenericHandlers = true;
            });
        })
        .Message.ShouldContain("The number of generic type parameters exceeds the maximum allowed");
    }

    [Fact]
    public void ShouldThrowExceptionWhenTimeoutOccurs()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Logger());

        Assembly assembly = GenerateTimeoutOccursAssembly();

        Should.Throw<TimeoutException>(() =>
        {
            services.AddMediator(cfg =>
            {
                cfg.MaxGenericTypeParameters = 0;
                cfg.MaxGenericTypeRegistrations = 0;
                cfg.MaxTypesClosing = 0;
                cfg.RegistrationTimeout = 1000;
                cfg.RegisterGenericHandlers = true;
                cfg.RegisterServicesFromAssembly(assembly);
            });
        })
        .Message.ShouldBe("The generic handler registration process timed out.");
    }

    [Fact]
    public void ShouldNotRegisterGenericHandlersWhenOptingOut()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Logger());

        Assembly assembly = GenerateOptOutAssembly();
        services.AddMediator(cfg =>
        {
            //opt out flag set
            cfg.RegisterGenericHandlers = false;
            cfg.RegisterServicesFromAssembly(assembly);
        });

        ServiceProvider provider = services.BuildServiceProvider();
        Type[] testClasses = Enumerable.Range(1, 2)
            .Select(i => assembly.GetType($"TestClass{i}")!)
            .ToArray();
        Type requestType = assembly.GetType("OptOutRequest")!;
        List<Type[]> combinations = GenerateCombinations(testClasses, 2);

        Type concreteRequestType = requestType.MakeGenericType(combinations.First());
        Type requestHandlerInterface = typeof(IRequestHandler<>).MakeGenericType(concreteRequestType);

        object? handler = provider.GetService(requestHandlerInterface);
        handler.ShouldBeNull($"Handler for {concreteRequestType} should be null");


    }
}
