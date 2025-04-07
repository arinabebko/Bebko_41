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
using System.Data.Entity; // Добавьте в начале файла
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

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO, int orderID, DateTime? precalculatedDeliveryDate = null)
        {

            InitializeComponent();
            if (precalculatedDeliveryDate.HasValue)
            {
                DateDeliverDP.SelectedDate = precalculatedDeliveryDate.Value;
            }
            else
            {
                SetDeliveryDate(); // Рассчитываем, если дата не передана
            }
            this.FIOO = FIO;
            this.existingOrderID = orderID;

            // Связываем OrderProduct с Product
            foreach (var op in selectedOrderProducts)
            {
                op.Product = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == op.ProductArticleNumber);
            }

            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;

            UpdateOrderInfo();

            // Загрузка пунктов выдачи
            //var pickUpPoints = Bebko_41Entities.GetContext().PickUpPoint.ToList();
            using (var context = new Bebko_41Entities())
            {
                var pickUpPoints = context.PickUpPoint.ToList();
                PickUpCombo.ItemsSource = pickUpPoints;
            }
          //  PickUpCombo.ItemsSource = pickUpPoints;

            // Установка источника данных для ListView
            ShoeListView.ItemsSource = selectedOrderProducts;

            // Заполнение информации о заказе
            ClientTB.Text = FIOO;
            OrderDP.SelectedDate = DateTime.Now;
            OrderNumber.Text = existingOrderID.ToString();

            // Если нужно загрузить существующие товары заказа из БД
            using (var context = new Bebko_41Entities())
            {
                var orderProductsFromDb = context.OrderProduct
                    .Where(op => op.OrderID == orderID)
                    .ToList();

                // Можно добавить их в selectedOrderProducts, если нужно
            }
        }







        private void SaveOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrderProducts.Count == 0 || OrderDP.SelectedDate == null ||
                DateDeliverDP.SelectedDate == null || PickUpCombo.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            try
            {
                using (var context = new Bebko_41Entities())
                {
                    // Создаем новый заказ
                    var newOrder = new Order
                    {
                        OrderID = GenerateNewOrderID(),
                        OrderDate = OrderDP.SelectedDate.Value,
                        OrderDeliveryDate = DateDeliverDP.SelectedDate.Value,
                        OrderPickupPoint = (PickUpCombo.SelectedItem as PickUpPoint).PickUpPointID,
                        OrderStatus = "Новый"
                    };

                    // Добавляем товары в заказ
                    foreach (var op in selectedOrderProducts)
                    {
                        // Создаем новый OrderProduct для текущего контекста
                        var newOrderProduct = new OrderProduct
                        {
                            OrderID = newOrder.OrderID,
                            ProductArticleNumber = op.ProductArticleNumber,
                            Quantity = op.Quantity
                        };
                        newOrder.OrderProduct.Add(newOrderProduct);
                    }

                    context.Order.Add(newOrder);
                    context.SaveChanges();
                }

                MessageBox.Show("Заказ сохранен!");
                selectedOrderProducts.Clear();
                selectedProducts.Clear();
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }




        private int GenerateNewOrderID()
        {
            using (var context = new Bebko_41Entities())
            {
                return context.Order.Any()
                    ? context.Order.Max(o => o.OrderID) + 1
                    : 1;
            }
        }
        private bool _isDateBeingSet = false; // Флаг для отслеживания


 


        private DateTime? recommendedDeliveryDate; // Добавьте это поле в начало класса

        private void SetDeliveryDate()
        {
            if (_isDateBeingSet || selectedOrderProducts.Count == 0)
                return;

            _isDateBeingSet = true;

            // Проверяем, все ли товары есть в наличии (количество > 0)
            bool allInStock = selectedOrderProducts.All(op =>
                op.Product != null && op.Product.ProductQuantityInStock >= op.Quantity);

            // Если все в наличии и более 3 позиций - 3 дня, иначе - 6
            int deliveryDays = (allInStock && selectedOrderProducts.Count > 3) ? 3 : 6;

            recommendedDeliveryDate = DateTime.Now.AddDays(deliveryDays);
            DateDeliverDP.SelectedDate = recommendedDeliveryDate;

            _isDateBeingSet = false;
        }

        private void DateDeliverDP_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Если дата изменяется программно - игнорируем
            if (_isDateBeingSet) return;

            // Если дата не выбрана - ничего не делаем
            if (DateDeliverDP.SelectedDate == null) return;

            // Проверка, что дата не раньше текущей
            if (DateDeliverDP.SelectedDate < DateTime.Now)
            {
                // Устанавливаем флаг, чтобы избежать рекурсии
                _isDateBeingSet = true;

                MessageBox.Show("Дата доставки не может быть раньше текущей!");
                DateDeliverDP.SelectedDate = DateTime.Now; // Корректируем на текущую дату

                // Снимаем флаг после изменения
                _isDateBeingSet = false;
                return;
            }

            // Проверка, что дата не раньше рекомендованной (только предупреждение)
            if (recommendedDeliveryDate.HasValue && DateDeliverDP.SelectedDate < recommendedDeliveryDate)
            {
                MessageBox.Show($"Рекомендуемая дата доставки: {recommendedDeliveryDate.Value.ToShortDateString()}.\n" +
                              "Вы выбрали более раннюю дату, что может привести к задержке.",
                              "Внимание",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }


        private void UpdateOrderInfo()
        {
            decimal total = 0;
            decimal discount = 0;

            foreach (var op in selectedOrderProducts)
            {
                var product = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == op.ProductArticleNumber);
                if (product != null)
                {
                    decimal sum = product.ProductCost * op.Quantity;
                    total += sum;
                    discount += sum * (product.ProductDiscountAmount ?? 0) / 100;
                }
            }

            TotalCostTB.Text = $"Сумма: {total:0.00} руб.";
            TotalDiscountTB.Text = $"Скидка: {discount:0.00} руб.";
            ShoeListView.Items.Refresh();
        }



        private void DelProd_Click_1(object sender, RoutedEventArgs e)
        {
            var orderProduct = (OrderProduct)((Button)sender).DataContext;
            selectedOrderProducts.Remove(orderProduct);
            selectedProducts.RemoveAll(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);
            UpdateOrderInfo();
            SetDeliveryDate();
        }

        private void IncQuantity_Click(object sender, RoutedEventArgs e)
        {
            var orderProduct = (OrderProduct)((Button)sender).DataContext;
            orderProduct.Quantity++;
            UpdateOrderInfo();
            SetDeliveryDate();
        }
        private void DecQuantity_Click(object sender, RoutedEventArgs e)
        {
            var orderProduct = (OrderProduct)((Button)sender).DataContext;
            if (orderProduct.Quantity > 1)
            {
                orderProduct.Quantity--;
            }
            else
            {
                selectedOrderProducts.Remove(orderProduct);
                selectedProducts.RemoveAll(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);
            }
            UpdateOrderInfo();
            SetDeliveryDate();
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
