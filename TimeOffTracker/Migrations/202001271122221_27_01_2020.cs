namespace TimeOffTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _27_01_2020 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        TelegramChatID = c.Int(nullable: false),
                        NotifyByTelegram = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "Id", "dbo.AspNetUsers");
            DropIndex("dbo.Notifications", new[] { "Id" });
            DropTable("dbo.Notifications");
        }
    }
}
