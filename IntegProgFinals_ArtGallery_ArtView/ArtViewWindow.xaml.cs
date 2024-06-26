using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
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

namespace IntegProgFinals_ArtGallery_ArtView
{
    /// <summary>
    /// Interaction logic for ArtViewWindow.xaml
    /// </summary>
    public partial class ArtViewWindow : Window
    {
        DataClasses1DataContext _dbConn = null;

        public ArtViewWindow()
        {
            InitializeComponent();

            ArtStartView();
        }

        public void ArtStartView()
        {
            ClearArtView();

            _dbConn = new DataClasses1DataContext(Properties.Settings.Default.ArtGalleryFinalsProjectConnectionString);

            var arttbl = from s in _dbConn.GetTable<Art>()
                         select s;
            var userstbl = from s in _dbConn.GetTable<RegisteredUser>()
                           select s;
            var extbl = from s in _dbConn.GetTable<Exhibition>()
                        select s;

            #region FILLING IN LIST BOX WITH ART
            foreach (var art in arttbl)
            {
                foreach (var user in userstbl)
                {
                    if (art.U_ID == user.U_ID)
                    {
                        ArtListBox.Items.Add(art.Art_Title + "|" + user.User_DisplayName);
                        break;
                    }
                }
            }
            #endregion

            #region FILLING IN COMBO BOX WITH EXHIBITIONS
            ExhibitionComboBx.Items.Add("All");
            foreach (var ex in extbl)
            {
                ExhibitionComboBx.Items.Add(ex.Exhibition_Name);
            }

            #endregion
        }

