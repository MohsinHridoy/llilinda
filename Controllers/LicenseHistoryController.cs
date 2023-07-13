using Backend.DbContextBD;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class LicenseHistoryController : ControllerBase
    {

        private readonly DataContext _context;

        public LicenseHistoryController(DataContext context)
        {
            _context = context;
        }






        /**************************************
         * 
         * Delete License History
         * 
         * ***************/

        [HttpGet("GetAllHistoryLicense")]
        public async Task<ActionResult<List<LicenseHistory>>> index()
        {
            return Ok(await _context.LicensesHistory.ToListAsync());
        }

    }
}
