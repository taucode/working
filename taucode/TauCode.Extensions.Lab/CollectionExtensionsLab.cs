using System.Collections.Generic;
using System.Linq;

namespace TauCode.Extensions.Lab
{
    public static class CollectionExtensionsLab
    {
        public static bool ListsAreEquivalent<T>(IReadOnlyList<T> list1, IReadOnlyList<T> list2, bool sort = true)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            IList<T> transformedList1 = list1.ToList();
            IList<T> transformedList2 = list2.ToList();

            if (sort)
            {
                transformedList1 = list1.OrderBy(x => x).ToList();
                transformedList2 = list2.OrderBy(x => x).ToList();
            }

            for (var i = 0; i < transformedList1.Count; i++)
            {
                var v1 = transformedList1[i];
                var v2 = transformedList2[i];

                if (v1 == null)
                {
                    if (v2 == null)
                    {
                        // ok
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var eq = v1.Equals(v2);
                    if (!eq)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
