using System;

namespace BankApp.Core.Models
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Alert,
        Transaction,
        Account,
        Security,
        Promotion
    }

    public enum NotificationStatus
    {
        Unread,
        Read,
        Archived
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string ActionUrl { get; set; }
        public string ActionText { get; set; }
        public bool IsImportant { get; set; }
        
        public int? UserId { get; set; }
        public User User { get; set; }
        
        public int? AccountId { get; set; }
        public Account Account { get; set; }

        public string Icon
        {
            get
            {
                return Type switch
                {
                    NotificationType.Success => "✅",
                    NotificationType.Warning => "⚠️",
                    NotificationType.Alert => "🚨",
                    NotificationType.Transaction => "💸",
                    NotificationType.Account => "💳",
                    NotificationType.Security => "🔒",
                    NotificationType.Promotion => "🎁",
                    _ => "ℹ️"
                };
            }
        }

        public string BackgroundColor
        {
            get
            {
                return Type switch
                {
                    NotificationType.Success => "#d1fae5",
                    NotificationType.Warning => "#fef3c7",
                    NotificationType.Alert => "#fee2e2",
                    NotificationType.Transaction => "#dbeafe",
                    NotificationType.Account => "#e0e7ff",
                    NotificationType.Security => "#fce7f3",
                    NotificationType.Promotion => "#f3e8ff",
                    _ => "#f1f5f9"
                };
            }
        }

        public string TextColor
        {
            get
            {
                return Type switch
                {
                    NotificationType.Success => "#065f46",
                    NotificationType.Warning => "#92400e",
                    NotificationType.Alert => "#991b1b",
                    NotificationType.Transaction => "#1e40af",
                    NotificationType.Account => "#3730a3",
                    NotificationType.Security => "#9d174d",
                    NotificationType.Promotion => "#6b21a8",
                    _ => "#475569"
                };
            }
        }
    }
}
