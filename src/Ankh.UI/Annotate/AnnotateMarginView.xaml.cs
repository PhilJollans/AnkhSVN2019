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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ankh.UI.Annotate
{
    /// <summary>
    /// WPF version of the annotate margin.
    ///
    /// Initially, this will be hosted in the AnnotateEditoView, but I am considering it in a
    /// custom margin in the Visual Studio editor.
    /// </summary>
    [CLSCompliant(false)]
    public partial class AnnotateMarginView : UserControl
    {
        public AnnotateMarginView ( )
        {
            InitializeComponent ();
        }

        public AnnotateMarginView ( AnnotateMarginViewModel vm )
        {
            InitializeComponent ();
            DataContext = vm ;
        }
    }
}
