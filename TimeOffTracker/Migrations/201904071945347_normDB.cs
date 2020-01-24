namespace TimeOffTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class normDB : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RequestChecks", "RequestId", "dbo.Requests");
            DropForeignKey("dbo.RequestChecks", "StatusId", "dbo.RequestStatuses");
            DropForeignKey("dbo.Requests", "VacationTypesId", "dbo.VacationTypes");
            DropIndex("dbo.RequestChecks", new[] { "RequestId" });
            DropIndex("dbo.RequestChecks", new[] { "StatusId" });
            DropIndex("dbo.Requests", new[] { "VacationTypesId" });
            RenameColumn(table: "dbo.RequestChecks", name: "ApproverId", newName: "Approver_Id");
            RenameColumn(table: "dbo.RequestChecks", name: "RequestId", newName: "Request_Id");
            RenameColumn(table: "dbo.RequestChecks", name: "StatusId", newName: "Status_Id");
            RenameColumn(table: "dbo.Requests", name: "EmployeeId", newName: "Employee_Id");
            RenameColumn(table: "dbo.Requests", name: "VacationTypesId", newName: "VacationTypes_Id");
            RenameIndex(table: "dbo.RequestChecks", name: "IX_ApproverId", newName: "IX_Approver_Id");
            RenameIndex(table: "dbo.Requests", name: "IX_EmployeeId", newName: "IX_Employee_Id");
            AlterColumn("dbo.RequestChecks", "Request_Id", c => c.Int());
            AlterColumn("dbo.RequestChecks", "Status_Id", c => c.Int());
            AlterColumn("dbo.Requests", "VacationTypes_Id", c => c.Int());
            CreateIndex("dbo.RequestChecks", "Request_Id");
            CreateIndex("dbo.RequestChecks", "Status_Id");
            CreateIndex("dbo.Requests", "VacationTypes_Id");
            AddForeignKey("dbo.RequestChecks", "Request_Id", "dbo.Requests", "Id");
            AddForeignKey("dbo.RequestChecks", "Status_Id", "dbo.RequestStatuses", "Id");
            AddForeignKey("dbo.Requests", "VacationTypes_Id", "dbo.VacationTypes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requests", "VacationTypes_Id", "dbo.VacationTypes");
            DropForeignKey("dbo.RequestChecks", "Status_Id", "dbo.RequestStatuses");
            DropForeignKey("dbo.RequestChecks", "Request_Id", "dbo.Requests");
            DropIndex("dbo.Requests", new[] { "VacationTypes_Id" });
            DropIndex("dbo.RequestChecks", new[] { "Status_Id" });
            DropIndex("dbo.RequestChecks", new[] { "Request_Id" });
            AlterColumn("dbo.Requests", "VacationTypes_Id", c => c.Int(nullable: false));
            AlterColumn("dbo.RequestChecks", "Status_Id", c => c.Int(nullable: false));
            AlterColumn("dbo.RequestChecks", "Request_Id", c => c.Int(nullable: false));
            RenameIndex(table: "dbo.Requests", name: "IX_Employee_Id", newName: "IX_EmployeeId");
            RenameIndex(table: "dbo.RequestChecks", name: "IX_Approver_Id", newName: "IX_ApproverId");
            RenameColumn(table: "dbo.Requests", name: "VacationTypes_Id", newName: "VacationTypesId");
            RenameColumn(table: "dbo.Requests", name: "Employee_Id", newName: "EmployeeId");
            RenameColumn(table: "dbo.RequestChecks", name: "Status_Id", newName: "StatusId");
            RenameColumn(table: "dbo.RequestChecks", name: "Request_Id", newName: "RequestId");
            RenameColumn(table: "dbo.RequestChecks", name: "Approver_Id", newName: "ApproverId");
            CreateIndex("dbo.Requests", "VacationTypesId");
            CreateIndex("dbo.RequestChecks", "StatusId");
            CreateIndex("dbo.RequestChecks", "RequestId");
            AddForeignKey("dbo.Requests", "VacationTypesId", "dbo.VacationTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RequestChecks", "StatusId", "dbo.RequestStatuses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RequestChecks", "RequestId", "dbo.Requests", "Id", cascadeDelete: true);
        }
    }
}
