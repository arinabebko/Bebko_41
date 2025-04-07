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
        private Order currentOrder = new Order();
        private OrderProduct currentProduct = new OrderProduct();
        private string FIOO; // Объявление переменной
        public List<OrderProduct> UpdatedSelectedOrderProducts { get; set; }
        public List<Product> UpdatedSelectedProducts { get; set; }

        private int newOrderID; // Объявление переменной

        private int existingOrderID;

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO, int orderID)
        {
            InitializeComponent();
            this.FIOO = FIO;
            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;
            this.existingOrderID = orderID;
            UpdateOrderInfo();
            var pickUpPoints = Bebko_41Entities.GetContext().PickUpPoint.ToList();
            PickUpCombo.ItemsSource = pickUpPoints;
         
            ShoeListView.ItemsSource = selectedProducts;
            // Остальной код конструктора
            ClientTB.Text = FIOO; // Если используете ClientTB
            OrderDP.Text = DateTime.Now.ToString("dd.MM.yyyy"); // Формат даты: день.месяц.год
            OrderNumber.Text = existingOrderID.ToString();
        }

        private void SaveOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            //  existingOrderID = newOrderID;
            try
            {
                if (PickUpCombo.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите пункт выдачи.");
                    return;
                }
                var order = new Order
                {
                    OrderID = existingOrderID,
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
                        OrderID = existingOrderID,
                        ProductArticleNumber = op.ProductArticleNumber,
                        Quantity = op.Quantity
                    };

                    Bebko_41Entities.GetContext().OrderProduct.Add(orderProduct);
                }

                Bebko_41Entities.GetContext().SaveChanges();

                // existingOrderID = newOrderID;
                this.DialogResult = true;
                this.UpdatedSelectedOrderProducts = selectedOrderProducts;
                this.UpdatedSelectedProducts = selectedProducts;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}");
            }


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




        private void UpdateOrderInfo()
        {
            decimal totalCost = 0;
            decimal totalDiscount = 0;
            ShoeListView.ItemsSource = selectedOrderProducts;
            foreach (var op in selectedOrderProducts)
            {
                // Ищем продукт в selectedProducts
                var product = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == op.ProductArticleNumber);

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



        private void DelProd_Click_1(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = (Product)button.DataContext;

            Console.WriteLine($"Удаляем продукт: {product.ProductName}");

            var existing = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (existing != null)
            {
                selectedOrderProducts.Remove(existing);
            }

            // Удалите продукт из selectedProducts
            var productToRemove = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == product.ProductArticleNumber);
            if (productToRemove != null)
            {
                selectedProducts.Remove(productToRemove);
            }

            if (!selectedOrderProducts.Any())
            {
                // Обработка случая без товаров
            }

            UpdateOrderInfo();
            //  ShoeListView.ItemsSource = null;
            ShoeListView.ItemsSource = selectedProducts;
            ShoeListView.ItemsSource = selectedOrderProducts; // Set the correct source

            ShoeListView.Items.Refresh(); // Refresh the ListView

        }

        private void IncQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = (Product)button.DataContext;

            var existing = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (existing != null)
            {
                existing.Quantity++;
            }

            UpdateOrderInfo();
            ShoeListView.ItemsSource = null;
            ShoeListView.ItemsSource = selectedProducts;
            ShoeListView.Items.Refresh();
        }

        private void DecQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = (Product)button.DataContext;

            var existing = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (existing != null && existing.Quantity > 1)
            {
                existing.Quantity--;
            }
            else if (existing != null && existing.Quantity == 1)
            {
                selectedOrderProducts.Remove(existing);
            }

            UpdateOrderInfo();
            ShoeListView.ItemsSource = null;
            ShoeListView.ItemsSource = selectedProducts;
            ShoeListView.Items.Refresh();
        }

        private void PickUpCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                if (PickUpCombo.SelectedItem != null)
                {
                    var selectedPickup = PickUpCombo.SelectedItem as PickUpPoint;
                    // Сохраните выбранный пункт выдачи для заказа
                    currentOrder.OrderPickupPoint = selectedPickup.PickUpPointID; // Предполагается, что у вас есть свойство PickUpPointID
                }
            }
        }

    }
}
