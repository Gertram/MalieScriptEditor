using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro;

namespace MSEGui
{
    public interface IFindable
    {
        void Find(string text,int start);
    }
    /// <summary>
    /// Interaction logic for FindWindow.xaml
    /// </summary>
    public partial class FindWindow : MetroWindow
    {
        public delegate void Find(string findable);
        private IFindable findable;
        public int Start { get; set; }
        public FindWindow(IFindable findable)
        {
            InitializeComponent();
            this.findable = findable;
        }

        private void FindCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            findable.Find(Findable.Text.Trim(),Start);
        }

        private void FindCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Findable.Text.Trim().Length > 0;
        }

        private void SelectFindableCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Findable.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Findable.Focus();
        }
    }
}
