using Microsoft.EntityFrameworkCore;
using BankApp.Core.Models;

namespace BankApp.Infrastructure.Data
{
    public class BankDbContext : DbContext
    {
        public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Payee> Payees { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanRepayment> LoanRepayments { get; set; }
        public DbSet<Investment> Investments { get; set; }
        public DbSet<InvestmentNews> InvestmentNews { get; set; }
        public DbSet<UserTheme> UserThemes { get; set; }
        public DbSet<EWallet> EWallets { get; set; }
        public DbSet<EWalletTransaction> EWalletTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Role).HasMaxLength(20);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AccountNumber).IsUnique();
                entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.Property(e => e.AvailableBalance).HasPrecision(18, 2);
                entity.Property(e => e.DailyTransferLimit).HasPrecision(18, 2);
                entity.Property(e => e.MonthlyTransferLimit).HasPrecision(18, 2);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Accounts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransactionReference).IsUnique();
                entity.Property(e => e.TransactionReference).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.CounterpartyName).HasMaxLength(100);
                entity.Property(e => e.CounterpartyAccount).HasMaxLength(20);
                entity.HasOne(e => e.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(e => e.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.CardNumber).HasMaxLength(20);
                entity.Property(e => e.AccountNumber).HasMaxLength(20);
                entity.Property(e => e.SortCode).HasMaxLength(10);
                entity.Property(e => e.CVV).HasMaxLength(4);
                entity.Property(e => e.DailyLimit).HasPrecision(18, 2);
                entity.Property(e => e.TransactionLimit).HasPrecision(18, 2);
                entity.HasOne(e => e.Account)
                      .WithMany(a => a.PaymentMethods)
                      .HasForeignKey(e => e.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ActionUrl).HasMaxLength(255);
                entity.Property(e => e.ActionText).HasMaxLength(50);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Account)
                      .WithMany()
                      .HasForeignKey(e => e.AccountId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CardNumber).IsUnique();
                entity.Property(e => e.CardNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DailySpendingLimit).HasPrecision(18, 2);
                entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Bill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.InterestRate).HasPrecision(18, 2);
                entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);
                entity.Property(e => e.RemainingBalance).HasPrecision(18, 2);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LoanRepayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.PrincipalAmount).HasPrecision(18, 2);
                entity.Property(e => e.InterestAmount).HasPrecision(18, 2);
                entity.HasOne(e => e.Loan).WithMany(l => l.Repayments).HasForeignKey(e => e.LoanId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Investment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InitialAmount).HasPrecision(18, 2);
                entity.Property(e => e.CurrentValue).HasPrecision(18, 2);
                entity.Property(e => e.ReturnPercentage).HasPrecision(10, 4);
                entity.Property(e => e.RiskLevel).HasPrecision(3, 2);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InvestmentNews>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Summary).HasMaxLength(300);
                entity.Property(e => e.Tags).HasMaxLength(200);
            });

            modelBuilder.Entity<UserTheme>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ThemeName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PrimaryColor).IsRequired().HasMaxLength(7);
                entity.Property(e => e.SecondaryColor).IsRequired().HasMaxLength(7);
                entity.Property(e => e.BackgroundColor).IsRequired().HasMaxLength(7);
                entity.Property(e => e.SurfaceColor).IsRequired().HasMaxLength(7);
                entity.Property(e => e.TextColor).IsRequired().HasMaxLength(7);
                entity.Property(e => e.TextMutedColor).IsRequired().HasMaxLength(7);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EWallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.WalletNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.Property(e => e.AvailableBalance).HasPrecision(18, 2);
                entity.HasIndex(e => e.WalletNumber).IsUnique();
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.LinkedAccount).WithMany().HasForeignKey(e => e.LinkedAccountId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EWalletTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.BalanceBefore).HasPrecision(18, 2);
                entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
                entity.HasOne(e => e.EWallet).WithMany(t => t.Transactions).HasForeignKey(e => e.EWalletId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ToWallet).WithMany().HasForeignKey(e => e.ToWalletId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.FromWallet).WithMany().HasForeignKey(e => e.FromWalletId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.BankTransaction).WithMany().HasForeignKey(e => e.BankTransactionId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
