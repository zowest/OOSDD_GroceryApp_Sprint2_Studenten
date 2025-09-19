using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }


        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();

            var allProducts = _productService.GetAll();
            foreach (var product in allProducts)
            {
                bool isOnGroceryList = false;
                foreach (var item in MyGroceryListItems)
                {
                    if (item.ProductId == product.Id)
                    {
                        isOnGroceryList = true;
                        break;
                    }
                }
                if (!isOnGroceryList && product.Stock > 0)
                {
                    AvailableProducts.Add(product);
                }
            }
        }


        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)

        {
            if (product == null || product.Id <= 0)
                return;

            var groceryListItem = new GroceryListItem(0, GroceryList.Id, product.Id, 1);
            _groceryListItemsService.Add(groceryListItem);

            product.Stock -= 1;
            _productService.Update(product);

            if (product.Stock == 0)
            {
                AvailableProducts.Remove(product);
            }
            OnGroceryListChanged(GroceryList);
        }

    }
}
