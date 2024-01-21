namespace Core.Shared.Utils.Extensions
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        public static bool HasValue<T>(this IEnumerable<T> collection)
        {
            return !IsEmpty(collection);
        }

        /// <summary>
        /// Split enumerable in chunks.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Chunks<T>(this IEnumerable<T> enumerable, int chunkSize)
        {
            if (chunkSize < 1) throw new ArgumentException("chunkSize must be positive");

            using var e = enumerable.GetEnumerator();

            while (e.MoveNext())
            {
                var remaining = chunkSize;    // elements remaining in the current chunk
                var innerMoveNext = new Func<bool>(() => --remaining > 0 && e.MoveNext());

                yield return e.GetChunk(innerMoveNext);
                while (innerMoveNext()) {/* discard elements skipped by inner iterator */}
            }
        }

        private static IEnumerable<T> GetChunk<T>(this IEnumerator<T> e, Func<bool> innerMoveNext)
        {
            do yield return e.Current;
            while (innerMoveNext());
        }
    }
}