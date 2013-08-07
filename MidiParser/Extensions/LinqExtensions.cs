using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiParser.Extensions
{
    public static class LinqExtensions
    {
        public static string Concatenate<T>(this IEnumerable<T> list)
        {
            return list.Concatenate(Environment.NewLine);
        }

        public static string Concatenate<T>(this IEnumerable<T> list, string delimiter)
        {
            var sb = new StringBuilder();
            var first = true;

            foreach (var entry in list)
            {
                if (first)
                    first = false;
                else
                    sb.Append(delimiter);

                sb.Append(entry);
            }

            return sb.ToString();
        }

        public static IEnumerable<T> AsEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        public static List<T> AsList<T>(this T obj)
        {
            return new List<T> { obj };
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> list, int batchSize)
        {
            if (batchSize == 0)
            {
                yield return list;
                yield break;
            }

            var batch = new List<T>(batchSize);
            foreach (var item in list)
            {
                batch.Add(item);

                // batch full?
                if (batch.Count % batchSize == 0)
                {
                    yield return batch;
                    batch = new List<T>();
                }
            }

            // yield remaining items in last batch
            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        public static IEnumerable<string> Parenthesize<T>(this IEnumerable<T> list)
        {
            return list.Parenthesize("(", ")");
        }

        public static IEnumerable<string> Parenthesize<T>(this IEnumerable<T> list, string parenthesis)
        {
            return list.Parenthesize(parenthesis, parenthesis);
        }

        public static IEnumerable<string> Parenthesize<T>(
            this IEnumerable<T> list,
            string parenthesisLeft,
            string parenthesisRight)
        {
            return list.Select(entry => string.Concat(parenthesisLeft, entry.ToString(), parenthesisRight));
        }
    }
}
