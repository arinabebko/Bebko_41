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
    /// 


    public partial class ProductPage : Page
    {
        private User currentUser;
        string FIOO;
        private int newOrderID = 1;
        private List<Product> savedSelectedProducts = new List<Product>();




        private List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();

        public ProductPage(User user)
        {

            InitializeComponent();
            OrderBtn.Visibility = Visibility.Collapsed;
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
                FIOO = FOITB.Text;
            }



            var currentProducts = Bebko_41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProducts;

            ComboType.SelectedIndex = 0;
            Updt();
        }

        private int GetNextOrderId()
        {
            return Bebko_41Entities.GetContext().Order.Any()
                ? Bebko_41Entities.GetContext().Order.Max(o => o.OrderID) + 1
                : 1;
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


            currentProducts = currentProducts.Where(p => (p.ProductName.ToLower().Contains(TBSearch.Text.ToLower()))).ToList();


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



        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedIndex >= 0)
            {

                var selectedProducts = ProductListView.SelectedItems.Cast<Product>().ToList();
                selectedProducts = selectedProducts.Distinct().ToList();
                var prod = ProductListView.SelectedItem as Product;
                //    selectedProducts.Add(prod);
                savedSelectedProducts = selectedProducts;
                var newOrderProd = new OrderProduct();

                newOrderProd.OrderID = newOrderID;

                newOrderProd.ProductArticleNumber = prod.ProductArticleNumber;
                newOrderProd.Quantity = 1;
                //  selectedOrderProducts.Add(newOrderProd); // Добавьте в список

                var selOP = selectedOrderProducts.Where(p => Equals(p.ProductArticleNumber, prod.ProductArticleNumber));
                if (selOP.Count() == 0)
                {
                    selectedOrderProducts.Add(newOrderProd);
                }
                else
                {
                    foreach (OrderProduct p in selectedOrderProducts)
                    {
                        if (p.ProductArticleNumber == prod.ProductArticleNumber)
                            p.Quantity++;
                    }
                }

                OrderBtn.Visibility = Visibility.Visible;
                ProductListView.SelectedIndex = -1;

            }

        }
    

        private void OrderBtn_Click(object sender, RoutedEventArgs e )
        {
            newOrderID = GetNextOrderId();
            OrderWindow orderWindow = new OrderWindow(selectedOrderProducts, savedSelectedProducts, FIOO, newOrderID);
            // orderWindow.ShowDialog();
            if (orderWindow.ShowDialog() == true)
            {
                // Если заказ был сохранен, обновляем OrderID и списки
                selectedOrderProducts = orderWindow.UpdatedSelectedOrderProducts;
                savedSelectedProducts = orderWindow.UpdatedSelectedProducts;
            }
            if (selectedOrderProducts.Any())
            {
                OrderBtn.Visibility = Visibility.Visible;
            }
            else
            {
                OrderBtn.Visibility = Visibility.Collapsed;
            }
        }
      


    }
}
