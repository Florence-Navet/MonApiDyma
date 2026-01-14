using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NorthWind.Data;
using NorthWind.Entities;
using NorthWind.Services;

namespace NorthWind.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployesController : ControllerBase
    {
        private readonly IServiceEmployes _serviceEmp;

        public EmployesController(IServiceEmployes service)
        {
            _serviceEmp = service;
        }

        // GET: api/Employes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employe>>> GetEmployés()
        {
            var employés = await _serviceEmp.ObtenirEmployes();
            return Ok(employés);
        }

        // GET: api/Employes/5
        //[HttpGet("{id}")] ou
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Employe>> GetEmploye(int id)
        {
            var employe = await _serviceEmp.ObtenirEmploye(id);

            if (employe == null)
            {
                return NotFound();
            }

            return Ok(employe);
        }

        // PUT: api/Employes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutEmploye(int id, Employe employe)
        //{
        //    if (id != employe.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(employe).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!EmployeExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Employes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Employe>> PostEmploye(Employe employe)
        //{
        //    _context.Employés.Add(employe);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetEmploye", new { id = employe.Id }, employe);
        //}

        // DELETE: api/Employes/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteEmploye(int id)
        //{
        //    var employe = await _context.Employés.FindAsync(id);
        //    if (employe == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Employés.Remove(employe);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //private bool EmployeExists(int id)
        //{
        //    return _context.Employés.Any(e => e.Id == id);
        //}
    }
}
