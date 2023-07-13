using Backend.DbContextBD;
using Backend.Models;
using Backend.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : Controller
    {
        private readonly DataContext _context;

        public LicenseController(DataContext context)
        {
            _context = context;
        }


        /**************************************
         * 
         * Add new License 
         * 
         * ***************/
        [HttpPost("addLicense")]
        public async Task<IActionResult> Create(License license)
        {
            // Get the user by user ID
            var user = _context.Users.FirstOrDefault(x => x.UserId == license.UserId);

            if (user == null)
            {
                return Problem($"User with ID {license.UserId} was not found in the database.");
            }

            // Get the access record from the database
            var access = await _context.Accesss.FirstOrDefaultAsync(a => a.AccessId == license.AccessId);

            if (access == null)
            {
                return Problem($"Access record with ID {license.AccessId} was not found in the database.");
            }

            // Create a new license key
            license.LastModificatedDate = DateTime.UtcNow;
            license.LicenseKey = Guid.NewGuid().ToString();
            switch (license.ActivationMonths)
            {
                case 3:
                    license.EndDate = license.StartDate.AddMonths(3);
                    break;
                case 6:
                    license.EndDate = license.StartDate.AddMonths(6);
                    break;
                case 12:
                    license.EndDate = license.StartDate.AddMonths(12);
                    break;
                default:
                    return BadRequest("Invalid activation period selected.");
            }

            // Serialize the license object to JSON
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
            };

            var json = JsonSerializer.Serialize(license, options);

            if (ModelState.IsValid)
            {
                _context.Add(license);
                await _context.SaveChangesAsync();
            }

            return Ok(license);
        }


        /**************************************
        * 
        * Display All Licenses
        * 
        * ***************/

        [HttpGet("GetAllLicense")]
        public async Task<ActionResult<List<License>>> index()
        {
            var licence = await _context.Licenses
                .Include(a => a.Access)
                .ThenInclude(b => b.Modules)
                .ToListAsync();

            return Ok(licence);
        }




        /**************************************
         * 
         * Display One License 
         * 
         * ***************/

        [HttpGet("secondGetLicenses")]
        public async Task<ActionResult<List<License>>> Index()
        {
            var licenses = await _context.Licenses.ToListAsync();
            if (licenses == null)
            {
                return NotFound();
            }

            var res = await _context.Licenses
                .Include(a => a.Access)
                .ThenInclude(l => l.Modules)
                .ThenInclude(m => m.Product)
                .Select(l => new
                {
                    l.LicenseId,
                    l.StartDate,
                    l.LicenseStatus,
                    l.EndDate,
                    l.User.Username,
                    Access = new
                    {
                        AccessName = l.Access.AccessName,
                        Modules = l.Access.Modules.Select(x=>x.ModuleName),
                        Product = l.Access.Modules.Select(x=>x.Product.ProductName).FirstOrDefault()               
                    }
                })
                .ToListAsync();

            return Ok(res);
        }



        /**************************************
        * 
        * Update License 
        * 
        * ***************/

        [HttpPut("UpdateLicense/{id}")]
        public async Task<ActionResult<License>> UpdateLicense(int id, LicenseUpdateRequest license)
        {
            var findLicense = await _context.Licenses.FindAsync(id);

            if (findLicense == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
        
            findLicense.StartDate = license.StartDate;
            findLicense.EndDate = license.EndDate;
            findLicense.LicenseStatus = license.LicenseStatus;
            findLicense.RenewMode = license.RenewMode;
            findLicense.LastModificatedDate = license.LastModificatedDate = DateTime.Now;
           

            _context.Entry(findLicense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Unable to save change. " +
                        "Try Again, if you have problem persists, " +
                        "Contact your system administrator");
            }

            return Ok(findLicense);
        }

        private bool LicenceExists(int id)
        {
            return _context.Licenses.Any(e => e.LicenseId == id);
        }



        /**************************************
          * 
          * Delete License 
          * 
          * ***************/
        [HttpDelete("DeleteLicense/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Licenses == null)
            {
                return Problem("Entity set 'MyDbContext.License'  is null.");
            }
            var license = await _context.Licenses.FindAsync(id);
            if (license != null)
            {
                _context.Licenses.Remove(license);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

    }

    
}