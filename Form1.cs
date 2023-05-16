using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Game
{
    public partial class Form1 : Form
    {
        private List<Hasla> HaslaList, FilteredList;
        string outputFilePath = "serializedHasla.xml";
        List<char> ignoreLetters = new List<char>();
        private Hasla selectedHaslo;
        private Random random;
        private int errors = 0;
        private int wins, losts;
        List<PictureBox> pictureBoxes;
        Image imageX = Image.FromFile("Images/x.png");
        Image imageO = Image.FromFile("Images/o.png");
        int[,] board;
        public Form1()
        {

            HaslaList = GetHaslaFromXML();
            FilteredList = HaslaList;
            InitializeComponent();
            this.Width = 1600;
            this.Height = 800;
            this.BackgroundImage = Image.FromFile("Images/wisielec.jpg");
            comboBox1.DataSource = Enum.GetValues(typeof(Dificulty));
            SetRandomHaslo();
            InitializeOX();
        }


        private void InitializeOX()
        {
            pictureBoxes = new List<PictureBox>
            {
                pictureBox1,
                pictureBox2,
                pictureBox3,
                pictureBox4,
                pictureBox5,
                pictureBox6,
                pictureBox7,
                pictureBox8,
                pictureBox9
            };
            foreach (var item in pictureBoxes)
            {
                item.Image = null;
                item.Tag = "free";
            }
            random = new Random();
            var AIStart = random.Next(2) == 1 ? true : false;
            board = new int[3, 3];
            //var AIStart = true;
            if (AIStart)
            {
                AITurn();
            }
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var selectedPictureBox = sender as PictureBox;
            if (selectedPictureBox.Tag.ToString() != "selected")
            {
                selectedPictureBox.Image = imageO;
                var row = (pictureBoxes.IndexOf(selectedPictureBox) / 3);
                var col = (pictureBoxes.IndexOf(selectedPictureBox) %3);
                board[row, col] = 1; // kolko
                selectedPictureBox.Tag = "selected";
                if (CheckWin())
                {
                    MessageBox.Show("Wygrales!");
                    InitializeOX();
                    return;
                }
                AITurn();
            }
        }
        private void AITurn()
        {
            if (pictureBoxes.Where(c => c.Tag.ToString() != "selected").ToList().Count > 0)
            {
            selectedPicture:
                random = new Random();
                var selectedPictureBox = pictureBoxes[random.Next(pictureBoxes.Count)];
                if (selectedPictureBox.Tag.ToString() != "selected")
                {
                    selectedPictureBox.Image = imageX;
                    var row = (pictureBoxes.IndexOf(selectedPictureBox) / 3);
                    var col = (pictureBoxes.IndexOf(selectedPictureBox) % 3);
                    board[row, col] = 2; // krzyzyk
                    selectedPictureBox.Tag = "selected";
                }
                else
                {
                    goto selectedPicture;
                }
            }
            if (CheckWin())
            {
                MessageBox.Show("AI wygralo!");
                InitializeOX();
            }
        }

        private bool CheckWin()
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2] && board[i, 0] != 0)
                {
                    return true;
                }
            }
            // Sprawdzanie kolumn
            for (int i = 0; i < 3; i++)
            {
                if ((board[0, i] == board[1, i]) && (board[1, i] == board[2, i]) && board[0, i] != 0)
                {
                    return true;
                }
            }

            // Sprawdzanie przekątnych
            if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2] && board[0, 0] != 0)
            {
                return true;
            }

            if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0] && board[0, 2] != 0)
            {
                return true;
            }

            return false;

        }
        private List<Hasla> GetHaslaFromXML()
        {
            List<Hasla> haslaList;
            try
            {
                using (StreamReader sr = new StreamReader(outputFilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Hasla>));
                    haslaList = (List<Hasla>)serializer.Deserialize(sr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Błąd podczas deserializacji:");
                Console.WriteLine(e.Message);
                return null;
            }
            return haslaList;

        }
        private (string haslo, string hashedHaslo) GetHashedHaslo(string haslo, List<char> ignoreLetters = null)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in haslo)
            {
                if (c == ' ' || ignoreLetters.Contains(c))
                {
                    sb.Append(c);
                    sb.Append(' ');
                }
                else
                {
                    sb.Append('_');
                    sb.Append(' ');
                }
            }
            return (haslo, sb.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetWrittenLetterAndCheckAll();
        }

        private void SetRandomHaslo()
        {
            ignoreLetters.Clear();
            var haslo = GetRandomHaslo();
            selectedHaslo = haslo;
            ignoreLetters.Add(haslo.Haslo[0]);
            wisielecPictureBox.Image = null;
            Haslo.Text = GetHashedHaslo(haslo.Haslo, ignoreLetters).hashedHaslo;
            category.Text = selectedHaslo.Kategoria.ToString();
            errors = 0;
            string trueValue = Haslo.Text.Replace(" ", "");
            progressBar1.Maximum = trueValue.Length;
        }
        private Hasla GetRandomHaslo()
        {
            random = new Random();
            if (FilteredList.Count == 0)
            {
                throw new InvalidOperationException("HaslaList is empty.");
            }

            int randomIndex = random.Next(FilteredList.Count);
            return FilteredList[randomIndex];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetRandomHaslo();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void letterTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GetWrittenLetterAndCheckAll();
            }
        }

        private void GetWrittenLetterAndCheckAll()
        {
            if (letterTextBox.Text.Length > 0)
            {
                CheckLetter(letterTextBox.Text);
                Haslo.Text = GetHashedHaslo(selectedHaslo.Haslo, ignoreLetters).hashedHaslo;
                if (!Haslo.Text.Contains("_"))
                {
                    wins++;
                    win.Text = $"Wygrane: {wins}";
                    errors = 0;
                    selectedHaslo = null;
                    ignoreLetters.Clear();
                    progressBar1.Value = 0;

                    SetRandomHaslo();
                }
                letterTextBox.Text = "";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InitializeOX();
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (selectedHaslo != null)
            {
                SetRandomHaslo();
                var filtredList = HaslaList.Where(c => c.Dificulty == (Dificulty)comboBox1.SelectedValue).ToList();
                FilteredList = filtredList;
            }

        }

        private void CheckLetter(string letter)
        {
            if (letter.Length > 0 && selectedHaslo.Haslo.Contains(letter[0]))
            {
                if (!ignoreLetters.Contains(letter[0])) ignoreLetters.Add(letter[0]);
                string trueValue = Haslo.Text.Replace("_", "").Replace(" ", "");
                progressBar1.Value = trueValue.Length;
            }
            else
            {
                if (errors < 5)
                {
                    wisielecPictureBox.Image = Image.FromFile($"Images/{errors + 1}.png");
                    errors++;

                }
                else
                {
                    losts++;
                    lost.Text = $"przegrane: {losts}";
                    errors = 0;
                    selectedHaslo = null;
                    ignoreLetters.Clear();
                    SetRandomHaslo();
                    MessageBox.Show("Przegrałeś!");
                    progressBar1.Value = 0;
                }
            }
        }
    }
}
