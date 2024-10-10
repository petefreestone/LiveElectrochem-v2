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

namespace Live_Electrochem
{
    /// <summary>
    /// Interaction logic for FilePropertiesWindow.xaml
    /// </summary>
    public partial class FilePropertiesWindow : Window
    {
        public FilePropertiesWindow()
        {
            InitializeComponent();



        }


        public LVBinFile File
        {
            get { return (LVBinFile)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for File.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileProperty =
            DependencyProperty.Register("File", typeof(LVBinFile), typeof(FilePropertiesWindow), new PropertyMetadata(null));




    }
}
