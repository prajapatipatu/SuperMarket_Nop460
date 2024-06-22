using Nop.Data.Mapping;
using SS.Plugin.Widgets.ProductReview.Domains;
using System;
using System.Collections.Generic;

namespace SS.Plugin.Widgets.ProductReview.Mapping
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>()
        {
            { typeof(ProductReviewImages), "SS_ProductReviewImages"  }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}