using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Dashboard.ViewModel
{
    [POCOViewModel]
    public class LookupEditViewModel : ControlViewModel
    {
        public virtual object ItemsSource { get; set; }

        public virtual int SelectedIndex { get; set; }

        protected LookupEditViewModel()
        {
            
        }

        public static LookupEditViewModel CreateLookupEdit()
        {
            return ViewModelSource.Create(() => new LookupEditViewModel());
        }
    }
}
