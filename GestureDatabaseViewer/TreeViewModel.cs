using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LfS.GestureDatabase;
using LfS.ModelLib;

namespace LfS.GestureDatabaseViewer
{
    interface ITreeViewItemViewModel : INotifyPropertyChanged
    {
        ObservableCollection<TreeViewItemViewModel> Children { get; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        TreeViewItemViewModel Parent { get; }
    }

    public class TraceTreeViewModel
    {
        readonly ReadOnlyCollection<UserViewModel> _users;

        public TraceTreeViewModel()
        {
            using (dbEntities ctx = new dbEntities())
            {
                var tmpUsers = new LinkedList<UserViewModel>();
                foreach (var user in ctx.Users)
                    tmpUsers.AddLast(new UserViewModel(user));
                _users = new ReadOnlyCollection<UserViewModel>(tmpUsers.ToList());
            }
        }

        public ReadOnlyCollection<UserViewModel> Users
        {
            get { return _users; }
        }
    }


    public class TreeViewItemViewModel : ITreeViewItemViewModel
    {
        protected TreeViewItemViewModel _parent;
        protected ObservableCollection<TreeViewItemViewModel> _children;
        protected bool _isExpanded;
        protected bool _isSelected;
        protected TreeViewItemViewModel _dummy;
        protected bool _AreChildrenLoaded = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        
        protected TreeViewItemViewModel()
        {
        }

        protected TreeViewItemViewModel(TreeViewItemViewModel parent)
        {
            _parent = parent;
            _children = new ObservableCollection<TreeViewItemViewModel>();
            //add dummy
            _dummy = new TreeViewItemViewModel();
            _children.Add(_dummy);
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// <span class="code-SummaryComment"></summary></span>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.Children.First() == _dummy)
                    this.LoadChildren();
            }
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// <span class="code-SummaryComment"></summary></span>
        public virtual void LoadChildren()
        {
        }

        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get { return _children; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }

                // Expand all the way up to the root.
                //if (_isSelected && _parent != null)
                    //this.IsExpanded = true;
            }
        }

        public TreeViewItemViewModel Parent
        {
            get { return _parent; }
        }
    }

    public class UserViewModel : TreeViewItemViewModel
    {
        readonly User _user;

        public UserViewModel(User user) 
            : base(null)
        {
            _user = user;
            //LoadChildren();
        }

        public string Name
        {
            get { return _user.Username; }
        }

        public override void LoadChildren()
        {
            if (_AreChildrenLoaded) return;
            _children.Clear();
            using (dbEntities ctx = new dbEntities())
            {
                foreach (var gesture in ctx.Gestures.Where(g => g.User.Id == _user.Id))
                    base.Children.Add(new GestureViewModel(gesture, this));
            }
            _AreChildrenLoaded = true;
        }
    }

    public class GestureViewModel : TreeViewItemViewModel
    {
        readonly Gesture _gesture;

        public GestureViewModel(Gesture gesture, TreeViewItemViewModel parent)
            : base(parent)
        {
            _gesture = gesture;
            //LoadChildren();
        }

        public string Name
        {
            get { return _gesture.Name; }
        }

        public long ID
        {
            get { return _gesture.Id; }
        }

        public override void LoadChildren()
        {
            if (_AreChildrenLoaded) return;
            _children.Clear();
            using (dbEntities ctx = new dbEntities())
            {
                foreach (var trace in ctx.Traces.Where(t => t.Gesture.Id == _gesture.Id))
                    base.Children.Add(new TraceViewModel(trace, this));
            }
            _AreChildrenLoaded = true;
        }
    }

    public class TraceViewModel : TreeViewItemViewModel
    {
        readonly Trace _trace;
        readonly TraceFeatures _features;

        public TraceViewModel(Trace trace, TreeViewItemViewModel parent)
            : base(parent)
        {
            _trace = trace;
            _features = new TraceFeatures(trace.Touches);
            _children.Clear(); //dummy entfernen
        }

        public string Name
        {
            get { return _trace.Id.ToString(); }
        }

        public long ID
        {
            get { return _trace.Id; }
        }

        public TraceFeatures Features
        {
            get { return _features; }
        }
    }
}
