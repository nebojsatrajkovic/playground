using System.Collections.Immutable;

namespace Core.Shared.Utils.Threads.Collections
{
    /// <summary>
    /// A threadsafe dictionary having as key a Guid and a Value of Type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Core_TSC_GuidDictionary<T> : IDisposable
    {
        private bool isDisposing = false;
        private object isDisposing_Object = new();

        private Dictionary<Guid, int> idxPositionNumber;
        private List<T> Content;

        /// <summary>
        /// The default constructor
        /// </summary>
        public Core_TSC_GuidDictionary()
        {
            idxPositionNumber = new Dictionary<Guid, int>();
            Content = new List<T>();
        }

        private object AddItemLock = new();

        /// <summary>
        /// The object through which each access of the values-collection should be locked
        /// (unexpected results might occur if you forget to do this)
        /// </summary>
        public object ContentLockingObject { get => AddItemLock; }

        /// <summary>
        /// Adds an item to the dictionary. The only way to fail this is when
        /// the item already exists
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pItemToAdd"></param>
        /// <returns>false: Item has not been added, true: Item has been added</returns>
        public bool TryAdd(Guid pKey, T pItemToAdd)
        {
            lock (isDisposing_Object)
            {
                // You cannot perform any action during disposal
                if (isDisposing) return false;
            }

            lock (AddItemLock)
            {
                // check if the item already exists
                if (idxPositionNumber.ContainsKey(pKey) == true) return false;

                Content.Add(pItemToAdd);
                idxPositionNumber.Add(pKey, Content.Count - 1);
            }

            return true;
        }

        /// <summary>
        /// Delivers the number of items of this dictionary
        /// </summary>
        public int Count
        {
            get
            {
                lock (isDisposing_Object)
                {
                    // You cannot perform any action during disposal
                    if (isDisposing) return 0;
                }

                lock (AddItemLock)
                {
                    return Content.Count;
                }
            }
        }

        /// <summary>
        /// Returns an immutable array of the values of the dictionary
        /// </summary>
        public ImmutableArray<T> Values
        {
            get
            {
                lock (isDisposing_Object)
                {
                    // You cannot perform any action during disposal
                    if (isDisposing)
                    {
                        ImmutableArray<T>.Builder builder2 = ImmutableArray.CreateBuilder<T>();
                        return builder2.ToImmutable();
                    }
                }

                lock (AddItemLock)
                {
                    return Content.ToImmutableArray();
                }
            }
        }

        /// <summary>
        /// Returns an immutable array of the values of the dictionary
        /// </summary>
        public ImmutableDictionary<Guid, T> KeyValuePairs
        {
            get
            {
                lock (isDisposing_Object)
                {
                    // You cannot perform any action during disposal
                    if (isDisposing)
                    {
                        ImmutableDictionary<Guid, T>.Builder builder2 = ImmutableDictionary.CreateBuilder<Guid, T>();
                        return builder2.ToImmutable();
                    }
                }

                ImmutableDictionary<Guid, T>.Builder builder = ImmutableDictionary.CreateBuilder<Guid, T>();
                lock (AddItemLock)
                {
                    foreach (KeyValuePair<Guid, int> i in idxPositionNumber)
                    {
                        builder.Add(i.Key, Content[i.Value]);
                    }
                }

                return builder.ToImmutable();
            }
        }


        /// <summary>
        /// This tries to get an item and adds the specified item, if no item exists with the provided key yet
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pItemToAddIfNotFound">The item to add, if not found</param>
        /// <returns></returns>
        public T? Get_Or_Add(Guid pKey, T pItemToAddIfNotFound)
        {
            if (pKey == Guid.Empty)
            {
                throw new Exception("Cannot add an item for an empty key!");
            }

            lock (isDisposing_Object)
            {
                // You cannot perform any action during disposal
                if (isDisposing) return default;
            }

            lock (AddItemLock)
            {
                bool ItemAlreadyExists = idxPositionNumber.TryGetValue(pKey, out int Position);
                if (ItemAlreadyExists) return Content[Position];

                // Item did not yet exist
                Content.Add(pItemToAddIfNotFound);
                idxPositionNumber.Add(pKey, Content.Count - 1);
                return pItemToAddIfNotFound;
            }
        }

        /// <summary>
        /// Retrieves the value of type T for the provided key.
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pResult"></param>
        /// <returns>true if an item existed with the key, false if it did not exist</returns>
        public bool TryGetValue(Guid pKey, out T? pResult)
        {
            lock (isDisposing_Object)
            {
                // You cannot perform any action during disposal
                if (isDisposing)
                {
                    pResult = default;
                    return false;
                }
            }

            lock (AddItemLock)
            {
                bool ItemAlreadyExists = idxPositionNumber.TryGetValue(pKey, out int Position);
                if (!ItemAlreadyExists)
                {
                    pResult = default;
                    return false;
                }

                pResult = Content[Position];
            }

            return true;
        }

        /// <summary>
        /// Retrieves the index of the item in the values collection
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns>-1 if no item with the key existed, otherwise the index</returns>
        public int TryGetIndexOfItem(Guid pKey)
        {
            lock (isDisposing_Object)
            {
                // You cannot perform any action during disposal
                if (isDisposing) return -1;
            }

            lock (AddItemLock)
            {
                bool ItemAlreadyExists = idxPositionNumber.TryGetValue(pKey, out int Position);
                return ItemAlreadyExists ? Position : -1;
            }
        }

        /// <summary>
        /// Retrieves the item at a certain position in the dictionary
        /// </summary>
        /// <param name="pPosition"></param>
        /// <returns></returns>
        public T? GetItemAt(int pPosition)
        {
            lock (isDisposing_Object)
            {
                // You cannot perform any action during disposal
                if (isDisposing) return default;
            }

            return Content[pPosition];
        }

        /// <summary>
        /// Tries to update the value. If the value does not exist yet
        /// the key value pair is just added with the value to update
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="preCalulatedResultSet"></param>
        /// <returns></returns>
        public bool TryUpdateValue(Guid pKey, T preCalulatedResultSet)
        {
            lock (isDisposing_Object)
            {
                // You cannot perform any action during disposal
                if (isDisposing) return false;
            }

            lock (AddItemLock)
            {
                bool ItemAlreadyExists = idxPositionNumber.TryGetValue(pKey, out int Position);
                if (!ItemAlreadyExists) return false;

                Content[Position] = preCalulatedResultSet;
                return true;
            }
        }

        /// <summary>
        /// Tries to delete the item with the specified UID. 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pSetItemToDefaultWhenFound">Sets the value to the default (which is in all reference types null)</param>
        /// <returns>true if the item was removed successfully</returns>
        public bool TryDeleteValue(Guid pKey, bool pSetItemToDefaultWhenFound = false)
        {

            lock (isDisposing_Object)
            {
                // You cannot perform any action during disposal
                if (isDisposing) return false;
            }

            lock (AddItemLock)
            {
                bool ItemDoesNotExist = idxPositionNumber.TryGetValue(pKey, out int Position);
                if (!ItemDoesNotExist) return false;

                idxPositionNumber.Remove(pKey);

                if (pSetItemToDefaultWhenFound) Content[Position] = default;

                Content.RemoveAt(Position);

                return true;
            }
        }



        /// <summary>
        /// Clears the dictionary
        /// </summary>
        public void Clear()
        {
            lock (isDisposing_Object)
            {
                if (isDisposing) return;
            }

            lock (AddItemLock)
            {
                Content.Clear();
                idxPositionNumber.Clear();
            }
        }

        public void Dispose()
        {
            Clear();

            lock (isDisposing_Object)
            {
                isDisposing = true;
            }

            idxPositionNumber.Clear();
            Content.Clear();
        }
    }
}
