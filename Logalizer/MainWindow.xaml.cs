using LogParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Logalizer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Parser parser;
        private string selectedLog;
        private BackgroundWorker backgroundAccessWorker;
        private BackgroundWorker backgroundAccessChartWorker;
        private BackgroundWorker backgroundErrorWorker;
        private BackgroundWorker backgroundErrorChartWorker;
        private BackgroundWorker backgroundMapWorker;

        public MainWindow()
        {
            InitializeComponent();
            parser = new Parser();
            backgroundAccessWorker = ((BackgroundWorker)FindResource("backgroundAccessWorker"));
            backgroundAccessChartWorker = ((BackgroundWorker)FindResource("backgroundAccessChartWorker"));
            backgroundErrorWorker = ((BackgroundWorker)FindResource("backgroundErrorWorker"));
            backgroundErrorChartWorker = ((BackgroundWorker)FindResource("backgroundErrorChartWorker"));
            backgroundMapWorker = ((BackgroundWorker)FindResource("backgroundMapWorker"));
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "Logalizer 1.0 Alpha\n" +
                "Роман Самохвалов\n" +
                "ДонНТУ, кафедра Программной инженерии\n" +
                "Авторские права © 2017", "О программе", MessageBoxButton.OK);
        }

        private void MenuHelp_Click(object sender, RoutedEventArgs e)
        {
            string filename = @"Users guide.pdf";
            if (File.Exists(filename))
                System.Diagnostics.Process.Start(filename);
            else
                System.Windows.MessageBox.Show("Файл \"Users guide.pdf\" не найден," +
                    " переустановите программу!", "Ошибка", MessageBoxButton.OK,MessageBoxImage.Error);
        }

        private void MenuAccessFolderOpen_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            if (fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                parser.AccessFolderPath = fb.SelectedPath;
                accessPanel.IsEnabled = true;
                accessFolder.Text = parser.AccessFolderPath;
            }
        }

        private void MenuErrorFolderOpen_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            if (fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                parser.ErrorFolderPath = fb.SelectedPath;
                errorPanel.IsEnabled = true;
                errorFolder.Text = parser.ErrorFolderPath;
            }


        }

        private void AccessAnalysis_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.GetFiles(parser.AccessFolderPath, "access-*.log").Count() > 0)
            {
                ProgressBar.Visibility = Visibility.Visible;
                accessAnalysis.IsEnabled = false;
                backgroundAccessWorker.RunWorkerAsync(new AccessDirectoryParser());
            }
            else System.Windows.MessageBox.Show("Не найдены файлы access.log", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ErrorAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var errordirparser = new ErrorDirectoryParser();
            if (Directory.GetFiles(parser.ErrorFolderPath, "error-*.log").Count() > 0)
            {
                errorAnalysis.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;
                backgroundErrorWorker.RunWorkerAsync(new ErrorDirectoryParser());
            }
            else System.Windows.MessageBox.Show("Не найдены файлы error.log", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void ResetAccessButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
            System.Windows.Application.Current.Shutdown();
        }       

        private void MenuAccessReportSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName="accessreport.axml",
                Filter = "Отчеты access.log(*.AXML)|*.AXML" + "|Все файлы (*.*)|*.* "
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SerializeToXAML(accessGrid, saveFileDialog.FileName);
            }
        }

        private void MenuErrorReportSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName = "errorreport.exml",
                Filter = "Отчеты access.log(*.EXML)|*.EXML" + "|Все файлы (*.*)|*.* "
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SerializeToXAML(errorGrid, saveFileDialog.FileName);
            }
        }

        private void MenuAccessReportOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Отчеты access.log(*.AXML)|*.AXML" + "|Все файлы (*.*)|*.* "
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Grid grid = DeSerializeXAML(openFileDialog.FileName) as Grid;
                while (grid.Children.Count > 0)
                {
                    UIElement obj = grid.Children[0];
                    grid.Children.Remove(obj);
                    accessGrid.Children.Add(obj);
                }
            }

        }

        private void MenuErrorReportOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Отчеты error.log(*.EXML)|*.EXML" + "|Все файлы (*.*)|*.* "
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Grid grid = DeSerializeXAML(openFileDialog.FileName) as Grid;
                while (grid.Children.Count > 0)
                {
                    UIElement obj = grid.Children[0];
                    grid.Children.Remove(obj);
                    errorGrid.Children.Add(obj);
                }
            }

        }

        private void BuildAccessChart_Click(object sender, RoutedEventArgs e)
        {
            buildAccessChart.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            backgroundAccessChartWorker.RunWorkerAsync(accessChartList.SelectedIndex);
        }

        private void BuildErrorChart_Click(object sender, RoutedEventArgs e)
        {
            buildErrorChart.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            backgroundErrorChartWorker.RunWorkerAsync(errorChartList.SelectedIndex);

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton pressed = (System.Windows.Controls.RadioButton)sender;
            buildMap.IsEnabled = true;
            selectedLog = pressed.Content.ToString();
        }

        private void BuildMap_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Для этого действия требуется подключение к сети Интернет, вы хотите продолжить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ProgressBar.Visibility = Visibility.Visible;
                backgroundMapWorker.RunWorkerAsync(selectedLog);
            }
        }

        private void AccessDateFromPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = accessDateFromPicker.SelectedDate;
            if (date != null)
            {
                parser.AccessDateFrom = date.GetValueOrDefault();
            }
            ValidateAccessDate();
        }

        private void AccessDateToPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = accessDateToPicker.SelectedDate;
            if (date != null)
            {
                parser.AccessDateTo = date.GetValueOrDefault();
            }
            ValidateAccessDate();
        }

        private void ErrorDateFromPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = errorDateFromPicker.SelectedDate;
            if (date != null)
            {
                parser.ErrorDateFrom = date.GetValueOrDefault();
            }
            ValidateErrorDate();
        }

        private void ErrorDateToPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = errorDateToPicker.SelectedDate;
            if (date != null)
            {
                parser.ErrorDateTo = date.GetValueOrDefault();
            }
            ValidateErrorDate();
        }

        private List<KeyValuePair<string, int>> PrepareAccessColumnChart(IEnumerable<AccessLog> log)
        {
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();
            var tmp = TrimAccessDate(log);
            var data = tmp.AsParallel().GroupBy(n => n.ClientIp).OrderByDescending(n => n.Count());
            foreach (var d in data) valueList.Add(new KeyValuePair<string, int>(d.Key, d.Count()));
            if (valueList.Count() > 5)
                valueList = valueList.Take(5).ToList();
            return valueList;
        }

        private void ShowAccessColumnChart(List<KeyValuePair<string, int>> valueList)
        {
            ClearChart();

            var chartSeries = new ColumnSeries()
            {
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Title = "Кол-во запросов"
            };
            chart.Series.Add(chartSeries);
            chart.Title = "Активность пользователей";
        }

        private List<KeyValuePair<string, int>> PrepareAccessPieChart(IEnumerable<AccessLog> log)
        {
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();
            var tmp = TrimAccessDate(log);
            var data = tmp.AsParallel().GroupBy(n => n.StatusCode / 100).OrderBy(n => n.Key);
            foreach (var d in data) valueList.Add(new KeyValuePair<string, int>
                (d.Key.ToString() + "xx", d.Count()));
            return valueList;
        }

        private void ShowAccessPieChart(List<KeyValuePair<string, int>> valueList)
        {
            ClearChart();

            var pieSeries = new PieSeries()
            {
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Title = "Код ответа"
            };
            chart.Series.Add(pieSeries);
            chart.Title = "Коды состояния HTTP";
        }

        private List<KeyValuePair<string, int>> PrepareAccessAreaChart(IEnumerable<AccessLog> log)
        {
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < 24; i++)
            {
                valueList.Add(new KeyValuePair<string, int>(i.ToString(), 0));
            }
            var tmp = TrimAccessDate(log);
            var data = tmp.AsParallel().GroupBy(n => n.DateTime.Hour);
            foreach (var d in data)
            {
                valueList.Remove(new KeyValuePair<string, int>(d.Key.ToString(), 0));
                valueList.Add(new KeyValuePair<string, int>(d.Key.ToString(), d.Count()));
            }
            return valueList.OrderBy(n => System.Convert.ToInt32(n.Key)).ToList();
        }

        private void ShowAccessAreaChart(List<KeyValuePair<string, int>> valueList)
        {
            ClearChart();

            var areaSeries = new AreaSeries()
            {
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Title = "Кол-во запросов"
            };
            chart.Series.Add(areaSeries);
            chart.Title = "Суточная активность";
        }

        private List<KeyValuePair<string, int>> PrepareAccessBarChart(IEnumerable<AccessLog> log)
        {
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();
            var tmp = TrimAccessDate(log);
            var data = tmp.AsParallel().GroupBy(n => n.ResourcePath).OrderByDescending(n => n.Count());
            foreach (var d in data) valueList.Add(new KeyValuePair<string, int>(d.Key.ToString(), d.Count()));
            if (valueList.Count() > 5)
                valueList = valueList.Take(5).Reverse().ToList();
            return valueList;
        }

        private void ShowAccessBarChart(List<KeyValuePair<string, int>> valueList)
        {
            ClearChart();

            var barSeries = new BarSeries()
            {
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Title = "Кол-во запросов"
            };
            chart.Series.Add(barSeries);
            chart.Title = "Популярность каталогов";
        }

        private List<KeyValuePair<string, int>> PrepareErrorColumnChart(IEnumerable<ErrorLog> log)
        {
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();
            var tmp = TrimErrorDate(log);
            var data = tmp.AsParallel().Where(n => n.ClientIp!=string.Empty).GroupBy(n => n.ClientIp).OrderByDescending(n => n.Count());
            foreach (var d in data) valueList.Add(new KeyValuePair<string, int>(d.Key, d.Count()));
            if (valueList.Count() > 5)
                valueList = valueList.Take(5).ToList();
            return valueList;
        }

        private void ShowErrorColumnChart(List<KeyValuePair<string, int>> valueList)
        {
            ClearChart();

            var chartSeries = new ColumnSeries()
            {
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Title = "Кол-во ошибок"
            };
            chart.Series.Add(chartSeries);
            chart.Title = "Ошибки пользователей";
        }

        private List<KeyValuePair<string, int>> PrepareErrorPieChart(IEnumerable<ErrorLog> log)
        {
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();
            var tmp = TrimErrorDate(log);
            var data = tmp.AsParallel().GroupBy(n => n.ErrorType).OrderBy(n => n.Key);
            foreach (var d in data) valueList.Add(new KeyValuePair<string, int>(d.Key, d.Count()));
            return valueList;
        }

        private void ShowErrorPieChart(List<KeyValuePair<string, int>> valueList)
        {
            ClearChart();

            var pieSeries = new PieSeries()
            {
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Title = "Тип ошибки"
            };
            chart.Series.Add(pieSeries);
            chart.Title = "Типы ошибок";
        }

        private List<KeyValuePair<string, int>> PrepareErrorAreaChart(IEnumerable<ErrorLog> log)
        {
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < 24; i++)
            {
                valueList.Add(new KeyValuePair<string, int>(i.ToString(), 0));
            }
            var tmp = TrimErrorDate(log);
            var data = tmp.AsParallel().GroupBy(n => n.DateTime.Hour);
            foreach (var d in data)
            {
                valueList.Remove(new KeyValuePair<string, int>(d.Key.ToString(), 0));
                valueList.Add(new KeyValuePair<string, int>(d.Key.ToString(), d.Count()));
            }
            return valueList.OrderBy(n => System.Convert.ToInt32(n.Key)).ToList();
        }

        private void ShowErrorAreaChart(List<KeyValuePair<string, int>> valueList)
        {
            ClearChart();

            var areaSeries = new AreaSeries()
            {
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = valueList,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Title = "Кол-во ошибок"
            };
            chart.Series.Add(areaSeries);
            chart.Title = "Ошибки по времени суток";
        }

        private void BackgroundAccessWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AccessDirectoryParser accessdirparser = (AccessDirectoryParser)e.Argument;
            var result = accessdirparser.Parse(parser.AccessFolderPath);
            e.Result = result;
            parser.Accesslog = (IEnumerable<AccessLog>)e.Result;
            parser.CountTotalAccessEntries();
            parser.CountUniqueAccessEntries();
            parser.CountAvgAccessSize();
            parser.CountAccessSession();
            parser.CountNotFoundError();

            parser.AccessDateFrom = parser.Accesslog.First().DateTime;
            parser.AccessDateTo = parser.Accesslog.Last().DateTime;
        }

        private void BackgroundAccessWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            accessDateFrom.Content = parser.AccessDateFrom;
            accessDateTo.Content = parser.AccessDateTo;
            totalAccessEntries.Content = parser.TotalAccessEntries;
            uniqueAccessEntries.Content = parser.UniqueAccessEntries;
            avgAccessSize.Content = parser.AvgAccessSize;
            avgAccessSession.Content = parser.AvgAccessSession;
            notFoundErrorCount.Content = parser.NotFoundErrorCount;

            accessDateFromPicker.DisplayDateStart = parser.AccessDateFrom;
            accessDateFromPicker.DisplayDateEnd = parser.AccessDateTo;
            accessDateFromPicker.DisplayDate = parser.AccessDateFrom;

            accessDateToPicker.DisplayDateStart = parser.AccessDateFrom;
            accessDateToPicker.DisplayDateEnd = parser.AccessDateTo;
            accessDateToPicker.DisplayDate = parser.AccessDateTo;

            ProgressBar.Visibility = Visibility.Collapsed;
            accessAnalysis.IsEnabled = true;
            buildAccessChart.IsEnabled = true;
            accessExpander.IsEnabled = true;
            accessRadio.IsEnabled = true;
        }

        private void BackgroundErrorWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ErrorDirectoryParser errordirparser = (ErrorDirectoryParser)e.Argument;
            var result = errordirparser.Parse(parser.ErrorFolderPath);
            e.Result = result;
            parser.Errorlog = (IEnumerable<ErrorLog>)e.Result;
            parser.CountTotalErrorEntries();
            parser.CountSortedErrors();
            parser.CountTopErrorClient();

            parser.ErrorDateFrom = parser.Errorlog.First().DateTime;
            parser.ErrorDateTo = parser.Errorlog.Last().DateTime;

        }

        private void BackgroundErrorWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            errorDateFrom.Content = parser.ErrorDateFrom;
            errorDateTo.Content = parser.ErrorDateTo;
            totalErrorEntries.Content = parser.TotalErrorEntries;
            sortedErrors.Content = parser.SortedErrors;
            topErrorClient.Content = parser.TopErrorClient;

            errorDateFromPicker.DisplayDateStart = parser.ErrorDateFrom;
            errorDateFromPicker.DisplayDateEnd = parser.ErrorDateTo;
            errorDateFromPicker.DisplayDate = parser.ErrorDateFrom;

            errorDateToPicker.DisplayDateStart = parser.ErrorDateFrom;
            errorDateToPicker.DisplayDateEnd = parser.ErrorDateTo;
            errorDateToPicker.DisplayDate = parser.ErrorDateTo;

            ProgressBar.Visibility = Visibility.Collapsed;
            errorAnalysis.IsEnabled = true;
            buildErrorChart.IsEnabled = true;
            errorExpander.IsEnabled = true;
            errorRadio.IsEnabled = true;
        }

        private void BackgroundAccessChartWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int selectedItem = (int)e.Argument;
            switch (selectedItem)
            {
                case 0:
                    e.Result = PrepareAccessColumnChart(parser.Accesslog);
                    break;
                case 1:
                    e.Result = PrepareAccessPieChart(parser.Accesslog);
                    break;
                case 2:
                    e.Result = PrepareAccessAreaChart(parser.Accesslog);
                    break;
                case 3:
                    e.Result = PrepareAccessBarChart(parser.Accesslog);
                    break;
                default: break;
            }
        }

        private void BackgroundAccessChartWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<KeyValuePair<string, int>> valueList = (List<KeyValuePair<string, int>>)e.Result;
            int selectedItem = accessChartList.SelectedIndex;
            switch (selectedItem)
            {
                case 0:
                    ShowAccessColumnChart(valueList);
                    break;
                case 1:
                    ShowAccessPieChart(valueList);
                    break;
                case 2:
                    ShowAccessAreaChart(valueList);
                    break;
                case 3:
                    ShowAccessBarChart(valueList);
                    break;
                default: break;
            }
            buildAccessChart.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void BackgroundErrorCharWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int selectedItem = (int)e.Argument;
            switch (selectedItem)
            {
                case 0:
                    e.Result = PrepareErrorColumnChart(parser.Errorlog);
                    break;
                case 1:
                    e.Result = PrepareErrorPieChart(parser.Errorlog);
                    break;
                case 2:
                    e.Result = PrepareErrorAreaChart(parser.Errorlog);
                    break;
                default: break;
            }

        }

        private void BackgroundErrorCharWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<KeyValuePair<string, int>> valueList = (List<KeyValuePair<string, int>>)e.Result;
            int selectedItem = errorChartList.SelectedIndex;
            switch (selectedItem)
            {
                case 0:
                    ShowErrorColumnChart(valueList);
                    break;
                case 1:
                    ShowErrorPieChart(valueList);
                    break;
                case 2:
                    ShowErrorAreaChart(valueList);
                    break;
                default: break;
            }
            buildErrorChart.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void BackgroundMapWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> ipList = new List<string>();
            List<GeoData> pointsList = new List<GeoData>();
            
            string selectedItem = (string)e.Argument;
            switch (selectedItem)
            {
                case "access.log":
                    var tmp = TrimAccessDate(parser.Accesslog);
                    ipList.AddRange(tmp.GroupBy(x => x.ClientIp).Select(x => x.Key));
                    break;
                case "error.log":
                    var tmp1 = TrimErrorDate(parser.Errorlog);
                    ipList.AddRange(parser.Errorlog.GroupBy(x => x.ClientIp).Select(x => x.Key));
                    break;
                default:break;
            }

            ipList = ipList.Distinct().Where(x => x != String.Empty).ToList();
            

            foreach (var ip in ipList)
            {
                var geo = Parser.GetCoords(ip);
                if (geo.Latitude != 0.0 && geo.Longitude != 0.0)
                    pointsList.Add(geo);
            }
            e.Result = pointsList;
        }

        private void BackgroundMapWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<GeoData> pointsList = (List<GeoData>)e.Result;
            List<ScatterDataPoint> geoList = new List<ScatterDataPoint>();

            ClearChart();

            foreach (var point in pointsList)
            {
                geoList.Add(new ScatterDataPoint()
                {          
                    Tag = point,
                    DependentValue = point.Latitude,
                    IndependentValue = point.Longitude,
                });
            }            

            LinearAxis xA = new LinearAxis()
            {
                Orientation = AxisOrientation.X,
                Minimum = -180,
                Maximum = 180
            };
            LinearAxis yA = new LinearAxis()
            {
                Orientation = AxisOrientation.Y,
                Minimum = -90,
                Maximum = 90
            };
            chart.Axes.Clear();
            chart.Axes.Add(xA);
            chart.Axes.Add(yA);

            var scatterSeries = new ScatterSeries()
            {
                DependentValuePath = "DependentValue",
                IndependentValuePath = "IndependentValue",
                ItemsSource = geoList
            };

            Style newDataPointStyle = (Style)Resources["NewDataPointStyle"];
            scatterSeries.DataPointStyle = newDataPointStyle;

            Style st = new Style { TargetType = typeof(Legend), BasedOn = chart.LegendStyle };
            st.Setters.Add(new Setter(WidthProperty, 0.0));
            chart.LegendStyle = st;

            ImageBrush ib = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/map.png")))
            {
                Stretch = Stretch.Fill
            };
            Style st1 = new Style { TargetType = typeof(Grid), BasedOn = chart.PlotAreaStyle };
            st1.Setters.Add(new Setter(BackgroundProperty, ib));
            chart.PlotAreaStyle = st1;  
            
            chart.Title = "Карта активности";
            chart.Series.Add(scatterSeries);
            
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void ValidateAccessDate()
        {
            if (parser.AccessDateFrom > parser.AccessDateTo)
            {
                var tmp = parser.AccessDateTo;
                parser.AccessDateTo = parser.AccessDateFrom;
                parser.AccessDateFrom = tmp;
            }
            accessDateFromPicker.SelectedDate = parser.AccessDateFrom;
            accessDateToPicker.SelectedDate = parser.AccessDateTo;
        }

        private void ValidateErrorDate()
        {
            if (parser.ErrorDateFrom > parser.ErrorDateTo)
            {
                var tmp = parser.ErrorDateTo;
                parser.ErrorDateTo = parser.ErrorDateFrom;
                parser.ErrorDateFrom = tmp;
            }
            errorDateFromPicker.SelectedDate = parser.ErrorDateFrom;
            errorDateToPicker.SelectedDate = parser.ErrorDateTo;
        }

        private void ClearChart()
        {
            Style style = new Style { TargetType = typeof(Grid), BasedOn = chart.PlotAreaStyle };
            style.Setters.Add(new Setter(BackgroundProperty, null));
            chart.PlotAreaStyle = style;

            Style st = new Style { TargetType = typeof(Legend), BasedOn = chart.LegendStyle };
            st.Setters.Add(new Setter(WidthProperty, double.NaN));
            chart.LegendStyle = st;

            chart.Axes.Clear();
            chart.Series.Clear();

        }

        private IEnumerable<AccessLog> TrimAccessDate(IEnumerable<AccessLog> log)
        {
            if (log.First().DateTime == parser.AccessDateFrom && log.Last().DateTime == parser.AccessDateTo)
                return log;
            else return log.Where(x => x.DateTime >= parser.AccessDateFrom && x.DateTime <= parser.AccessDateTo).Select(x => x);
        }

        private IEnumerable<ErrorLog> TrimErrorDate(IEnumerable<ErrorLog> log)
        {
            if (log.First().DateTime == parser.ErrorDateFrom && log.Last().DateTime == parser.ErrorDateTo)
                return log;
            else return log.Where(x => x.DateTime >= parser.ErrorDateFrom && x.DateTime <= parser.ErrorDateTo).Select(x => x);
        }

        public static void SerializeToXAML(UIElement element, string filename)
        {
            string strXAML = System.Windows.Markup.XamlWriter.Save(element);

            using (FileStream fs = File.Create(filename))
            {
                using (StreamWriter streamwriter = new StreamWriter(fs))
                {
                    streamwriter.Write(strXAML);
                }
            }
        }

        public static UIElement DeSerializeXAML(string filename)
        {
            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                return System.Windows.Markup.XamlReader.Load(fs) as UIElement;
            }
        }
    }
}
