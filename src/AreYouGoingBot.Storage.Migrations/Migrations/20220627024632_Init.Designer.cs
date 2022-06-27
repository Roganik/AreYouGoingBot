﻿// <auto-generated />
using AreYouGoingBot.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AreYouGoingBot.Storage.Migrations.Migrations
{
    [DbContext(typeof(AttendersDb))]
    [Migration("20220627024632_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.6");

            modelBuilder.Entity("AreYouGoingBot.Storage.Models.ChatEvent", b =>
                {
                    b.Property<int>("ChatEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("TelegramChatId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChatEventId");

                    b.ToTable("ChatEvents");
                });

            modelBuilder.Entity("AreYouGoingBot.Storage.Models.ChatEventParticipant", b =>
                {
                    b.Property<int>("ChatEventParticipantId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChatEventId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("TelegramUserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChatEventParticipantId");

                    b.HasIndex("ChatEventId");

                    b.ToTable("ChatEventParticipant");
                });

            modelBuilder.Entity("AreYouGoingBot.Storage.Models.ChatEventParticipant", b =>
                {
                    b.HasOne("AreYouGoingBot.Storage.Models.ChatEvent", "ChatEvent")
                        .WithMany("Participants")
                        .HasForeignKey("ChatEventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ChatEvent");
                });

            modelBuilder.Entity("AreYouGoingBot.Storage.Models.ChatEvent", b =>
                {
                    b.Navigation("Participants");
                });
#pragma warning restore 612, 618
        }
    }
}