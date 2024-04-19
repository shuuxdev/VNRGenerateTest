public class ResultModel
{
    public string followingName { get; set; }
    public string viewName { get; set; }
    public string category { get; set; }
    public string className { get; set; }
    public string fieldType { get; set; }
    public string pageType { get; set; }

    public string languageKey { get; set; }

    public string fieldName { get; set; }

    public string name { get; set; }

    public string htmlElement { get; set; }


}
public static class ResultModeLExtensionMethods
{
    public static IEnumerable<ResultModel> FilterBadElement(this List<ResultModel> models)
    {
        return models.Where(item => (
            !string.IsNullOrEmpty(item.followingName) &&
            !string.IsNullOrEmpty(item.viewName) &&
            !string.IsNullOrEmpty(item.category) &&
            !string.IsNullOrEmpty(item.className) &&
            !string.IsNullOrEmpty(item.fieldType) &&
            !string.IsNullOrEmpty(item.pageType) &&
            !string.IsNullOrEmpty(item.languageKey) &&
            !string.IsNullOrEmpty(item.fieldName) &&
            !string.IsNullOrEmpty(item.name) &&
            !string.IsNullOrEmpty(item.htmlElement)
        ));
    }
}