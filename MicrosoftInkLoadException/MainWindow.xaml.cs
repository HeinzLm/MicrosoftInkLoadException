using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Ink;

namespace MicrosoftInkLoadException
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();

        public MainWindow()
        {
            InitializeComponent();
            LbNotes.ItemsSource = Notes;
            (Canvas.Strokes as INotifyCollectionChanged).CollectionChanged += delegate
            {
                InkToText();
            };
        }


        private void InkToText()
        {
            //the strokes have changed
            using (MemoryStream ms = new MemoryStream())
            {
                new StrokeCollection(Canvas.Strokes).Save(ms);
                var tempInk = new Microsoft.Ink.Ink();
                tempInk.Load(ms.ToArray());

                using (Microsoft.Ink.RecognizerContext myRecoContext = new Microsoft.Ink.RecognizerContext())
                {
                    Microsoft.Ink.RecognitionStatus status;
                    myRecoContext.Strokes = tempInk.Strokes;
                    if (tempInk.Strokes.Count == 0)
                    {
                        TbInkText.Text = string.Empty;
                        return;
                    }
                    var recoResult = myRecoContext.Recognize(out status);

                    if (status == Microsoft.Ink.RecognitionStatus.NoError)
                    {
                        TbInkText.Text = recoResult.TopString;
                        tempInk.Strokes.Clear();
                    }
                    else
                    {
                        MessageBox.Show("ERROR: " + status.ToString());
                    }
                }
                tempInk.Dispose();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            using (MemoryStream file = new MemoryStream())
            {
                Canvas.Strokes.Save(file);
                file.Close();
                Notes.Add(new Note
                {
                    NoteData = file.ToArray(),
                    Text = "Note " + Notes.Count+1
                });
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            if(LbNotes.SelectedItem != null)
            {
                Canvas.Strokes.Clear();

                var selectedNote = LbNotes.SelectedItem as Note;
                using (MemoryStream ms = new MemoryStream(selectedNote.NoteData))
                {

                    Canvas.Strokes = new StrokeCollection(ms);
                    (Canvas.Strokes as INotifyCollectionChanged).CollectionChanged += delegate
                    {
                        InkToText();
                    };
                    ms.Close();
                    InkToText();
                }
            }
        }
    }

    public class Note
    {
        public string Text { get; set; }
        public byte[] NoteData { get; set; }
    }
}
