﻿namespace MassTransit.EntityFrameworkCoreIntegration.Audit
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;


    public class AuditMapping :
        IEntityTypeConfiguration<AuditRecord>
    {
        readonly string _tableName;

        public AuditMapping(string tableName)
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<AuditRecord> builder)
        {
            builder.ToTable(_tableName);

            builder.HasKey(x => x.AuditRecordId);
            builder.Property(x => x.AuditRecordId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ContextType);
            builder.Property(x => x.MessageId);
            builder.Property(x => x.InitiatorId);
            builder.Property(x => x.ConversationId);
            builder.Property(x => x.CorrelationId);
            builder.Property(x => x.RequestId);
            builder.Property(x => x.SentTime);
            builder.Property(x => x.SourceAddress);
            builder.Property(x => x.DestinationAddress);
            builder.Property(x => x.ResponseAddress);
            builder.Property(x => x.FaultAddress);
            builder.Property(x => x.InputAddress);
            builder.Property(x => x.MessageType);
            builder.Property(x => x._headers).HasColumnName("Headers");
            builder.Property(x => x._custom).HasColumnName("Custom");
            builder.Property(x => x._message).HasColumnName("Message");
        }
    }
}
