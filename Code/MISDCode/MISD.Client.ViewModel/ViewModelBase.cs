using MISD.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Client.ViewModel
{
    /// <summary>
    /// This is the base class for all view model classes. This class is abstract.
    /// </summary>
    public abstract class ViewModelBase : BindableBase
    {
        #region Data Model

        /// <summary>
        /// Gets the singleton instance of the application's data model. 
        /// </summary>
        /// <remarks>
        /// This property makes direct databinding to the data model easier.
        /// </remarks>
        public DataModel DataModel
        {
            get
            {
                return DataModel.Instance;
            }
        }

        #endregion
    }
}
