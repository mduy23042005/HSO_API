using HSOEntities.Models;
using System.Collections.Generic;

public class RegisterRequest
{
    public Account Account { get; set; }
    public List<Account_Equipment> Equipment { get; set; }
}