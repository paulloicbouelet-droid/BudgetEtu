namespace BudgetEtu.Models;

public record UserProfile(string FullName, string Email)
{
    public string FirstName => FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? FullName;

    /// <summary>« Paul Loïc » -> « PL »</summary>
    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpperInvariant(),
                _ => $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            };
        }
    }
}
