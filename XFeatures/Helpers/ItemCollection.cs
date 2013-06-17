using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace XFeatures.Helpers
{
    public interface IUpdateable
    {

        // ----------------------------------------------------------------------
        bool IsUpdating { get; }

        // ----------------------------------------------------------------------
        void BeginUpdate();

        // ----------------------------------------------------------------------
        void EndUpdate();

    } // interface IUpdateable

    public interface IItemCollection<TItem> : ICollection<TItem>,
       INotifyCollectionChanged, INotifyPropertyChanged, IUpdateable
    {

        // ----------------------------------------------------------------------
        void AddAll(IEnumerable<TItem> items);
    } // interface IItemCollection

    // ------------------------------------------------------------------------
    //This class improves the performance of observablecollection. Because for each modification in observable collection updates the screen painting.
    //If we are adding multiple entry continously no need update screen for each addition. Call BeginUpdate() and add the elements and call EndUpdate() to notify for painting all.
    public class ItemCollection<TItem> : ObservableCollection<TItem>, IItemCollection<TItem>
    {
        public bool IsUpdating
        {
            get { return _updateCount > 0; }
        }

        public virtual void BeginUpdate()
        {
            _updateCount++;
        }
        public virtual void EndUpdate()
        {
            if (_updateCount == 0)
            {
                throw new InvalidOperationException();
            }
            _updateCount--;
            UpdateCollectionChanged();
        }
        public virtual void AddAll(IEnumerable<TItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            foreach (TItem item in items)
            {
                Add(item);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsUpdating)
            {
                // break the workflow to reduce the UI communication
                return;
            }
            try
            {
                base.OnCollectionChanged(e);
            }
            catch (Exception ex)
            {
                //suppress not supported exception. At any case do not crash studio from this extension.
            }
            
        }
        protected void UpdateCollectionChanged()
        {
            if (IsUpdating)
            {
                return;
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private int _updateCount;


    } // class ItemCollection
}
