using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Radzen;
using StorybookCabinPOCBlazor.Components;
using StorybookCabinPOCBlazor.Models;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Security.Claims;

namespace StorybookCabinPOCBlazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Read the connection string from the appsettings.json file
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // Get HostingEnvironment
            var env = builder.Environment;
            builder.Configuration.AddJsonFile($"appsettings{env.EnvironmentName}.json", optional: true);
            builder.Configuration.AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddMicrosoftIdentityConsentHandler();

            builder.Services.AddCascadingAuthenticationState();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<StorybookCabinPOCBlazorContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<HttpContextAccessor>();
            builder.Services.AddScoped<StorybookCabinPOCBlazorService>();
            builder.Services.AddHttpClient<ProtectedApiCallHelper>();

            // Radzen Services
            builder.Services.AddRadzenComponents();

            // This is where you wire up to events to detect when a user Log in
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    builder.Configuration.Bind("AzureAdB2C", options);
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = async ctxt =>
                        {
                            // Invoked before redirecting to the identity provider to authenticate. 
                            // This can be used to set ProtocolMessage.State
                            // that will be persisted through the authentication process. 
                            // The ProtocolMessage can also be used to add or customize
                            // parameters sent to the identity provider.
                            await Task.Yield();
                        },
                        OnAuthenticationFailed = async ctxt =>
                        {
                            // They tried to log in but it failed
                            await Task.Yield();
                        },
                        OnSignedOutCallbackRedirect = async ctxt =>
                        {
                            ctxt.HttpContext.Response.Redirect(ctxt.Options.SignedOutRedirectUri);
                            ctxt.HandleResponse();
                            await Task.Yield();
                        },
                        OnTicketReceived = async ctxt =>
                        {
                            if (ctxt.Principal != null)
                            {
                                if (ctxt.Principal.Identity is ClaimsIdentity identity)
                                {
                                    // Set common values
                                    AuthClaims objAuthClaims = new AuthClaims();

                                    var colClaims = await ctxt.Principal.Claims.ToDynamicListAsync();

                                    objAuthClaims.IdentityProvider = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider")?.Value;

                                    objAuthClaims.Objectidentifier = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                                    objAuthClaims.EmailAddress = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

                                    objAuthClaims.FirstName = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;

                                    objAuthClaims.LastName = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value;

                                    objAuthClaims.AzureB2CFlow = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.microsoft.com/claims/authnclassreference")?.Value;

                                    objAuthClaims.auth_time = colClaims.FirstOrDefault(
                                        c => c.Type == "auth_time")?.Value;

                                    objAuthClaims.DisplayName = colClaims.FirstOrDefault(
                                        c => c.Type == "name")?.Value;

                                    objAuthClaims.idp_access_token = colClaims.FirstOrDefault(
                                        c => c.Type == "idp_access_token")?.Value;

                                    if (objAuthClaims.IdentityProvider != null)
                                    {
                                        // Google login
                                        if (objAuthClaims.IdentityProvider.ToLower().Contains("google"))
                                        {
                                            objAuthClaims.AuthenticationType = "Google";
                                        }

                                        // Microsoft account login
                                        if (objAuthClaims.IdentityProvider.ToLower().Contains("live"))
                                        {
                                            objAuthClaims.AuthenticationType = "Microsoft";
                                        }

                                        // Twitter login
                                        if (objAuthClaims.IdentityProvider.ToLower().Contains("twitter"))
                                        {
                                            objAuthClaims.AuthenticationType = "Twitter";
                                        }
                                    }

                                    // Azure Active Directory login
                                    // But this will only work if Azure B2C Custom Policy is configured
                                    // to pass the idp_access_token
                                    // See \!AzureB2CConfig\TrustFrameworkExtensions.xml
                                    // for an example that does that
                                    if (objAuthClaims.idp_access_token != null)
                                    {
                                        objAuthClaims.AuthenticationType = "Azure Active Directory";

                                        try
                                        {
                                            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(objAuthClaims.idp_access_token);
                                            objAuthClaims.EmailAddress = token.Claims.FirstOrDefault(c => c.Type == "upn")?.Value;
                                        }
                                        catch (System.Exception)
                                        {
                                            // Could not decode - do nothing 
                                        }
                                    }

                                    var request = ctxt.HttpContext.Request;
                                    var host = request.Host.ToUriComponent();

                                    // Insert into Database
                                    var optionsBuilder = new DbContextOptionsBuilder<StorybookCabinPOCBlazorContext>();
                                    optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                                    StorybookCabinPOCBlazorContext _context = new StorybookCabinPOCBlazorContext(optionsBuilder.Options);

                                    var ExistingUser = _context.Users
                                    .Where(x => x.Objectidentifier == objAuthClaims.Objectidentifier)
                                    .FirstOrDefault();

                                    if (ExistingUser == null)
                                    {
                                        // New User

                                        // Create User object
                                        var objUser = new User();

                                        try
                                        {
                                            objUser.Objectidentifier = objAuthClaims.Objectidentifier;
                                            objUser.AuthenticationType = objAuthClaims.AuthenticationType;
                                            objUser.IdentityProvider = objAuthClaims.IdentityProvider;
                                            objUser.SigninMethod = objAuthClaims.AzureB2CFlow;
                                            objUser.DisplayName = objAuthClaims.DisplayName;
                                            objUser.Email = objAuthClaims.EmailAddress;
                                            objUser.FirstName = objAuthClaims.FirstName;
                                            objUser.LastName = objAuthClaims.LastName;
                                            objUser.LastAuthTime = Convert.ToInt32(objAuthClaims.auth_time);
                                            objUser.LastidpAccessToken = objAuthClaims.idp_access_token;
                                            objUser.LastIpaddress = host;
                                            objUser.IsSuperUser = false;
                                            objUser.CreatedDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);

                                            _context.Users.Add(objUser);
                                            _context.SaveChanges();

                                            // Write to Log
                                            var objLog = new Log();

                                            objLog.LogType = "Login";
                                            objLog.IsError = false;
                                            objLog.LogDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);
                                            objLog.LogDetail = "New User";
                                            objLog.UserId = objUser.Id;
                                            objLog.LogIpaddress = host;

                                            _context.Logs.Add(objLog);
                                            _context.SaveChanges();

                                            // Every new Account gets 10 Credit
                                            List<Credit> colCredit = new List<Credit>();
                                            for (int i = 0; i < 10; i++)
                                            {
                                                Credit objCredit = new Credit();
                                                objCredit.CreatedDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);
                                                objCredit.User = objUser;
                                                objCredit.Active = true;

                                                colCredit.Add(objCredit);
                                            }

                                            _context.Credits.AddRange(colCredit);
                                            _context.SaveChanges();

                                            // Write to Log
                                            objLog = new Log();

                                            objLog.LogType = "Credit - New Account";
                                            objLog.IsError = false;
                                            objLog.LogDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);
                                            objLog.LogDetail = "10 Credit created";
                                            objLog.UserId = objUser.Id;
                                            objLog.LogIpaddress = host;

                                            _context.Logs.Add(objLog);
                                            _context.SaveChanges();
                                        }
                                        catch (Exception ex)
                                        {
                                            // Write to Log
                                            var objLog = new Log();

                                            objLog.LogType = "Login Error - New User";
                                            objLog.IsError = true;
                                            objLog.LogDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);
                                            objLog.LogDetail = String.Format($"User: {objUser.DisplayName} Objectidentifier: {objUser.Objectidentifier} Message: {ex.GetBaseException().Message}");
                                            objLog.LogIpaddress = host;

                                            _context.Logs.Add(objLog);
                                            _context.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        // Update Existing User

                                        try
                                        {
                                            ExistingUser.AuthenticationType = objAuthClaims.AuthenticationType;
                                            ExistingUser.IdentityProvider = objAuthClaims.IdentityProvider;
                                            ExistingUser.SigninMethod = objAuthClaims.AzureB2CFlow;
                                            ExistingUser.DisplayName = objAuthClaims.DisplayName;
                                            ExistingUser.Email = objAuthClaims.EmailAddress;
                                            ExistingUser.FirstName = objAuthClaims.FirstName;
                                            ExistingUser.LastName = objAuthClaims.LastName;
                                            ExistingUser.LastAuthTime = Convert.ToInt32(objAuthClaims.auth_time);
                                            ExistingUser.LastidpAccessToken = objAuthClaims.idp_access_token;
                                            ExistingUser.LastIpaddress = host;
                                            ExistingUser.UpdatedDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);

                                            _context.SaveChanges();

                                            // Write to Log

                                            var objLog = new Log();

                                            objLog.LogType = "Login";
                                            objLog.IsError = false;
                                            objLog.LogDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);
                                            objLog.LogDetail = "Existing User";
                                            objLog.UserId = ExistingUser.Id;
                                            objLog.LogIpaddress = host;

                                            _context.Logs.Add(objLog);
                                            _context.SaveChanges();
                                        }
                                        catch (Exception ex)
                                        {
                                            // Write to Log
                                            var objLog = new Log();

                                            objLog.LogType = "Login Error - Existing User";
                                            objLog.IsError = true;
                                            objLog.LogDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);
                                            objLog.UserId = ExistingUser.Id;
                                            objLog.LogDetail = ex.GetBaseException().Message;
                                            objLog.LogIpaddress = host;

                                            _context.Logs.Add(objLog);
                                            _context.SaveChanges();
                                        }
                                    }
                                }
                            }

                            await Task.Yield();
                        },
                    };
                });

            builder.Services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // **** Godot MIME Support ****

            // Set up custom content types - associating file extension to MIME type
            var provider = new FileExtensionContentTypeProvider();

            // Add new mappings
            provider.Mappings[".pck"] = "application/octet-stream";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
