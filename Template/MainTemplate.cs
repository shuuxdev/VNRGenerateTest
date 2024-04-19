using AutomationBase.Extensions;
using HRMMain.Extensition;
using HRMMain.Object.CATEGORY_GOES_HERE;
using HRMMain.Object;
using HRMMain.Pages.CATEGORY_GOES_HERE;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationBase.Helper;
using HRMMain.Common;
using System.Data;

namespace HRMMain.Test.CATEGORY_GOES_HERE.CLASSNAME_GOES_HERE
{
    [TestFixture]
    [Category("Common Test")]
    [Parallelizable(ParallelScope.All)]
    [DefineTestInfomation(Module: "CATEGORY_GOES_HERE"
        , Feature: "Quản lý dữ liệu liên quan đến quá trình làm việc của nhân viên"
        , TestSuit: "Quản lý quá trình làm việc"
        , TestScenario: "PAGENAME_GOES_HERE - Chức năng Thêm mới"
        , ScenarioDescription: "Kiểm tra chức năng tạo mới dữ liệu và các chức năng (ràng buộc) liên quan")]
    public class TaoMoi : MainSetup
    {
        [SetUp]
        public void SetUpTaoMoi()
        {
            CLASSNAME_GOES_HEREObject obj = new CLASSNAME_GOES_HEREObject();
            var dataTest = DataTestHelper.GetDataTestByID(this.TestCaseID);

            if (!AppSettings.DataTestConfig_CreateNewData)
            {
                CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
                if (this.TestCaseID == "CATEGORY_GOES_HERE133-001" || this.TestCaseID == "CATEGORY_GOES_HERE133-002"
                || this.TestCaseID == "CATEGORY_GOES_HERE133-003" || this.TestCaseID == "CATEGORY_GOES_HERE133-004")
                {
                    this._logger.Precondition("Đảm bảo dữ liệu tạo mới KHÔNG tồn tại (trùng lặp) trong hệ thống.");
                    if (dataTest == null)
                    {
                        this._logger.ChildError($"Vui lòng bổ sung data test cho Testcase [{this.TestCaseID}]");
                        Assert.Fail();
                    }
                    obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.Encode(dataTest);

                    this._logger.ChildInformation("Vào màn hình");
                    page.GoToPage();
                    this._logger.ChildInformation("Kiểm tra dữ liệu sẽ tạo mới có tồn tại trong hệ thống không?");
                    page.CheckDuplicateBeforCreate(obj);
                }
                if (this.TestCaseID == "CATEGORY_GOES_HERE133-006")
                {
                    obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.Encode(dataTest);

                    this._logger.Precondition("Đảm bảo tồn tại 1 dòng dữ liệu trong hệ thống để tạo trùng.");
                    this._logger.ChildInformation("Vào màn hình");
                    page.GoToPage();

                    this._logger.ChildInformation("Nhấn button Tạo mới");
                    page.PressCreate();

                    this._logger.ChildInformation("Nhập dữ liệu tạo mới");
                    page.InputCreate(obj, true, false);

                    this._logger.Step("Kiểm tra việc lưu");
                    page.Save(obj, CommonVariable.SAVE);
                }
            }
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-001", "Thêm mới khen thưởng phòng ban thành công và đóng popUp thêm mới")]
        public void CATEGORY_GOES_HERE133_001()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            var obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.GetDataTestByID(TestCaseID);

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Nhấn button Tạo mới");
            page.PressCreate();

            this._logger.Step("Nhập dữ liệu vào các trường trên form tạo mới");
            page.InputCreate(obj);

            this._logger.Step("Nhấn Lưu và đóng");
            var saveResult = page.Save(obj);

            if (saveResult.Success)
            {
                this._logger.Step("Kiểm tra việc lưu và load dữ liệu sau khi tạo mới");
                page.CheckLoadDataAfterSave(obj);
            }
            else
            {
                this._logger.ChildError($"Xảy ra lỗi trong việc kiểm tra lưu. Lỗi [{saveResult.Message}].");
            }

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-002", "Thêm mới khen thưởng phòng ban thành công và điều chỉnh dữ liệu sau khi lưu")]
        public void CATEGORY_GOES_HERE133_002()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            var obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.GetDataTestByID(TestCaseID);

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Nhấn button Tạo mới");
            page.PressCreate();

            this._logger.Step("Nhập dữ liệu vào các trường trên form tạo mới");
            page.InputCreate(obj);

            this._logger.Step("Nhấn Lưu");
            var saveResult = page.Save(obj, CommonVariable.SAVE);

            //Có thể xảy ra lỗi khi clear field nhưng lưu mới vẫn thành công.
            if (saveResult.Success || saveResult.IsContinute)
            {
                this._logger.Step("Kiểm tra việc lưu và load dữ liệu sau khi tạo mới");
                page.CheckLoadDataAfterSave(obj);
            }
            else
            {
                this._logger.ChildError($"Xảy ra lỗi trong việc kiểm tra lưu. Lỗi [{saveResult.Message}].");
            }

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-003", "Thêm mới khen thưởng phòng ban thành công và tiếp tục thêm mới")]
        public void CATEGORY_GOES_HERE133_003()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            var obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.GetDataTestByID(TestCaseID);

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Nhấn button Tạo mới");
            page.PressCreate();

            this._logger.Step("Nhập dữ liệu vào các trường trên form tạo mới");
            page.InputCreate(obj);

            this._logger.Step("Nhấn Lưu và tạo mới");
            var saveResult = page.Save(obj, CommonVariable.SAVE_AND_RENEW);

            //Có thể xảy ra lỗi khi clear field nhưng lưu mới vẫn thành công.
            if (saveResult.Success || saveResult.IsContinute)
            {
                this._logger.Step("Kiểm tra việc lưu và load dữ liệu sau khi tạo mới");
                page.CheckLoadDataAfterSave(obj);
            }
            else
            {
                this._logger.ChildError($"Xảy ra lỗi trong việc kiểm tra lưu. Lỗi [{saveResult.Message}].");
            }

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }
        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-004", "Thêm mới khen thưởng phòng ban thành công và gửi email")]
        public void CATEGORY_GOES_HERE133_004()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            var obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.GetDataTestByID(TestCaseID);

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Nhấn button Tạo mới");
            page.PressCreate();

