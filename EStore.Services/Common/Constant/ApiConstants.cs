using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Services.Common.Constant
{
    public static class ApiConstants
    {
        public const string GetUserDetails = "/api/user/get-user-info";
        public const string GetAllUsers = "/api/user/get-all-users";
        public const string CheckUserExists = "/api/user/exists";
        public const string CheckSchoolExists = "/api/School/service/school-exists";
        public const string SchoolDetails = "/api/School/get-school-details";

        public const string GetUsersByRole = "/api/user/get-users-by-role";
        public const string CreateNotification = "/api/hrmsnotification/create-notification";
        public const string GetHolidaysBySchool = "/api/Holidays/get-all";
    }
}
