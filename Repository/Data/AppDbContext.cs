using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<AnswerSelection> AnswerSelections { get; set; }

    public virtual DbSet<Choice> Choices { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Response> Responses { get; set; }

    public virtual DbSet<Survey> Surveys { get; set; }
    public virtual DbSet<User> User { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasIndex(e => e.ResponseId, "IX_Answers_ResponseId");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Answers_Questions");

            entity.HasOne(d => d.Response).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ResponseId)
                .HasConstraintName("FK_Answers_Responses");
        });

        modelBuilder.Entity<AnswerSelection>(entity =>
        {
            entity.HasOne(d => d.Answer).WithMany(p => p.AnswerSelections)
                .HasForeignKey(d => d.AnswerId)
                .HasConstraintName("FK_AnswerSelections_Answers");

            entity.HasOne(d => d.Choice).WithMany(p => p.AnswerSelections)
                .HasForeignKey(d => d.ChoiceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AnswerSelections_Choices");
        });

        modelBuilder.Entity<Choice>(entity =>
        {
            entity.Property(e => e.ChoiceText).HasMaxLength(200);

            entity.HasOne(d => d.Question).WithMany(p => p.Choices)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Choices_Questions");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasIndex(e => e.SurveyId, "IX_Questions_SurveyId");

            entity.Property(e => e.IsRequired).HasDefaultValue(true);
            entity.Property(e => e.QuestionText).HasMaxLength(500);
            entity.Ignore(q => q.QuestionTypeEnum); // Ignore the QuestionTypeEnum property in EF Core mapping

            entity.HasOne(d => d.Survey).WithMany(p => p.Questions)
                .HasForeignKey(d => d.SurveyId)
                .HasConstraintName("FK_Questions_Surveys");
        });

        modelBuilder.Entity<Response>(entity =>
        {
            entity.HasIndex(e => e.SurveyId, "IX_Responses_SurveyId");
            entity.HasIndex(e => e.UserId, "IX_Responses_UserID");

            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Survey).WithMany(p => p.Responses)
                .HasForeignKey(d => d.SurveyId)
                .HasConstraintName("FK_Responses_Surveys");

            entity.HasOne(d => d.User).WithMany(p => p.SubmittedResponses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Responses_User");
        });

        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasIndex(e => e.Status, "IX_Surveys_Status");

            entity.HasIndex(e => e.UserId, "IX_Surveys_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.User).WithMany(p => p.CreatedSurveys)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Surveys_User");
        });
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email);

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");



            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
