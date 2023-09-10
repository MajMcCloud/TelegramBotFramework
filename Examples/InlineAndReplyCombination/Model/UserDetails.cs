using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InlineAndReplyCombination.Model
{
    [DebuggerDisplay("{AgeRange}, {FavouriteColor},  {FavouriteCity}")]
    public class UserDetails
    {
        public String AgeRange { get; set; }

        public String FavouriteColor { get; set; }

        public String FavouriteCity { get; set; }

    }
}
