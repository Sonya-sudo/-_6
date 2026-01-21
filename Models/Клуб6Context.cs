using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Клуб_6.Models;

public partial class Клуб6Context : DbContext
{
    public Клуб6Context()
    {
    }

    public Клуб6Context(DbContextOptions<Клуб6Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Criterion> Criteria { get; set; }

    public virtual DbSet<Discipline> Disciplines { get; set; }

    public virtual DbSet<Dog> Dogs { get; set; }

    public virtual DbSet<DogCriteriaResults_DogList> DogCriteriaResultsDogLists { get; set; } 

    public virtual DbSet<DogDiscipline> DogDisciplines { get; set; }

    public virtual DbSet<DogList> DogLists { get; set; }

    public virtual DbSet<DogOwner> DogOwners { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventComposition> EventCompositions { get; set; }

    public virtual DbSet<EventStatus> EventStatuses { get; set; }

    public virtual DbSet<Kennel> Kennels { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=SOFYCO;Database=Клуб_6;TrustServerCertificate=True;Integrated Security=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.CriterionID).HasName("PK__Criteria__647C3BD1AA41C4FB");

            entity.Property(e => e.CriterionID).HasColumnName("CriterionID");
            entity.Property(e => e.CriterionName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsImportant).HasDefaultValue(false);
            entity.Property(e => e.CompositionID).HasColumnName("CompositionID");

            entity.HasMany(d => d.Options)
            .WithOne(p => p.Criterion)
            .HasForeignKey(d => d.CriterionID)
            .HasConstraintName("FK_Options_Criteria");
        });

        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.HasKey(e => e.DisciplineId).HasName("PK__Discipli__29093750E59CB0B5");

            entity.Property(e => e.DisciplineId).HasColumnName("DisciplineID");
            entity.Property(e => e.Coefficient).HasDefaultValue(1);
            entity.Property(e => e.DisciplineName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CompositionID).HasColumnName("CompositionID");
        });

        modelBuilder.Entity<Dog>(entity =>
        {
            entity.HasKey(e => e.ChipNumber).HasName("PK__Dogs__7460557C7A3AB5C3");

            entity.Property(e => e.ChipNumber).ValueGeneratedNever();
            entity.Property(e => e.Breed)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DogName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FatherName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.HeightCm).HasColumnName("HeightCM");
            entity.Property(e => e.IsAlive).HasDefaultValue(true);
            entity.Property(e => e.KennelId).HasColumnName("KennelID");
            entity.Property(e => e.MotherName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.WeightKg).HasColumnName("WeightKG");

            entity.HasOne(d => d.Kennel).WithMany(p => p.Dogs)
                .HasForeignKey(d => d.KennelId)
                .HasConstraintName("FK__Dogs__KennelID__4F7CD00D");
        });

        modelBuilder.Entity<DogCriteriaResults_DogList>(entity =>
        {
            entity.HasKey(e => e.ResultIdRecordIdId).HasName("PK__DogCrite__AA41C4FB12345678"); 

            entity.ToTable("DogCriteriaResults_DogList");

            entity.Property(e => e.ResultIdRecordIdId)
                .HasColumnName("ResultID_RecordID_ID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.CriterionId).HasColumnName("CriterionID");
            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.UserInput).HasColumnType("text");

            entity.HasOne(d => d.Criterion)
            .WithMany()
            .HasForeignKey(d => d.CriterionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__DogCritResDogList__Criterion");


            entity.HasOne(d => d.Record)
                .WithMany(p => p.DogCriteriaResultsDogLists)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DogCritResDogList__DogList");

            entity.HasIndex(e => e.RecordId, "IX_DogCriteriaResults_DogList_RecordID");
        });

        modelBuilder.Entity<DogDiscipline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DogDisci__3214EC27DC7E3462");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DisciplineId).HasColumnName("DisciplineID");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");

            entity.HasOne(d => d.Discipline).WithMany(p => p.DogDisciplines)
                .HasForeignKey(d => d.DisciplineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DogDiscip__Disci__7C4F7684");

            entity.HasOne(d => d.Record).WithMany(p => p.DogDisciplines)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DogDiscip__Recor__7B5B524B");
        });

        modelBuilder.Entity<DogList>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__DogList__FBDF78C92E05AB2D");

            entity.ToTable("DogList");

            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.DogId).HasColumnName("DogID");
            entity.Property(e => e.DogName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Owner)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ParticipantNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrialPassed).HasColumnName("TrialPassed");
            entity.Property(e => e.TrialFailed)
                .HasColumnName("TrialFailed")
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Dog).WithMany(p => p.DogLists)
                .HasForeignKey(d => d.DogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DogList__DogID__6FE99F9F");

            entity.HasOne(d => d.Event).WithMany(p => p.DogLists)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DogList__EventID__6EF57B66");

            entity.HasMany(d => d.DogCriteriaResultsDogLists)
                .WithOne(p => p.Record)
                .HasForeignKey(d => d.RecordId)
                .HasConstraintName("FK__DogList__DogCritResDogList");
        });

        modelBuilder.Entity<DogOwner>(entity =>
        {
            entity.HasKey(e => e.DogOwnerId).HasName("PK__DogOwner__EA0FED7342E7CAF5");

            entity.Property(e => e.DogOwnerId).HasColumnName("DogOwnerID");
            entity.Property(e => e.OwnerId).HasColumnName("OwnerID");

            entity.HasOne(d => d.ChipNumberNavigation).WithMany(p => p.DogOwners)
                .HasForeignKey(d => d.ChipNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DogOwners__ChipN__5629CD9C");

            entity.HasOne(d => d.Owner).WithMany(p => p.DogOwners)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DogOwners__Owner__5535A963");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C8708520FBCD");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.CommitteeChairman)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CompositionId).HasColumnName("CompositionID");
            entity.Property(e => e.EventVenue)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Judge1)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Judge2)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Organization)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StatusId).HasDefaultValue(1);
            entity.Property(e => e.TestOrganizer)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Host) 
            .HasMaxLength(100)
            .IsUnicode(false);

            entity.HasOne(d => d.Composition).WithMany(p => p.Events)
                .HasForeignKey(d => d.CompositionId)
                .HasConstraintName("FK__Events__Composit__6E01572D");

            entity.HasOne(d => d.Status).WithMany(p => p.Events)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Events__StatusId__6D0D32F4");
        });

        modelBuilder.Entity<EventComposition>(entity =>
        {
            entity.HasKey(e => e.CompositionId).HasName("PK__EventCom__B8E2333F11031CD1");

            entity.ToTable("EventComposition");

            entity.Property(e => e.CompositionId).HasColumnName("CompositionID");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EventStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__EventSta__C8EE2063DB185660");

            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Kennel>(entity =>
        {
            entity.HasKey(e => e.KennelId).HasName("PK__Kennels__921B7EC337B69530");

            entity.Property(e => e.KennelId).HasColumnName("KennelID");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Kazakhstan");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.KennelName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Options__92C7A1DF0928F6DE");

            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.OptionType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.OptionValue)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TextInfo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CompositionID).HasColumnName("CompositionID");
            entity.Property(e => e.CriterionID).HasColumnName("CriterionID");
            entity.HasOne(d => d.Criterion)
           .WithMany(p => p.Options)
           .HasForeignKey(d => d.CriterionID)
           .HasConstraintName("FK_Options_Criteria");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PK__Owners__8193859888119E94");

            entity.Property(e => e.OwnerId).HasColumnName("OwnerID");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}