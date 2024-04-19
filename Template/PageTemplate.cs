using AutomationBase;
using AutomationBase.Extensions;
using AutomationBase.Helper;
using AutomationBase.Models;
using HRMMain.Common;
using HRMMain.Extensition;
using HRMMain.Object;
using HRMMain.Object.CATEGORY_GOES_HERE;
using HRMMain.Pages.Interface;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebElement = AutomationBase.WebElement;

namespace HRMMain.Pages.CATEGORY_GOES_HERE
{
    public class CLASSNAME_GOES_HEREPage : BasePage, IPage<CLASSNAME_GOES_HEREObject>
    {
        public CLASSNAME_GOES_HEREPage(IWebDriver webDriver, Logger logger) : base(webDriver, logger)
        {
            PageUrl = "CATEGORY_GOES_HERE_OrgReward/Index";
        }

        #region Webelement

        XPATH_GOES_HERE

        #endregion


        public bool CheckClearField(CLASSNAME_GOES_HEREObject obj)
        {
            throw new NotImplementedException();
        }

        public bool CheckDataIsExistBeforRun(CLASSNAME_GOES_HEREObject obj)
        {
            return CheckDataIsExistBeforRunTestCase(this, obj).Success;
        }

        public bool CheckDuplicateBeforCreate(CLASSNAME_GOES_HEREObject obj)
        {
            return CheckDuplicateDataBeforeCreate(this, obj).Success;
        }

        public bool CheckEnterOverflowValueAndFormat(CLASSNAME_GOES_HEREObject obj)
        {
            return AutomaticCheckEnterOverflowValueAndFormat(this, obj, btn_TM_Luu);
        }

        public bool CheckGridPager()
        {
            return CheckGridPagerUtility().Success;
        }

        public bool CheckLoadDataAfterSave(CLASSNAME_GOES_HEREObject obj)
        {
            return CheckLoadDataAfterCreate(this, obj).Success;
        }

        public bool CheckRequiredField(CLASSNAME_GOES_HEREObject obj)
        {
            return AutomaticCheckRequiredField(this, obj, btn_TM_Luu);
        }

        public bool Delete(params string[] records)
        {
            throw new NotImplementedException();
        }

        public bool InputCreate(CLASSNAME_GOES_HEREObject obj, bool onlyFillRequired = false, bool useObjectValue = false)
        {
            AutomaticInputCreate(this, obj, onlyFillRequired, useObjectValue);
            Thread.Sleep(1000);

            return true;
        }

        public bool InputSearch(CLASSNAME_GOES_HEREObject obj)
        {
            return AutomaticInputSearch(this, obj).Success;
        }

        public ActionResult Save(CLASSNAME_GOES_HEREObject obj, string option = "")
        {
            return CheckSaveAction(this, obj, option,
                btn_TM_Luu, btn_TM_LuuVaTaoMoi, btn_TM_LuuVaDong, btn_TM_LuuVaGuiEmail);
        }

        public bool Search(CLASSNAME_GOES_HEREObject obj, params string[] records)
        {
            return AutomaticSearch(this, obj);
        }

        public bool Update(CLASSNAME_GOES_HEREObject obj)
        {
            return AutomaticUpdate(this, obj, CommonVariable.SAVE_AND_CLOSE, btnLuuVaDongXpath, true);
        }
    }
}
