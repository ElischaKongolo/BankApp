using System;

namespace BankApp.Core.Models
{
    public class InvestmentNews
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public string Source { get; set; }
        public DateTime PublishedAt { get; set; }
        public string ImageUrl { get; set; }
        public string Summary { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public string Tags { get; set; }
    }

    public enum NewsCategory
    {
        MarketNews = 0,
        StockUpdates = 1,
        EconomicIndicators = 2,
        CompanyNews = 3,
        InvestmentTips = 4,
        RegulatoryNews = 5
    }
}
