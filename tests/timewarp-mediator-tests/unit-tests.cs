// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class UnitTests
{
  [Fact]
  public async Task Should_be_equal_to_each_other()
  {
    Unit unit1 = Unit.Value;
    Unit unit2 = await Unit.ValueTask;

    Assert.Equal(unit1, unit2);
    Assert.True(unit1 == unit2);
    Assert.False(unit1 != unit2);
  }

  [Fact]
  public void Should_be_equitable()
  {
    Dictionary<Unit, string> dictionary = new()
        {
            {new Unit(), "value"},
        };

    Assert.Equal("value", dictionary[default]);
  }

  [Fact]
  public void Should_tostring()
  {
    Unit unit = Unit.Value;
    Assert.Equal("()", unit.ToString());
  }

  public static object[][] ValueData()
  {
    return new[]
    {
            new object[] {new(), false},
            new object[] {"", false},
            new object[] {"()", false},
            new object[] {null!, false},
            new object[] {new Uri("https://www.google.com"), false},
            new object[] {new Unit(), true},
            new object[] {Unit.Value, true},
            new object[] {default(Unit), true},
        };
  }

  [Theory]
  [MemberData(nameof(ValueData))]
  public void Should_be_equal(object value, bool isEqual)
  {
    Unit unit1 = Unit.Value;

    if (isEqual)
      Assert.True(unit1.Equals(value));
    else
      Assert.False(unit1.Equals(value));
  }
}