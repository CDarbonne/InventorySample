#region copyright
// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Inventory.Data;
using Inventory.Models;
using Inventory.Services;

namespace Inventory.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public DashboardViewModel(ICustomerService customerService, IOrderService orderService, IProductService productService, ICommonServices commonServices) : base(commonServices)
        {
            CustomerService = customerService;
            OrderService = orderService;
            ProductService = productService;
        }

        public ICustomerService CustomerService { get; }
        public IOrderService OrderService { get; }
        public IProductService ProductService { get; }

        private IList<CustomerModel> _customers = null;
        public IList<CustomerModel> Customers
        {
            get => _customers;
            set => Set(ref _customers, value);
        }

        //New list created to store top customers ordered list
        private IList<CustomerModel> _customersTop = null;
        public IList<CustomerModel> CustomersTop
        {
            get => _customersTop;
            set => Set(ref _customersTop, value);
        }

        private IList<ProductModel> _products = null;
        public IList<ProductModel> Products
        {
            get => _products;
            set => Set(ref _products, value);
        }

        private IList<OrderModel> _orders = null;
        public IList<OrderModel> Orders
        {
            get => _orders;
            set => Set(ref _orders, value);
        }

        public async Task LoadAsync()
        {
            StartStatusMessage("Loading dashboard...");
            await LoadCustomersAsync();
            await LoadTopCustomersAsync(); //new await async call for top customers
            await LoadOrdersAsync();
            await LoadProductsAsync();
            EndStatusMessage("Dashboard loaded");
        }
        public void Unload()
        {
            Customers = null;
            CustomersTop = null; //new null setting
            Products = null;
            Orders = null;
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                var request = new DataRequest<Customer>
                {
                    OrderByDesc = r => r.CreatedOn
                };
                Customers = await CustomerService.GetCustomersAsync(0, 5, request);
            }
            catch (Exception ex)
            {
                LogException("Dashboard", "Load Customers", ex);
            }
        }

        private async Task LoadTopCustomersAsync() //Created new async task to load top customers list with different order TODO: change order to descending by number of orders once function is written.
        {
            try
            {
                var request = new DataRequest<Customer>
                {
                    OrderByDesc = r => r.ChildrenAtHome //Changed order to be unique to help track success of changes made during local machine testing
                };
                CustomersTop = await CustomerService.GetCustomersAsync(0, 5, request);
            }
            catch (Exception ex)
            {
                LogException("Dashboard", "Load Customers", ex);
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                var request = new DataRequest<Order>
                {
                    OrderByDesc = r => r.OrderDate
                };
                Orders = await OrderService.GetOrdersAsync(0, 5, request);
            }
            catch (Exception ex)
            {
                LogException("Dashboard", "Load Orders", ex);
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var request = new DataRequest<Product>
                {
                    OrderByDesc = r => r.StockUnits //Changed order from arbitrary order to number of units stocked as this more closely reperesents the "top" products. TODO: change order to descending by number of products sold after corresponding function is written
                };
                Products = await ProductService.GetProductsAsync(0, 5, request);
            }
            catch (Exception ex)
            {
                LogException("Dashboard", "Load Products", ex);
            }
        }

        public void ItemSelected(string item)
        {
            switch (item)
            {
                case "Customers":
                    NavigationService.Navigate<CustomersViewModel>(new CustomerListArgs { OrderByDesc = r => r.CreatedOn });
                    break;
                case "Orders":
                    NavigationService.Navigate<OrdersViewModel>(new OrderListArgs { OrderByDesc = r => r.OrderDate });
                    break;
                case "Products":
                    NavigationService.Navigate<ProductsViewModel>(new ProductListArgs { OrderByDesc = r => r.ListPrice });
                    break;
                default:
                    break;
            }
        }
    }
}
