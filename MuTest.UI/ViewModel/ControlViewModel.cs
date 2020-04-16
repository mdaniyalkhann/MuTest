using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Dashboard.ViewModel
{
    [POCOViewModel]
    public class ControlViewModel
    {
        public virtual Visibility Visibility { get; set; } = Visibility.Hidden;

        public virtual bool IsChecked { get; set; }

        protected ControlViewModel()
        {
        }

        public static ControlViewModel Create()
        {
            return ViewModelSource.Create(() => new ControlViewModel());
        }

        public static ControlViewModel CreateWithChecked()
        {
            return ViewModelSource.Create(() => new ControlViewModel
            {
                IsChecked = true
            });
        }
    }
}