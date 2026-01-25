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
        private readonly ILogger<EmployesController> _logger;

        public EmployesController(IServiceEmployes service, ILogger<EmployesController> logger)
        {
            _serviceEmp = service;
            _logger = logger;
        }
      #region Requete GET

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

      // GET: api/Regions/{id}
      [HttpGet("/api/Regions/{id}")]
      public async Task<ActionResult<Region>> GetRégions(int id)
      {
         Region? region = await _serviceEmp.ObtenirRégion(id);

         if (region == null)
         {
            return NotFound();
         }

         return Ok(region);
      }

      #endregion

      #region Requete POST
      //POST : api/ Employes
      [HttpPost]
      public async Task<ActionResult<Employe>> PostEmployé(Employe emp)
      {
            try
            {
                // enregistre l'employe dans la base et le recuperer avec son ID genere automatiquement
                Employe res = await _serviceEmp.AjouterEmployé(emp);

            //Renvoie reponde 201 avec l'entête
            return CreatedAtAction(nameof(GetEmploye), new { id = res.Id }, res);

                //}
                //gestion d'erreur de la class DbUpdateException
                //catch (DbUpdateException e)
                //{
                //    ProblemDetails pb = e.ConvertToProblemDetails();
                //    //detail : msg erreur, instance, statue: code http, titre : type d'erreur
                //    return Problem(pb.Detail, null, pb.Status, pb.Title);

            }
            //gestion erreur de ControllerBaseEx
            catch (Exception e)
            {
                //return this.CustomResponseForError(e);
                return this.CustomResponseForError(e, emp, _logger);
            }

            //si je desactive le try catch, le middleware de gestion des erreurs de bdd s'en charge

        }

        //POST : api/affectations
        [HttpPost("/api/Affectations")]
      public async Task<ActionResult<Affectation>> PostAffectation([FromForm] Affectation a)
      {
         //enregistre les donnees en base
         await _serviceEmp.AjouterAffectation(a);

         //renvoie reponse 201 avec entete
         return CreatedAtAction(nameof(GetEmploye), new { id = a.IdEmploye }, a);
      }

      //POST : api/Employes/formdata
      [HttpPost("formdata")]
      public async Task<ActionResult<Employe>> PostEmployéFormData([FromForm] FormEmploye fe)
      {
         Employe emp = new()
         {
            IdAdresse = fe.IdAdresse,
            IdManager = fe.IdManager,
            Nom = fe.Nom,
            Prenom = fe.Prenom,
            Fonction = fe.Fonction,
            Civilite = fe.Civilite,
            DateNaissance = fe.DateNaissance,
            DateEmbauche = fe.DateEmbauche
         };

         //recupere les donnees de l'adresse

         emp.Adresse = new()
         {
            Id = fe.Adresse.Id,
            Rue = fe.Adresse.Rue,
            CodePostal = fe.Adresse.CodePostal,
            Ville = fe.Adresse.Ville,
            Region = fe.Adresse.Region,
            Pays = fe.Adresse.Pays,
            Tel = fe.Adresse.Tel
         };

         //recuperer les donnes du fichier photo
         if (fe.Photo != null)
         {
            using Stream stream = fe.Photo.OpenReadStream();// envoie ss type de flux en c#
            emp.Photo = new byte[fe.Photo.Length]; // transforme en tableau de byte de la longueur de la photo
            await stream.ReadAsync(emp.Photo); //transfere le stream dans le tableau de byte
         }

         // Récupère les données du fichier notes
         if (fe.Notes != null) // verifions si fichier est diff de null
         {
            using StreamReader reader = new(fe.Notes.OpenReadStream());  //lire le contenu du fichier en utilisant le constructeur
            emp.Notes = await reader.ReadToEndAsync(); //transfere le contenu dans une chaine en utilisant sa methode
         }

         Employe res = await _serviceEmp.AjouterEmployé(emp);

         // Renvoie une réponse de code 201 avec l'en-tête 
         // "location: <url d'accès à l’employé>" et un corps contenant l’employé
         return CreatedAtAction(nameof(GetEmploye), new { id = emp.Id }, res); // permet l'envoie d'une 201 pour l'envoie d'un enregistrement et cree le location
      }

      #endregion



   }
}
