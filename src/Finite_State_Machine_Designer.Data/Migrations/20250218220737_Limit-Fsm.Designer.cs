﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Finite_State_Machine_Designer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Finite_State_Machine_Designer.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250218220737_Limit-Fsm")]
    partial class LimitFsm
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppRoleClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppUserClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppUserLogin", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppUserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppUserToken", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(221)
                        .HasColumnType("nvarchar(221)");

                    b.Property<string>("Name")
                        .HasMaxLength(221)
                        .HasColumnType("nvarchar(221)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .IsUnique()
                        .HasDatabaseName("EmailIndex")
                        .HasFilter("[NormalizedEmail] IS NOT NULL");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Models.FSM.FiniteState", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("FiniteStateMachineId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDrawable")
                        .HasColumnType("bit");

                    b.Property<bool>("IsFinalState")
                        .HasColumnType("bit");

                    b.Property<float>("Radius")
                        .HasColumnType("real");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.ComplexProperty<Dictionary<string, object>>("Coordinate", "Finite_State_Machine_Designer.Models.FSM.FiniteState.Coordinate#CanvasCoordinate", b1 =>
                        {
                            b1.Property<double>("X")
                                .HasColumnType("float");

                            b1.Property<double>("Y")
                                .HasColumnType("float");
                        });

                    b.HasKey("Id");

                    b.HasIndex("FiniteStateMachineId");

                    b.ToTable("States");
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Models.FSM.FiniteStateMachine", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ApplicationUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2056)
                        .HasColumnType("nvarchar(2056)");

                    b.Property<int>("Height")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTime>("TimeCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TimeUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int>("TransitionSearchRadius")
                        .HasColumnType("int");

                    b.Property<int>("Width")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("StateMachines");
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Models.FSM.Transition", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("FiniteStateMachineId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("FromStateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsReversed")
                        .HasColumnType("bit");

                    b.Property<double>("MinPerpendicularDistance")
                        .HasColumnType("float");

                    b.Property<double>("ParallelAxis")
                        .HasColumnType("float");

                    b.Property<double>("PerpendicularAxis")
                        .HasColumnType("float");

                    b.Property<double>("Radius")
                        .HasColumnType("float");

                    b.Property<double>("SelfAngle")
                        .HasColumnType("float");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<Guid>("ToStateId")
                        .HasColumnType("uniqueidentifier");

                    b.ComplexProperty<Dictionary<string, object>>("CenterArc", "Finite_State_Machine_Designer.Models.FSM.Transition.CenterArc#CanvasCoordinate", b1 =>
                        {
                            b1.Property<double>("X")
                                .HasColumnType("float");

                            b1.Property<double>("Y")
                                .HasColumnType("float");
                        });

                    b.HasKey("Id");

                    b.HasIndex("FiniteStateMachineId");

                    b.HasIndex("FromStateId");

                    b.HasIndex("ToStateId");

                    b.ToTable("Transitions");
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppRoleClaim", b =>
                {
                    b.HasOne("Finite_State_Machine_Designer.Data.Identity.AppRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppUserClaim", b =>
                {
                    b.HasOne("Finite_State_Machine_Designer.Data.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppUserLogin", b =>
                {
                    b.HasOne("Finite_State_Machine_Designer.Data.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppUserRole", b =>
                {
                    b.HasOne("Finite_State_Machine_Designer.Data.Identity.AppRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Finite_State_Machine_Designer.Data.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.AppUserToken", b =>
                {
                    b.HasOne("Finite_State_Machine_Designer.Data.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Models.FSM.FiniteState", b =>
                {
                    b.HasOne("Finite_State_Machine_Designer.Models.FSM.FiniteStateMachine", null)
                        .WithMany("States")
                        .HasForeignKey("FiniteStateMachineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Models.FSM.FiniteStateMachine", b =>
                {
                    b.HasOne("Finite_State_Machine_Designer.Data.Identity.ApplicationUser", null)
                        .WithMany("StateMachines")
                        .HasForeignKey("ApplicationUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Models.FSM.Transition", b =>
                {
                    b.HasOne("Finite_State_Machine_Designer.Models.FSM.FiniteStateMachine", null)
                        .WithMany("Transitions")
                        .HasForeignKey("FiniteStateMachineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Finite_State_Machine_Designer.Models.FSM.FiniteState", "FromState")
                        .WithOne()
                        .HasForeignKey("Finite_State_Machine_Designer.Models.FSM.Transition", "FromStateId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Finite_State_Machine_Designer.Models.FSM.FiniteState", "ToState")
                        .WithOne()
                        .HasForeignKey("Finite_State_Machine_Designer.Models.FSM.Transition", "ToStateId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("FromState");

                    b.Navigation("ToState");
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Data.Identity.ApplicationUser", b =>
                {
                    b.Navigation("StateMachines");
                });

            modelBuilder.Entity("Finite_State_Machine_Designer.Models.FSM.FiniteStateMachine", b =>
                {
                    b.Navigation("States");

                    b.Navigation("Transitions");
                });
#pragma warning restore 612, 618
        }
    }
}
