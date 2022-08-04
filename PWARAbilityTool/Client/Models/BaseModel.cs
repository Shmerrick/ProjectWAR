using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.ComponentModel;

namespace PWARAbilityTool.Client.Models
{
    public abstract class BaseModel : ObservableObject, INotifyPropertyChanged, IDataErrorInfo
    {
        protected Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();

        public new event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// this list holds all properties of the model that needs to be inserted/updated => needed for building the queries.
        /// </summary>
        public List<string> ToUpdateMembers { get; set; }

        #region property changed stuff

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Update(string propertyName)
        {
            if (ToUpdateMembers != null && !ToUpdateMembers.Contains(propertyName))
                ToUpdateMembers.Add(propertyName);

            OnPropertyChanged(propertyName);
        }

        #endregion property changed stuff

        #region error handling stuff

        protected virtual void CollectErrors()
        {
            //must be implemented by childs
            return;
        }

        public string this[string columnName]
        {
            get
            {
                CollectErrors();
                return Errors.ContainsKey(columnName) ? Errors[columnName] : string.Empty;
            }
        }

        public string Error => "!!!!!!Error!!!!!!";

        #endregion error handling stuff
    }
}