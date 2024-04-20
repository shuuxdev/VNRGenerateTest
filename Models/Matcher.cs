using Microsoft.VisualBasic.FileIO;


public class Matcher
{
    public string Pattern { get; set; }
    public delegate Task<List<VnrControl>> PatternMatcher(string cshtmlPath, string modelPath, string pattern);

    public PatternMatcher Method;

}