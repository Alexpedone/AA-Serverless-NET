using Newtonsoft.Json;

public class Personne
{
    public string Nom { get; set; } = string.Empty;
    public int Age { get; set; }

    public string Hello(bool isLowercase)
    {
        var message = $"hello {Nom}, you are {Age}";
        return isLowercase ? message : message.ToUpperInvariant();
    }
}