            this._logger.Step("Nhập dữ liệu vào các trường trên form tạo mới");
            page.InputCreate(obj);

            this._logger.Step("Nhấn Lưu và gửi email");
            var saveResult = page.Save(obj, CommonVariable.SAVE_AND_SENDEMAIL);

            if (saveResult.Success)
            {
                this._logger.Step("Kiểm tra việc lưu và load dữ liệu sau khi tạo mới");
                page.CheckLoadDataAfterSave(obj);
            }
            else
            {
                this._logger.ChildError($"Xảy ra lỗi trong việc kiểm tra lưu. Lỗi [{saveResult.Message}].");
            }

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-005", "Hiển thị thông báo thành công khi nhấn lưu liên tục 2 lần")]
        public void CATEGORY_GOES_HERE133_005()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            CLASSNAME_GOES_HEREObject obj = new CLASSNAME_GOES_HEREObject();

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Nhấn button Tạo mới");
            page.PressCreate();

            this._logger.Step("Nhập dữ liệu vào các trường bắt buộc trên form tạo mới");
            page.InputCreate(obj, true, false);

            this._logger.Step("Nhấn Lưu liên tục 2 lần");
            page.Save(obj, CommonVariable.SAVE_DOUBLE_CLICK);
            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-006", "Hiển thị cảnh báo với các trường bắt buộc nhập bị bỏ trống khi thêm mới khen thưởng phòng ban")]
        public void CATEGORY_GOES_HERE133_006()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            CLASSNAME_GOES_HEREObject obj = new CLASSNAME_GOES_HEREObject();

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Nhấn button Tạo mới");
            page.PressCreate();

