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
        public async Task<ActionResult<IEnumerable<Employe>>> GetEmployés(
           [FromQuery] string? rechercheNom, [FromQuery] DateTime? dateEmbaucheMax)
        {
            var employés = await _serviceEmp.ObtenirEmployes(rechercheNom, dateEmbaucheMax);
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

       
    }
}
