﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace xDC.Infrastructure.Application
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class kashflowDBEntities : DbContext
    {
        public kashflowDBEntities()
            : base("name=kashflowDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetActiveDirectoryUsers> AspNetActiveDirectoryUsers { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Config_Dropdown> Config_Dropdown { get; set; }
        public virtual DbSet<Config_Key> Config_Key { get; set; }
        public virtual DbSet<Config_Approver> Config_Approver { get; set; }
        public virtual DbSet<App_Notification> App_Notification { get; set; }
        public virtual DbSet<Form_Header> Form_Header { get; set; }
        public virtual DbSet<AMSD_InflowFund> AMSD_InflowFund { get; set; }
        public virtual DbSet<EDW_TradeItem> EDW_TradeItem { get; set; }
        public virtual DbSet<EDW_BankBalance> EDW_BankBalance { get; set; }
        public virtual DbSet<ISSD_Balance> ISSD_Balance { get; set; }
        public virtual DbSet<ISSD_FormHeader> ISSD_FormHeader { get; set; }
        public virtual DbSet<ISSD_TradeSettlement> ISSD_TradeSettlement { get; set; }
        public virtual DbSet<Form_Workflow> Form_Workflow { get; set; }
        public virtual DbSet<Config_Application> Config_Application { get; set; }
        public virtual DbSet<FID_TS10> FID_TS10 { get; set; }
        public virtual DbSet<FID_TS10_TradeItem> FID_TS10_TradeItem { get; set; }
        public virtual DbSet<FID_TS10_Approval> FID_TS10_Approval { get; set; }
        public virtual DbSet<FID_TS10_OpeningBalance> FID_TS10_OpeningBalance { get; set; }
        public virtual DbSet<EDW_FID_List> EDW_FID_List { get; set; }
        public virtual DbSet<EDW_Maturity> EDW_Maturity { get; set; }
        public virtual DbSet<FID_Treasury> FID_Treasury { get; set; }
        public virtual DbSet<FID_Treasury_Deposit> FID_Treasury_Deposit { get; set; }
        public virtual DbSet<FID_Treasury_Item> FID_Treasury_Item { get; set; }
    }
}
