using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dashboard.ViewModel;
using DevExpress.Data.TreeList;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;

namespace Dashboard.Views
{
    /// <summary>
    /// Interaction logic for ShimViewer.xaml
    /// </summary>
    public partial class MutantViewer : UserControl
    {
        public MutantViewer()
        {
            InitializeComponent();
        }

        private void MutatorList_OnLoaded(object sender, RoutedEventArgs e)
        {
            var selectedMutators = (MutantViewerViewModel)DataContext;
            foreach (var mutator in selectedMutators.SelectedMutators)
            {
                MutatorList.SelectedItems.Add(mutator);
            }
        }

        private void MutantsDetail_OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            if (MutantsDetail.View != null)
            {
                MutantsTreeList.ExpandAllNodes();
            }
        }

        private void MutantsTreeList_OnNodeChanged(object sender, TreeListNodeChangedEventArgs e)
        {
            var context = (MutantViewerViewModel)DataContext;
            var nodeContent = (MutantDetail)e.Node.Content;

            if (e.ChangeType == NodeChangeType.Add)
            {
                Dispatcher?.BeginInvoke(new Action(() =>
                {
                    var nodeParentNode = e.Node.ParentNode;
                    if (nodeParentNode != null && !nodeParentNode.IsExpanded)
                    {
                        nodeParentNode.IsExpanded = true;
                    }

                    if (nodeParentNode != null && !nodeParentNode.IsChecked.GetValueOrDefault())
                    {
                        nodeParentNode.IsChecked = true;
                    }

                    if (context.SelectedMutants.All(x => x.Id != nodeContent.Id))
                    {
                        context.SelectedMutants.Add(nodeContent);
                    }
                }));
            }

            if (e.ChangeType == NodeChangeType.CheckBox)
            {
                if (e.Node.IsChecked.GetValueOrDefault())
                {
                    if (context.SelectedMutants.All(x => x.Id != nodeContent.Id))
                    {
                        context.SelectedMutants.Add(nodeContent);
                    }

                    foreach (var mutantDetail in context.MutantsDetails.Where(x => x.ParentId == nodeContent.Id))
                    {
                        if (context.SelectedMutants.All(x => x.Id != mutantDetail.Id))
                        {
                            context.SelectedMutants.Add(mutantDetail);
                        }

                        foreach (var detail in context.MutantsDetails.Where(x => x.ParentId == mutantDetail.Id))
                        {
                            if (context.SelectedMutants.All(x => x.Id != detail.Id))
                            {
                                context.SelectedMutants.Add(detail);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var mutantDetail in context.MutantsDetails.Where(x => x.ParentId == nodeContent.Id))
                    {
                        foreach (var detail in context.MutantsDetails.Where(x => x.ParentId == mutantDetail.Id))
                        {
                            context.SelectedMutants.Remove(detail);
                        }

                        context.SelectedMutants.Remove(mutantDetail);
                    }

                    context.SelectedMutants.Remove(nodeContent);
                }
            }
        }
    }
}