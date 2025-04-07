using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace Bebko_41
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<Product> savedSelectedProducts;

        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        List<Product> selectedProducts = new List<Product>();
        private Order currentOrder=new Order();
        private OrderProduct currentProduct=new OrderProduct();
        private string FIOO; // Объявление переменной


        private int newOrderID; // Объявление переменной



        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO)
        {
            InitializeComponent();
            this.FIOO = FIO; // Инициализация
            newOrderID = GenerateNewOrderID();
            this.savedSelectedProducts = selectedProducts;
            UpdateOrderInfo();
            //     ShoeListView.ItemsSource = selectedProducts;


            var currentPickups = Bebko_41Entities.GetContext().PickUpPoint.ToList();
            PickUpCombo.ItemsSource = currentPickups;
            PickUpCombo.DisplayMemberPath = "PickUpPointStreet";

            PickUpCombo.ItemsSource = currentPickups;
          

            if (FIO != null)
            {
                ClientTB.Text = FIO;
            }
            else
            {
                ClientTB.Text = "Вы зашли как гость";
            }
            //OrderIDTB.Text=selectedOrderProducts.First().OrderID.ToString();
            //
            if (selectedOrderProducts.Any())
            {
                OrderIDTB.Text = selectedOrderProducts.First().OrderID.ToString();
            }
            else
            {
                // Обработка случая без товаров
            }

            ShoeListView.ItemsSource = selectedProducts;
            
            foreach(Product p in selectedProducts)
            {
                p.Quantity = 1;
                foreach (OrderProduct q in selectedOrderProducts)
                {
                    if (p.ProductArticleNumber == q.ProductArticleNumber)
                        p.Quantity = q.Quantity;
                }
            }

            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;
          OrderDP.Text = DateTime.Now.ToString();
         SetDeliveryDate();


            

        }
        private int GenerateNewOrderID()
        {
            return Bebko_41Entities.GetContext().Order.Any()
                ? Bebko_41Entities.GetContext().Order.Max(o => o.OrderID) + 1
                : 1;
        }
        private void SetDeliveryDate()
        {
            DateTime now = DateTime.Now;
            if (DateDeliverDP.SelectedDate < now)
            {
                MessageBox.Show("Дата доставки не может быть раньше текущей даты.");
            }
        }

        private void DateDeliverDP_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDeliveryDate();
        }

     

        private void DelProd_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = (Product)button.DataContext;

            var existing = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (existing != null)
            {
                selectedOrderProducts.Remove(existing);
            }

            if (!selectedOrderProducts.Any())
            {
                // Обработка случая без товаров
            }

            UpdateOrderInfo();
            ShoeListView.ItemsSource = null;
            ShoeListView.ItemsSource = selectedProducts;
        }
        private void UpdateOrderInfo()
{
    decimal totalCost = 0;
    decimal totalDiscount = 0;

    foreach (var op in selectedOrderProducts)
    {
        var product = savedSelectedProducts.FirstOrDefault(p => p.ProductArticleNumber == op.ProductArticleNumber);

        if (product != null)
        {
            Console.WriteLine($"Товар: {product.ProductName}, Количество: {op.Quantity}, Стоимость: {product.ProductCost}, Скидка: {product.ProductDiscountAmount}");
            totalCost += product.ProductCost * op.Quantity;
            totalDiscount += (product.ProductCost * op.Quantity * (product.ProductDiscountAmount ?? 0) / 100);
        }
        else
        {
            Console.WriteLine("Товар не найден");
        }
    }

    Console.WriteLine($"Общая сумма: {totalCost}, Общая скидка: {totalDiscount}");

    TotalCostTB.Text = $"Сумма: {totalCost} рублей";
    TotalDiscountTB.Text = $"Скидка: {totalDiscount} рублей";
}

        private void SaveOrderBtn_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (PickUpCombo.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите пункт выдачи.");
                    return;
                }
                var order = new Order
                {
                    OrderID = newOrderID,
                    OrderDate = DateTime.Now,
                    OrderDeliveryDate = DateDeliverDP.SelectedDate ?? DateTime.Now.AddDays(3), // Установите дату доставки
                    OrderPickupPoint = (int)PickUpCombo.SelectedValue, // Установите пункт выдачи
                    OrderStatus = "Новый"
                };

                if (!string.IsNullOrEmpty(FIOO))
                {
                    // Если клиент авторизован, установите его ID
                    var client = Bebko_41Entities.GetContext().User.FirstOrDefault(u => u.UserSurname + " " + u.UserName + " " + u.UserPatronymic == FIOO);
                    if (client != null)
                    {
                        order.OrderClientID = client.UserID;
                    }
                }

                Bebko_41Entities.GetContext().Order.Add(order);
                Bebko_41Entities.GetContext().SaveChanges();

                foreach (var op in selectedOrderProducts)
                {
                    var orderProduct = new OrderProduct
                    {
                        OrderID = newOrderID,
                        ProductArticleNumber = op.ProductArticleNumber,
                        Quantity = op.Quantity
                    };

                    Bebko_41Entities.GetContext().OrderProduct.Add(orderProduct);
                }

                Bebko_41Entities.GetContext().SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}");
            }
        }

        private void DelProd_Click_1(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = (Product)button.DataContext;

            var existing = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (existing != null)
            {
                selectedOrderProducts.Remove(existing);
            }

            if (!selectedOrderProducts.Any())
            {
                // Скрыть кнопку просмотра заказа
            }

            UpdateOrderInfo();
            ShoeListView.ItemsSource = null;
            ShoeListView.ItemsSource = selectedProducts;
        }
    }

}
