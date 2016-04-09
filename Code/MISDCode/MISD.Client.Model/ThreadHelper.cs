/*
 * Copyright 2012 Paul Brombosch, Ehssan Doust, David Krauss,
 * Fabian Müller, Yannic Noller, Hanna Schäfer, Jonas Scheurich,
 * Arno Schneider, Sebastian Zillessen
 *
 * This file is part of MISD-OWL, a project of the
 * University of Stuttgart (Institution VISUS, Studienprojekt Spring 2012).
 *
 * MISD-OWL is published under GNU Lesser General Public License Version 3.
 * MISD-OWL is free software, you are allowed to redistribute and/or
 * modify it under the terms of the GNU Lesser General Public License
 * Version 3 or any later version. For details see here:
 * http://www.gnu.org/licenses/lgpl.html
 *
 * MISD-OWL is distributed without any warranty, witmlhout even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MISD.Client.Model
{
    /// <summary>
    /// Provides extension methods to switch from a workter thread to the dispatcher thread.
    /// </summary>
    public static class ThreadHelper
    {
        public static ExtendedObservableCollection<T> AddRange<T>(this ExtendedObservableCollection<T> collection, IEnumerable<T> range)
        {
            foreach (var x in range)
            {
                collection.AddOnUI(x);
            }
            return collection;
        }

        public static ExtendedObservableCollection<T> BeginAddRange<T>(this ExtendedObservableCollection<T> collection, IEnumerable<T> range)
        {
            foreach (var x in range)
            {
                collection.BeginAddOnUI(x);
            }
            return collection;
        }


        /// <summary>
        /// Adds an item to a Collection of data in the Dispatcher-Thread. This is necessary because you can't change an ExtendedObservableCollection in a WorkerThread.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        public static void AddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.Invoke(addMethod, item);
        }


        /// <summary>
        /// Adds an item to a Collection of data in the Dispatcher-Thread. This is necessary because you can't change an ExtendedObservableCollection in a WorkerThread.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        public static void BeginAddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        /// <summary>
        /// Removes an item from a Collection of data in the Dispatcher-Thread. This is necessary because you can't change an ExtendedObservableCollection in a WorkerThread.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        public static void RemoveOnUI<T>(this ICollection<T> collection, T item)
        {
            Application.Current.Dispatcher.Invoke(() => { collection.Remove(item); });
        }

        public static void ClearOnUI<T>(this ICollection<T> collection)
        {
            Application.Current.Dispatcher.Invoke(() => { if (collection != null) collection.Clear(); });
        }

        public static void Sort<T>(this ExtendedObservableCollection<T> collection) where T : IComparable<T>
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            for (var startIndex = 0; startIndex < collection.Count - 1; startIndex += 1)
            {
                var indexOfSmallestItem = startIndex;
                for (var i = startIndex + 1; i < collection.Count; i += 1)
                    if (collection[i].CompareTo(collection[indexOfSmallestItem]) < 0)
                        indexOfSmallestItem = i;
                if (indexOfSmallestItem != startIndex)
                    collection.Move(indexOfSmallestItem, startIndex);
            }
        }
    }
}
