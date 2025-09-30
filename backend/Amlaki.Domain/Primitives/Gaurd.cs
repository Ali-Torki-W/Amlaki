namespace Domain.Primitives;

public static class Guard
{
    public static string NotEmpty(string? value, string name)
        => string.IsNullOrWhiteSpace(value) ? throw new Domain.Exceptions.DomainException($"{name} is required.") : value.Trim();

    public static void Range(bool condition, string message)
    {
        if (!condition) throw new Domain.Exceptions.DomainException(message);
    }
}
