public class VnrControl
{
    
    public string controlTypeShortname { get; set; }
    //LanguageKey của control, lấy theo property
    public string languageKey { get; set; }
    public string property { get; set; }
    //ID của control trên trình duyệt, nếu k tìm thấy thuộc tính name trong BuilderInfo 
    //sẽ lấy tên property dùng làm ID
    public string name { get; set; }
    //Element sinh ra theo tài liệu của anh Luân
    public string htmlElement { get; set; }


}
public static class VnrControlExtensionMethods
{
    public static IEnumerable<VnrControl> FilterBadElement(this List<VnrControl> models)
    {
        return models.Where(item => (
            !string.IsNullOrEmpty(item.controlTypeShortname) &&
            !string.IsNullOrEmpty(item.languageKey) &&
            !string.IsNullOrEmpty(item.property) &&
            !string.IsNullOrEmpty(item.name) &&
            !string.IsNullOrEmpty(item.htmlElement)
        ));
    }
}