namespace TEcommerceWebApi.Helpers
{
    public static class Permissions
    {
        // Category Permissions
        public const string CategoriesCreate = "categories.create";
        public const string CategoriesUpdate = "categories.update";
        public const string CategoriesDelete = "categories.delete";

        // Product Permissions
        public const string ProductsCreate = "products.create";
        public const string ProductsUpdate = "products.update";
        public const string ProductsDelete = "products.delete";

        // Analytics Permissions
        public const string AnalyticsView = "analytics.view";

        // Order Permissions
        public const string OrdersCreate = "orders.create";
        public const string OrdersViewAll = "orders.view_all";
        public const string OrdersManageStatus = "orders.manage_status";
    }
}