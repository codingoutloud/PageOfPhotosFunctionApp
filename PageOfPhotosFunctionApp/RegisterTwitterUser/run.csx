//#define DEBUG
using System.Net;

public static void Run(HttpRequestMessage req, out PageRef pageRef, TraceWriter log)
{
    log.Info("BEGIN - RegisterTwitterUser");

    var claims = ((System.Security.Claims.ClaimsPrincipal) (System.Threading.Thread.CurrentPrincipal)).Claims;
    if (claims == null)
    {
        return req.CreateResponse(HttpStatusCode.BadRequest, "Please authenticate with Twitter before calling this endpoint. Probably this means that the Authentication from Azure Functions or Web Apps is not configured - or that the Twitter App configuration is not aligned.");
    }

    var isAuth = ((System.Security.Claims.ClaimsPrincipal) (System.Threading.Thread.CurrentPrincipal)).Identity.IsAuthenticated;

    log.Info($"Is Authenticated? {isAuth}");

#if DEBUG
    foreach (var claim in claims)
    {
        log.Info($"Claim '{claim.Type}' = '{claim.Value}'");
    }
#endif

    var SlugClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
    var NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
    var DescriptionClaimType = "urn:twitter:description";
    var ProfileImageUrlClaimType = "urn:twitter:profile_image_url_https";

    var slug = claims.FirstOrDefault(c => c.Type == SlugClaimType).Value;
    var name = claims.FirstOrDefault(c => c.Type == NameClaimType).Value;
    var desc = claims.FirstOrDefault(c => c.Type == DescriptionClaimType).Value;
    var url  = claims.FirstOrDefault(c => c.Type == ProfileImageUrlClaimType).Value;
    
    pageRef =
         new PageRef() { 
             PartitionKey = "registered_user", 
             RowKey = slug, 
             Name = name,
             Description = $"{Guid.NewGuid()} - {desc}",
             UserAvatarUrl = url
         };

    log.Info("END - RegisterTwitterUser");
}

public class PageRef
{
    public string PartitionKey { get; set; }  // "registered_user"
    public string RowKey { get; set; }        // slug = twitter handle
    public string Name { get; set; }          // user's name
    public string Description { get; set; }   // description of user
    public string UserAvatarUrl { get; set; } // url to user's profile photo
}

