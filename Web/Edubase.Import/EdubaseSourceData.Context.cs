//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Edubase.Import
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EdubaseSourceDataEntities : DbContext
    {
        public EdubaseSourceDataEntities()
            : base("name=EdubaseSourceDataEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Accomodationchanged> Accomodationchanged { get; set; }
        public virtual DbSet<Administrativeward> Administrativeward { get; set; }
        public virtual DbSet<Admissionspolicy> Admissionspolicy { get; set; }
        public virtual DbSet<Appointingbody> Appointingbody { get; set; }
        public virtual DbSet<Boarders> Boarders { get; set; }
        public virtual DbSet<Boardingestablishment> Boardingestablishment { get; set; }
        public virtual DbSet<Casward> Casward { get; set; }
        public virtual DbSet<Ccdisadvantagedarea> Ccdisadvantagedarea { get; set; }
        public virtual DbSet<Ccgovernance> Ccgovernance { get; set; }
        public virtual DbSet<Ccgrouplead> Ccgrouplead { get; set; }
        public virtual DbSet<Ccoperationalhours> Ccoperationalhours { get; set; }
        public virtual DbSet<Ccphasetype> Ccphasetype { get; set; }
        public virtual DbSet<Childcarefacilities> Childcarefacilities { get; set; }
        public virtual DbSet<Contactpreference> Contactpreference { get; set; }
        public virtual DbSet<County> County { get; set; }
        public virtual DbSet<Diocese> Diocese { get; set; }
        public virtual DbSet<Directprovisionofearlyyears> Directprovisionofearlyyears { get; set; }
        public virtual DbSet<Districtadministrative> Districtadministrative { get; set; }
        public virtual DbSet<Establishmentlinks> Establishmentlinks { get; set; }
        public virtual DbSet<Establishments> Establishments { get; set; }
        public virtual DbSet<Establishmentstatus> Establishmentstatus { get; set; }
        public virtual DbSet<Establishmenttypegroup> Establishmenttypegroup { get; set; }
        public virtual DbSet<Federationflag> Federationflag { get; set; }
        public virtual DbSet<Ftprov> Ftprov { get; set; }
        public virtual DbSet<Furthereducationtype> Furthereducationtype { get; set; }
        public virtual DbSet<Gender> Gender { get; set; }
        public virtual DbSet<GOR> GOR { get; set; }
        public virtual DbSet<Governors> Governors { get; set; }
        public virtual DbSet<GroupData> GroupData { get; set; }
        public virtual DbSet<GroupLinks> GroupLinks { get; set; }
        public virtual DbSet<GroupType> GroupType { get; set; }
        public virtual DbSet<Gsslacode> Gsslacode { get; set; }
        public virtual DbSet<Headtitle> Headtitle { get; set; }
        public virtual DbSet<Independentschooltype> Independentschooltype { get; set; }
        public virtual DbSet<Inspectorate> Inspectorate { get; set; }
        public virtual DbSet<Inspectoratename> Inspectoratename { get; set; }
        public virtual DbSet<LocalAuthority> LocalAuthority { get; set; }
        public virtual DbSet<Localgovernors> Localgovernors { get; set; }
        public virtual DbSet<LSOA> LSOA { get; set; }
        public virtual DbSet<Marketingoptinout> Marketingoptinout { get; set; }
        public virtual DbSet<MSOA> MSOA { get; set; }
        public virtual DbSet<Nationality> Nationality { get; set; }
        public virtual DbSet<Nurseryprovision> Nurseryprovision { get; set; }
        public virtual DbSet<Officialsixthform> Officialsixthform { get; set; }
        public virtual DbSet<Ofstedratings> Ofstedratings { get; set; }
        public virtual DbSet<Ofstedspecialmeasures> Ofstedspecialmeasures { get; set; }
        public virtual DbSet<Parliamentaryconstituency> Parliamentaryconstituency { get; set; }
        public virtual DbSet<Phaseofeducation> Phaseofeducation { get; set; }
        public virtual DbSet<PRUEBD> PRUEBD { get; set; }
        public virtual DbSet<Prueducatedbyothers> Prueducatedbyothers { get; set; }
        public virtual DbSet<Prufulltimeprovision> Prufulltimeprovision { get; set; }
        public virtual DbSet<PRUSEN> PRUSEN { get; set; }
        public virtual DbSet<Reasonestablishmentclosed> Reasonestablishmentclosed { get; set; }
        public virtual DbSet<Reasonestablishmentopened> Reasonestablishmentopened { get; set; }
        public virtual DbSet<Reasonforchange> Reasonforchange { get; set; }
        public virtual DbSet<Religiouscharacter> Religiouscharacter { get; set; }
        public virtual DbSet<Religiousethos> Religiousethos { get; set; }
        public virtual DbSet<Resourcedprovision> Resourcedprovision { get; set; }
        public virtual DbSet<Section41approved> Section41approved { get; set; }
        public virtual DbSet<Specialclasses> Specialclasses { get; set; }
        public virtual DbSet<Specialeducationaneeds> Specialeducationaneeds { get; set; }
        public virtual DbSet<Teenagemothers> Teenagemothers { get; set; }
        public virtual DbSet<Trustflag> Trustflag { get; set; }
        public virtual DbSet<Typeofestablishment> Typeofestablishment { get; set; }
        public virtual DbSet<Typeofresourcedprovision> Typeofresourcedprovision { get; set; }
        public virtual DbSet<Urbanrural> Urbanrural { get; set; }
    }
}
