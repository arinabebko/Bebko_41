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
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private User currentUser;
        public ProductPage(User user)
        {
            InitializeComponent();
            if (user == null)
            {
                FOITB.Text = "Гость";
                RoleTB.Text = "Посетитель";

            }
            else
            {
                FOITB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
                switch (user.UserRole)
                {
                    case 1:
                        RoleTB.Text = "Администратор"; break;
                    case 2:
                        RoleTB.Text = "Клиент"; break;
                    case 3:
                        RoleTB.Text = "Менеджер"; break;
                }

            }



            var currentProducts = Bebko_41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProducts;

            ComboType.SelectedIndex = 0;
            Updt();
        }


        private void Updt()
        {
            var currentProducts = Bebko_41Entities.GetContext().Product.ToList();



            if (ComboType.SelectedIndex == 0)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 0 && Convert.ToInt32(p.ProductDiscountAmount) <= 100)).ToList();
            }
            if (ComboType.SelectedIndex == 1)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 0 && Convert.ToInt32(p.ProductDiscountAmount) < 10)).ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 10 && Convert.ToInt32(p.ProductDiscountAmount) < 15)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 15)).ToList();
            }


            currentProducts=currentProducts.Where(p => (p.ProductName.ToLower().Contains(TBSearch.Text.ToLower()))).ToList();


            ProductListView.ItemsSource = currentProducts.ToList();


            if (RButtonDown.IsChecked.Value)
                ProductListView.ItemsSource = currentProducts.OrderByDescending(p => p.ProductCost).ToList();
            if (RButtonUp.IsChecked.Value)
                ProductListView.ItemsSource = currentProducts.OrderBy(p => p.ProductCost).ToList();
            UpdateCountDisplay(currentProducts.Count);
        }
        int totalCount;

        private void UpdateCountDisplay(int filteredCount)
        {

            int totalCount = Bebko_41Entities.GetContext().Product.Count(); // Общее количество записей в базе
            CountText.Text = $"Количество: {filteredCount} из {totalCount}";
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Updt();

        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Updt();
        }

  
        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            Updt();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            Updt();
        }
    }
}
