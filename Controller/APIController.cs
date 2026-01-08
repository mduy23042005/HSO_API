using System.Linq;
using System.Web.Http;

public class APIController : ApiController
{
    private HSOEntities.Models.HSOEntities db = new HSOEntities.Models.HSOEntities();
    public APIController()
    {
        db.Configuration.ProxyCreationEnabled = false; // không tạo proxy
        db.Configuration.LazyLoadingEnabled = false;  // không lazy load
    }

    [HttpGet]
    [Route("api/account/login")]
    public IHttpActionResult Login(string username, string password)
    {
        var account = db.Accounts.FirstOrDefault(a => a.Username == username && a.Password == password);

        if (account == null)
            return NotFound();

        return Ok(account);
    }

    [HttpPost]
    [Route("api/account/register")]
    public IHttpActionResult Register([FromBody] RegisterRequest request)
    {
        if (request == null || request.Account == null || request.Equipment == null)
            return BadRequest("Dữ liệu gửi lên không hợp lệ.");

        // Kiểm tra username hoặc NameChar đã tồn tại
        if (db.Accounts.Any(a => a.Username == request.Account.Username))
            return BadRequest("{\"errorField\":\"Username\",\"message\":\"Username đã tồn tại.\"}");

        if (db.Accounts.Any(a => a.NameChar == request.Account.NameChar))
            return BadRequest("{\"errorField\":\"NameChar\",\"message\":\"Tên nhân vật đã tồn tại.\"}");

        // Tạo account mới
        var newAccount = request.Account;
        db.Accounts.Add(newAccount);

        // Tạo inventory khởi đầu và thêm vào bảng
        var newInventory = new HSOEntities.Models.Account_Item0
        {
            IDAccount = newAccount.IDAccount,  // gán IDAccount vừa tạo
            IDItem0 = 1, // Giả sử item khởi đầu có IDItem0 là 1
            Category = 1  // Giả sử category khởi đầu là 0
        };
        db.Account_Item0.Add(newInventory);
        
        // Gán IDAccount cho equipment và thêm vào bảng
        foreach (var eq in request.Equipment)
        {
            eq.IDAccount = newAccount.IDAccount;
            db.Account_Equipment.Add(eq);
        }

        db.SaveChanges();

        return Ok(new
        {
            message = "Đăng ký thành công!",
            IDAccount = newAccount.IDAccount
        });
    }

    [HttpGet]
    [Route("api/account/{idAccount}/equipment")]
    public IHttpActionResult Equipment(int idAccount)
    {
        var item0_1 = db.Account_Equipment
            .Where(x => x.IDAccount == idAccount)
            .Select(x => new
            {
                id = x.ID,
                idItem0_1 = x.IDItem0_1,
                category = x.Category,
            })
            .ToList();

        if (!item0_1.Any())
            return NotFound();

        return Ok(item0_1);
    }

    #region Đọc Attribute từ equipment
    [HttpGet]
    [Route("api/account/{idAccount}/equipItem/{id}/listAttributes")]
    public IHttpActionResult EquipmentListAttribute(int id)
    {
        /*
        var id = db.Account_Equipment.Where(x => x.IDItem0_1 == idItem).Select(x => x.ID).FirstOrDefault();

        //Tạm thời sẽ lấy Attribute từ bảng Item0_Attribute
        var listAttributes = db.Account_Equipment_Attribute.Where(x => x.IDAccountEquipment == id).ToList();
        */

        var idItem0_1 = db.Account_Equipment.Where(x => x.ID == id).Select(x => x.IDItem0_1).FirstOrDefault();
        var category = db.Account_Equipment.Where(x => x.ID == id).Select(x => x.Category).FirstOrDefault();

        var nameItem = db.Item0.Where(x => x.IDItem0 == idItem0_1).Select(x => x.NameItem0).FirstOrDefault();

        var listAttributes = db.Item0_Attribute.Where(x => x.IDItem0 == idItem0_1 && x.Category == category)
            .Select(x => new
            {
                idItem0_1,
                category,
                nameItem,
                x.Value,
                x.IDAttribute
            }).ToList();

        if (!listAttributes.Any())
            return NotFound();
        
        return Ok(listAttributes);
    }
    [HttpGet]
    [Route("api/account/{idAccount}/equipItem/{idItem0_1}/listAttributes/{idAttribute}")]
    public IHttpActionResult EquipmentNameAttribute(int idAttribute)
    {
        var nameAttributes = db.Attributes.Where(x => x.IDAttribute == idAttribute).Select(x => x.NameAttribute).FirstOrDefault();

        if (string.IsNullOrEmpty(nameAttributes))
            return NotFound();

        return Ok(nameAttributes);
    }
    #endregion

