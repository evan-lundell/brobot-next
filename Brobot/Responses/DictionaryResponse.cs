namespace Brobot.Responses;

public class DictionaryResponse
{
    public DictionaryMeaning[]? Meanings { get; set; }
}

public class DictionaryMeaning
{
    public string? PartOfSpeech { get; set; }
    public DictionaryDefinition[]? Definitions { get; set; }

}

public class DictionaryDefinition
{
    public string? Definition { get; set; }
}