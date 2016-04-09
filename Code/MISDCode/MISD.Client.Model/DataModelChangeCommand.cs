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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISD.Client.Model.Managers;

namespace MISD.Client.Model
{
    [Serializable]
    public enum DataModelCommand
    {
        UPDATE_ELEMENTS
    }

    [Serializable]
    public class DataModelChangeCommand
    {
        #region Fields

        private DataModelCommand commandType;
        private string data;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the CommandType of this command
        /// </summary>
        public DataModelCommand CommandType
        {
            get
            {
                return this.commandType;
            }

            private set
            {
                this.commandType = value;
            }
        }

        private string Data
        {
            get
            {
                return this.data;
            }

            set
            {
                this.data = value;
            }
        }
        
        #endregion

        #region Constructors

        public DataModelChangeCommand(ExtendedObservableCollection<TileableElement> elements)
        {
            CommandType = DataModelCommand.UPDATE_ELEMENTS;
            this.Data = DataModelManager.Instance.SerializeBase64(elements);
        }

        #endregion

        #region TypeConverter for each type

        public ExtendedObservableCollection<TileableElement> GetElements()
        {
            if (data != null && !data.Equals("") && CommandType == DataModelCommand.UPDATE_ELEMENTS)
            {
                ExtendedObservableCollection<TileableElement> elements = DataModelManager.Instance.DeserializeBase64(this.Data) as ExtendedObservableCollection<TileableElement>;
                return elements;
            }
            else
            {
                throw new ArgumentException("This is no UPDATE_ELEMENTS Command");
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.CommandType + " Data: " + data;
        }

        #endregion

    }
}
