using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OneJaxDashboard.Data;

#nullable disable

namespace StrategicDashboard.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("OneJaxDashboard.Models.Event", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AdminNotes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("AssignmentDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("Attendees")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAssignedByAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OwnerUsername")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PostAssessmentData")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PreAssessmentData")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("SatisfactionScore")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("StrategicGoalId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("StrategicGoalId1")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("StrategyId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("StrategicGoalId");

                    b.HasIndex("StrategicGoalId1");

                    b.HasIndex("StrategyId");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.GoalMetric", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("CurrentValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Q1Value")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Q2Value")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Q3Value")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Q4Value")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("StrategicGoalId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Target")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TargetDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("StrategicGoalId");

                    b.ToTable("GoalMetrics");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.Metric", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Progress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("StrategyId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Target")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TimePeriod")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("StrategyId");

                    b.ToTable("Metric");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.ProfessionalDevelopment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProfessionalDevelopmentYear26")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProfessionalDevelopmentYear27")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ProfessionalDevelopments");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.StaffSurvey_22D", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProfessionalDevelopmentCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SatisfactionRate")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("StaffSurveys_22D");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.Staffauth", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique()
                        .HasFilter("[Username] IS NOT NULL");

                    b.ToTable("Staffauth");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.StrategicGoal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("StrategicGoals");
                });

            modelBuilder.Entity("Strategy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("StrategicGoalId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Time")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("StrategicGoalId");

                    b.ToTable("Strategies");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.Event", b =>
                {
                    b.HasOne("OneJaxDashboard.Models.StrategicGoal", "StrategicGoal")
                        .WithMany()
                        .HasForeignKey("StrategicGoalId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("OneJaxDashboard.Models.StrategicGoal", null)
                        .WithMany("Events")
                        .HasForeignKey("StrategicGoalId1");

                    b.HasOne("Strategy", "Strategy")
                        .WithMany()
                        .HasForeignKey("StrategyId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("StrategicGoal");

                    b.Navigation("Strategy");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.GoalMetric", b =>
                {
                    b.HasOne("OneJaxDashboard.Models.StrategicGoal", null)
                        .WithMany("Metrics")
                        .HasForeignKey("StrategicGoalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OneJaxDashboard.Models.Metric", b =>
                {
                    b.HasOne("Strategy", null)
                        .WithMany("Metrics")
                        .HasForeignKey("StrategyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Strategy", b =>
                {
                    b.HasOne("OneJaxDashboard.Models.StrategicGoal", "StrategicGoal")
                        .WithMany("Strategies")
                        .HasForeignKey("StrategicGoalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StrategicGoal");
                });

            modelBuilder.Entity("OneJaxDashboard.Models.StrategicGoal", b =>
                {
                    b.Navigation("Events");

                    b.Navigation("Metrics");

                    b.Navigation("Strategies");
                });

            modelBuilder.Entity("Strategy", b =>
                {
                    b.Navigation("Metrics");
                });
#pragma warning restore 612, 618
        }
    }
}