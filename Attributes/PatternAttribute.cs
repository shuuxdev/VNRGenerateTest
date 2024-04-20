
[AttributeUsage(AttributeTargets.Method)]
public class PatternAttribute : Attribute
{
    public PatternAttribute(string pattern)
    {
        this.pattern = pattern;
    }
    public string pattern { get; set; }
}