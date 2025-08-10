using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace LoanWise.Api.Auth
{   

    public sealed class SignalRUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var u = connection.User;
            return u?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? u?.FindFirst("sub")?.Value
                ?? u?.FindFirst("uid")?.Value;
        }
    }

}
