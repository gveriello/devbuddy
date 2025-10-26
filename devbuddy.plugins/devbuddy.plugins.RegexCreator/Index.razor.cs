using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

public class RegexCreatorBase : ComponentBase
{
    protected string Pattern { get; set; }
    protected string TestInput { get; set; }
    protected bool IgnoreCase { get; set; }
    protected bool Multiline { get; set; }

    protected List<Match> Matches { get; set; }
    protected string ErrorMessage { get; set; }

    protected void TestRegex()
    {
        Matches = null;
        ErrorMessage = null;
        try
        {
            var options = RegexOptions.None;
            if (IgnoreCase) options |= RegexOptions.IgnoreCase;
            if (Multiline) options |= RegexOptions.Multiline;
            var regex = new Regex(Pattern, options);
            Matches = regex.Matches(TestInput).Cast<Match>().ToList();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        StateHasChanged();
    }
}
