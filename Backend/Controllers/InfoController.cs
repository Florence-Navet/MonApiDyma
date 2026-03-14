using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BlazorWasm.Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        // GET: api/<InfoController>
        [HttpGet("EncodedToken")]
        public async Task<ActionResult<string?>> GetJetonEncodé()
        {
            UserTokenRequestParameters? p = null;
            UserToken jeton = await HttpContext.GetUserAccessTokenAsync(p);
            return jeton.AccessToken;
        }



       
        [HttpGet("DecodedToken")]
        public async Task<ActionResult<string?>> GetJetonDécodé()
        {
            //REcupère le jeton d'accès de l'utilisateur courant
            UserTokenRequestParameters? p = null;
            UserToken jeton = await HttpContext.GetUserAccessTokenAsync(p);

            string jetonDécodé = string.Empty;
            //decode le jeton et renvoie le resultat sous forme de teste JSON indente
            if (!string.IsNullOrEmpty(jeton.AccessToken))
            {
                var jwt = new JwtSecurityToken(jeton.AccessToken); // cree un jeton JWT à partir du jeton d'accès
                var doc = JsonDocument.Parse(jwt.Payload.SerializeToJson()); // parse le payload du jeton JWT en un document JSON
                jetonDécodé = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }); // sérialise le document JSON en une chaîne JSON indente
            }

            return jetonDécodé;
        }

    }


        

    

}
