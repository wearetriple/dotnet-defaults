using System;
using System.Linq;

namespace ServiceTests
{
    internal class Service
    {
        private IGateway _gateway;

        public Service(IGateway gateway)
        {
            _gateway = gateway;
        }

        public object GetDashboardByUserName(string userName)
        {
            var dashboard = string.Empty;

            try
            {
                var userId = _gateway.GetUserIdByUserName(userName);
                dashboard += userId;

                var hobbies = _gateway.GetHobbiesByUserId(userId);
                AddHobbiesToDashboard(hobbies, ref dashboard);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Logging exception: {ex}");
                throw;
            }

            return dashboard;
        }

        private static void AddHobbiesToDashboard(string[] hobbies, ref string dashboard)
        {
            if (hobbies.Any())
            {
                foreach (var hobby in hobbies)
                {
                    dashboard += $", {hobby}";
                }
            }
        }
    }
}