        private void ClearArtView()
        {
            ArtListBox.Items.Clear();
            ExhibitionComboBx.Items.Clear();
            ArtListBox.SelectedIndex = -1;
            ArtTitleLbl.Content = "";
            ArtistLbl.Content = "Artist:";
            ArtDescTxtBlck.Text = "Description:";
            ExhibitionLbl.Content = "Exhibition:";
            ArtStatusLbl.Content = "Status:";
            ArtDateLbl.Content = "Date:";
            ArtPriceLbl.Content = "Price:";
        }
        private void ExhibitionDescBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ExhibitionComboBx.SelectedIndex == -1)
            {
                MessageBox.Show("Click on this button to learn more about the exhibition selected", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var exhibitions = from s in _dbConn.GetTable<Exhibition>()
                                  select s;

                if (ExhibitionComboBx.SelectedItem.ToString() == "All")
                {
                    MessageBox.Show("Show all art regardless of exhibition", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                foreach (var ex in exhibitions)
                {
                    if (ExhibitionComboBx.SelectedItem.ToString() == ex.Exhibition_Name)
                    {
                        MessageBox.Show(ex.Exhibition_Desc, "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        private void ArtistBioBtn_Click(object sender, RoutedEventArgs e)
        {
            var userstbl = from s in _dbConn.GetTable<RegisteredUser>()
                           select s;

            if (ArtListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Click on this button to learn more about the labeled artist", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                string[] item = ArtListBox.SelectedItem.ToString().Split('|');
                foreach (var user in userstbl)
                {
                    if (user.User_DisplayName == item[1])
                    {
                        MessageBox.Show(user.User_Bio, "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        private void ArtListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ArtListBox.SelectedIndex != -1)
            {
                string[] item = ArtListBox.SelectedItem.ToString().Split('|');

                var userstbl = from s in _dbConn.GetTable<RegisteredUser>()
                               select s;

                var arttbl = from s in _dbConn.GetTable<Art>()
                             select s;

                var extbl = from s in _dbConn.GetTable<Exhibition>()
                            select s;

                var astbl = from s in _dbConn.GetTable<ArtStatus>()
                            select s;

                foreach (var art in arttbl)
                {
                    foreach (var user in userstbl)
                    {
                        if (art.Art_Title == item[0].ToString() && art.U_ID == user.U_ID)
                        {
                            ArtTitleLbl.Content = art.Art_Title;
                            ArtistLbl.Content = "Artist: " + user.User_DisplayName;
                            ArtDescTxtBlck.Text = "Description: " + art.Art_Desc;
                            foreach (var ex in extbl)
                            {
                                if (art.Exhibition_ID == ex.Exhibition_ID)
                                {
                                    ExhibitionLbl.Content = "Exhibition: " + ex.Exhibition_Name;
                                }
                            }
                            foreach (var ast in astbl)
                            {
                                if (art.ArtStatus_ID == ast.ArtStatus_ID)
                                {
                                    ArtStatusLbl.Content = "Status: " + ast.ArtStatus_Desc;
                                }
                            }
                            ArtDateLbl.Content = "Date: " + art.Art_Date;
                            ArtPriceLbl.Content = "Price: " + art.Art_Price;
                        }
                    }
                }
            }
        }

        private void UpdateArtListBox()
        {
            ArtListBox.Items.Clear();

            var arttbl = from s in _dbConn.GetTable<Art>()
                         select s;

            var extbl = from s in _dbConn.GetTable<Exhibition>()
                        select s;

            var astbl = from s in _dbConn.GetTable<ArtStatus>()
                        select s;

            var userstbl = from s in _dbConn.GetTable<RegisteredUser>()
                           select s;

            bool filtercheck = true;
            bool founduser = false;
            int index = 0;

            if ((ExhibitionComboBx.SelectedIndex == -1 || ExhibitionComboBx.SelectedItem.ToString() == "All") && (ArtViewSearchBr.Text == "" || ArtViewSearchBr.Text == null)
                && ShowArtForSaleCheckBox.IsChecked == false)
            {
                foreach (var art in arttbl)
                {
                    foreach (var user in userstbl)
                    {
                        if (art.U_ID == user.U_ID)
                        {
                            ArtListBox.Items.Add(art.Art_Title + "|" + user.User_DisplayName);
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (var art in arttbl)
                {
                    filtercheck = true;

                    #region FILTERING EXHIBITION
                    if (ExhibitionComboBx.SelectedItem != null)
                    {
                        if(ExhibitionComboBx.SelectedItem.ToString() != "All")
                        {
                            foreach (var ex in extbl)
                            {
                                if (art.Exhibition_ID == ex.Exhibition_ID)
                                {
                                    if (ex.Exhibition_Name != ExhibitionComboBx.SelectedItem.ToString())
                                    {
                                        filtercheck = false;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    //THIS REGION MAY CHANGE DEPENDING ON HOW DATA IS STRUCTURED(FOR SALE ID = AS1)
                    #region FILTERING ART FOR SALE
                    if (filtercheck)
                    {
                        foreach (var ast in astbl)
                        {
                            if (ShowArtForSaleCheckBox.IsChecked == true)
                            {
                                if (art.ArtStatus_ID != "AS1")
                                {
                                    filtercheck = false;
                                }
                            }
                        }
                    }
                    #endregion

                    #region FILTERING ART NAME AND ARTIST NAME
                    if (filtercheck)
                    {
                        index = 0;
                        if (ArtViewSearchBr.Text != "" || ArtViewSearchBr.Text != null)
                        {
                            foreach (char c in ArtViewSearchBr.Text)
                            {
                                if (index > art.Art_Title.Length - 1)
                                {
                                    filtercheck = false;
                                    break;
                                }
                                if (c.ToString().ToUpper() != art.Art_Title[index].ToString().ToUpper())
                                {
                                    filtercheck = false;
                                    break;
                                }
                                index++;
                            }
                            if(!filtercheck)
                            {
                                founduser = false;
                                foreach(var user in userstbl)
                                {
                                    index = 0;
                                    if(art.U_ID ==  user.U_ID)
                                    {
                                        founduser = true;
                                        foreach (char c in ArtViewSearchBr.Text)
                                        {
                                            if (index > user.User_DisplayName.Length - 1)
                                            {
                                                filtercheck = false;
                                                break;
                                            }
                                            if (c.ToString().ToUpper() != user.User_DisplayName[index].ToString().ToUpper())
                                            {
                                                filtercheck = false;
                                                break;
                                            }
                                            index++;
                                        }
                                    }
                                    if (founduser)
                                    {
                                        founduser = false;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    #endregion

                    if (filtercheck)
                    {
                        foreach (var user in userstbl)
                        {
                            if (art.U_ID == user.U_ID)
                            {
                                ArtListBox.Items.Add(art.Art_Title + "|" + user.User_DisplayName);
                                break;
                            }
                        }
                    }

                }
            }
        }

        private void ExhibitionComboBx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateArtListBox();
        }

        private void ArtViewSearchBr_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateArtListBox();
        }

        private void ShowArtForSaleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateArtListBox();
        }

        private void ShowArtForSaleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateArtListBox();
        }
    }
}
