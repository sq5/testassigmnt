// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CloudArchive.Services;
using COMMON.Common.Services.ContextService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace COMMON.Common.Services.AuthorizationService
{
    public class TokenFilter : ActionFilterAttribute
    {
        private IHttpContextAccessor httpContextAccessor { get; }
        private readonly IContextService _clientService;
        private readonly ITokenService _tokenService;

        public TokenFilter(IHttpContextAccessor contextAccessor, IContextService clientService, ITokenService tokenService)
        {
            httpContextAccessor = contextAccessor;
            _clientService = clientService;
            _tokenService = tokenService;
        }


        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (!httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
                actionContext.Result = new ForbidResult("No Authorization header ( #err-80)");
            var authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
            var token = string.Empty;
            if (authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }

            if (string.IsNullOrEmpty(token))
                actionContext.Result = new BadRequestObjectResult(new { message = "No token received ( #err-30)" });

            var client = _clientService.ClientDto;
            if (client == null)
                actionContext.Result = new ForbidResult("Could not find any client for this token ( #err-40)");

            var result = _tokenService.CheckToken(token, client).GetAwaiter().GetResult();

            if (!string.IsNullOrEmpty(result))
                actionContext.Result = new BadRequestObjectResult(new { message = result });
            base.OnActionExecuting(actionContext);
        }
    }
}
