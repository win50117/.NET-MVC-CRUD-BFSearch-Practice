using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC3.Models;

namespace MVC3.Controllers
{
    public class UserDBController : Controller
    {
        private Models.MVC_UserDBContext _db = new Models.MVC_UserDBContext();
        // GET: UserDB
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);    
        }

        //如果找不到動作（Action）或是輸入錯誤的動作名稱，一律跳回首頁
        //Controller的ideHandleUnknownAction方法為virtual，所以可用overr覆寫。
        protected override void HandleUnknownAction(string actionName)
        {
            Response.Redirect("http://首頁(網址)/"); //自訂結果 --找不到動作就跳回首頁
            base.HandleUnknownAction(actionName);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName("Create")]   // 可以把下面的動作名稱改成別的，然後指定由誰回傳。
        [ValidateAntiForgeryToken]   // 避免CSRF攻擊
        public ActionResult Create(UserTable _userTable)
        {
            if ((_userTable != null) && (ModelState.IsValid))   // ModelState.IsValid，通過表單驗證（Server-side validation）需搭配 Model底下類別檔的 [驗證]
            {   // 第一種方法
                _db.UserTables.Add(_userTable);
                _db.SaveChanges();

                //// 第二種方法（作法類似後續的 Edit動作）
                //// 資料來源  https://msdn.microsoft.com/en-us/library/jj592676(v=vs.113).aspx
                //_db.Entry(_userTable).State = System.Data.Entity.EntityState.Added;  //確認新增一筆（狀態：Added）
                //_db.SaveChanges();

                //return Content(" 新增一筆記錄，成功！");    // 新增成功後，出現訊息（字串）。
                return RedirectToAction("List");
            }
            else
            {   // 搭配 ModelState.IsValid，如果驗證沒過，就出現錯誤訊息。
                ModelState.AddModelError("Value1", " 自訂錯誤訊息(1) "); // 第一個輸入值是 key，第二個是錯誤訊息（字串）
                ModelState.AddModelError("Value2", " 自訂錯誤訊息(2) ");
                return View(); // 將錯誤訊息，返回並呈現在「新增」的檢視畫面上
            }
        }

        //===================================
        //== 列出多筆紀錄的主表（List） ==
        //===================================
        public ActionResult List()
        {
            //第一種寫法：  //*** 查詢結果是一個 IQueryable ***            
            IQueryable<UserTable> ListAll = from _userTable in _db.UserTables select _userTable;              

            if (ListAll == null)
            {   // 找不到這一筆記錄
                return HttpNotFound();
            }
            else
            {
                return View(ListAll.ToList());
                // 直到程式的最後，把查詢結果 IQueryable呼叫.ToList()時，上面那一段LINQ才會真正被執行
            }
            ////第二種寫法：
            //if (_db.UserTables == null)
            //{   // 找不到任何記錄
            //    return HttpNotFound();
            //// return Content("抱歉！找不到！");
            //}
            //else
            //{
            //    return View(_db.UserTables.ToList());   //直接把 UserTables的全部內容 列出來
            //    // 翻譯成SQL指令的成果，跟第一種方法相同。
            //}
        }

        //===================================
        //== 列出一筆記錄的明細（Details） ==
        //===================================
        //[HttpPost]    // 改成這樣會報錯。輸入網址/UserDB/Details?_ID=4
        ////                 // 錯誤訊息 -- '/' 應用程式中發生伺服器錯誤。        找不到資源。 
        //   可以對照底下三個 Search動作，可以更清楚得知這個錯誤與修正方法。
        [HttpGet]
        public ActionResult Details(int? _ID = 1)
        {
            if(_ID == null)
            {   //沒有輸入文章編號（ID），就會報錯 - Bad Request
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            //// 第一種寫法：===
            ////var ListOne = from _userTable in _db.UserTables
            ////              where _userTable.UserId == _ID
            ////              select _userTable;
            ////也可以寫成下面這樣：
            //var ListOne = from m in _db.UserTables
            //              where m.UserId == _ID
            //              select m;           

            //if (ListOne == null)
            //{    // 找不到這一筆記錄
            //    return HttpNotFound();
            //}
            //else   
            //{
            //    return View(ListOne.FirstOrDefault());
            //}

            //// 第二種寫法： 透過 .Where() 函數===
            //var ListOne2 = _db.UserTables.Where(x => x.UserId == _ID);
            //if (ListOne2 == null)
            //{    // 找不到這一筆記錄
            //    return HttpNotFound();
            //}
            //else  {
            //    return View(ListOne2.FirstOrDefault());
            //    // 翻譯成SQL指令的結果，同上（第一種方法）。
            //}

            //// 第三種寫法： 透過 .FirstOrDefault() 函數===
            //var ListOne3 = _db.UserTables.FirstOrDefault(b => b.UserId == _ID);
            //// 翻譯成SQL指令的結果，同上（第一種方法）。
            //if (ListOne3 == null)
            //{    // 找不到這一筆記錄
            //    return HttpNotFound();
            //}
            //else   
            //{
            //    return View(ListOne3);
            //}

            // 第四種寫法：透過 .Find() 函數
            UserTable ut = _db.UserTables.Find(_ID);
            //翻譯成SQL指令的結果，同上（第一種方法）
            if(ut == null) 
            {   //找不到這一筆紀錄
                return HttpNotFound();
            }
            else
            {
                return View(ut);
            }
        }

        //===================================
        //== 刪除 ==  
        //===================================
        //== 刪除前的 Double-Check，先確認這筆記錄的內容
        public ActionResult Delete(int? _ID)    // 網址 http://xxxxxx/UserDB/Delete?_ID=1 
        {
            if (_ID == null)
            {   // 沒有輸入文章編號（_ID），就會報錯 - Bad Request
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            // 使用上方 Details動作的程式，先列出這一筆的內容，給使用者確認
            UserTable ut = _db.UserTables.Find(_ID);

            if (ut == null)
            {   // 找不到這一筆記錄
                return HttpNotFound();
            }
            else
            {
                return View(ut);
            }
        }

        //== 真正刪除這一筆，並回寫資料庫 ===============
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]   // 避免CSRF攻擊       
        public ActionResult DeleteConfirm(int _ID)
        {
            if (ModelState.IsValid)   // ModelState.IsValid，通過表單驗證（Server-side validation）需搭配 Model底下類別檔的 [驗證]
            {
                //// 這種寫法有錯。public ActionResult DeleteConfirm(UserTable _userTable)
                //// 第一種方法 -- 錯誤訊息：無法刪除此物件，因為在 ObjectStateManager 中找不到。
                //_db.UserTables.Remove(_userTable);
                //_db.SaveChanges();
                // 修正後的正確版本：請看第三種方法，必須先鎖定、先找到這一筆記錄。找得到，才能刪除。

                //// 第二種方法（作法類似後續的 Edit動作）
                //// 資料來源  https://msdn.microsoft.com/en-us/library/jj592676(v=vs.113).aspx
                ////--錯誤訊息：Store update, insert, or delete statement affected an unexpected number of rows (0). Entities may have been modified or deleted since entities were loaded. See http://go.microsoft.com/fwlink/?LinkId=472540 for information on understanding and handling optimistic concurrency exceptions.
                ////_db.Entry(_userTable).State = System.Data.Entity.EntityState.Deleted;  //確認刪除一筆（狀態：Deleteed）
                ////_db.SaveChanges();
                //// 修正後的正確版本：
                //// 必須先鎖定、先找到這一筆記錄。找得到，才能刪除！
                //UserTable ut = _db.UserTables.Find(_ID);
                //_db.Entry(ut).State = System.Data.Entity.EntityState.Deleted;  //確認刪除一筆（狀態：Deleteed）
                //_db.SaveChanges();
                ////**** 刪除以後，必須執行 .SaveChanges()方法，才能真正去DB刪除這一筆記錄 ****

                // 第三種方法。必須先鎖定、先找到這一筆記錄。找得到，才能刪除！
                UserTable ut = _db.UserTables.Find(_ID);
                _db.UserTables.Remove(ut);
                _db.SaveChanges();

                //return Content(" 刪除一筆記錄，成功！");    // 刪除成功後，出現訊息（字串）。
                return RedirectToAction("List");
            }
            else
            {   // 搭配 ModelState.IsValid，如果驗證沒過，就出現錯誤訊息。
                ModelState.AddModelError("Value1", " 自訂錯誤訊息(1) ");
                ModelState.AddModelError("Value2", " 自訂錯誤訊息(2) ");
                return View();   // 將錯誤訊息，返回並呈現在「刪除」的檢視畫面上
            }
        }

        //===================================
        //== 修改（編輯）畫面 #1 ==
        //===================================
        public ActionResult Edit(int? _ID)
        {
            if (_ID == null)
            {   // 沒有輸入文章編號（_ID），就會報錯 - Bad Request
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            // 使用上方 Details動作的程式，先列出這一筆的內容，給使用者確認
            UserTable ut = _db.UserTables.Find(_ID);

            if (ut == null)
            {   // 找不到這一筆記錄
                return HttpNotFound();
            }
            else
            {
                return View(ut);
            }
        }

        //== 修改（更新），回寫資料庫 #1 ===這裡的輸入值是一個 UserTable
        [HttpPost]
        [ValidateAntiForgeryToken]   // 避免CSRF攻擊
        // [Bind(Include=.......)] 也可以寫在Model的類別檔裡面，就不用重複地寫在新增、刪除、修改每個動作之中。需搭配 System.Web.Mvc命名空間。
        // 可以避免 overposting attacks （過多發佈）攻擊
        public ActionResult Edit([Bind(Include = "UserId, UserName, UserSex, UserBirthDay, UserMobilePhone")]UserTable _userTable)
        {   
            if (_userTable == null)
            {   // 沒有輸入內容，就會報錯 - Bad Request
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)   // ModelState.IsValid，通過表單驗證（Server-side validation）需搭配 Model底下類別檔的 [驗證]
            {   
                // 第一種寫法：
                _db.Entry(_userTable).State = System.Data.Entity.EntityState.Modified;  //確認被修改（狀態：Modified）
                _db.SaveChanges();

                //// 第二種寫法：=========================================                
                //// 使用上方 Details動作的程式，先列出這一筆的內容，給使用者確認
                //UserTable ut = _db.UserTables.Find(_userTable.UserId);                

                //if (ut == null)
                //{   // 找不到這一筆記錄
                //    return HttpNotFound();
                //}
                //else   {
                //    ut.UserName = _userTable.UserName;  // 修改後的值
                //    ut.UserSex = _userTable.UserSex;
                //    ut.UserBirthDay = _userTable.UserBirthDay;
                //    ut.UserMobilePhone = _userTable.UserMobilePhone;

                //    _db.SaveChanges();   // 回寫資料庫（進行修改）
                //}
                //// 第二種寫法：=========================================

                //return Content(" 更新一筆記錄，成功！");    // 更新成功後，出現訊息（字串）。
                return RedirectToAction("List");
            }
            else
            {
                return View(_userTable);  // 若沒有修改成功，則列出原本畫面
                //return Content(" *** 更新失敗！！*** "); 
            }
        }

        //===================================
        //== 修改（編輯）畫面 #2 ==
        //===================================
        public ActionResult Edit2(int? _ID) // 跟 Edit動作 #1相同，沒有變化
        {
            if (_ID == null)
            {   // 沒有輸入文章編號（_ID），就會報錯 - Bad Request
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // 使用上方 Details動作的程式，先列出這一筆的內容，給使用者確認
            UserTable ut = _db.UserTables.Find(_ID);

            if (ut == null)
            {   // 找不到這一筆記錄
                return HttpNotFound();
            }
            else
            {
                return View(ut);
            }
        }

        //== 修改（更新），回寫資料庫 #2 ============ 注意！這裡的輸入值是一個 int _ID
        [HttpPost, ActionName("Edit2")]
        [ValidateAntiForgeryToken]   // 避免XSS、CSRF攻擊
        // [Bind(Include=.......)] 也可以寫在 Model的類別檔裡面，就不用重複地寫在新增、刪除、修改每個動作之中。需搭配System.Web.Mvc命名空間。
        // 可以避免 overposting attacks （過多發佈）攻擊                                                                                                                                   //***** 輸入 _ID而非 UserTable
        public ActionResult Edit2Confirm([Bind(Include = "UserId, UserName, UserSex, UserBirthDay, UserMobilePhone")]int _ID)
        {  
            ////// 第三種寫法：=========================================
            ////// 如果寫成 TryUpdateModel(_userTable) 無法運作。寫成 TryUpdateModel(_userTable.UserId) 則成功。            
            //if (ModelState.IsValid && TryUpdateModel(_db.UserTables.Find(_ID)))
            //{
            //    _db.SaveChanges();
            //    return RedirectToAction("List");
            //    //return Content(" 更新一筆記錄，成功！");    // 更新成功後，出現訊息（字串）。
            //}

            //// 第四種寫法：===========================================
            //// 如果寫成 TryUpdateModel(_userTable) 無法運作。寫成 TryUpdateModel(_userTable.UserId) 則成功。
            if (ModelState.IsValid &&
                TryUpdateModel(_db.UserTables.Find(_ID), "", new string[] { "UserName", "UserSex", "UserBirthDay", "UserMobilePhone" }))
            {   
                //  第一個參數： 要更新的模型執行個體。
                //  第二個參數： 要更新之模型的屬性清單（字串陣列）。
                _db.SaveChanges();
                return RedirectToAction("List");   // 修改成功
            }

            return Content(" *** 更新失敗！！*** ");
        }

        //===================================
        //== 搜尋關鍵字。類似上面的 列表（Master） ==
        //
        //== .Wehere() 與 .Contains()的寫法 
        // (1) 搜尋  日期    .Where(o => o.OrderDate >= new DateTime(2003, 12, 1))
        // (2) 搜尋  符合兩個條件（用&&）  .Where(o => o.OrderQty > orderQtyMin && order.OrderQty < orderQtyMax)
        // (3) 搜尋  符合陣列裡面的「值」。產品模組ID符合  19/26/18。或是 產品尺寸符合 L/XL
        // .Where(p => (new int?[] { 19, 26, 18 }).Contains(p.ProductModelID) ||   (new string[] { "L", "XL" }).Contains(p.Size));
        //===================================
        [HttpGet]
        // 透過「List範本」產生檢視畫面，來執行搜尋的成果。
        // 錯誤： 第一個搜尋的動作，採用POST，所以URL輸入 http://xxxxxx/UserDB/Search1?_SearchWord=MVC  會報錯
        // 錯誤： 直接在網址輸入 http://xxxxxx/UserDB/Search/MVC  （需要修改 /App_Start目錄下的  Route設定。把 id 改 _SearchWord 才行）
        // 自行輸入網址，需改成 [HttpGet]才行。看Search3動作。
        public ActionResult Search(string _SearchWord = "MVC")
        {
            // return Content("<h3> 檢視頁面傳來的 -- " + _SearchWord + "</h3>");
            ViewData["SW"] = _SearchWord;
            ////第一種寫法：
            //if (String.IsNullOrEmpty(_SearchWord) && ModelState.IsValid)
            //{   // 沒有輸入內容，就會報錯
            //    return Content("請輸入「關鍵字」才能搜尋");
            //}

            //var ListAll = from _userTable in _db.UserTables
            //                   where _userTable.UserName.Contains(_SearchWord)     
            //                   // .Contains()對應T-SQL指令的 LIKE，但搜尋關鍵字有「大小寫」的區分
            //                   select _userTable;

            //if (ListAll == null)
            //{   // 找不到任何記錄
            //    return HttpNotFound();
            //}
            //else   {
            //    return View(ListAll.ToList());
            //    //檢視畫面的「範本」請選 List。因為搜尋到的結果可能會有多筆記錄。
            //}

            //第二種寫法：  
            IQueryable<UserTable> ListAll = from _userTable in _db.UserTables select _userTable;

            if (!String.IsNullOrEmpty(_SearchWord) && ModelState.IsValid)
            {
                return View(ListAll.Where(s => s.UserName.Contains(_SearchWord)));
            }
            else
            {   // 找不到任何記錄（請參閱最下方的 override HandleUnknowAction()）
                return HttpNotFound();
            }            
        }

        // 第一個搜尋的動作，採用POST，所以URL輸入 http://xxxxxx/UserDB/Search1?_SearchWord=MVC 會報錯
        // 下面的搜尋針對 Route做調整，加入一列程式碼，可以正確執行。
        // 正確執行。直接在網址輸入 http://xxxxxx/UserDB/Search2?_ID=MVC
        // 正確執行。直接在網址輸入 http://xxxxxx/UserDB/Search2/MVC

        //[HttpPost]   // 重點(1)！這一列務必註解、不執行！
        public ActionResult Search2(string _ID)   // 重點(2)！！因為目前的路由設定，只能接受 id這個變數 （需要修改 /App_Start目錄下的  Route設定。把 id 改 _ID 才行）
        {
            string _SearchWord = _ID;   // 重點(3)！！

            ViewData["SW"] = _SearchWord;

            ////第二種寫法：
            var ListAll = from _userTable in _db.UserTables
                          select _userTable;

            if (!String.IsNullOrEmpty(_SearchWord) && ModelState.IsValid)
            {
                return View(ListAll.Where(s => s.UserName.Contains(_SearchWord)));
                // .Where() 與 .Contains()的寫法 https://docs.microsoft.com/zh-tw/dotnet/framework/data/adonet/ef/language-reference/method-based-query-syntax-examples-filtering
            }
            else
            {   // 找不到任何記錄
                return HttpNotFound();
            }
        }


        // 第一個搜尋的動作，採用POST，所以URL輸入 http://xxxxxx/UserDB/Search1?_SearchWord=MVC  會報錯。這裡改成HttpGet就OK。
        // 輸入這樣也是錯誤   http://xxxxxx/UserDB/Search3/MVC   （需要修改 /App_Start目錄下的  Route設定。把 id 改 _ID 才行）
        // 下面的搜尋採用 GET，可以正確執行。http://xxxxxx/UserDB/Search3?_SearchWord=MVC
        [HttpGet]
        public ActionResult Search3(string _SearchWord)
        {
            ViewData["SW"] = _SearchWord;   // 如果您寫成 ViewBag也可以

            ////第二種寫法：
            var ListAll = from _userTable in _db.UserTables
                          select _userTable;

            if (!String.IsNullOrEmpty(_SearchWord) && ModelState.IsValid)
            {
                return View(ListAll.Where(s => s.UserName.Contains(_SearchWord)));
                // .Where() 與 .Contains()的寫法 https://docs.microsoft.com/zh-tw/dotnet/framework/data/adonet/ef/language-reference/method-based-query-syntax-examples-filtering
            }
            else
            {   // 找不到任何記錄
                return HttpNotFound();
            }
        }

        //===================================
        //== 搜尋關鍵字。  畫面上有「多個」搜尋條件。
        //===================================
        public ActionResult Search4_Multi()
        {
            return View();   //產生一個搜尋畫面。類似「新增 Create」的畫面。  可以輸入多個搜尋條件。
        }

        [HttpPost]
        [ValidateAntiForgeryToken]   // 避免CSRF攻擊
        public ActionResult Search4_Multi(UserTable _userTable)
        {   //                                                 ********************修改的重點！

            string uName = _userTable.UserName;   // 從畫面上，輸入的第一個搜尋條件。  姓名。
            string uMobilePhone = _userTable.UserMobilePhone;   // 從畫面上，輸入的第二個搜尋條件。   手機號碼。
                              
            var ListAll = _db.UserTables.Select(s => s);

            if (!string.IsNullOrWhiteSpace(uName))  // 「有填寫」搜尋條件的，才會進行搜尋。
            {                                       //  畫面上留空白，表示這個條件不搜尋。
                //ListAll = ListAll.Where(s => s.UserName == uName);
                ListAll = ListAll.Where(s => s.UserName.Contains(uName));
                //                                     // ********** 模糊搜尋
            }

            if (!string.IsNullOrWhiteSpace(uMobilePhone))
            {
                ListAll = ListAll.Where(s => s.UserMobilePhone.Contains(uMobilePhone));
                //                                             // ********** 模糊搜尋
            }
            //********************************************************** 

            if ((_userTable != null) && (ModelState.IsValid))
            {
                return View("Search4_Result", ListAll);
                // 搜尋結果（ListAll.ToList()），導向另一個「檢視畫面（Search4_Result）」！
            }
            else
            {   // 找不到任何記錄
                return HttpNotFound();
            }
        }

        //===================================
        //== 分頁 ==  LINQ的 .Skip() 與 .Take()
        // https://docs.microsoft.com/zh-tw/dotnet/framework/data/adonet/ef/language-reference/method-based-query-syntax-examples-partitioning
        //===================================
        public ActionResult IndexPage(int _ID = 1)
        {
            // id變數，目前位於第幾頁？
            // PageSize變數，每一頁，要展示幾筆記錄？            
            int PageSize = 3;

            // NowPageCount，目前正在觀賞這一頁的紀錄
            int NowPageCount = 0;
            if (_ID > 0)
            {
                NowPageCount = (_ID - 1) * PageSize;    // PageSize，每頁展示3筆紀錄（上面設定過了）
            }

            // 這段指令的 .Skip()與 . Take()，其實跟T-SQL指令的 offset...fetch....很類似（SQL 2012起可用）
            var ListAll = (from _userTable in _db.UserTables
                           orderby _userTable.UserId   // 若寫 descending ，則是反排序（由大到小）
                           select _userTable).Skip(NowPageCount).Take(PageSize);    // .Skip() 從哪裡開始（忽略前面幾筆記錄）。 .Take()呈現幾筆記錄

            //*** 查詢結果 ListAll 是一個 IQueryable ***
            if (ListAll == null)
            {   // 找不到任何記錄
                return HttpNotFound();
            }
            else
            {
                return View(ListAll.ToList());               
            }
        }


        //== 畫面下方，加入「上一頁」、「下一頁」、每十頁作間隔 ===
        public ActionResult IndexPage2(int _ID = 1)
        {   // id變數，目前位於第幾頁？
            // PageSize變數，每一頁，要展示幾筆記錄？            
            int PageSize = 3;

            // RecordCount變數，符合條件的總共有幾筆記錄？
            int RecordCount = _db.UserTables.Count();

            // NowPageCount，目前正在觀賞這一頁的紀錄
            int NowPageCount = 0;
            if (_ID > 0)
            {
                NowPageCount = (_ID - 1) * PageSize;    // PageSize，每頁展示3筆紀錄（上面設定過了）
            }

            // 這段指令的 .Skip()與 . Take()，其實跟T-SQL指令的 offset...fetch....很類似（SQL 2012起可用）
            var ListAll = (from _userTable in _db.UserTables
                           orderby _userTable.UserId   // 若寫 descending ，則是反排序（由大到小）
                           select _userTable).Skip(NowPageCount).Take(PageSize);    // .Skip() 從哪裡開始（忽略前面幾筆記錄）。 .Take()呈現幾筆記錄

            if (ListAll == null)
            {   // 找不到任何記錄
                return HttpNotFound();
            }
            else
            {   //************** 比上一個範例  多的程式碼。 *****************************************(start)

                #region    // 畫面下方的「分頁列」。「每十頁」一間隔，分頁功能

                // Pages變數，「總共需要幾頁」才能把所有紀錄展示完畢？
                int Pages;
                if ((RecordCount % PageSize) > 0)
                {   //-- %，除法，傳回餘數
                    Pages = ((RecordCount / PageSize) + 1);   //-- ( / )除法。傳回整數。  如果無法整除，有餘數，則需要多出一頁來呈現。 
                }
                else
                {
                    Pages = (RecordCount / PageSize);   //-- ( /)除法。傳回整數
                }

                System.Text.StringBuilder sbPageList = new System.Text.StringBuilder();
                if (Pages > 0)
                {   //有傳來「頁數(p)」，而且頁數正確（大於零），出現<上一頁>、<下一頁>這些功能
                    sbPageList.Append("<div align='center'>");

                    //** 可以把檔名刪除，只留下 ?P=  即可！一樣會運作，但IE 11會出現 JavaScript錯誤。**
                    //** 抓到目前網頁的「檔名」。 System.IO.Path.GetFileName(Request.PhysicalPath) **
                    if (_ID > 1)
                    {   //======== 分頁功能（上一頁 / 下一頁）=========start===                
                        sbPageList.Append("<a href='?id=" + (_ID - 1) + "'>[<<<上一頁]</a>");
                    }
                    sbPageList.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b><a href='http://127.0.0.1/'>[首頁]</a></b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    if (_ID < Pages)
                    {
                        sbPageList.Append("<a href='?id=" + (_ID + 1) + "'>[下一頁>>>]</a>");
                    }  //======== 分頁功能（上一頁 / 下一頁）=========end====


                    //==========================================================
                    //========= MIS2000 Lab.自製的「每十頁」一間隔，分頁功能=========start====
                    sbPageList.Append("<hr width='97%' size=1>");

                    int block_page = 0;
                    block_page = _ID / 10;   //--只取除法的整數成果（商），若有餘數也不去管它。

                    if (block_page > 0)
                    {
                        sbPageList.Append("<a href='?id=" + (((block_page - 1) * 10) + 9) + "'> [前十頁<<]  </a>&nbsp;&nbsp;");
                    }

                    for (int K = 0; K <= 10; K++)
                    {
                        if ((block_page * 10 + K) <= Pages)
                        {   //--- Pages 資料的總頁數。共需「幾頁」來呈現所有資料？
                            if (((block_page * 10) + K) == _ID)
                            {   //--- id 就是「目前在第幾頁」
                                sbPageList.Append("[<b>" + _ID + "</b>]" + "&nbsp;&nbsp;&nbsp;");
                            }
                            else
                            {
                                if (((block_page * 10) + K) != 0)
                                {
                                    sbPageList.Append("<a href='?id=" + (block_page * 10 + K) + "'>" + (block_page * 10 + K) + "</a>");
                                    sbPageList.Append("&nbsp;&nbsp;&nbsp;");
                                }
                            }
                        }
                    }  //for迴圈 end 

                    if ((block_page < (Pages / 10)) & (Pages >= (((block_page + 1) * 10) + 1)))
                    {
                        sbPageList.Append("&nbsp;&nbsp;<a href='?id=" + ((block_page + 1) * 10 + 1) + "'>  [>>後十頁]  </a>");
                    }
                    sbPageList.Append("</div>");
                }
                //========= MIS2000 Lab.自製的「每十頁」一間隔，分頁功能=========end====
                #endregion

                ViewBag.PageList = sbPageList.ToString();
                //************** 比上一個範例  多的程式碼。 *****************************************(end)

                return View(ListAll.ToList());
            }
        }
        public ActionResult Index()
        {
            //using (Models.MVC_UserDBContext DB = new Models.MVC_UserDBContext())
            //{                
            //}
                return View();
        }

        
    }
}