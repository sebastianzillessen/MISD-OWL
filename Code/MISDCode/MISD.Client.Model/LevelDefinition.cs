using System;
using System.Xml.Serialization;
namespace MISD.Client.Model
{
    /// <summary>
    /// This class is used to define a tile level.
    /// </summary>
    public class LevelDefinition : BindableBase
    {
        #region Fields

        private bool useCustomUI;
        private bool hasStatusBar;
        private int rows;

        private Guid guid = Guid.NewGuid();

        #endregion

        #region Properties


        /// <summary>
        /// used to identify the level in the layout manager.
        /// </summary>
        public int LevelID
        {
            get
            {
                return Math.Abs(this.ID.GetHashCode());
            }
        }

        public Guid ID
        {
            get
            {
                return guid;
            }
            set
            {
                this.guid = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the name of this tile level.
        /// </summary>
        [XmlIgnore()]
        public string Name
        {
            get
            {
                return "Level " + this.Level;
            }
        }

        /// <summary>
        /// Gets or sets a value that sorts the levels.
        /// </summary>
        /// <remarks>
        /// This value is auto-computed by multiplying the Columns with the Rows.
        /// </remarks>
        [XmlIgnore()]
        public int Level
        {
            get
            {
                return 1 + (this.UseCustomUI ? 100 : 0) + this.Rows;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the tile should display custom plugin UI or not.
        /// </summary>
        public bool UseCustomUI
        {
            get
            {
                return useCustomUI;
            }
            set
            {
                this.useCustomUI = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a list of plugins that are in warning or critical state should be displayed at the bottom.
        /// </summary>
        public bool HasStatusBar
        {
            get
            {
                return hasStatusBar;
            }
            set
            {
                this.hasStatusBar = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that defines the number of rows that a tile in this level should offer for plugin visualizations.
        /// </summary>
        public int Rows
        {
            get
            {
                return rows;
            }
            set
            {
                this.rows = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Bestimmt, ob die Instanzen der LevelDefinition-Klasse gleich sind.
        /// </summary>
        /// <param name="a">Die erste Instanz der LevelDefinition-Klasse.</param>
        /// <param name="b">Die zweite Instanz der LevelDefinition-Klasse.</param>
        /// <returns>true, falls beide Instanzen übereinstimmen, sonst false.</returns>
        public static bool operator ==(LevelDefinition a, LevelDefinition b)
        {
            if (System.Object.ReferenceEquals(a, b)) return true;
            if (((object)a == null) || ((object)b == null)) return false;

            /*
             * Wenn beide Instanzen nicht null sind, dann die Equals-Methode aufrufen.
             */
            return a.Equals(b);
        }

        /// <summary>
        /// Bestimmt, ob die Instanzen der LevelDefinition-Klasse ungleich sind.
        /// </summary>
        /// <param name="a">Die erste Instanz der LevelDefinition-Klasse.</param>
        /// <param name="b">Die zweite Instanz der LevelDefinition-Klasse.</param>
        /// <returns>true, falls beide Instanzen nicht übereinstimmen, sonst false.</returns>
        public static bool operator !=(LevelDefinition a, LevelDefinition b)
        {
            return !(a == b);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bestimmt, ob das angegebene System.Object und das aktuelle Objekt gleich sind.
        /// </summary>
        /// <param name="obj">Das System.Object, das mit dem aktuellen System.Object verglichen werden soll.</param>
        /// <returns>true, wenn das angegebene Object gleich dem aktuellen Object ist, andernfalls false.</returns>
        public override bool Equals(object obj)
        {
            LevelDefinition other = obj as LevelDefinition;
            if ((object)other == null) return false;

            if (this.HasStatusBar == other.HasStatusBar &&
                this.Rows == other.Rows &&
                this.UseCustomUI == other.UseCustomUI)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Fungiert als Hashfunktion für LevelDefinition.
        /// </summary>
        /// <returns>Ein Hashcode für das aktuelle LevelDefinition-Objekt.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
