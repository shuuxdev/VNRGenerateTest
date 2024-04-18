

using Microsoft.AspNetCore.Mvc;
public class HomeController : Controller
{

    public HomeController()
    {

    }
    public IActionResult Index()
    {
        return View();
    }
}
public class A : B,C {
    
}
public class B {
    public virtual int Get() {
        return 1;
    }
}
public interface C {
    int Get();
}