            this._logger.Information("Bắt đầu kiểm tra cảnh báo với các trường bắt buộc nhập");
            page.CheckRequiredField(obj);

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-007", "Hiển thị thông báo trùng dữ liệu khi nhập dữ liệu đã tồn tại trong hệ thống")]
        public void CATEGORY_GOES_HERE133_007()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            CLASSNAME_GOES_HEREObject obj = new CLASSNAME_GOES_HEREObject();

            this._logger.ChildInformation("Vào màn hình");
            page.GoToPage();

            this._logger.ChildInformation("Nhấn button Tạo mới");
            page.PressCreate();

            this._logger.ChildInformation("Nhập dữ liệu tạo mới trùng với dữ liệu đã có trong hệ thống.");
            page.InputCreate(obj, true, true);

            this._logger.Step("Kiểm tra việc nhấn Lưu có cảnh báo trùng hay không?");
            page.Save(obj, CommonVariable.SAVE_DUPLICATE);

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-008", "Hiển thị thông báo vượt quá giới hạn ký tự khi nhập tràn ký tự mỗi field")]
        public void CATEGORY_GOES_HERE133_008()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            CLASSNAME_GOES_HEREObject obj = new CLASSNAME_GOES_HEREObject();

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Nhấn button Tạo mới");
            page.PressCreate();

