using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//using AForge.Video;
//using AForge.Video.DirectShow;
using System.Drawing;
//using System.Drawing.Imaging;
using System.Windows.Interop;

namespace IntegProgFinals_ArtGallery_ArtView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataClasses1DataContext dbConn = null;
        private RegisteredUser currentUser = null;
        private BitmapImage ArtistpfpImage = new BitmapImage();
        private string ArtistpfpPath = @"C:\ArtistProfilePic\";

        //FilterInfoCollection fic = null;
        //VideoCaptureDevice vcd = null;



        public MainWindow()
        {
            InitializeComponent();
            dbConn = new DataClasses1DataContext(Properties.Settings.Default.ArtGalleryFinalsProjectConnectionString);
            ArtistpfpPath = @"C:\ArtistProfilePic\";
            InitializeDefaultImage();
        }

        #region LOGIN-PAGE
        private void Username_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Password_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Username.Text) && !string.IsNullOrWhiteSpace(Password.Text))
            {
                try
                {
                    var query = from user in dbConn.RegisteredUsers
                                where user.User_Username == Username.Text
                                select user;

                    if (query.Any())
                    {
                        var userMember = query.First();

                        if (userMember.User_Password == Password.Text)
                        {
                            currentUser = userMember;
                            string userRole = currentUser.AccountRole_ID;

                            if (userRole == "AR1")
                            {
                                MessageBox.Show("Welcome Admin");
                                LogInPage.Visibility = Visibility.Collapsed;
                                Username.Clear();
                                Password.Clear();
                            }
                            else if (userRole == "AR2")
                            {
                                MessageBox.Show("Welcome Artist");
                                LogInPage.Visibility = Visibility.Collapsed;
                                ArtistView.Visibility = Visibility.Visible;
                                ArtistHomePage.Visibility = Visibility.Visible;
                                Sidebar.Visibility = Visibility.Visible;
                                Username.Clear();
                                Password.Clear();
                                LoadUserProfile();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Incorrect password. Please try again.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Username not found. Please try again or create a new user.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please enter both username and password.");
            }
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            LogInPage.Visibility = Visibility.Collapsed;
            SignUpPage.Visibility = Visibility.Visible;
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
        #endregion

        #region SIGN-UP-PAGE
        private void DisplayNameTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void UsernNameTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void PasswordTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BioTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void GenderCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ConfirmNewUser_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(DisplayNameTxtBox.Text) && !string.IsNullOrWhiteSpace(UsernNameTxtBox.Text) &&
                !string.IsNullOrWhiteSpace(BioTxtBox.Text) && !string.IsNullOrWhiteSpace(PasswordTxtBox.Text))
            {

                if (dbConn.RegisteredUsers.Any(s => s.User_Username == UsernNameTxtBox.Text))
                {
                    MessageBox.Show("Username already exists. Please choose a different username.");
                    return;
                }

                string UserID = GenerateUserId();
                char userSex = ' ';

                if (GenderCB.SelectedItem != null)
                {
                    string selectedItem = GenderCB.Text;

                    if (!string.IsNullOrEmpty(selectedItem))
                    {
                        userSex = selectedItem[0];
                    }
                }

                RegisteredUser newUser = new RegisteredUser
                {
                    U_ID = UserID,
                    User_DisplayName = DisplayNameTxtBox.Text,
                    User_Username = UsernNameTxtBox.Text,
                    User_Password = PasswordTxtBox.Text,
                    User_Bio = BioTxtBox.Text,
                    AccountRole_ID = "AR2",
                    User_Sex = userSex,
                    UserStatus_ID = "US1",
                };

                dbConn.RegisteredUsers.InsertOnSubmit(newUser);

                try
                {
                    dbConn.SubmitChanges();
                    MessageBox.Show("New user created successfully");
                    DisplayNameTxtBox.Clear();
                    UsernNameTxtBox.Clear();
                    PasswordTxtBox.Clear();
                    BioTxtBox.Clear();

                }
                catch
                {
                    MessageBox.Show("Error creating new user");
                }
            }
            else
            {
                MessageBox.Show("Please fill in all required fields.");
            }
        }
        private string GenerateUserId()
        {
            int existingUsersCount = dbConn.RegisteredUsers.Count();

            string UserID = "U" + (existingUsersCount + 1).ToString("D2");

            return UserID;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            SignUpPage.Visibility = Visibility.Collapsed;
            LogInPage.Visibility = Visibility.Visible;
            DisplayNameTxtBox.Clear();
            UsernNameTxtBox.Clear();
            BioTxtBox.Clear();
            PasswordTxtBox.Clear();
            GenderCB.SelectedIndex = -1;
        }


        #endregion

        #region ArtistFunctions

        #region ArtistProfileCode
        private void ArtistProfile_Click(object sender, RoutedEventArgs e)
        {
            ArtistUploadsPage.Visibility = Visibility.Collapsed;
            ArtistProfilePage.Visibility = Visibility.Visible;
            ArtistHomePage.Visibility = Visibility.Collapsed;

            if (IsUserLoggedIn())
            {
                PopulateProfilePage(CurrentLoggedInUser());
            }
        }
        private void PopulateProfilePage(RegisteredUser user)
        {
            lblUserID.Content = "UserID: " + user.U_ID;
            lblUserDisplayName.Content = "Name: " + user.User_DisplayName;
            lblUserSex.Content = "Email: " + user.User_Sex;
            lblUserStatus.Content = "Contact No: " + user.UserStatus_ID;
            lblUserDate.Content = "Joined" + user.User_Date;
            lblUserBio.Content = user.User_Bio;
        }
        private bool IsUserLoggedIn()
        {
            return currentUser != null;
        }
        private RegisteredUser CurrentLoggedInUser()
        {
            return currentUser;
        }
        private void AlterInfo_Click(object sender, RoutedEventArgs e)
        {
            Sidebar.Visibility = Visibility.Collapsed;
            ArtistProfilePage.Visibility = Visibility.Collapsed;
            UpdateInfoPage.Visibility = Visibility.Visible;
            PopulateUpdateInfoPage();
        }

        private void ArtistPfp_Click(object sender, RoutedEventArgs e)
        {
            Sidebar.Visibility = Visibility.Collapsed;
            ArtistProfilePage.Visibility = Visibility.Collapsed;
            UploadArtistPfp.Visibility = Visibility.Visible;
        }
        #endregion

        #region ArtistSideBarFunctions
        private void ArtistUploads_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ArtistHome_Click(object sender, RoutedEventArgs e)
        {
            ArtistHomePage.Visibility = Visibility.Visible;
            ArtistProfilePage.Visibility = Visibility.Collapsed;
            ArtistUploadsPage.Visibility = Visibility.Collapsed;
        }

        private void ArtistLogout_Click(object sender, RoutedEventArgs e)
        {
            ArtistView.Visibility = Visibility.Collapsed;
            LogInPage.Visibility = Visibility.Visible;
            ArtistProfilePage.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region UpdateArtistInformation
        private void PopulateUpdateInfoPage()
        {
            if (currentUser != null)
            {
                NewDisplayNameTxtBox.Text = currentUser.User_DisplayName;
                NewUsernNameTxtBox.Text = currentUser.User_Username;
                NewBioTxtBox.Text = currentUser.User_Bio;
                NewPasswordTxtBox.Text = currentUser.User_Password;

                if (currentUser.User_Sex == 'M')
                {
                    NewGenderCB.SelectedIndex = 0;
                }
                else if (currentUser.User_Sex == 'F')
                {
                    NewGenderCB.SelectedIndex = 1;
                }
            }
        }
        private bool IsUsernameAvailable(string username)
        {
            var existingUser = dbConn.RegisteredUsers.FirstOrDefault(u => u.User_Username == username);
            return existingUser == null;
        }
        private void NewDisplayNameTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NewUsernNameTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NewBioTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NewPasswordTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void ConfirmNewUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                string newUsername = NewUsernNameTxtBox.Text.Trim();

                if (!IsUsernameAvailable(newUsername))
                {
                    MessageBox.Show("Username already exists. Please choose a different username.");
                    return;
                }

                currentUser.User_DisplayName = NewDisplayNameTxtBox.Text;
                currentUser.User_Username = newUsername;
                currentUser.User_Bio = NewBioTxtBox.Text;
                currentUser.User_Password = NewPasswordTxtBox.Text;

                if (NewGenderCB.SelectedItem != null)
                {
                    ComboBoxItem selectedGender = (ComboBoxItem)NewGenderCB.SelectedItem;
                    currentUser.User_Sex = selectedGender.Content.ToString()[0];
                }

                try
                {
                    dbConn.SubmitChanges();
                    MessageBox.Show("User information updated successfully!");
                    UpdateInfoPage.Visibility = Visibility.Collapsed;
                    ArtistProfilePage.Visibility = Visibility.Visible;
                    Sidebar.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No user is currently logged in.");
            }
        }


        private void UpdateCancel_Click(object sender, RoutedEventArgs e)
        {
            NewDisplayNameTxtBox.Clear();
            NewUsernNameTxtBox.Clear();
            NewBioTxtBox.Clear();
            NewPasswordTxtBox.Clear();
            UpdateInfoPage.Visibility = Visibility.Collapsed;
            ArtistProfilePage.Visibility = Visibility.Visible;
            Sidebar.Visibility = Visibility.Visible;
        }
        #endregion


        //--------------//
        private void InitializeDefaultImage()
        {
            if (ArtistpfpImage != null)
                return;

            ArtistpfpImage.BeginInit();
            ArtistpfpImage.UriSource = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/DefaultImage.png"));
            ArtistpfpImage.CacheOption = BitmapCacheOption.OnLoad;
            ArtistpfpImage.EndInit();
            imgPreview.Source = ArtistpfpImage;
        }
        private void btnBrowsePicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (openFileDialog.ShowDialog() == true)
            {
                txtPicturePath.Text = openFileDialog.FileName;


                ArtistpfpImage = new BitmapImage();
                ArtistpfpImage.BeginInit();
                ArtistpfpImage.UriSource = new Uri(openFileDialog.FileName);
                ArtistpfpImage.CacheOption = BitmapCacheOption.OnLoad;
                ArtistpfpImage.EndInit();
                imgPreview.Source = ArtistpfpImage;
            }
        }
        private void ConfirmArtistPfp_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                if (!string.IsNullOrWhiteSpace(txtPicturePath.Text))
                {
                    string fileName = System.IO.Path.GetFileName(txtPicturePath.Text);
                    string destPath = System.IO.Path.Combine(ArtistpfpPath, fileName);
                    try
                    {
                        File.Copy(txtPicturePath.Text, destPath, true);
                        currentUser.User_pfpFilePath = destPath;

                        dbConn.SubmitChanges();
                        MessageBox.Show("Profile picture updated successfully!");
                        UpdateProfilePicture();
                        UploadArtistPfp.Visibility = Visibility.Collapsed;
                        ArtistProfilePage.Visibility = Visibility.Visible;
                        Sidebar.Visibility = Visibility.Visible;
                        ArtistpfpImage = new BitmapImage();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a picture to upload.");
                }
            }
            else
            {
                MessageBox.Show("No user is currently logged in.");
            }
        }
        private void UpdateProfilePicture()
        {
            if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.User_pfpFilePath))
            {
                BitmapImage profileImage = new BitmapImage();
                profileImage.BeginInit();
                profileImage.UriSource = new Uri(currentUser.User_pfpFilePath);
                profileImage.CacheOption = BitmapCacheOption.OnLoad;
                profileImage.EndInit();
                ArtistProfilePicture.Source = profileImage;
            }
        }
        private void LoadUserProfile()
        {


            if (currentUser != null)
            {
                lblUserID.Content = currentUser.U_ID.ToString();
                lblUserDisplayName.Content = currentUser.User_DisplayName;
                lblUserSex.Content = currentUser.User_Sex;
                lblUserStatus.Content = currentUser.User_Sex;
                lblUserDate.Content = currentUser.User_Date.ToString();
                lblUserBio.Content = currentUser.User_Bio;

                UpdateProfilePicture();
            }
        }
        private void CancelArtistPfp_Click(object sender, RoutedEventArgs e)
        {
            txtPicturePath.Clear();
            InitializeDefaultImage();
            UploadArtistPfp.Visibility = Visibility.Collapsed;
            ArtistProfilePage.Visibility = Visibility.Visible;
            Sidebar.Visibility = Visibility.Visible;
        }
        #endregion

        private void UploadArt_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Uploader Functions
        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ConfirmFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Picture_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Camera Logic
        /// <summary>
        /// This method will start the video camera of the selected device
        /// </summary>

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //    vcd = new VideoCaptureDevice(fic[cmbCamera.SelectedIndex].MonikerString);
            //    vcd.NewFrame += Vcd_NewFrame;
            //    vcd.Start();
        }

        /// <summary>
        /// This updates the content of the display source
        /// In our case the Image box with frames coming from the camerasource
        /// </summary>

        //private void Vcd_NewFrame(object sender, NewFrameEventArgs eventArgs)
        //{
        //    pic.Source = eventArgs.Frame.Clone();

        //    This line allows the event to modify the content of the image box
        //     without this block of code, the thread holding the Image box and the thread
        //     holding the image will not talk to each other
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        /*
        //         * This convoluted piece of code will convert the bitmap image of the video
        //         * source (Your selected webcam) into something the Imagebox can load.
        //         * I suggest not to change this section of the code.
        //         */
        //        pic.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //                ((Bitmap)eventArgs.Frame.Clone()).GetHbitmap(),
        //                IntPtr.Zero,
        //                System.Windows.Int32Rect.Empty,
        //                BitmapSizeOptions.FromWidthAndHeight((int)pic.Width, (int)pic.Height));
        //});

        //}



        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //foreach (FilterInfo fi in fic)
            //    cmbCamera.Items.Add(fi.Name);
            //cmbCamera.SelectedIndex = 0;
            //vcd = new VideoCaptureDevice();
        }

        /// <summary>
        /// This method will convert the image displayed in the Image box into a PNG file
        /// </summary>
        /// <param name="filePath"></param>
        public void ImageToFile(string filePath)
        {
            var image = pic.Source;
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image));
                encoder.Save(fileStream);
            }
        }

        /// <summary>
        /// This event, upon clicking will stop the webcam from running
        /// Save the file in the desired location
        /// </summary>

        private void btnCaptureImage_Click(object sender, RoutedEventArgs e)
        {
            //if (vcd.IsRunning)
            //{
            //    ImageToFile("Test.png");
            //    vcd.WaitForStop();
            //    vcd = null;
            //    //vcd.Stop();asddww
            //}


            //Denzel commented this out
            //ImageToFile("Test.png");
            //this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (vcd.IsRunning)
            //{
            //    //ImageToFile("Test.png");
            //    //vcd.WaitForStop();
            //    vcd.SignalToStop(); // tells camera to stop working
            //    //vcd.WaitForStop();
            //    vcd = null;

            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();

            //}
        }
        #endregion

        private void ArtView_Click(object sender, RoutedEventArgs e)
        {
            WIndowManager.aw = new ArtViewWindow();
            WIndowManager.aw.Show();
        }
    }
}
