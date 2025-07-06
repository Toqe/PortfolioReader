using System.Collections.Generic;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business.Models
{
    public class ClassificationRebalancingModel
    {
        public PTaxonomy.Classification Classification { get; set; }

        public string Name => this.Classification?.Name;

        public double MarketValue { get; set; }

        public double SupposedValue { get; set; }

        public double CurrentWeightPercentage { get; set; }

        public double SupposedWeightPercentage { get; set; }

        public double CurrentTotalWeightPercentage { get; set; }

        public double SupposedTotalWeightPercentage { get; set; }

        public ClassificationRebalancingModel Parent { get; set; }

        public List<ClassificationRebalancingModel> Children { get; set; } = new List<ClassificationRebalancingModel>();

        public List<SecurityValueModel> SecurityValues { get; set; } = new List<SecurityValueModel>();
    }
}
