﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SessionOne
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MedicalLaboratoryEntities : DbContext
    {
        public MedicalLaboratoryEntities()
            : base("name=MedicalLaboratoryEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Administrators> Administrators { get; set; }
        public virtual DbSet<Analyzers> Analyzers { get; set; }
        public virtual DbSet<DataAnalyzator> DataAnalyzator { get; set; }
        public virtual DbSet<History> History { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<OrdersArchive> OrdersArchive { get; set; }
        public virtual DbSet<Pacients> Pacients { get; set; }
        public virtual DbSet<Services> Services { get; set; }
        public virtual DbSet<StrahovieCompanii> StrahovieCompanii { get; set; }
        public virtual DbSet<SuccessService> SuccessService { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<Types> Types { get; set; }
        public virtual DbSet<Users> Users { get; set; }
    }
}
