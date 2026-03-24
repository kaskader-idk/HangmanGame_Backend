namespace Hangman_BackendWpf;

public partial class MainWindow
{
  private void TestBackend()
  {
    try
    {
      var reply = _api.DbtestHangmanDBGet();
      Title = $"IsOk={reply.IsOk} / Nr={reply.Nr}";
    }
    catch (Exception ex)
    {
      Title = ex.Message;
    }
  }
}

