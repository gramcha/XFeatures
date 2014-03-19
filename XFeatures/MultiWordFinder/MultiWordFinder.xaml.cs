using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using XFeatures.Helpers;
using System.Windows.Threading;
using XFeatures.Settings;
using System.Text.RegularExpressions;

namespace XFeatures.MultiWordFinder
{
    /// <summary>
    /// Interaction logic for MultiWordFinder.xaml
    /// </summary>
    public partial class MultiWordFinder : UserControl, INotifyPropertyChanged
    {
        FinderDataset dataset;
        private ICommand _addCommand;
        public ICommand Addcommand
        {
            get
            {

                return _addCommand;
            }
            set
            {
                _addCommand = value;
                OnPropertyChanged("Addcommand");
            }
        }
        
        public MultiWordFinder()
        {
            InitializeComponent();
            this.DataContext = this;
            Addcommand = new RelayCommand(AddItem);
            InitControls();
        }
        //~MultiWordFinder()
        //{
        //    CacheDataset();
        //}
        void InitControls()
        {
            dataset = SettingsProvider.GetFinderDataset();
            switch (dataset.LookIn)
            {
                case Lookin.AllOpenDocs:
                    allopeneddocs.IsChecked = true;
                    break;
                case Lookin.CurrentDoc:
                    curdoc.IsChecked = true;
                    break;
                case Lookin.CurrentProj:
                    project.IsChecked = true;
                    break;
                case Lookin.EntireSoln:
                    solution.IsChecked = true;
                    break;
            }
            matchcase.IsChecked = dataset.MatchCase;
            matchwholeword.IsChecked = dataset.MatchWholeWord;
            filetype.Text = dataset.FileTypes;
            switch(dataset.ResultWnd)
            {
                case ResultWindow.One:
                    findresultwnd1.IsChecked = true;
                    break;
                case ResultWindow.Two:
                    findresultwnd2.IsChecked = true;
                    break;
            }
            displayfilenameonly.IsChecked = dataset.DisplayFileNamesOnly;
            foreach (var item in dataset.FindWhat)
            {
                findlist.Items.Add(item);
            }
        }
        void CacheDataset()
        {
            if(allopeneddocs.IsChecked == true)
            {
                dataset.LookIn = Lookin.AllOpenDocs;
            }
            else if(curdoc.IsChecked == true)
            {
                dataset.LookIn = Lookin.CurrentDoc;
            }
            else if(project.IsChecked == true)
            {
                dataset.LookIn =Lookin.CurrentProj;
            }
            else if(solution.IsChecked == true)
            {
                dataset.LookIn =Lookin.EntireSoln;
            }
            dataset.MatchCase=matchcase.IsChecked??true;
            dataset.MatchWholeWord=matchwholeword.IsChecked??true;
            dataset.FileTypes=filetype.Text;
            if(findresultwnd1.IsChecked == true)
            {
                dataset.ResultWnd = ResultWindow.One;
            }
            else
            {
                dataset.ResultWnd = ResultWindow.Two;
            }
            dataset.FindWhat.Clear();
            foreach (var item in findlist.Items)
            {
                dataset.FindWhat.Add(item.ToString());
            }
            SettingsProvider.WriteFinderDataSet(dataset);
        }
        private void addbtn_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }
        private void AddItem()
        {
            if (string.IsNullOrEmpty(inputword.Text) || string.IsNullOrWhiteSpace(inputword.Text))
                return;
            findlist.Items.Add(inputword.Text);
            inputword.Text = "";
        }
        private void removebtn_Click(object sender, RoutedEventArgs e)
        {
            int i = findlist.SelectedIndex;
            if (i > -1)
            {
                findlist.Items.RemoveAt(i);
                if (findlist.Items.Count > i)
                {
                    findlist.SelectedIndex = i;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }

        private void findbtn_Click(object sender, RoutedEventArgs e)
        {
            DteExtensions.DTE.Find.FindWhat = GenerateQuery();
            DteExtensions.DTE.Find.PatternSyntax = EnvDTE.vsFindPatternSyntax.vsFindPatternSyntaxRegExpr;
            DteExtensions.DTE.Find.SearchPath = GetSearchPath();
            DteExtensions.DTE.Find.FilesOfType = filetype.Text;
            DteExtensions.DTE.Find.ResultsLocation = GetResultLocation();
            DteExtensions.DTE.Find.Action = EnvDTE.vsFindAction.vsFindActionFindAll;
            DteExtensions.DTE.Find.MatchCase = matchcase.IsChecked ?? true;
            DteExtensions.DTE.Find.MatchWholeWord = matchwholeword.IsChecked ?? true;
            DteExtensions.DTE.Find.MatchInHiddenText = true;
            DteExtensions.DTE.Find.SearchSubfolders = true;
            //DteExtensions.DTE.Find.
            //StudioUtility.StartMulltitextFinder();
            DteExtensions.DTE.Find.Execute();
            //StudioUtility.EndMulltitextFinder();
            CacheDataset();
            CloseWindow();
            DteExtensions.DTE.Find.PatternSyntax = EnvDTE.vsFindPatternSyntax.vsFindPatternSyntaxLiteral;
            //DteExtensions.DTE.ExecuteCommand("Edit.FindinFiles");
        }
        //class FindSet
        //{
        //    public string FindWhat {get;set;}
        //    EnvDTE.vsFindPatternSyntax PatternSyntax {get;set;}
        //    public string SearchPath  {get;set;}
        //    public string FilesOfType  {get;set;}
        //    public string ResultsLocation  {get;set;}
        //    public EnvDTE.vsFindAction Action  {get;set;}
        //    public bool MatchCase {get;set;}
        //    public bool MatchWholeWord {get;set;}
        //    public bool MatchInHiddenText {get;set;}
        //    public bool SearchSubfolders { get; set; }

        //}
        string GetSearchPath()
        {
            if (allopeneddocs.IsChecked == true)
            {
                return "All Open Documents";
            }
            else if (curdoc.IsChecked == true)
            {
                return "Current Document";
            }
            else if (project.IsChecked == true)
            {
                return "Current Project";
            }
            else if (solution.IsChecked == true)
            {
                return "Entire Solution";
            }
            return "Entire Solution";
        }
        EnvDTE.vsFindResultsLocation GetResultLocation()
        {
            if(findresultwnd1.IsChecked == true)
            {
                return EnvDTE.vsFindResultsLocation.vsFindResults1;
            }
            else
            {
                return EnvDTE.vsFindResultsLocation.vsFindResults2;
            }
        }
        void CloseWindow()
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Close();
        }
        string GenerateQuery()
        {
            string result="",temp="";
            int i=0;
            foreach (var item in findlist.Items)
            {
                if(i!=0)
                {
                    result = result + "|";
                }
                temp = Regex.Escape(item.ToString());
                temp = temp.Replace("<","\\<");
                temp = temp.Replace(">", "\\>");
                temp = temp.Replace(":", "\\:");
                temp = temp.Replace("@", "\\@");
                temp = temp.Replace("~", "\\~");
                result += "(" + temp + ")";
                i = 1;
            }
            return result + "|(mgurlatmictheaxt)";
        }

        private void inputword_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate()
                {
                    inputword.Focus();
                }));
            } 
        }

        private void removeallbtn_Click(object sender, RoutedEventArgs e)
        {
            findlist.Items.Clear();
        }
    }
}
