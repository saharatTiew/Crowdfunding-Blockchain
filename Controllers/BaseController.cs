using blockchain.Data;
using Microsoft.AspNetCore.Mvc;

namespace blockchain.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ApplicationDbContext _db;
       
        public BaseController(ApplicationDbContext _db)
        {
            this._db = _db;
        }
    }
}