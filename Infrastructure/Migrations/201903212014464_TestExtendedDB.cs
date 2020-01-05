namespace TimeOffTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestExtendedDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RequestChecks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Priority = c.Int(nullable: false),
                        Reason = c.String(nullable: false),
                        Approver_Id = c.String(nullable: false, maxLength: 128),
                        Request_Id = c.Int(nullable: false),
                        Status_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Approver_Id, cascadeDelete: true)
                .ForeignKey("dbo.Requests", t => t.Request_Id, cascadeDelete: true)
                .ForeignKey("dbo.RequestStatuses", t => t.Status_Id, cascadeDelete: true)
                .Index(t => t.Approver_Id)
                .Index(t => t.Request_Id)
                .Index(t => t.Status_Id);
            
            CreateTable(
                "dbo.Requests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateStart = c.DateTime(nullable: false),
                        DateEnd = c.DateTime(nullable: false),
                        Description = c.String(nullable: false),
                        Employee_Id = c.String(maxLength: 128),
                        VacationTypes_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Employee_Id)
                .ForeignKey("dbo.VacationTypes", t => t.VacationTypes_Id, cascadeDelete: true)
                .Index(t => t.Employee_Id)
                .Index(t => t.VacationTypes_Id);
            
            CreateTable(
                "dbo.VacationTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        MaxDays = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RequestStatuses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserVacationDays",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VacationDays = c.Int(nullable: false),
                        LastUpdate = c.DateTime(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                        VacationType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.VacationTypes", t => t.VacationType_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.VacationType_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserVacationDays", "VacationType_Id", "dbo.VacationTypes");
            DropForeignKey("dbo.UserVacationDays", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.RequestChecks", "Status_Id", "dbo.RequestStatuses");
            DropForeignKey("dbo.RequestChecks", "Request_Id", "dbo.Requests");
            DropForeignKey("dbo.Requests", "VacationTypes_Id", "dbo.VacationTypes");
            DropForeignKey("dbo.Requests", "Employee_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.RequestChecks", "Approver_Id", "dbo.AspNetUsers");
            DropIndex("dbo.UserVacationDays", new[] { "VacationType_Id" });
            DropIndex("dbo.UserVacationDays", new[] { "User_Id" });
            DropIndex("dbo.Requests", new[] { "VacationTypes_Id" });
            DropIndex("dbo.Requests", new[] { "Employee_Id" });
            DropIndex("dbo.RequestChecks", new[] { "Status_Id" });
            DropIndex("dbo.RequestChecks", new[] { "Request_Id" });
            DropIndex("dbo.RequestChecks", new[] { "Approver_Id" });
            DropTable("dbo.UserVacationDays");
            DropTable("dbo.RequestStatuses");
            DropTable("dbo.VacationTypes");
            DropTable("dbo.Requests");
            DropTable("dbo.RequestChecks");
        }
    }
}
