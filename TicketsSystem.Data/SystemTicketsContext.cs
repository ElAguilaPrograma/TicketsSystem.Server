using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Data;

public partial class SystemTicketsContext : DbContext
{
    public SystemTicketsContext()
    {
    }

    public SystemTicketsContext(DbContextOptions<SystemTicketsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Mcprequest> Mcprequests { get; set; }

    public virtual DbSet<Mcpresponse> Mcpresponses { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketComment> TicketComments { get; set; }

    public virtual DbSet<TicketHistory> TicketHistories { get; set; }

    public virtual DbSet<TicketPriority> TicketPriorities { get; set; }

    public virtual DbSet<TicketStatus> TicketStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-0TIEB7HR;Database=systemTickets;Trusted_Connection=True;Integrated Security=True;Trust Server Certificate = True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mcprequest>(entity =>
        {
            entity.HasKey(e => e.McprequestId).HasName("PK__MCPReque__17476F69DB750726");

            entity.ToTable("MCPRequests");

            entity.Property(e => e.McprequestId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("MCPRequestId");
            entity.Property(e => e.PromptVersion).HasMaxLength(20);
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UseCase).HasMaxLength(100);

            entity.HasOne(d => d.Ticket).WithMany(p => p.Mcprequests)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MCPRequest_Ticket");
        });

        modelBuilder.Entity<Mcpresponse>(entity =>
        {
            entity.HasKey(e => e.McpresponseId).HasName("PK__MCPRespo__198540A99C9B3D60");

            entity.ToTable("MCPResponses");

            entity.Property(e => e.McpresponseId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("MCPResponseId");
            entity.Property(e => e.Confidence).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.McprequestId).HasColumnName("MCPRequestId");
            entity.Property(e => e.ResponseType).HasMaxLength(50);

            entity.HasOne(d => d.Mcprequest).WithMany(p => p.Mcpresponses)
                .HasForeignKey(d => d.McprequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MCPResponse_Request");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E128C070050");

            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");

            entity.Property(e => e.NotificationId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_User");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC60788205755");

            entity.HasIndex(e => e.AssignedToUserId, "IX_Tickets_AssignedTo");

            entity.HasIndex(e => e.CreatedAt, "IX_Tickets_CreatedAt");

            entity.HasIndex(e => e.PriorityId, "IX_Tickets_Priority");

            entity.HasIndex(e => e.StatusId, "IX_Tickets_Status");

            entity.Property(e => e.TicketId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.TicketAssignedToUsers)
                .HasForeignKey(d => d.AssignedToUserId)
                .HasConstraintName("FK_Tickets_AssignedTo");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.TicketCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_CreatedBy");

            entity.HasOne(d => d.Priority).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.PriorityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Priority");

            entity.HasOne(d => d.Status).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Status");
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__TicketCo__C3B4DFCA2E78B4B3");

            entity.HasIndex(e => e.TicketId, "IX_Comments_TicketId");

            entity.Property(e => e.CommentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_Ticket");

            entity.HasOne(d => d.User).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_User");
        });

        modelBuilder.Entity<TicketHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__TicketHi__4D7B4ABD8C5F12E3");

            entity.ToTable("TicketHistory");

            entity.HasIndex(e => e.TicketId, "IX_History_TicketId");

            entity.Property(e => e.HistoryId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FieldName).HasMaxLength(100);
            entity.Property(e => e.NewValue).HasMaxLength(255);
            entity.Property(e => e.OldValue).HasMaxLength(255);

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.TicketHistories)
                .HasForeignKey(d => d.ChangedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_History_User");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketHistories)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_History_Ticket");
        });

        modelBuilder.Entity<TicketPriority>(entity =>
        {
            entity.HasKey(e => e.PriorityId).HasName("PK__TicketPr__D0A3D0BE49B3DD32");

            entity.HasIndex(e => e.Name, "UQ__TicketPr__737584F69307F0F8").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TicketStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__TicketSt__C8EE20633E5ECBD6");

            entity.HasIndex(e => e.Name, "UQ__TicketSt__737584F649C40AC9").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C755FA5C7");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341B6078ED").IsUnique();

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
