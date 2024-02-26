using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Radzen;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace StorybookCabinPOCBlazor.Models
{
#nullable disable
    public class StorybookCabinPOCBlazorService
    {
        private readonly StorybookCabinPOCBlazorContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration Configuration;

        public StorybookCabinPOCBlazorService(StorybookCabinPOCBlazorContext context,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            Configuration = configuration;
        }

        // Users

        #region public IQueryable<Users> GetAllUsersAsync()
        public IQueryable<User> GetAllUsersAsync()
        {
            return _context.Users.AsQueryable();
        }
        #endregion

        #region public string UpdateUser(User paramUser)
        public string UpdateUser(User paramUser)
        {
            try
            {
                var objUser = _context.Users.Where(x => x.Id == paramUser.Id).FirstOrDefault();

                objUser.DisplayName = paramUser.DisplayName;
                objUser.Email = paramUser.Email;
                objUser.FirstName = paramUser.FirstName;
                objUser.LastName = paramUser.LastName;
                objUser.UpdatedDate = DateTime.Now;

                _context.SaveChanges();

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region public int GetUserIdFromObjectidentifier()
        public int GetUserIdFromObjectidentifier(string paramObjectidentifier)
        {
            return _context.Users.Where(x => x.Objectidentifier == paramObjectidentifier).Select(x => x.Id).FirstOrDefault();
        }
        #endregion

        #region public async Task<Users> GetUserFromObjectidentifierAsync(string paramObjectidentifier)
        public async Task<User> GetUserFromObjectidentifierAsync(string paramObjectidentifier)
        {
            return await _context.Users.Where(x => x.Objectidentifier == paramObjectidentifier).FirstOrDefaultAsync();
        }
        #endregion

        #region public async Task<int> GetUserCreditsFromObjectidentifierAsync(string paramObjectidentifier)
        public async Task<int> GetUserCreditsFromObjectidentifierAsync(string paramObjectidentifier)
        {
            return await (from user in _context.Users
                          from credit in _context.Credits
                          where credit.Active == true
                          where user.Id == credit.UserId
                          where user.Objectidentifier == paramObjectidentifier
                          select credit).CountAsync();
        }
        #endregion

        #region public async Task<IEnumerable<ServiceSettings>> LoadServiceSettings(LoadDataArgs args)
        public async Task<IEnumerable<ServiceSetting>> LoadServiceSettings(LoadDataArgs args)
        {
            var query = _context.ServiceSettings.OrderByDescending(x => x.ServiceName).AsQueryable();

            if (!string.IsNullOrEmpty(args.Filter))
            {
                query = query.Where(args.Filter);
            }

            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                query = query.OrderBy(args.OrderBy);
            }

            return await query.Skip(args.Skip.Value).Take(args.Top.Value).ToListAsync();
        }
        #endregion

        #region public int GetTotalServiceSettingsCount()
        public int GetTotalServiceSettingsCount()
        {
            return _context.ServiceSettings.Count();
        }
        #endregion

        #region public void UpdateServiceSettings(ServiceSettings paramServiceSettings)
        public void UpdateServiceSettings(ServiceSetting paramServiceSettings)
        {
            try
            {
                var objServiceSettings = _context.ServiceSettings.Where(x => x.Id == paramServiceSettings.Id).FirstOrDefault();
                objServiceSettings.Active = paramServiceSettings.Active;
                _context.SaveChanges();
            }
            catch
            {
                DetachAllEntities();
                throw;
            }
        }
        #endregion

        // Credits

        #region public async Task DeleteCreditFromUser(int UserId)
        public async Task DeleteCreditFromUser(int UserId)
        {
            var objCredit = await _context.Credits.Where(x => x.UserId == UserId).FirstOrDefaultAsync();

            if (objCredit != null)
            {
                _context.Credits.Remove(objCredit);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        // Logs

        #region public void CreateGeneralLogAsync(Logs paramLogs)
        public void CreateGeneralLogAsync(Log paramLogs)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext.User;

                var Objectidentifier =
                    user.Claims.FirstOrDefault(
                    c => c.Type ==
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?
                    .Value;

                int CurrentUserId = GetUserIdFromObjectidentifier(Objectidentifier);

                Log objLogs = new Log();

                objLogs.LogDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);
                objLogs.LogIpaddress = paramLogs.LogIpaddress;
                objLogs.LogType = "General";
                objLogs.IsError = false;
                objLogs.LogDetail = paramLogs.LogDetail;
                objLogs.UserId = CurrentUserId;

                _context.Logs.Add(objLogs);
                _context.SaveChanges();
            }
            catch
            {
                DetachAllEntities();
                throw;
            }
        }
        #endregion

        #region public void CreateLogAsync(Log paramLogs, string paramLogType)
        public void CreateLogAsync(Log paramLogs, string paramLogType)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext.User;

                var Objectidentifier =
                    user.Claims.FirstOrDefault(
                    c => c.Type ==
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?
                    .Value;

                int CurrentUserId = GetUserIdFromObjectidentifier(Objectidentifier);

                Log objLogs = new Log();

                objLogs.LogDate = Utilities.ConvertDateToPacificStandardTime(DateTime.Now);
                objLogs.LogIpaddress = paramLogs.LogIpaddress;
                objLogs.LogType = paramLogType;
                objLogs.IsError = false;
                objLogs.VideoRequestId = paramLogs.VideoRequestId;
                objLogs.LogDetail = paramLogs.LogDetail;
                objLogs.UserId = CurrentUserId;

                _context.Logs.Add(objLogs);
                _context.SaveChanges();
            }
            catch
            {
                DetachAllEntities();
                throw;
            }
        }
        #endregion

        #region public IQueryable<Logs> GetLogs()
        public IQueryable<Log> GetLogs()
        {
            return _context.Logs.Include(Logs => Logs.User).OrderByDescending(x => x.LogDate).AsQueryable();
        }
        #endregion

        // Utility

        #region public void DetachAllEntities()
        public void DetachAllEntities()
        {
            var changedEntriesCopy = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();
            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }
        #endregion

    }
}