using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISD.Client.Managers;

namespace MISD.Client.Model
{
    /// <summary>
    /// Base class for tileable elements like monitored systems and organizational units.
    /// </summary>
    [Serializable]
    public abstract class TileableElement : BindableBase, IComparable<MISD.Client.Model.TileableElement>
    {
        #region fields
        private int id;
        private string name;
        private string fqdn;
        #endregion

        #region constructors
        private TileableElement()
        {
        }

        public TileableElement(int id, string name, string fqdn)
        {
            this.ID = id;
            this.FQDN = fqdn;
            this.Name = name;
            LayoutManager.Instance.SetValue(ID, IsOu.Value, ID);
            LayoutManager.Instance.ValueChanged += Instance_ValueChanged;
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the name of this tilable Element.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets the id of this tilable Element
        /// </summary>
        public int ID
        {
            get
            {
                return this.id;
            }
            set
            {
                if (this.id != value)
                {
                    this.id = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the FQDN of this tilable Element.
        /// </summary>
        public string FQDN
        {
            get
            {
                return this.fqdn;
            }
            set
            {
                if (this.fqdn != value)
                {
                    this.fqdn = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private float? sortingProperty = null;

        public float SortingProperty
        {
            get
            {
                if (!sortingProperty.HasValue)
                {

                    bool? isOu = IsOu;
                    if (isOu.HasValue)
                    {
                        sortingProperty = LayoutManager.Instance.GetValue(id, isOu.Value);
                    }
                    else
                    {
                        sortingProperty = 0.0f;
                    }
                }
                return sortingProperty.Value;
            }
            set
            {
                if (!sortingProperty.HasValue || sortingProperty.Value != value)
                {
                    sortingProperty = value;
                    MoveElement(ID, sortingProperty.Value);
                }
            }
        }

        #endregion

        #region private Methods

        protected abstract void MoveElement(int ID, float Value);

        private void Instance_ValueChanged(int ID, bool IsOu, float Value)
        {
            if (this.ID == ID && IsOu == this.IsOu)
            {
                SortingProperty = Value;
            }
        }


        private bool? IsOu
        {
            get
            {
                if (this.GetType() == typeof(MonitoredSystem))
                {
                    return false;
                }
                else if (this.GetType() == typeof(OrganizationalUnit))
                {
                    return true;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        public int CompareTo(TileableElement obj)
        {
            if (obj == null) return 1;

            return this.SortingProperty.CompareTo(obj.SortingProperty);


        }
    }
}
