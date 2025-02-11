using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bebko_41
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        string correct;
        public AuthPage()
        {
            InitializeComponent();
            captchaSP.Visibility = Visibility.Collapsed;
         
        }

        private void BtnEnterGuest_Click(object sender, RoutedEventArgs e)
        {
            string login = TBlogin.Text;
            string password = TBparol.Text;
            User user = Bebko_41Entities.GetContext().User.ToList().Find(p => p.UserLogin == login && p.UserPassword == password);

            Manager.MainFrame.Navigate(new ProductPage(user=null));
            userscaptcha.Clear();
            rand();
        }



        private void rand()
        {
            string symbolsForCaptcha = "andhfjl1728738ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            // Генерируем случайные символы для капчи
            Random random = new Random();

            captchaOne.Text = symbolsForCaptcha[random.Next(symbolsForCaptcha.Length)].ToString();
            captchaTwo.Text = symbolsForCaptcha[random.Next(symbolsForCaptcha.Length)].ToString();
            captchaThree.Text = symbolsForCaptcha[random.Next(symbolsForCaptcha.Length)].ToString();

            captchaFour.Text = symbolsForCaptcha[random.Next(symbolsForCaptcha.Length)].ToString();
            correct = captchaOne.Text + captchaTwo.Text + captchaThree.Text + captchaFour.Text;
            userscaptcha.Clear();
        }


        private async void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            string login = TBlogin.Text;
            string password = TBparol.Text;
            int f;
            f = 0;
            userscaptcha.Clear();
            if (login =="" || password=="")
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }
            if (f == 1)
            {
                string userSequence = userscaptcha.Text;

                if (userSequence == correct)// Проверка на совпадение с правильной последовательностью 
                {
                    MessageBox.Show("Капча введена правильно!");
                    // Дальнейшие действия при успешном прохождении капчи...

                   
                    captchaSP.Visibility = Visibility.Collapsed;
                    userscaptcha.Clear();
                }
                else
                {
                    MessageBox.Show("Попробуйте еще раз.");
                    userscaptcha.Clear();
                    rand();

                    //   BtnEnte.await Task.Delay(10000);

                    // Очистка текстовых полей и повторная генерация новой последовательности символом может быть полезна здесь.
                }
            }

            User user = Bebko_41Entities.GetContext().User.ToList().Find(p => p.UserLogin == login && p.UserPassword == password);
            if (user != null)
            {
                Manager.MainFrame.Navigate(new ProductPage(user));
                TBlogin.Text = "";
                TBparol.Text = "";
                captchaSP.Visibility = Visibility.Collapsed;

            }
            else
            {
                MessageBox.Show("ошибка авторизации, введите капчу");
                BtnEnter.IsEnabled = false;
                f = 1;

                //  System.Threading.Thread.Sleep(10000);
                //Capcha capchaWindow = new Capcha(); // Создаем экземпляр окна капчи
                //capchaWindow.ShowDialog(); // Открываем окно в режиме диалога



                rand();


                captchaSP.Visibility = Visibility.Visible;




                await Task.Delay(10000);

                BtnEnter.IsEnabled = true;

            }

        }
        }

       
    
}

