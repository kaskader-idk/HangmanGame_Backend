using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

using Org.OpenAPITools.Api;
using Org.OpenAPITools.Model;

namespace HangmanBackendWpf;

public partial class MainWindow : Window
{
  private readonly HangmanBackendApi _api = new("http://localhost:5000");
  public MainWindow() => InitializeComponent();

  private void Window_Loaded(object sender, RoutedEventArgs e)
  {
    TestBackend();
  }

}
