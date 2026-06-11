using Microsoft.EntityFrameworkCore;
using SNUS.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Sensor> Sensors
        {
            get { return Set<Sensor>(); }
        }

        public DbSet<SensorReading> SensorReadings
        {
            get { return Set<SensorReading>(); }
        }

        public DbSet<Alarm> Alarms
        {
            get { return Set<Alarm>(); }
        }

        public DbSet<ConsensusValue> ConsensusValues
        {
            get { return Set<ConsensusValue>(); }
        }

        public DbSet<ProcessedMessage> ProcessedMessages
        {
            get { return Set<ProcessedMessage>(); }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExternalId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => x.ExternalId)
                    .IsUnique();

                entity.Property(x => x.DataQuality)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<SensorReading>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Sensor)
                    .WithMany(x => x.Readings)
                    .HasForeignKey(x => x.SensorId);

                entity.Property(x => x.DataQuality)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(x => x.AlarmPriority)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasIndex(x => new { x.SensorId, x.MessageId })
                    .IsUnique();
            });

            modelBuilder.Entity<Alarm>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Sensor)
                    .WithMany(x => x.Alarms)
                    .HasForeignKey(x => x.SensorId);

                entity.HasOne(x => x.SensorReading)
                    .WithMany()
                    .HasForeignKey(x => x.SensorReadingId);

                entity.Property(x => x.Priority)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<ConsensusValue>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Algorithm)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ProcessedMessage>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Sensor)
                    .WithMany()
                    .HasForeignKey(x => x.SensorId);

                entity.HasIndex(x => new { x.SensorId, x.MessageId })
                    .IsUnique();
            });
        }
    }
}