    [HttpGet]
    [Route("api/account/{idAccount}/inventory")]
    public IHttpActionResult Inventory(int idAccount)
    {
        var item0 = db.Account_Item0.Where(x => x.IDAccount == idAccount)
            .Select(i => new 
            { 
                id = i.ID,
                idItem0 = i.IDItem0, 
                category = i.Category,
                typeItem0 = i.Item0.TypeItem0,
                idSchool = i.Item0.IDSchool
            })
            .ToList();

        if (item0 == null)
            return NotFound();

        return Ok(item0);
    }

    #region Đọc Attribute từ inventory
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem/{id}/listAttributes")]
    public IHttpActionResult InventoryListAttribute(int id)
    {
        var idItem0 = db.Account_Item0.Where(x => x.ID == id).Select(x => x.IDItem0).FirstOrDefault();
        var category = db.Account_Item0.Where(x => x.ID == id).Select(x => x.Category).FirstOrDefault();

        var nameItem = db.Item0.Where(x => x.IDItem0 == idItem0).Select(x => x.NameItem0).FirstOrDefault();

        //Tạm thời sẽ lấy Attribute từ bảng Item0_Attribute
        var listAttributes = db.Item0_Attribute.Where(x => x.IDItem0 == idItem0 && x.Category == category)
            .Select(x => new
            {
                idItem0,
                category,
                nameItem,
                value = x.Value,
                idAttribute = x.IDAttribute
            }).ToList();

        if (!listAttributes.Any())
            return NotFound();

        return Ok(listAttributes);
    }
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem/{id}/listAttributes/{idAttribute}")]
    public IHttpActionResult InventoryNameAttribute(int idAttribute)
    {
        var nameAttributes = db.Attributes.Where(x => x.IDAttribute == idAttribute).Select(x => x.NameAttribute).FirstOrDefault();

        if (string.IsNullOrEmpty(nameAttributes))
            return NotFound();

        return Ok(nameAttributes);
    }
    #endregion

    #region Hoán đổi Item0 giữa inventory và equipment
    [HttpPost]
    [Route("api/account/{idAccount}/equipItem0/{id}")]
    public IHttpActionResult EquipItem0(int idAccount, int id, string slotName)
    {
        var inventoryData = db.Account_Item0.Where(x => x.IDAccount == idAccount && x.ID == id).FirstOrDefault();

        var typeInventoryData = db.Item0.Where(x => x.IDItem0 == inventoryData.IDItem0)
            .Select(x => new 
            { 
                x.TypeItem0, 
                x.IDSchool 
            })
            .FirstOrDefault();

        string typeCheck = typeInventoryData.TypeItem0;

        if (typeCheck.Equals("Ring"))
        {
            typeCheck = slotName;
        }

        var idSchool = db.Accounts.Where(x => x.IDAccount == idAccount).Select(x => x.IDSchool).FirstOrDefault();
        if (idSchool != typeInventoryData.IDSchool && typeInventoryData.IDSchool != 0)
        {
            return BadRequest("Không thể trang bị vật phẩm từ trường phái khác.");
        }

        var equipmentData = db.Account_Equipment.Where(x => x.IDAccount == idAccount && x.SlotName == typeCheck).FirstOrDefault();

        int tempItemId = equipmentData.IDItem0_1;
        int tempCategory = equipmentData.Category;

        equipmentData.IDItem0_1 = inventoryData.IDItem0;
        equipmentData.Category = inventoryData.Category;

        if (tempItemId == 0)
        {
            db.Account_Item0.Remove(inventoryData);
        }
        else
        {
            inventoryData.IDItem0 = tempItemId;
            inventoryData.Category = tempCategory;
        }

        db.SaveChanges();

        return Ok("Equipped");
    }
    #endregion
}
