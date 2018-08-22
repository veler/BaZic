using BaZic.Core.ComponentModel;
using BaZic.Core.Logs;
using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BaZic.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private BaZicInterpreter _interpreter;
        private string _logs;
        private SynchronizationContext _synchronizationContext;

        #endregion

        #region Properties

        public string Logs
        {
            get { return _logs; }
            set
            {
                _logs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Logs)));
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            var culture = new System.Globalization.CultureInfo("en");
            Core.Localization.LocalizationHelper.SetCurrentCulture(culture);
            Runtime.Localization.LocalizationHelper.SetCurrentCulture(culture);

            Logger.Initialize<LogSession>();
            Logger.Instance.SessionStarted();

            LoadRandomProgramButton_Click(this, null);

            ThreadHelper.RunOnStaThread(() =>
            {
                _synchronizationContext = SynchronizationContext.Current;
            });
        }

        #endregion

        #region Methods

        private void Log(string log)
        {
            Logs += log + Environment.NewLine;
        }

        #endregion

        #region Handled Methods

        private void LoadRandomProgramButton_Click(object sender, RoutedEventArgs e)
        {
            var resourceNames = typeof(MainWindow).Assembly.GetManifestResourceNames();
            var resources = new List<string>();

            for (int i = 0; i < resourceNames.Length; i++)
            {
                if (resourceNames[i].StartsWith("BaZic.Sample.Samples."))
                {
                    var shortName = resourceNames[i].Substring("BaZic.Sample.Samples.".Length).Split('.').FirstOrDefault();
                    if (!resources.Contains(shortName))
                    {
                        resources.Add(shortName);
                    }
                }
            }

            var index = new Random().Next(0, resources.Count);
            var resource = resources[index];
            var baZicCodeFilePath = $"BaZic.Sample.Samples.{resource}.BaZic.txt";
            var xamlCodeFilePath = $"BaZic.Sample.Samples.{resource}.Xaml.txt";

            if (resourceNames.Contains(baZicCodeFilePath))
            {
                BaZicCodeTextBox.Text = new StreamReader(typeof(MainWindow).Assembly.GetManifestResourceStream(baZicCodeFilePath)).ReadToEnd();
            }
            else
            {
                BaZicCodeTextBox.Text = string.Empty;
            }

            if (resourceNames.Contains(xamlCodeFilePath))
            {
                XamlCodeTextBox.Text = new StreamReader(typeof(MainWindow).Assembly.GetManifestResourceStream(xamlCodeFilePath)).ReadToEnd();
            }
            else
            {
                XamlCodeTextBox.Text = string.Empty;
            }
        }

        private void RunProgramButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BaZicCodeTextBox.Text))
            {
                MessageBox.Show("There is no BaZic code to run.");
                return;
            }

            Logs = string.Empty;

            if (string.IsNullOrWhiteSpace(XamlCodeTextBox.Text))
            {
                XamlCodeTextBox.Text = string.Empty;
            }

            var lexer = new BaZicLexer();
            var parser = new BaZicParser();

            var tokens = lexer.Tokenize(BaZicCodeTextBox.Text);
            var abstractSyntaxTree = parser.Parse(tokens, XamlCodeTextBox.Text, OptimizeCheckBox.IsChecked.Value);

            foreach (var issue in abstractSyntaxTree.Issues.InnerExceptions.OfType<BaZicParserException>())
            {
                Log(issue.ToString());
            }

            if (abstractSyntaxTree.Program != null && abstractSyntaxTree.Issues.InnerExceptions.OfType<BaZicParserException>().All(issue => issue.Level != Core.Enums.BaZicParserExceptionLevel.Error))
            {
                RunProgramButton.Visibility = Visibility.Collapsed;
                RunProgramReleaseButton.Visibility = Visibility.Collapsed;
                OptimizeCheckBox.Visibility = Visibility.Collapsed;
                PauseButton.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Visible;

                _interpreter = new BaZicInterpreter(abstractSyntaxTree.Program);
                _interpreter.StateChanged += Interpreter_StateChanged;
                var t = _interpreter.StartDebugAsync(true);
            }
            else
            {
                Log("The program has not been interpreted.");
            }
        }

        private void RunProgramReleaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BaZicCodeTextBox.Text))
            {
                MessageBox.Show("There is no BaZic code to run.");
                return;
            }

            Logs = string.Empty;

            if (string.IsNullOrWhiteSpace(XamlCodeTextBox.Text))
            {
                XamlCodeTextBox.Text = string.Empty;
            }

            var lexer = new BaZicLexer();
            var parser = new BaZicParser();

            var tokens = lexer.Tokenize(BaZicCodeTextBox.Text);
            var abstractSyntaxTree = parser.Parse(tokens, XamlCodeTextBox.Text, OptimizeCheckBox.IsChecked.Value);

            foreach (var issue in abstractSyntaxTree.Issues.InnerExceptions.OfType<BaZicParserException>())
            {
                Log(issue.ToString());
            }

            if (abstractSyntaxTree.Program != null && abstractSyntaxTree.Issues.InnerExceptions.OfType<BaZicParserException>().All(issue => issue.Level != Core.Enums.BaZicParserExceptionLevel.Error))
            {
                RunProgramButton.Visibility = Visibility.Collapsed;
                RunProgramReleaseButton.Visibility = Visibility.Collapsed;
                OptimizeCheckBox.Visibility = Visibility.Collapsed;
                PauseButton.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Visible;

                _interpreter = new BaZicInterpreter(abstractSyntaxTree.Program);
                _interpreter.StateChanged += Interpreter_StateChanged;
                var t = _interpreter.StartReleaseAsync(true);
            }
            else
            {
                Log("The program has not been build.");
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            _interpreter.Pause();
            PauseButton.Visibility = Visibility.Collapsed;
            NextStepButton.Visibility = Visibility.Visible;
            ResumeButton.Visibility = Visibility.Visible;
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await _interpreter.Stop();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            _interpreter.Resume();
            PauseButton.Visibility = Visibility.Visible;
            NextStepButton.Visibility = Visibility.Collapsed;
            ResumeButton.Visibility = Visibility.Collapsed;
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            _interpreter.NextStep();
        }

        private void Interpreter_StateChanged(object sender, BaZicInterpreterStateChangeEventArgs e)
        {
            Log(e.ToString());

            if (e.State == BaZicInterpreterState.Stopped || e.State == BaZicInterpreterState.StoppedWithError)
            {
                _synchronizationContext.Send((d) =>
                {
                    RunProgramButton.Visibility = Visibility.Visible;
                    RunProgramReleaseButton.Visibility = Visibility.Visible;
                    OptimizeCheckBox.Visibility = Visibility.Visible;
                    PauseButton.Visibility = Visibility.Collapsed;
                    StopButton.Visibility = Visibility.Collapsed;
                    ResumeButton.Visibility = Visibility.Collapsed;
                    NextStepButton.Visibility = Visibility.Collapsed;
                }, null);

                Task.Run(() =>
                {
                    Task.Delay(500).Wait();
                    _interpreter.Dispose();
                });
            }
        }

        #endregion
    }
}
