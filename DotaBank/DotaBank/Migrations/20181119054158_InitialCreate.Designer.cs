﻿// <auto-generated />
using DotaBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotaBank.Migrations
{
    [DbContext(typeof(DotaBankContext))]
    [Migration("20181119054158_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("DotaBank.Models.DotaItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Height");

                    b.Property<string>("Legs");

                    b.Property<string>("Tags");

                    b.Property<string>("Title");

                    b.Property<string>("Uploaded");

                    b.Property<string>("Url");

                    b.Property<string>("Width");

                    b.HasKey("Id");

                    b.ToTable("DotaItem");
                });
#pragma warning restore 612, 618
        }
    }
}
