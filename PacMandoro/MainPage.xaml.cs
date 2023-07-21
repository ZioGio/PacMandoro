using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PacMandoro
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string PacManEatFruitSoundFile = "Pac-Man_Eat_Fruit.wav";
        private const string PacManInsertCoinSoundFile = "Insert_Coin.wav";
        private const string PacManBeginningSoundFile = "Pac-Man_Beginning.wav";
        private const string PacManDeathSoundFile = "Pac-Man_Death.wav";
        private const string PacManIntermissionSoundFile = "Pac-Man_Intermission.wav";
        private const string PacManEatGhostSoundFile = "Pac-Man_Eat_Ghost.wav";
        private DispatcherTimer timer;
        private int timeRemaining;
        private readonly int taskTime = 15; // seconds 
        private readonly int breakTime = 10; // seconds 
        private string resourceString;
        private TimeSpan ts;

        public MainPage()
        {
            InitializeComponent();
            Timer();
            Loaded += StartScreen_Loaded;

            // Sets preferred window size
            ApplicationView.PreferredLaunchViewSize = new Size(707, 700);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        // Start Screen
        #region Start Screen
        private async void StartScreen_Loaded(object sender, RoutedEventArgs e)
        {
            // Change background color of TitleBar
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Black;

            // Start text animation
            await StartScreenTextReveal();
        }

        private async void StartScreenPlayButton_Click(object sender, RoutedEventArgs e)
        {
            await InsertCoinSound();
            InitializeTaskScreen();
        }
        #endregion

        // Task Screen
        #region Task Screen
        private void InitializeTaskScreen()
        {
            StartScreenGrid.Visibility = Visibility.Collapsed;
            TaskScreenGrid.Visibility = Visibility.Visible;
            TimerScreenGrid.Visibility = Visibility.Collapsed;

            // TextBox Placeholder text
            ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
            resourceString = resourceLoader.GetString("TaskTextBoxPlaceholder/Text");
            TaskTextBox.PlaceholderText = resourceString;

            EnableTaskButtons();
        }

        private async void TaskTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                TaskListView.Items.Add(TaskTextBox.Text);
                TaskTextBox.Text = "";
                TaskListView.SelectedIndex = 0;
                EnableTaskButtons();
                await InsertCoinSound();
            }
        }

        private async void RemoveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            TaskListView.Items.Remove(TaskListView.SelectedItem);
            EnableTaskButtons();
            await InsertCoinSound();
        }

        private void GoToTimerButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeTimerScreen();
        }

        private void EnableTaskButtons()
        {
            if (TaskListView.Items.Count > 0 && TaskListView.SelectedItem != null)
            {
                RemoveTaskButton.IsEnabled = true;
                GoToTimerButton.IsEnabled = true;
            }
            else
            {
                RemoveTaskButton.IsEnabled = false;
                GoToTimerButton.IsEnabled = false;
            }
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableTaskButtons();
        }
        #endregion

        // Timer Screen
        #region Timer Screen
        private void Timer()
        {
            // Create timer
            NavigationCacheMode = NavigationCacheMode.Required;
            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            timer.Tick += Timer_TickAsync;
        }

        private void TimerString()
        {
            ts = TimeSpan.FromSeconds(timeRemaining);
            TimerTextBlock.Text = ts.ToString("%m\\:ss");
        }

        private void StartTimerString(string s)
        {
            ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
            resourceString = resourceLoader.GetString(s);
            StartTimerButton.Content = resourceString;
        }

        private async void Timer_TickAsync(object sender, object e)
        {
            // I could set each tick to the "munch" sound, but that defeats aim of the user focusing on their task without distraction            
            timeRemaining--;
            TimerString();
            TaskProgressBar.Value = TaskProgressBar.Maximum - timeRemaining;
            BreakProgressBar.Value = BreakProgressBar.Maximum - timeRemaining;

            if (timeRemaining == 0)
            {
                timer.Stop();
                await SwapTaskBreak();
            }
        }

        private void ShowTimer()
        {
            TimerTextBlock.Visibility = Visibility.Visible;
            TaskFinishedButton.Visibility = Visibility.Collapsed;
        }

        private void ShowFinishedButton()
        {
            TimerTextBlock.Visibility = Visibility.Collapsed;
            TaskFinishedButton.Visibility = Visibility.Visible;
        }

        private void TimerStartButtonEnabled()
        {
            StartTimerButton.IsEnabled = true;
            StopTimerButton.IsEnabled = false;
            RestartTimerButton.IsEnabled = false;
            SkipTimerButton.IsEnabled = false;
        }

        private void TimerRestartSkipButtonsEnabled()
        {
            StartTimerButton.IsEnabled = true;
            StopTimerButton.IsEnabled = false;
            RestartTimerButton.IsEnabled = true;
            SkipTimerButton.IsEnabled = true;
        }

        private void InitializeTimerScreen()
        {
            StartScreenGrid.Visibility = Visibility.Collapsed;
            TaskScreenGrid.Visibility = Visibility.Collapsed;
            TimerScreenGrid.Visibility = Visibility.Visible;

            if (TaskListView.Items.Count > 0 && TaskListView.SelectedItem != null)
            {
                ItemNameTextBlock.Text = TaskListView.SelectedItem.ToString();
            }

            timeRemaining = taskTime;
            TimerString();

            ShowTimer();

            TaskProgressBar.Visibility = Visibility.Collapsed;
            TaskProgressBar.Value = 0;
            TaskProgressBar.Maximum = taskTime;

            BreakProgressBar.Visibility = Visibility.Collapsed;
            BreakProgressBar.Value = 0;
            BreakProgressBar.Maximum = breakTime;

            timeRemaining = taskTime;

            StartTimerString("Start/Content");

            TimerStartButtonEnabled();
        }

        private async Task SwapTaskBreak()
        {
            StartTimerString("Start/Content");

            TimerStartButtonEnabled();

            if (TaskProgressBar.Visibility == Visibility.Visible && timeRemaining == 0)
            {
                PacManDeathSound();
                await Task.Delay(1000);  // 1 second 

                TaskProgressBar.Visibility = Visibility.Collapsed;

                timeRemaining = breakTime;
                TimerString();

                BreakProgressBar.Value = 0;

                ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
                resourceString = resourceLoader.GetString("BreakTime/Text");
                ItemNameTextBlock.Text = resourceString;
            }

            if (BreakProgressBar.Visibility == Visibility.Visible && timeRemaining == 0)
            {
                PacManEatGhostSound();
                await Task.Delay(1000);  // 1 second 

                BreakProgressBar.Visibility = Visibility.Collapsed;

                timeRemaining = taskTime;
                TimerString();

                TaskProgressBar.Value = 0;

                if (TaskListView.Items.Count > 0)
                {
                    ItemNameTextBlock.Text = TaskListView.SelectedItem.ToString();
                }
            }
        }

        private async void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            StartTimerButton.IsEnabled = false;
            StopTimerButton.IsEnabled = false;
            RestartTimerButton.IsEnabled = false;
            SkipTimerButton.IsEnabled = false;

            ShowTimer();

            if (timeRemaining == taskTime && TaskProgressBar.Visibility == Visibility.Collapsed && BreakProgressBar.Visibility == Visibility.Collapsed)
            {
                ReadyTextBlock.Visibility = Visibility.Visible;
                await PacManBeginningSound();
                ReadyTextBlock.Visibility = Visibility.Collapsed;
                TaskProgressBar.Visibility = Visibility.Visible;
            }

            if (timeRemaining == breakTime && TaskProgressBar.Visibility == Visibility.Collapsed && BreakProgressBar.Visibility == Visibility.Collapsed)
            {
                PacManIntermissionSound();
                BreakProgressBar.Visibility = Visibility.Visible;
            }

            timer.Start();

            StopTimerButton.IsEnabled = true;
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            ShowFinishedButton();

            StartTimerString("Resume/Content");

            TimerRestartSkipButtonsEnabled();
        }

        private void RestartTimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskProgressBar.Visibility == Visibility.Visible)
            {
                timeRemaining = taskTime;
                TaskProgressBar.Value = 0;
            }

            else if (BreakProgressBar.Visibility == Visibility.Visible)
            {
                timeRemaining = breakTime;
                BreakProgressBar.Value = 0;
            }

            TimerString();

            StartTimerString("Start/Content");

            ShowTimer();

            TimerStartButtonEnabled();
        }

        private async void SkipTimerButton_Click(object sender, RoutedEventArgs e)
        {
            timeRemaining = 0;
            timer.Stop();
            ShowTimer();
            await SwapTaskBreak();
        }

        private void TaskFinishedButton_Click(object sender, RoutedEventArgs e)
        {
            TaskListView.Items.Remove(TaskListView.SelectedItem);
            InitializeTimerScreen();
            InitializeTaskScreen();
        }
        #endregion

        // Sounds
        #region Sounds
        private async void Play(string fileName)
        {
            MediaElement mediaElement = new MediaElement();
            StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets\\WavFiles");
            StorageFile file = await folder.GetFileAsync(fileName);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            mediaElement.SetSource(stream, file.ContentType);
            mediaElement.Play();
        }

        private async Task StartScreenTextReveal()
        {
            StartScreenStoryboard.Begin();
            await Task.Delay(9750); // 9.75 seconds

            Play(PacManEatFruitSoundFile);
        }

        private async Task InsertCoinSound()
        {
            Play(PacManInsertCoinSoundFile);

            ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
            resourceString = resourceLoader.GetString("Credit/Text");
            CreditsTextBlock.Text = resourceString + "   " + TaskListView.Items.Count.ToString();
            await Task.Delay(1250); // 1.25 seconds
        }

        private async Task PacManBeginningSound()
        {
            Play(PacManBeginningSoundFile);
            await Task.Delay(4500);  // 4.5 seconds 
        }

        private void PacManDeathSound()
        {
            Play(PacManDeathSoundFile);
        }

        private void PacManIntermissionSound()
        {
            Play(PacManIntermissionSoundFile);
        }

        private void PacManEatGhostSound()
        {
            Play(PacManEatGhostSoundFile);
        }
        #endregion
    }
}