using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Dashboard.ViewModel
{
    [POCOViewModel]
    public class ListBoxEditViewModel : ControlViewModel
    {
        public virtual object ItemsSource { get; set; }

        protected ListBoxEditViewModel()
        {

        }

        public static ListBoxEditViewModel CreateListBoxEdit()
        {
            return ViewModelSource.Create(() => new ListBoxEditViewModel());
        }
    }
}
