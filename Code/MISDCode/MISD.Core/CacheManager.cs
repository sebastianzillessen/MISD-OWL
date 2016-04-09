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
* MISD-OWL is distributed without any warranty, without even the
* implied warranty of merchantability or fitness for a particular purpose.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace MISD.Core
{
    /// <summary>
    /// Provides some basic caching functionality for key value pairs.
    /// </summary>
    public sealed class CacheManager<T, V>
        where V : class
    {
        #region Fields

        private int size = 5000;
        private Dictionary<T, V> cache;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the cache hashtable.
        /// </summary>
        private Dictionary<T, V> Cache
        {
            get
            {
                if (this.cache == null) this.cache = new Dictionary<T, V>();
                return this.cache;
            }
        }

        /// <summary>
        /// Gets or sets the caching size of this cache manager.
        /// </summary>
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                if (this.size != value && this.size >= 1)
                {
                    this.size = value;
                    while (this.Cache.Count > value)
                    {
                        this.Cache.Remove(this.Cache.First().Key);
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the given key and value to the internal cache.
        /// </summary>
        public void Add(T key, V value)
        {
            while (this.Cache.Count >= this.Size)
            {
                this.Cache.Remove(this.Cache.First().Key);
            }

            if (!this.Cache.ContainsKey(key))
            {
                this.Cache.Add(key, value);
            }
            else
            {
                this.Cache.Remove(key);
                this.Cache.Add(key, value);
            }
        }

        /// <summary>
        /// Returns the corresponding value for the given key.
        /// </summary>
        /// <returns>The value for the given key, or null if key is not existent.</returns>
        public V Get(T key)
        {
            if (this.Cache.ContainsKey(key))
            {
                return this.Cache[key];
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}