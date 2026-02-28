// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HoteIdentiyServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Duende.IdentityServer.Services;

namespace HoteIdentiyServer.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IIdentityServerInteractionService _interaction;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger,
            IIdentityServerInteractionService interaction)
        {
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
        }

        public async Task<IActionResult>OnGet(string returnUrl = null)
        {
            return await OnPost(returnUrl);
        }


        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            string logoutId = Request.Query["logoutId"].ToString(); //permet de recupere un Id de deconnexion  à partir de la requete
                                                                    //reçu par la page


            if (returnUrl != null)

                return LocalRedirect(returnUrl);

            // on utilise l'id dans ce bloc de code
            if (!string.IsNullOrEmpty(logoutId))
            {
                //recupere l'url vers laquelle rediriger après deconnexion
                var req = await _interaction.GetLogoutContextAsync(logoutId);
                returnUrl = req.PostLogoutRedirectUri;

                //redirige vers cette url si elle existe
                if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            }

            return Page(); // raffraichi la page
        }
    }
}