            this._logger.Step("Kiểm tra cảnh báo nhập tràn ký tự/ sai format với các field");
            page.CheckEnterOverflowValueAndFormat(obj);

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }
    }


    [TestFixture]
    [Category("Common Test")]
    [Parallelizable(ParallelScope.All)]
    [DefineTestInfomation(Module: "CATEGORY_GOES_HERE"
        , Feature: "Quản lý dữ liệu liên quan đến quá trình làm việc của nhân viên"
        , TestSuit: "Quản lý quá trình làm việc"
        , TestScenario: "PAGENAME_GOES_HERE - Chức năng Lọc dữ liệu (Tìm kiếm)"
        , ScenarioDescription: "Kiểm tra chức năng tìm kiếm (lọc) dữ liệu và các chức năng (ràng buộc) liên quan")]
    public class TimKiem : MainSetup
    {
        [SetUp]
        public void SetUpTimKiem()
        {
            CLASSNAME_GOES_HEREObject obj = new CLASSNAME_GOES_HEREObject();
            var dataTest = DataTestHelper.GetDataTestByID(this.TestCaseID);
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            if (this.TestCaseID == "CATEGORY_GOES_HERE133-009")
            {
                if (!AppSettings.DataTestConfig_CreateNewData)
                {
                    if (dataTest == null)
                    {
                        this._logger.ChildError($"Vui lòng bổ sung data test cho Testcase [{this.TestCaseID}]");
                        Assert.Fail();
                    }

                    this._logger.Precondition("Đảm bảo tồn tại ít nhất 1 dòng dữ liệu trong hệ thống để cập nhật.");
                    this._logger.ChildInformation("Vào màn hình");

                    //Kiểm tra dữ liệu có tồn tại trước khi cập nhật không
                    var result = page.CheckDataIsExistBeforRun(obj);
                    if (!result)
                        TestCaseIsBlock();
                }
                else
                {
                    this._logger.Precondition("Đảm bảo tồn tại ít nhất 1 dòng dữ liệu trong hệ thống để tìm kiếm.");
                    this._logger.ChildInformation("Tiến hành tạo mới (Theo cấu hình Tạo mới data)");
                    this._logger.ChildInformation("Vào màn hình");
                    page.GoToPage();

                    this._logger.ChildInformation("Nhấn button Tạo mới");
                    page.PressCreate();

                    this._logger.ChildInformation("Nhập dữ liệu tạo mới");
                    page.InputCreate(obj);

                    this._logger.ChildInformation("Kiểm tra việc lưu");
                    var saveResult = page.Save(obj, CommonVariable.SAVE_SETUP_DATA);
                    if (!saveResult.IsContinute)
                    {
                        this._logger.ChildError($"Lưu gặp lỗi - Message: {saveResult.Message}", true, null);
                        TestCaseIsBlock();
                    }
                }
            }
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-009", "Hiển thị đúng dữ liệu theo điều kiện lọc truyền vào")]
        public void CATEGORY_GOES_HERE133_009()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            var obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.GetDataTestByID(TestCaseID);

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Information("Bắt đầu testcase cập nhật");
            page.Update(obj);

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }
        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-010", "Tự chuyển về trang 1 khi tìm kiếm ở trang khác trang 1")]
        public void CATEGORY_GOES_HERE133_010()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Kiểm tra: Tự chuyển về trang 1 khi tìm kiếm ở trang khác trang 1");
            page.CheckGridPager();
        }
    }

    [TestFixture]
    [Category("Common Test")]
    [Parallelizable(ParallelScope.All)]
    [DefineTestInfomation(Module: "CATEGORY_GOES_HERE"
    , Feature: "Quản lý dữ liệu liên quan đến quá trình làm việc của nhân viên"
    , TestSuit: "Quản lý quá trình làm việc"
    , TestScenario: "PAGENAME_GOES_HERE - Chức năng Cập nhật"
    , ScenarioDescription: "Kiểm tra chức năng cập nhật dữ liệu và các chức năng (ràng buộc) liên quan")]
    public class CapNhat : MainSetup
    {
        [SetUp]
        public void SetUpCapNhat()
        {
            var dataTest = DataTestHelper.GetDataTestByID(this.TestCaseID);
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            CLASSNAME_GOES_HEREObject obj = new CLASSNAME_GOES_HEREObject();
            if (!AppSettings.DataTestConfig_CreateNewData)
            {
                if (dataTest == null)
                {
                    this._logger.ChildError($"Vui lòng bổ sung data test cho Testcase [{this.TestCaseID}]");
                    Assert.Fail();
                }
                obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.Encode(dataTest);

                this._logger.Precondition("Đảm bảo tồn tại ít nhất 1 dòng dữ liệu trong hệ thống để cập nhật.");
                this._logger.ChildInformation("Vào màn hình");
                page.GoToPage();

                //Kiểm tra dữ liệu có tồn tại trước khi cập nhật không
                var result = page.CheckDataIsExistBeforRun(obj);
                if (!result)
                    TestCaseIsBlock();
            }
            else
            {
                this._logger.Precondition("Đảm bảo tồn tại ít nhất 1 dòng dữ liệu trong hệ thống để cập nhật.");
                this._logger.ChildInformation("Tiến hành tạo mới (Theo cấu hình Tạo mới data)");
                this._logger.ChildInformation("Vào màn hình");
                page.GoToPage();

                this._logger.ChildInformation("Nhấn button Tạo mới");
                page.PressCreate();

                this._logger.ChildInformation("Nhập dữ liệu tạo mới");
                page.InputCreate(obj);

                this._logger.ChildInformation("Kiểm tra việc lưu");
                var saveResult = page.Save(obj, CommonVariable.SAVE_SETUP_DATA);
                if (!saveResult.IsContinute)
                {
                    this._logger.ChildError($"Lưu gặp lỗi - Message: {saveResult.Message}", true, null);
                    TestCaseIsBlock();
                }
            }
        }

        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-012", "Cập nhật thông tin khen thưởng phòng ban thành công")]
        public void CATEGORY_GOES_HERE133_012()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);
            var obj = ConverDataTest<CLASSNAME_GOES_HEREObject>.GetDataTestByID(TestCaseID);

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Information("Bắt đầu testcase cập nhật");
            page.Update(obj);

            //Kiểm tra kết quả TCs
            page.CheckTestCaseResult();
        }
        [Test]
        [DefineTC("CATEGORY_GOES_HERE133-013", "Hiển thị thông báo trùng dữ liệu khi cập nhật trùng dữ liệu đã tồn tại trong hệ thống")]
        public void CATEGORY_GOES_HERE133_013()
        {
            CLASSNAME_GOES_HEREPage page = new CLASSNAME_GOES_HEREPage(this.driver, _logger);

            this._logger.Step("Vào màn hình");
            page.GoToPage();

            this._logger.Step("Kiểm tra: Tự chuyển về trang 1 khi tìm kiếm ở trang khác trang 1");
            page.CheckGridPager();
        }
    }

}
