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

    #region Load basic data
    [HttpGet]
    [Route("api/load/Map/full")]
    public IHttpActionResult LoadMapFull()
    {
        var mapList = db.Maps.ToList();
        var mapMobs = db.Map_Mob.ToList();
        var mapNpcs = db.Map_NPC.ToList();

        var mapData = mapList.Select(map => new
        {
            map = new
            {
                map.IDMap,
                map.NameMap,
            },

            mobsData =
                (from mm in db.Map_Mob
                 join mob in db.Mobs on mm.IDMob equals mob.IDMob
                 where mm.IDMap == map.IDMap
                 select new
                 {
                     mob = new
                     {
                         mob.IDMob,
                         mob.NameMob,
                         mob.Boss,
                         mob.Level,
                         mob.HP
                     },

                     id = mm.ID,
                     posX = mm.PosX,
                     posY = mm.PosY,
                 }).ToList(),

            npcsData =
                (from mn in db.Map_NPC
                 join npc in db.NPCs on mn.IDNPC equals npc.IDNPC
                 where mn.IDMap == map.IDMap
                 select new
                 {
                     npc = new
                     {
                         npc.IDNPC,
                         npc.NameNPC,
                     },

                     posX = mn.PosX,
                     posY = mn.PosY
                 }).ToList()
        }).ToList();

        return Ok(mapData);
    }

    [HttpGet]
    [Route("api/load/Item0/full")]
    public IHttpActionResult LoadItem0Full()
    {
        var item0List = db.Item0.ToList();
        var itemAttrs = db.Item0_Attribute.ToList();
        var attributes = db.Attributes.ToList();

        var item0Data = item0List.Select(item => new
        {
            item0 = new
            {
                item.IDItem0,
                item.NameItem0,
                item.TypeItem0,
                item.IDSchool
            },
            item0_Attributes = itemAttrs
                .Where(a => a.IDItem0 == item.IDItem0 && a.Category == 1)
                .Select(a => new
                {
                    a.IDAttribute,
                    a.Value,
                    a.Category
                })
                .ToList(),
            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.IDAttribute equals attr.IDAttribute
                where a.IDItem0 == item.IDItem0
                select new
                {
                    attr.IDAttribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(item0Data);
    }

    [HttpGet]
    [Route("api/load/Item1/full")]
    public IHttpActionResult LoadItem1Full()
    {
        var item1List = db.Item1.ToList();
        var itemAttrs = db.Item1_Attribute.ToList();
        var attributes = db.Attributes.ToList();

        var item1Data = item1List.Select(item => new
        {
            item1 = new
            {
                item.IDItem1,
                item.NameItem1,
                item.TypeItem1,
            },
            item1_Attributes = itemAttrs
                .Where(a => a.IDItem1 == item.IDItem1 && a.Category == 1)
                .Select(a => new
                {
                    a.IDAttribute,
                    a.Value,
                    a.Category
                })
                .ToList(),
            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.IDAttribute equals attr.IDAttribute
                where a.IDItem1 == item.IDItem1
                select new
                {
                    attr.IDAttribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(item1Data);
    }

    [HttpGet]
    [Route("api/load/Item2/full")]
    public IHttpActionResult LoadItem2Full()
    {
        var item2List = db.Item2.ToList();
        if (item2List == null || !item2List.Any())
            return NotFound();
        return Ok(item2List);
    }

    [HttpGet]
    [Route("api/load/Item3/full")]
    public IHttpActionResult LoadItem3Full()
    {
        var item3List = db.Item3.ToList();
        if (item3List == null || !item3List.Any())
            return NotFound();
        return Ok(item3List);
    }

    [HttpGet]
    [Route("api/load/Item4/full")]
    public IHttpActionResult LoadItem4Full()
    {
        var item4List = db.Item4.ToList();
        if (item4List == null || !item4List.Any())
            return NotFound();

        return Ok(item4List);
    }
    #endregion

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

        // Tạo inventory khởi đầu và thêm vào bảng lúc register
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

    #region Load equipment data
    [HttpGet]
    [Route("api/account/{idAccount}/equipment")]
    public IHttpActionResult Equipment(int idAccount)
    {
        var equipments = db.Account_Equipment.Where(x => x.IDAccount == idAccount).ToList();

        if (!equipments.Any())
            return NotFound();

        var itemAttrs = db.Item0_Attribute.ToList();
        var attributes = db.Attributes.ToList();

        var result = equipments.Select(eq => new
        {
            id = eq.ID,
            idItem0_1 = eq.IDItem0_1,
            nameItem0_1 = db.Item0.Where(i => i.IDItem0 == eq.IDItem0_1).Select(i => i.NameItem0).FirstOrDefault(),
            category = eq.Category,
            slotName = eq.SlotName,

            item0_Attributes = itemAttrs
                .Where(a =>
                    a.IDItem0 == eq.IDItem0_1 &&
                    a.Category == eq.Category)
                .Select(a => new
                {
                    a.IDAttribute,
                    a.Value,
                    a.Category
                })
                .ToList(),

            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.IDAttribute equals attr.IDAttribute
                where a.IDItem0 == eq.IDItem0_1
                select new
                {
                    attr.IDAttribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(result);
    }
    #endregion

    #region Load inventory data
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem0")]
    public IHttpActionResult InventoryItem0(int idAccount)
    {
        var inventory = db.Account_Item0.Where(x => x.IDAccount == idAccount).ToList();

        if (!inventory.Any())
            return NotFound();

        var item0List = db.Item0.ToList();
        var itemAttrs = db.Item0_Attribute.ToList();
        var attributes = db.Attributes.ToList();

        var inventoryData = inventory.Select(inv => new
        {
            id = inv.ID,
            idItem0 = inv.IDItem0,
            nameItem0 = item0List.First(i => i.IDItem0 == inv.IDItem0).NameItem0,
            typeItem0 = item0List.First(i => i.IDItem0 == inv.IDItem0).TypeItem0,
            category = item0List.First(i => i.IDItem0 == inv.IDItem0).Level,
            idschool = item0List.First(i => i.IDItem0 == inv.IDItem0).IDSchool,

            item0_Attributes = itemAttrs
                .Where(a => a.IDItem0 == inv.IDItem0 && a.Category == inv.Category)
                .Select(a => new
                {
                    a.IDAttribute,
                    a.Value,
                    a.Category
                })
                .ToList(),
            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.IDAttribute equals attr.IDAttribute
                where a.IDItem0 == inv.IDItem0
                select new
                {
                    attr.IDAttribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(inventoryData);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem1")]
    public IHttpActionResult InventoryItem1(int idAccount)
    {
        var inventory = db.Account_Item1.Where(x => x.IDAccount == idAccount).ToList();

        if (!inventory.Any())
            return NotFound();

        var item1List = db.Item1.ToList();
        var itemAttrs = db.Item1_Attribute.ToList();
        var attributes = db.Attributes.ToList();

        var inventoryData = inventory.Select(inv => new
        {
            idItem1 = inv.IDItem1,
            nameItem1 = item1List.First(i => i.IDItem1 == inv.IDItem1).NameItem1,
            typeItem1 = item1List.First(i => i.IDItem1 == inv.IDItem1).TypeItem1,

            item1_Attributes = itemAttrs
                .Where(a => a.IDItem1 == inv.IDItem1 && a.Category == 1)
                .Select(a => new
                {
                    a.IDAttribute,
                    a.Value,
                    a.Category
                })
                .ToList(),
            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.IDAttribute equals attr.IDAttribute
                where a.IDItem1 == inv.IDItem1
                select new
                {
                    attr.IDAttribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(inventoryData);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem2")]
    public IHttpActionResult InventoryItem2(int idAccount)
    {
        var item2 = db.Account_Item2.Where(x => x.IDAccount == idAccount).ToList();

        if (!item2.Any())
            return NotFound();

        return Ok(item2);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem3")]
    public IHttpActionResult InventoryItem3(int idAccount)
    {
        var item3 = db.Account_Item3.Where(x => x.IDAccount == idAccount).ToList();

        if (!item3.Any())
            return NotFound();

        return Ok(item3);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem4")]
    public IHttpActionResult InventoryItem4(int idAccount)
    {
        var item4 = db.Account_Item4.Where(x => x.IDAccount == idAccount).ToList();

        if (!item4.Any())
            return NotFound();

        return Ok(item4);
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
