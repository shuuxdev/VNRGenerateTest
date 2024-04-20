using AutomationBase;
using AutomationBase.Extensions;
using AutomationBase.Helper;
using AutomationBase.Models;
using HRMMain.Common;
using HRMMain.Extensition;
using HRMMain.Object;
using HRMMain.Object.Hre;
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

namespace HRMMain.Pages.Hre
{
    public class Hre_PassportSearchModelPage : BasePage, IPage<Hre_PassportSearchModelObject>
    {
        public Hre_PassportSearchModelPage(IWebDriver webDriver, Logger logger) : base(webDriver, logger)
        {
            PageUrl = "Hre_OrgReward/Index";
        }

        #region Webelement

        [LanguageKeyMapping("ProfileIDs")]
        public WebElement sml_TK_ProfileIDs => FindWebElement(By.XPath("//span[@id='SProProfileID']"));
[LanguageKeyMapping("ProfileName")]
        public WebElement txt_TK_ProfileName => FindWebElement(By.XPath("//input[@id='SProProfileID']"));
[LanguageKeyMapping("CodeEmp")]
        public WebElement txt_TK_CodeEmp => FindWebElement(By.XPath("//input[@id='SProProfileID']"));
[LanguageKeyMapping("PositionID")]
        public WebElement sml_TK_PositionID => FindWebElement(By.XPath("//span[@id='PositionID']"));
[LanguageKeyMapping("JobTitleID")]
        public WebElement sml_TK_JobTitleID => FindWebElement(By.XPath("//span[@id='JobTitleID']"));
[LanguageKeyMapping("PassportNo")]
        public WebElement txt_TK_PassportNo => FindWebElement(By.XPath("//input[@id='PassportNo']"));
[LanguageKeyMapping("StatusSyn")]
        public WebElement sml_TK_StatusSyn => FindWebElement(By.XPath("//span[@id='Hre_Passport_StatusSyn']"));
[LanguageKeyMapping("HRM_HR_Passport_IsLastPassport")]
        public WebElement ckb_TK_IsLastPassport => FindWebElement(By.XPath("//input[@id='Hre_Passport_StatusSyn']"));
[LanguageKeyMapping("HRM_HR_Passport_IsProfileQuit")]
        public WebElement ckb_TK_IsProfileQuit => FindWebElement(By.XPath("//input[@id='IsProfileQuit']"));
[LanguageKeyMapping("HRM_HR_Passport_IsPassportNew")]
        public WebElement ckb_TK_IsPassportNew => FindWebElement(By.XPath("//input[@id='IsPassportNew']"));
[LanguageKeyMapping("")]
        public WebElement ddl_TK_ => FindWebElement(By.XPath("//input[@id='ddlScreenName']"));
[LanguageKeyMapping("")]
        public WebElement TreeViewDropdDownBuilderInfo_TK_OrgStructureID => FindWebElement(By.XPath("//input[@id='OrgStructureID']"));


        #endregion


        public bool CheckClearField(Hre_PassportSearchModelObject obj)
        {
            throw new NotImplementedException();
        }

        public bool CheckDataIsExistBeforRun(Hre_PassportSearchModelObject obj)
        {
            return CheckDataIsExistBeforRunTestCase(this, obj).Success;
        }

        public bool CheckDuplicateBeforCreate(Hre_PassportSearchModelObject obj)
        {
            return CheckDuplicateDataBeforeCreate(this, obj).Success;
        }

        public bool CheckEnterOverflowValueAndFormat(Hre_PassportSearchModelObject obj)
        {
            return AutomaticCheckEnterOverflowValueAndFormat(this, obj, btn_TM_Luu);
        }

        public bool CheckGridPager()
        {
            return CheckGridPagerUtility().Success;
        }

        public bool CheckLoadDataAfterSave(Hre_PassportSearchModelObject obj)
        {
            return CheckLoadDataAfterCreate(this, obj).Success;
        }

        public bool CheckRequiredField(Hre_PassportSearchModelObject obj)
        {
            return AutomaticCheckRequiredField(this, obj, btn_TM_Luu);
        }

        public bool Delete(params string[] records)
        {
            throw new NotImplementedException();
        }

        public bool InputCreate(Hre_PassportSearchModelObject obj, bool onlyFillRequired = false, bool useObjectValue = false)
        {
            AutomaticInputCreate(this, obj, onlyFillRequired, useObjectValue);
            Thread.Sleep(1000);

            return true;
        }

        public bool InputSearch(Hre_PassportSearchModelObject obj)
        {
            return AutomaticInputSearch(this, obj).Success;
        }

        public ActionResult Save(Hre_PassportSearchModelObject obj, string option = "")
        {
            return CheckSaveAction(this, obj, option,
                btn_TM_Luu, btn_TM_LuuVaTaoMoi, btn_TM_LuuVaDong, btn_TM_LuuVaGuiEmail);
        }

        public bool Search(Hre_PassportSearchModelObject obj, params string[] records)
        {
            return AutomaticSearch(this, obj);
        }

        public bool Update(Hre_PassportSearchModelObject obj)
        {
            return AutomaticUpdate(this, obj, CommonVariable.SAVE_AND_CLOSE, btnLuuVaDongXpath, true);
        }
    }
}
