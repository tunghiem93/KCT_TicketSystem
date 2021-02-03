namespace TicketDesk.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb2303 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Tickets", "ToDate");
            DropColumn("dbo.Tickets", "FromDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tickets", "FromDate", c => c.String());
            AddColumn("dbo.Tickets", "ToDate", c => c.String());
        }
    }
}
