using Backend.DbContextBD;
using Backend.Dtos;
using Backend.Models;
using Backend.Requests;
//using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using ServiceStack.Text;
using System.ComponentModel.DataAnnotations;
using Zaabee.Extensions;

namespace Backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AccessController : ControllerBase
    {

        private readonly DataContext _context;

        public AccessController(DataContext context)
        {
            _context = context;
        }

        /**************************************
              * 
              * Add  new Access 
              * 
              * ***************/


        //[HttpPost("addNewAccess")]
        //public async Task<ActionResult> AddAccess(AccessDTO input)
        //{
        //    if (_context.Accesss.Any(p => p.AccessName == input.AccessName))
        //    {
        //        return BadRequest(new
        //        {
        //            Message = "Role already exists!"
        //        });
        //    }

        //    var access = new Access
        //    {
        //        AccessName = input.AccessName,
        //        CreatedDate = DateTime.Now,
        //        LastModificatedDate = DateTime.Now,
        //        Modules = new List<Module>(),
        //    };
        //    var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == input.ProductId);
        //    if (!string.IsNullOrEmpty(input.ModuleNames))
        //    {
        //        var moduleNames = input.ModuleNames.Split(',');
        //        foreach (var moduleName in moduleNames)
        //        {
        //            var module = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleName == moduleName.Trim());
        //            if (module != null)
        //            {
        //                // find product... var product = await  _productrepository.getprodyct by Id... by name... 

        //                // module have a product? if  find it and add to module like module.Product = produc.

        //                module.Product = product;

        //                access.Modules.Add(module);
        //            }
        //        }
        //    }

        //    _context.Accesss.Add(access);
        //    await _context.SaveChangesAsync();
        //    return Ok(new
        //    {
        //        Message = "Access added successfully!"
        //    });
        //}
        [HttpPost("addNewAccess")]
        public async Task<ActionResult> AddAccess(AccessDTO input)
        {
            if (_context.Accesss.Any(p => p.AccessName == input.AccessName))
            {
                return BadRequest(new
                {
                    Message = "Role already exists!"
                });
            }

            var moduleList=new List<Module>();

            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == input.ProductId);
            if (!string.IsNullOrEmpty(input.ModuleName))
            {
                var moduleNames = input.ModuleName.Split(',');
                foreach (var moduleName in moduleNames)
                {
                    
                    var module = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleName == moduleName.Trim());
                    if (module != null)
                    {
                        // find product... var product = await  _productrepository.getprodyct by Id... by name... 

                        // module have a product? if  find it and add to module like module.Product = produc.
                        
                        module.Product = product;
                        moduleList.Add(module);
                      //  module.AccessId = access.AccessId;
                        //access.Modules.Add(module);
                        //_context.Modules.Add(module);
                    }
                }
            }

            var access = new Access
            {
                AccessName = input.AccessName,
                CreatedDate = DateTime.Now,
                LastModificatedDate = DateTime.Now,
                Modules = moduleList,
                //CreatedBy = input.CreatedBy
            };
            _context.Accesss.Add(access);
            await _context.SaveChangesAsync();

            //_context.Modules.Add(access.Modules);
    //        await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = "Access added successfully!"
            });
        }



        /**************************************
         * 
         * Display All Accesss
         * 
         * ***************/
        [HttpGet("GetAllAccess")]
        public async Task<ActionResult<List<AccessDTO>>> Index()
        {
            var accessWithModuleAndProductNames1 = await _context.Accesss
                             .Include(a => a.Modules)
                             .ThenInclude(m => m.Product)
                             .ToListAsync();

            var module=await _context.Modules.Where(x=>x.ModuleId==1).ToListAsync();

            var accessWithModuleAndProductNames = await _context.Accesss 
                .Include (a=> a.Modules)
                .ThenInclude(m=>m.Product)
                .Select(a => new AccessDTO
                {
                    AccessId = a.AccessId,
                    AccessName = a.AccessName,
                    LastModificatedDate = a.LastModificatedDate, 
                    ModuleName = string.Join(",", a.Modules
                        .Select(m => m.ModuleName)),
                    ProductName = string.Join(",", a.Modules
                        .Select(m => m.Product.ProductName))
                })
                .ToListAsync();

            return Ok(accessWithModuleAndProductNames);
        }

    


    /**************************************
    * 
    * Display One Access
    * 
    * ***************/
    [HttpGet("{id} GetAccessByOne")]
        public async Task<ActionResult<Access>> GetById(int id)
        {
            var access = await _context.Accesss.FindAsync(id);

            return Ok(access);
        }
        /**************************************
         * 
         * Update Access
         * 
         * ***************/
        //[HttpPut("updateAccess/{id}")]
        //public async Task<ActionResult> UpdateAccess(int id, AccessUpdateRequest accessUpdateRequest)
        //{
        //    var access = await _context.Accesss.Include(p => p.Modules).FirstOrDefaultAsync(m => m.AccessId == id);

        //    if (access == null)
        //    {
        //        return NotFound("Module not found.");
        //    }


        //    // Check if all moduleIds exist in the Modules table
        //    var invalidModuleIds = accessUpdateRequest.ModuleIds.Except(
        //        await _context.Modules.Select(m => m.ModuleId).ToListAsync()).ToList();

        //    if (invalidModuleIds.Any())
        //    {
        //        return BadRequest($"The following ModuleIds do not exist: {string.Join(",", invalidModuleIds)}");
        //    }

        //    if (!await _context.Products.AnyAsync(p => p.ProductId == accessUpdateRequest.ProductId))
        //    {
        //        return BadRequest($"The ProductId {accessUpdateRequest.ProductId} does not exist.");
        //    }

        //    access.AccessName = accessUpdateRequest.AccessName;
        //    access.LastModificatedDate = DateTime.Now;
        //    access.ModuleIds = accessUpdateRequest.ModuleIds;
        //    access.ProductId = accessUpdateRequest.ProductId;

        //    // Clear the existing ModuleNames and populate the new ones
        //    access.ModuleNames.Clear();
        //    foreach (var moduleId in accessUpdateRequest.ModuleIds)
        //    {
        //        var module = await _context.Modules.FindAsync(moduleId);
        //        if (module != null)
        //        {
        //            access.ModuleNames.Add(module.ModuleName);
        //        }
        //    }

        //    var product = await _context.Products.FindAsync(accessUpdateRequest.ProductId);
        //    if (product != null)
        //    {
        //        access.ProductName = product.ProductName;
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}


        /**************************************
         * 
         * Delete Access
         * 
         * ***************/

        [HttpDelete("DeleteAccess/{id}")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accesss == null)
            {
                return Problem("Entity set 'MyDbContext.Accesss'  is null.");
            }
            var access = await _context.Accesss.FindAsync(id);
            if (access != null)
            {
                _context.Accesss.Remove(access);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}

