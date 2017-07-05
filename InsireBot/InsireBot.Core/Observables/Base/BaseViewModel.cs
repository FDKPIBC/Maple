namespace Maple.Core
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class BaseViewModel<TViewModel> : ObservableObject
    {
        /// <summary>
        /// The busy stack
        /// </summary>
        protected readonly BusyStack _busyStack;

        private TViewModel _model;
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public TViewModel Model
        {
            get { return _model; }
            protected set { SetValue(ref _model, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetValue(ref _isBusy, value); }
        }

        protected BaseViewModel()
        {
            _busyStack = new BusyStack();
            _busyStack.OnChanged += (isBusy) => IsBusy = isBusy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewModel{T}"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        protected BaseViewModel(TViewModel model)
            : this()
        {
            Model = model;
        }
    }
}
