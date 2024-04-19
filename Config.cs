public static class Config
{
    public static ModelParsingMode modelParsingMode = ModelParsingMode.StringProcessing;

    public static LoggingMode loggingMode = LoggingMode.All;
}
public enum LoggingMode
{
    Control,
    Model,
    All,
    None
}
public enum Category
{
    Hre,
    Sys,
    Cat,
    Ins,
    All
}
public enum ModelParsingMode
{
    DynamicCompilation,
    StringProcessing
}
public enum TemplatePlaceholder
{
    CLASSNAME_GOES_HERE,
    PAGENAME_GOES_HERE,
    CATEGORY_GOES_HERE,
    PROPERTIES_GOES_HERE,
    XPATH_GOES_HERE

}