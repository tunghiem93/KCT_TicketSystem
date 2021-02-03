using System.Data.Entity;
using TicketDesk.Web.Identity.Migrations;

namespace TicketDesk.Web.Identity
{
    public class TdIdentityDbInitializer : MigrateDatabaseToLatestVersion<TdIdentityContext, Configuration>
    {
        //no implementation, defined here to simplify and unify naming conventions and usage patterns 
    }

}
